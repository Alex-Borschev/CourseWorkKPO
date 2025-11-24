using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Server.Handlers;
using Server.Database;

namespace Server.Tests.HandlersTests
{
    public class UploadPostHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsUrl_WhenValidBase64()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            var context = new ServerContext(dbMock.Object);

            var handler = new UploadPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            string fileContent = "Hello world!";
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

            var payloadJson = JsonSerializer.Serialize(new
            {
                name = "test.txt",
                base64 = base64
            });

            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            string responseBody = await reader.ReadToEndAsync();

            Assert.Contains("/uploads/", responseBody);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenInvalidBase64()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            var context = new ServerContext(dbMock.Object);

            var handler = new UploadPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new
            {
                name = "test.txt",
                base64 = "not_base64"
            });

            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            string responseBody = await reader.ReadToEndAsync();

            Assert.Contains("Invalid base64 data", responseBody);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenMissingFields()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            var context = new ServerContext(dbMock.Object);

            var handler = new UploadPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new { });

            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            string responseBody = await reader.ReadToEndAsync();

            Assert.Contains("Invalid request format", responseBody);
        }
    }
}
