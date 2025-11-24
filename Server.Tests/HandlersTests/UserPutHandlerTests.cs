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
    public class UserPutHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsUserData_WhenAuthorized()
        {
            // Arrange
            var user = new UserData
            {
                Id = "1",
                Username = "Alice",
                Personality = "Admin",
                Favorites = new System.Collections.Generic.List<string> { "term1" },
                Messages = new System.Collections.Generic.List<MessageEntry>
                {
                    new MessageEntry { Author = "Bob", Content = "Hi", Theme = "Test", Timestamp = DateTime.Now }
                },
                Notes = new System.Collections.Generic.List<UserNotes>
                {
                    new UserNotes { NotedTerm = "term1", NotedData = "note1", Timestamp = DateTime.Now }
                },
                RatedTerms = new System.Collections.Generic.List<RatedTerm>
                {
                    new RatedTerm { Term = "term1", Rating = 5 }
                },
                RegistrationDate = DateTime.Now.AddDays(-10)
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.FindUserByID("1")).Returns(user);

            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("valid-token"))
                       .Returns(new SessionData { UserId = "1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new UserPutHandler();

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
            var root = jsonDoc.RootElement;

            Assert.Equal("1", root.GetProperty("id").GetString());
            Assert.Equal("Alice", root.GetProperty("username").GetString());
            Assert.Equal("Admin", root.GetProperty("personality").GetString());

            var favorites = root.GetProperty("favorites").EnumerateArray().Select(x => x.GetString()).ToList();
            Assert.Contains("term1", favorites);

            var messages = root.GetProperty("messages").EnumerateArray().ToList();
            Assert.Single(messages);
            Assert.Equal("Hi", messages[0].GetProperty("Content").GetString());

            var notes = root.GetProperty("notes").EnumerateArray().ToList();
            Assert.Single(notes);
            Assert.Equal("term1", notes[0].GetProperty("NotedTerm").GetString());

            var rated = root.GetProperty("ratedTerms").EnumerateArray().ToList();
            Assert.Single(rated);
            Assert.Equal("term1", rated[0].GetProperty("Term").GetString());
            Assert.Equal(5, rated[0].GetProperty("Rating").GetInt32());

            dbMock.Verify(d => d.FindUserByID("1"), Times.Once);
            sessionMock.Verify(s => s.ValidateToken("valid-token"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            var sessionMock = new Mock<ITokenSessionService>();
            sessionMock.Setup(s => s.ValidateToken("invalid-token")).Returns((SessionData?)null);

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = sessionMock.Object
            };

            var handler = new UserPutHandler();

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
