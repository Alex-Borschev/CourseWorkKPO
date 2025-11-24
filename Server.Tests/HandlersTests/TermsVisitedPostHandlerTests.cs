using System;
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
    public class TermsVisitedPostHandlerTests
    {
        [Fact]
        public async Task Handle_UpdatesTermLastAccessedAndPopularity()
        {
            // Arrange
            var term = new Term
            {
                id = "term1",
                term = "Routing",
                popularity = 5,
                lastAccessed = DateTime.MinValue,
                media = new List<MediaEntry>
                {
                    new MediaEntry { url = "/media/img1.png" }
                }
            };

            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.GetTermByID("term1")).Returns(term);

            var context = new ServerContext(dbMock.Object);

            var handler = new TermsVisitedPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new { term = "term1" });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            dbMock.Verify(d => d.UpdateTerm(It.Is<Term>(t => t.id == "term1" && t.popularity == 6 && t.lastAccessed > DateTime.MinValue)), Times.Once);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("Routing", responseBody);
            Assert.Contains("/media/img1.png", responseBody);
        }
    }
}
