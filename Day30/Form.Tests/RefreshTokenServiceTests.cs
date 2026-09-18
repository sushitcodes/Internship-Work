using Form.Entities;
using Form.Interfaces;
using Form.Services;
using Moq;
using Xunit;
//Test we do 3 A (Arrange , Assert , Act)
namespace Form.Tests
{
    public class RefreshTokenServiceTests
    {
        [Fact]//branch 1 
        public  async Task ValidateAndRoatateAsync_Unknown_ReturnFailure()
        { 
            // ARRANGE — build a fake repository and tell it exactly how to behave.
            var mockRepo = new Mock<IRefreshTokenRepository>();
            // The real repo would query the DB by hash and find nothing for a
            // token that was never issued. We simulate that directly: no matter
            // what hash string comes in, return null
            mockRepo
                .Setup(r=>r.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken?)null);
            // The service only depends on the interface, so we can construct it
            // with our fake in place of the real EF Core repository.

            var sut = new RefreshTokenService(mockRepo.Object);

            // ACT — call the method we're actually testing.
            var result = await sut.ValidateAndRotateAsync("some-raw-token-that-was-never-issued");

            // ASSERT — check the service did the right thing with a token it
            // has never seen before: reject, no user, no new token.
            Assert.False(result.Success);
            Assert.Null(result.User);
            Assert.Null(result.NewRawToken);
            // Nothing here should ever have tried to write anything — an
            // unknown token isn't a revocation event, it's a no-op.

            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
        [Fact]
        public async Task ValidateAndRotateASync_ExpiredToken_ReturnFailure()
        {
            // ARRANGE

            var mockRepo = new Mock<IRefreshTokenRepository>();


            // Build a fake token that looks exactly like one the DB would return,
            // except ExpiresAt is in the past. This is the object GetByHashAsync
            // will hand back — we're simulating "found it, but it's expired."
            var expiredToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TokenHash = "irrelevant for the test",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-8),
            };

            mockRepo
    .Setup(r => r.GetByHashAsync(It.IsAny<string>()))
    .ReturnsAsync(expiredToken);

            var sut = new RefreshTokenService(mockRepo.Object);

            //Act
            var result = await sut.ValidateAndRotateAsync("some-raw-token");

            // ASSERT
            Assert.False(result.Success);
            Assert.Null(result.User);

            // The expired branch returns immediately without touching the DB:
            //   if (existing.ExpiresAt < DateTime.UtcNow)
            //       return new RefreshRotationResult(false, null, null, null);
            // No mutation, no save. If a future change starts marking expired
            // tokens as revoked on the way out, this test will fail — which is
            // exactly the signal you want.
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);

        }

        [Fact]
        public async Task ValidateAndRotateAsync_RevokedTokenWithReplacement_RevokesAllUserTOken()
        {
            //Arrange

            var mockRepo = new Mock<IRefreshTokenRepository>();
            var userId = Guid.NewGuid();

            // This token is ALREADY revoked AND already has a replacement —
            // meaning it was legitimately rotated once, and now it's being
            // presented again. That combination is the theft signal.
            var reusedToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = "irrelevant-for-this-test",
                IsRevoked = true,
                ReplacedByTokenId = Guid.NewGuid(), // proves it was rotated already
                ExpiresAt = DateTime.UtcNow.AddDays(5), // not expired — that's not the issue here
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            mockRepo
                .Setup(r => r.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(reusedToken);

            var sut = new RefreshTokenService(mockRepo.Object);

            // ACT
            var result = await sut.ValidateAndRotateAsync("some-raw-token");
            // ASSERT
            Assert.False(result.Success);

            // This is the assertion that actually proves the security behavior:
            // not just "it said no," but "it went and killed every session for
            // this exact user." Times.Once with the specific userId — if the
            // service called it with the wrong id, or called it zero times,
            // this test should fail.
            mockRepo.Verify(r => r.RevokeAllForUserAsync(userId), Times.Once);
        }
        [Fact]
        public async Task ValidateAndRotateAsync_ValidToken_RotateSuccessfully()
        {
            // ARRANGE: a genuinely valid token — not revoked, not expired.
            // We attach a real User object because the service returns
            // `existing.User` on success, and the controller needs that
            // to build the new access token's claims.
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "student@example.com" };
            var validToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = "irrelevant-for-this-test",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                User = user,
            };

            var mockRepo = new Mock<IRefreshTokenRepository>();
            mockRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>())).ReturnsAsync(validToken);
            var sut = new RefreshTokenService (mockRepo.Object);

            //Act
            var result = await sut.ValidateAndRotateAsync("valid raw token");

            // ASSERT: success, a genuinely new token string came back (not
            // the same one we sent in), and it's tied to the right user.

            Assert.True(result.Success);
            Assert.NotNull(result.NewRawToken);
            Assert.Equal(user, result.User);

            // Behavioral proof of "rotation": the OLD token object must now
            // be marked revoked with a ReplacedByTokenId set — this is what
            // makes the theft-detection test above possible in the first
            // place. If this line regresses, that whole security branch
            // silently stops working.

            Assert.True(validToken.IsRevoked);
            Assert.NotNull(validToken.ReplacedByTokenId);

            // A new RefreshToken row must have been persisted for the
            // rotated-in replacement, and the update to the old row saved.

            mockRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);



        }
    }
}
