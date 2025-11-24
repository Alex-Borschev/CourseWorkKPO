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

namespace Server.Tests.HandlersTests
{
    public class TermsGetAllHandlerTests
    {
        private class TermResponse
        {
            public string id { get; set; }
            public string term { get; set; }
            public string category { get; set; }
            public int popularity { get; set; }
            public string difficultyLevel { get; set; }
            public DateTime addedDate { get; set; }
            public List<int> difficultyRatings { get; set; }
            public string mediaUrl { get; set; }
        }

        [Fact]
        public async Task Handle_ReturnsAllTermsWithMediaUrl()
        {
            // Arrange
            var terms = new List<Term>
            {
                new Term
                {
                    id = "1",
                    term = "Routing",
                    category = "Networking",
                    popularity = 10,
                    difficultyLevel = "Easy",
                    addedDate = DateTime.UtcNow,
                    difficultyRatings = new List<int> { 3, 4 },
                    media = new List<MediaEntry> { new MediaEntry { url = "/images/routing.png" } }
                },
                new Term
                {
                    id = "2",
                    term = "SQL",
                    category = "Databases",
                    popularity = 15,
                    difficultyLevel = "Medium",
                    addedDate = DateTime.UtcNow,
                    difficultyRatings = new List<int> { 5 },
                    media = null
                }
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.GetAllTerms()).Returns(terms);

            var context = new ServerContext(dbMock.Object);

            var handler = new TermsGetAllHandler();
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Set environment variables for HOST and PORT
            Environment.SetEnvironmentVariable("HOST", "localhost");
            Environment.SetEnvironmentVariable("PORT", "5000");

            var payload = JsonDocument.Parse("{}").RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var result = JsonSerializer.Deserialize<List<TermResponse>>(responseBody);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var firstTerm = result[0];
            Assert.Equal("Routing", firstTerm.term);
            Assert.Equal("Networking", firstTerm.category);
            Assert.Contains("/images/routing.png", firstTerm.mediaUrl);

            var secondTerm = result[1];
            Assert.Equal("SQL", secondTerm.term);
            Assert.Equal("Databases", secondTerm.category);
            Assert.Null(secondTerm.mediaUrl);
        }
    }
}
