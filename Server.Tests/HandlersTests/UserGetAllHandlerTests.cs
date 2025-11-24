using System;
using System.IO;
using System.Linq;
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
    public class UserGetAllHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsAllUsers_WhenAuthorized()
        {
            // Arrange
            var users = new[]
            {
                new UserData { Id = "1", Username = "Alice", Personality = "Admin" },
                new UserData { Id = "2", Username = "Bob", Personality = "User" }
            }.ToList();

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.GetAllUsers()).Returns(users);

            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("valid-token"))
                       .Returns(new TokenServiceLibrary.SessionData { UserId = "1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new UserGetAllHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            // Act
            await handler.Handle(JsonDocument.Parse("{}").RootElement, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var jsonDoc = JsonDocument.Parse(responseBody);
            var result = jsonDoc.RootElement.EnumerateArray().ToList();

            Assert.Equal(2, result.Count);

            Assert.Contains(result, u => u.GetProperty("login").GetString() == "Alice" &&
                                         u.GetProperty("role").GetString() == "Admin");
            Assert.Contains(result, u => u.GetProperty("login").GetString() == "Bob" &&
                                         u.GetProperty("role").GetString() == "User");

            dbMock.Verify(d => d.GetAllUsers(), Times.Once);
            sessionMock.Verify(s => s.ValidateToken("valid-token"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("invalid-token")).Returns((TokenServiceLibrary.SessionData?)null);

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new UserGetAllHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "invalid-token";
            httpContext.Response.Body = new MemoryStream();

            // Act
            await handler.Handle(JsonDocument.Parse("{}").RootElement, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("The user is not authorized", responseBody);
            sessionMock.Verify(s => s.ValidateToken("invalid-token"), Times.Once);
        }
    }
}
