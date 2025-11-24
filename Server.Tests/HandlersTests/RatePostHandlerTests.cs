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
    public class RatePostHandlerTests
    {
        [Fact]
        public async Task Handle_AddsRating_WhenNewRatingProvided()
        {
            // Arrange
            var term = new Term
            {
                id = "term1",
                term = "Routing",
                difficultyRatings = new List<int>()
            };

            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                RatedTerms = new List<RatedTerm>()
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);
            dbMock.Setup(db => db.GetTermByID("term1")).Returns(term);
            dbMock.Setup(db => db.UpdateTerm(It.IsAny<Term>())).Verifiable();
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("valid-token"))
                     .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new RatePostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""term"": ""term1"",
                ""rating"": 5
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Single(user.RatedTerms);
            Assert.Equal("term1", user.RatedTerms[0].Term);
            Assert.Equal(5, user.RatedTerms[0].Rating);

            Assert.Single(term.difficultyRatings);
            Assert.Equal(5, term.difficultyRatings[0]);

            dbMock.Verify(db => db.UpdateUser(user), Times.Once);
            dbMock.Verify(db => db.UpdateTerm(term), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("term1", responseBody);
            Assert.Contains("5", responseBody);
        }

        [Fact]
        public async Task Handle_RemovesRating_WhenRatingIsZero()
        {
            // Arrange
            var term = new Term
            {
                id = "term1",
                term = "Routing",
                difficultyRatings = new List<int> { 5 }
            };

            var user = new UserData
            {
                Id = "user1",
                Username = "Alice",
                RatedTerms = new List<RatedTerm>
                {
                    new RatedTerm { Term = "term1", Rating = 5 }
                }
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.FindUserByID("user1")).Returns(user);
            dbMock.Setup(db => db.GetTermByID("term1")).Returns(term);
            dbMock.Setup(db => db.UpdateTerm(It.IsAny<Term>())).Verifiable();
            dbMock.Setup(db => db.UpdateUser(It.IsAny<UserData>())).Verifiable();

            var tokenMock = new Mock<ITokenSessionService>();
            tokenMock.Setup(ts => ts.ValidateToken("valid-token"))
                     .Returns(new SessionData { UserId = "user1" });

            var context = new ServerContext(dbMock.Object)
            {
                SessionService = tokenMock.Object
            };

            var handler = new RatePostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "valid-token";
            httpContext.Response.Body = new MemoryStream();

            var payload = JsonDocument.Parse(@"
            {
                ""term"": ""term1"",
                ""rating"": 0
            }").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            Assert.Empty(user.RatedTerms);
            Assert.Empty(term.difficultyRatings);

            dbMock.Verify(db => db.UpdateUser(user), Times.Once);
            dbMock.Verify(db => db.UpdateTerm(term), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("term1", responseBody);
        }
    }
}
