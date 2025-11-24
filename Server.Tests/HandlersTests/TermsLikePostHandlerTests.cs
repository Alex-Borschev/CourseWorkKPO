using System;
using System.Collections.Generic;
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
    public class TermsLikePostHandlerTests
    {
        [Fact]
        public async Task Handle_AddsAndRemovesFavorites()
        {
            // Arrange
            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Favorites = new List<string>()
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.FindUserByID("user1")).Returns(user);

            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("valid-token")).Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new TermsLikePostHandler();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();
            var payloadJson = JsonSerializer.Serialize(new { term = "Routing", isFavorite = true });
            var payload = JsonDocument.Parse(payloadJson).RootElement;
            await handler.Handle(payload, httpContext, context);
            Assert.Contains("Routing", user.Favorites);
            httpContext.Response.Body = new MemoryStream();
            payloadJson = JsonSerializer.Serialize(new { term = "Routing", isFavorite = false });
            payload = JsonDocument.Parse(payloadJson).RootElement;
            await handler.Handle(payload, httpContext, context);
            Assert.DoesNotContain("Routing", user.Favorites);
        }
    }
}
