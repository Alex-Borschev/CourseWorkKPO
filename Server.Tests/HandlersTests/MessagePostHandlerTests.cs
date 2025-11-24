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
    public class MessagePostHandlerTests
    {
        [Fact]
        public async Task Handle_AddsMessageToRecipient_WhenDataIsValid()
        {
            // Arrange
            var sender = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Messages = new List<MessageEntry>()
            };

            var recipient = new UserData
            {
                Id = "user2",
                Username = "Bob",
                Messages = new List<MessageEntry>()
            };

            // Мок базы данных
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(sender);
            dbMock.Setup(db => db.FindUserByID("user2")).Returns(recipient);
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            // Мок сессий
            var tokenServiceMock = new Mock<ITokenSessionService>();
            tokenServiceMock.Setup(ts => ts.ValidateToken("valid-token"))
                            .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenServiceMock.Object
            };

            var handler = new MessagePostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""to"": ""user2"",
                ""theme"": ""Greetings"",
                ""content"": ""Hello!""
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Single(recipient.Messages);
            var message = recipient.Messages[0];
            Assert.Equal("Alice", message.Author);
            Assert.Equal("Greetings", message.Theme);
            Assert.Equal("Hello!", message.Content);

            // Проверяем ответ
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("user2", responseBody);

            dbMock.Verify(db => db.UpdateUser(recipient), Times.Once);
        }
    }
}
