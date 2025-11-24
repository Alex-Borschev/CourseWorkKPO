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
    public class NotePostHandlerTests
    {
        [Fact]
        public async Task Handle_AddsNewNote_WhenNoteDoesNotExist()
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
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("valid-token"))
                     .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new NotePostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""term"": ""Routing"",
                ""note"": ""This is a test note""
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Single(user.Notes);
            Assert.Equal("Routing", user.Notes[0].NotedTerm);
            Assert.Equal("This is a test note", user.Notes[0].NotedData);

            dbMock.Verify(db => db.UpdateUser(user), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("The note has been updated.", responseBody);
        }

        [Fact]
        public async Task Handle_UpdatesExistingNote_WhenNoteAlreadyExists()
        {
            // Arrange
            var existingNote = new UserNotes
            {
                NotedTerm = "Routing",
                NotedData = "Old note",
                Timestamp = DateTime.UtcNow.AddDays(-1)
            };

            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                Notes = new List<UserNotes> { existingNote }
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("valid-token"))
                     .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new NotePostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""term"": ""Routing"",
                ""note"": ""Updated note""
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Single(user.Notes);
            Assert.Equal("Routing", user.Notes[0].NotedTerm);
            Assert.Equal("Updated note", user.Notes[0].NotedData);

            dbMock.Verify(db => db.UpdateUser(user), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("The note has been updated.", responseBody);
        }
    }
}
