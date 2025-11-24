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
    public class TermsPutHandlerTests
    {
        [Fact]
        public async Task Handle_UpdatesTerm_WhenAdminAndValidPayload()
        {
            // Arrange
            var term = new Term
            {
                id = "term1",
                term = "Routing",
                translations = new Dictionary<string, string> { { "en", "Routing" } },
                category = "Networking",
                difficultyLevel = "Medium",
                relatedTerms = new List<string>(),
                history = new List<HistoryEntry>()
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
            dbMock.Setup(db => db.UpdateTerm(It.IsAny<Term>())).Verifiable();

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("admin-token"))
                     .Returns(new SessionData { UserId = "admin1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new TermsPutHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "admin-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""id"": ""term1"",
                ""newDefinition"": {""en"": ""Updated Routing""},
                ""changeNote"": ""Updated definition"",
                ""newCategory"": ""Networking Updated"",
                ""newDifficulty"": ""Hard"",
                ""newSource"": ""TestSource"",
                ""newRelated"": [""term2"", ""term3""]
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Equal("Networking Updated", term.category);
            Assert.Equal("Hard", term.difficultyLevel);
            Assert.Equal("TestSource", term.source);
            Assert.Contains("term2", term.relatedTerms);
            Assert.Contains("term3", term.relatedTerms);
            Assert.Single(term.history);
            Assert.Equal("Updated definition", term.history[0].change);
            Assert.Equal("AdminUser", term.history[0].author);

            dbMock.Verify(db => db.UpdateTerm(term), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("Term updated", responseBody);
            Assert.Contains("term1", responseBody);
        }
    }
}
