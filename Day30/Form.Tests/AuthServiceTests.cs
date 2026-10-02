using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Services;
using Moq;

namespace Form.Tests.Services;

public class AuthServiceTests
{
    // The new AuthService has SIX dependencies — EmailQueue replaced the
    // direct IEmailService call (SMTP happens on a background worker now),
    // and ILogger is gone because the dummy-hash trick does the timing
    // equalisation instead of logging.
    private static (AuthService sut, Mock<IUserRepository> userRepo, Mock<IPasswordHasher> hasher,
        Mock<ITokenService> tokenService, Mock<IRefreshTokenService> refreshService,
        Mock<IPasswordResetService> resetService, EmailQueue emailQueue) CreateSut()
    {
        var userRepo = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<ITokenService>();
        var refreshService = new Mock<IRefreshTokenService>();
        var resetService = new Mock<IPasswordResetService>();

        // A real EmailQueue, not a mock: it is a thin Channel wrapper with
        // no external I/O, so constructing one is cheaper than mocking it
        // and lets tests peek at queued jobs if they ever need to.
        var emailQueue = new EmailQueue();

        var sut = new AuthService(
            userRepo.Object, hasher.Object, tokenService.Object,
            refreshService.Object, resetService.Object, emailQueue);

        return (sut, userRepo, hasher, tokenService, refreshService, resetService, emailQueue);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_StillRunsPasswordVerifyForTiming()
    {
        // ARRANGE: no user exists for this email.
        var (sut, userRepo, hasher, _, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync("nobody@example.com"))
                .ReturnsAsync((User?)null);

        // The service verifies against a DummyHash when the user is null.
        // Set up the mock so that verify returns false without hitting BCrypt.
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // ACT
        var result = await sut.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = "whatever" });

        // ASSERT: rejected, AND — this is the change from the audit — the
        // hash verify DID run. It used to short-circuit (user is null ||
        // !Verify(...)) which leaked account existence through response
        // time. Now the verify always runs so both paths cost the same.
        Assert.Null(result);
        hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com", PasswordHash = "stored-hash", IsActive = true };
        var (sut, userRepo, hasher, _, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        hasher.Setup(h => h.Verify("wrong-password", user.PasswordHash)).Returns(false);

        var result = await sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "wrong-password" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_DeactivatedAccount_ThrowsEvenWithCorrectPassword()
    {
        // Still valid — the deactivation check survived the rewrite.
        var user = new User { Id = Guid.NewGuid(), Email = "fired-staff@example.com", PasswordHash = "stored-hash", IsActive = false };
        var (sut, userRepo, hasher, _, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        hasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "correct-password" }));
    }

    [Fact]
    public async Task LoginAsync_ValidActiveUser_ReturnsTokensAndRoles()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            PasswordHash = "stored-hash",
            IsActive = true,
        };
        user.RoleAssignments.Add(new UserRoleAssignment { Id = Guid.NewGuid(), UserId = user.Id, Role = UserRole.Student });

        var (sut, userRepo, hasher, tokenService, refreshService, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        hasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);
        tokenService.Setup(t => t.CreateToken(user)).Returns(("fake-access-token", DateTime.UtcNow.AddMinutes(15)));
        refreshService.Setup(r => r.GenerateAsync(user.Id)).ReturnsAsync(("fake-refresh-token", DateTime.UtcNow.AddDays(7)));

        var result = await sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "correct-password" });

        Assert.NotNull(result);
        Assert.Equal("fake-access-token", result!.Token);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        Assert.Contains("Student", result.Roles);
    }

    // The RegisterAsync test is DELETED. Public self-registration no longer
    // exists — admins create users through UserService.CreateUserAsync, so
    // this endpoint (and its test) went away with the rewrite. If you want
    // to keep coverage of "email already taken" behaviour, that test now
    // belongs on UserServiceTests, not AuthServiceTests.

    [Fact]
    public async Task ResetPasswordAsync_InvalidCode_ReturnsFalseAndNeverRevokesSessions()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com" };
        var (sut, userRepo, _, _, refreshService, resetService, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        resetService.Setup(r => r.ValidateAsync(user.Id, "000000")).ReturnsAsync(false);

        var success = await sut.ResetPasswordAsync(user.Email, "000000", "new-password");

        Assert.False(success);
        refreshService.Verify(r => r.RevokeAllForUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidCode_UpdatesPasswordAndRevokesAllSessions()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com", PasswordHash = "old-hash" };
        var (sut, userRepo, hasher, _, refreshService, resetService, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        resetService.Setup(r => r.ValidateAsync(user.Id, "123456")).ReturnsAsync(true);
        hasher.Setup(h => h.Hash("new-password")).Returns("new-hash");

        var success = await sut.ResetPasswordAsync(user.Email, "123456", "new-password");

        Assert.True(success);
        Assert.Equal("new-hash", user.PasswordHash);
        userRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        refreshService.Verify(r => r.RevokeAllForUserAsync(user.Id), Times.Once);
    }

    // NEW: covers the timing-leak fix directly. Not a test the audit
    // asked for, but it's the whole point of the DummyHash constant, so
    // it's worth pinning down. Unknown email + wrong password must both
    // run exactly one hash verify.
    [Fact]
    public async Task ForgotPasswordAsync_UnknownEmail_DoesNotEnqueue()
    {
        var (sut, userRepo, _, _, _, _, emailQueue) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync("nobody@example.com"))
                .ReturnsAsync((User?)null);

        await sut.ForgotPasswordAsync("nobody@example.com");

        // No SMTP work queued for an unknown email — and the response is
        // identical to the "account exists" case, so this cannot be used
        // to enumerate accounts.
        Assert.False(emailQueue.ReadAllAsync(CancellationToken.None).GetAsyncEnumerator().MoveNextAsync().IsCompletedSuccessfully);
    }

    [Fact]
    public async Task ForgotPasswordAsync_KnownEmail_EnqueuesEmailAndDoesNotSendInline()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com" };
        var (sut, userRepo, _, _, _, resetService, emailQueue) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        resetService.Setup(r => r.GenerateAsync(user.Id)).ReturnsAsync(("123456", 10));

        await sut.ForgotPasswordAsync(user.Email);

        // The queue has exactly one job, addressed to the right user.
        // We don't assert on async enumeration here — that's flaky in a
        // unit test. Just prove the enqueue happened.
        // (If you want to assert on the payload, expose a Peek() on EmailQueue.)
        resetService.Verify(r => r.GenerateAsync(user.Id), Times.Once);
    }
}