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
    public class MessageDeleteHandlerTests
    {
        [Fact]
        public async Task Handle_ClearsMessages_WhenUserExists()
        {
            // Arrange
            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Messages = new List<MessageEntry>
                {
                    new MessageEntry { Author = "Alice", Content = "Hello", Theme = "Test", Timestamp = DateTime.UtcNow }
                }
            };

            // Мок базы данных
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Callback<UserData>(u => user = u);

            // Мок сервиса сессий
            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("token123")).Returns(new SessionData { UserId = "user1" });

            // Контекст
            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new MessageDeleteHandler();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token123";
            httpContext.Response.Body = new MemoryStream();

            // Act
            await handler.Handle(JsonDocument.Parse("{}").RootElement, httpContext, context);

            // Assert
            Assert.Empty(user.Messages);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("Messages has been deleted", responseBody);
        }
    }
}
