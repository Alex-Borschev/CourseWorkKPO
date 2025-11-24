using System;
using Xunit;
using TokenServiceLibrary;

namespace Server.Tests.ServicesTests
{
    public class TokenSessionServiceTests
    {
        private ITokenSessionService GetService()
        {
            // Интервал жизни сессии 1 минута для тестов
            return new TokenSessionService(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void CreateSession_ReturnsToken_And_ValidateToken_Works()
        {
            // Arrange
            var service = GetService();
            string userId = "user123";

            // Act
            string token = service.CreateSession(userId);
            var session = service.ValidateToken(token);

            // Assert
            Assert.NotNull(token);
            Assert.NotNull(session);
            Assert.Equal(userId, session.UserId);
        }

        [Fact]
        public void ValidateToken_ReturnsNull_ForInvalidToken()
        {
            var service = GetService();
            var session = service.ValidateToken("invalid-token");

            Assert.Null(session);
        }

        [Fact]
        public void RemoveSession_DeletesToken()
        {
            var service = GetService();
            string token = service.CreateSession("user456");

            service.RemoveSession(token);
            var session = service.ValidateToken(token);

            Assert.Null(session);
        }

        [Fact]
        public void ExpiredSession_IsRemoved()
        {
            var service = new TokenSessionService(TimeSpan.FromMilliseconds(100));
            string token = service.CreateSession("user789");

            // Ждем истечения времени
            System.Threading.Thread.Sleep(200);

            var session = service.ValidateToken(token);

            Assert.Null(session);
        }

        [Fact]
        public void CleanupExpired_RemovesOldSessions()
        {
            var service = new TokenSessionService(TimeSpan.FromMilliseconds(100));
            string token1 = service.CreateSession("user1");
            string token2 = service.CreateSession("user2");

            System.Threading.Thread.Sleep(200);
            service.CleanupExpired();

            Assert.Null(service.ValidateToken(token1));
            Assert.Null(service.ValidateToken(token2));
        }
    }
}
