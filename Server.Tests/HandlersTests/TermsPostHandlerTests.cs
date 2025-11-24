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
    public class TermsPostHandlerTests
    {
        [Fact]
        public async Task Handle_AddsNewTerm_WhenUserIsAdmin()
        {
            // Arrange
            var user = new UserData
            {
                Id = "admin1",
                Username = "AdminUser",
                Personality = "Admin"
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.FindUserByID("admin1")).Returns(user);
            dbMock.Setup(d => d.GetTermByName(It.IsAny<string>())).Returns((Term)null);

            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("valid-token")).Returns(new SessionData { UserId = "admin1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new TermsPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var newTerm = new Term
            {
                term = "Routing",
                definition = "Definition",
                category = "Networking"
            };

            var payloadJson = JsonSerializer.Serialize(new { termData = newTerm });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            dbMock.Verify(d => d.AddTerm(It.Is<Term>(t => t.term == "Routing" && t.author == "AdminUser")), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("Routing", responseBody);
        }
    }
}
