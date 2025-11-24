using System;
using System.Collections.Generic;
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
    public class MessageSuggestionPostHandlerTests
    {
        [Fact]
        public async Task Handle_SendsSuggestionToAllAdmins()
        {
            // Arrange
            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Personality = "User"
            };

            var admin1 = new UserData
            {
                Id = "admin1",
                Username = "AdminOne",
                Personality = "Admin",
                Messages = new List<MessageEntry>()
            };

            var admin2 = new UserData
            {
                Id = "admin2",
                Username = "AdminTwo",
                Personality = "Admin",
                Messages = new List<MessageEntry>()
            };

            var allUsers = new List<UserData> { user, admin1, admin2 };

            // Мок базы данных
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);
            dbMock.Setup(db => db.GetAllUsers()).Returns(allUsers);
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            // Мок сессий
            var tokenServiceMock = new Mock<ITokenSessionService>();
            tokenServiceMock.Setup(ts => ts.ValidateToken("valid-token"))
                            .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenServiceMock.Object
            };

            var handler = new MessageSuggestionPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""termName"": ""Routing"",
                ""suggestion"": ""Add more examples""
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Single(admin1.Messages);
            Assert.Single(admin2.Messages);

            var msg1 = admin1.Messages[0];
            var msg2 = admin2.Messages[0];

            Assert.Equal("Routing", msg1.Theme);
            Assert.Equal("Add more examples", msg1.Content);
            Assert.Equal("Alice", msg1.Author);

            Assert.Equal("Routing", msg2.Theme);
            Assert.Equal("Add more examples", msg2.Content);
            Assert.Equal("Alice", msg2.Author);

            dbMock.Verify(db => db.UpdateUser(admin1), Times.Once);
            dbMock.Verify(db => db.UpdateUser(admin2), Times.Once);

            // Проверяем ответ
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("Suggestion has been sent", responseBody);
        }
    }
}
