using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Server.Handlers;
using EntitiesLibrary;
using Server.Database;
using TokenServiceLibrary;

namespace Server.Tests.HandlersTests
{
    public class UserAuthPostHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsTokenAndUserData_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Personality = "Admin",
                Messages = new System.Collections.Generic.List<MessageEntry>()
            };

            // Мок базы данных
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.ValidateUser("Alice", "password123")).Returns(user);

            // Мок сессий
            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.CreateSession("user1")).Returns("mocked-token");

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new UserAuthPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new { login = "Alice", password = "password123" });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("mocked-token", responseBody);
            Assert.Contains("Alice", responseBody);
            Assert.Contains("Admin", responseBody);

            dbMock.Verify(d => d.ValidateUser("Alice", "password123"), Times.Once);
            sessionMock.Verify(s => s.CreateSession("user1"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenCredentialsAreInvalid()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.ValidateUser("Alice", "wrongpass")).Returns((UserData?)null);

            var context = new ServerContext(dbMock.Object);

            var handler = new UserAuthPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new { login = "Alice", password = "wrongpass" });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("Wrong data", responseBody);
        }
    }
}
