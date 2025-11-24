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
    public class TermsDeleteHandlerTests
    {
        [Fact]
        public async Task Handle_DeletesTerm_WhenAdminAndTermExists()
        {
            // Arrange
            var term = new Term
            {
                id = "term1",
                term = "Routing"
            };

            var adminUser = new UserData
            {
                Id = "admin1",
                Username = "AdminUser",
                Personality = "Admin"
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.GetTermByID("term1")).Returns(term);
            dbMock.Setup(db => db.FindUserByID("admin1")).Returns(adminUser);
            dbMock.Setup(db => db.DeleteTermByID("term1")).Verifiable();

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("admin-token"))
                     .Returns(new SessionData { UserId = "admin1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new TermsDeleteHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "admin-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"{ ""term"": ""term1"" }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            dbMock.Verify(db => db.DeleteTermByID("term1"), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("term1", responseBody);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenUserIsNotAdmin()
        {
            // Arrange
            var user = new UserData
            {
                Id = "user1",
                Username = "NormalUser",
                Personality = "User"
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("user-token"))
                     .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new TermsDeleteHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "user-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"{ ""term"": ""term1"" }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            dbMock.Verify(db => db.DeleteTermByID(It.IsAny<string>()), Times.Never);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("can not delete terms", responseBody);
        }
    }
}
