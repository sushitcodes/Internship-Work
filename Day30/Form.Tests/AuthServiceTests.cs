using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Form.Tests.Services;

public class AuthServiceTests
{
    // AuthService takes SEVEN dependencies. Rather than repeat this setup
    // in every test, one small helper builds all the mocks and hands back
    // both the service and the mocks — so each test only configures the
    // ONE or TWO mocks it actually cares about.
    private static (AuthService sut, Mock<IUserRepository> userRepo, Mock<IPasswordHasher> hasher,
        Mock<ITokenService> tokenService, Mock<IRefreshTokenService> refreshService,
        Mock<IPasswordResetService> resetService) CreateSut()
    {
        var userRepo = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<ITokenService>();
        var refreshService = new Mock<IRefreshTokenService>();
        var resetService = new Mock<IPasswordResetService>();
        var emailService = new Mock<IEmailService>();
        var logger = new Mock<ILogger<AuthService>>();

        var sut = new AuthService(
            userRepo.Object, hasher.Object, tokenService.Object,
            refreshService.Object, resetService.Object, emailService.Object, logger.Object);

        return (sut, userRepo, hasher, tokenService, refreshService, resetService);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsNullWithoutCheckingPassword()
    {
        // ARRANGE: no user exists for this email.
        var (sut, userRepo, hasher, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync("nobody@example.com")).ReturnsAsync((User?)null);

        // ACT
        var result = await sut.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = "whatever" });

        // ASSERT: rejected, AND — this is the actual security property —
        // password verification never even ran. `user is null || !Verify(...)`
        // short-circuits, so Verify should never be called for an email
        // that doesn't exist. This is what stops the response from being
        // usable to enumerate which emails are registered.
        Assert.Null(result);
        hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        // ARRANGE: real user exists, but the password won't match.
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com", PasswordHash = "stored-hash", IsActive = true };
        var (sut, userRepo, hasher, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        hasher.Setup(h => h.Verify("wrong-password", user.PasswordHash)).Returns(false);

        // ACT
        var result = await sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "wrong-password" });

        // ASSERT: same null as "unknown email" above — deliberately
        // indistinguishable from the outside, so a failed login never
        // tells an attacker whether the email or the password was wrong.
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_DeactivatedAccount_ThrowsEvenWithCorrectPassword()
    {
        // This is a regression test. Look at the comment already sitting
        // in AuthService.LoginAsync: "Was missing entirely — this is the
        // actual check that never existed." A deactivated admin can still
        // remember their correct password — this test exists specifically
        // so that check can never silently disappear again.
        var user = new User { Id = Guid.NewGuid(), Email = "fired-staff@example.com", PasswordHash = "stored-hash", IsActive = false };
        var (sut, userRepo, hasher, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        hasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);

        // ACT + ASSERT: expect the specific exception the service throws,
        // not just "something went wrong."
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "correct-password" }));
    }

    [Fact]
    public async Task LoginAsync_ValidActiveUser_ReturnsTokensAndRoles()
    {
        // ARRANGE: the full success path — everything lines up.
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            PasswordHash = "stored-hash",
            IsActive = true,
        };
        user.RoleAssignments.Add(new UserRoleAssignment { Id = Guid.NewGuid(), UserId = user.Id, Role = UserRole.Student });

        var (sut, userRepo, hasher, tokenService, refreshService, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        hasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);
        tokenService.Setup(t => t.CreateToken(user)).Returns(("fake-access-token", DateTime.UtcNow.AddMinutes(15)));
        refreshService.Setup(r => r.GenerateAsync(user.Id)).ReturnsAsync(("fake-refresh-token", DateTime.UtcNow.AddDays(7)));

        // ACT
        var result = await sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "correct-password" });

        // ASSERT: the DTO the controller actually returns to the browser
        // is built correctly from all the pieces.
        Assert.NotNull(result);
        Assert.Equal("fake-access-token", result!.Token);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        Assert.Contains("Student", result.Roles);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsAndNeverCreatesUser()
    {
        // ARRANGE: GetByEmailAsync finds a match — this email is taken.
        var (sut, userRepo, _, _, _, _) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync("taken@example.com")).ReturnsAsync(new User { Email = "taken@example.com" });

        // ACT + ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.RegisterAsync(new RegisterRequest { Email = "taken@example.com", Password = "whatever" }));

        // Proof the rejection happened BEFORE any write — a duplicate
        // attempt must never reach the database.
        userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidCode_ReturnsFalseAndNeverRevokesSessions()
    {
        // ARRANGE: user exists, but the reset code they typed is wrong.
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com" };
        var (sut, userRepo, _, _, refreshService, resetService) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        resetService.Setup(r => r.ValidateAsync(user.Id, "000000")).ReturnsAsync(false);

        // ACT
        var success = await sut.ResetPasswordAsync(user.Email, "000000", "new-password");

        // ASSERT: failed, and nothing about the account changed — a wrong
        // code must not be able to log everyone else out as a side effect.
        Assert.False(success);
        refreshService.Verify(r => r.RevokeAllForUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidCode_UpdatesPasswordAndRevokesAllSessions()
    {
        // This is the other regression-protection test: a real password
        // reset MUST kill every existing session, otherwise an attacker
        // who was already logged in stays logged in after the real owner
        // takes their account back.
        var user = new User { Id = Guid.NewGuid(), Email = "student@example.com", PasswordHash = "old-hash" };
        var (sut, userRepo, hasher, _, refreshService, resetService) = CreateSut();
        userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        resetService.Setup(r => r.ValidateAsync(user.Id, "123456")).ReturnsAsync(true);
        hasher.Setup(h => h.Hash("new-password")).Returns("new-hash");

        // ACT
        var success = await sut.ResetPasswordAsync(user.Email, "123456", "new-password");

        // ASSERT
        Assert.True(success);
        Assert.Equal("new-hash", user.PasswordHash); // the in-memory object was actually updated
        userRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        refreshService.Verify(r => r.RevokeAllForUserAsync(user.Id), Times.Once);
    }
}