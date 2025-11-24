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
    public class NoteDeleteHandlerTests
    {
        [Fact]
        public async Task Handle_DeletesNote_WhenNoteExists()
        {
            // Arrange
            var note = new UserNotes
            {
                NotedTerm = "Routing",
                NotedData = "Some note",
                Timestamp = DateTime.UtcNow
            };

            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Notes = new List<UserNotes> { note }
            };

            // Мок базы данных
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            // Мок сессий
            var tokenServiceMock = new Mock<ITokenSessionService>();
            tokenServiceMock.Setup(ts => ts.ValidateToken("valid-token"))
                            .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenServiceMock.Object
            };

            var handler = new NoteDeleteHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""term"": ""Routing""
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Empty(user.Notes);
            dbMock.Verify(db => db.UpdateUser(user), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("The note has been deleted", responseBody);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenNoteDoesNotExist()
        {
            // Arrange
            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Notes = new List<UserNotes>()
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);

            var tokenServiceMock = new Mock<ITokenSessionService>();
            tokenServiceMock.Setup(ts => ts.ValidateToken("valid-token"))
                            .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenServiceMock.Object
            };

            var handler = new NoteDeleteHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""term"": ""Routing""
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("There is no such note", responseBody);
        }
    }
}
