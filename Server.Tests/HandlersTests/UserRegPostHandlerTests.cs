using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Server.Handlers;
using Server.Database;
using EntitiesLibrary;

namespace Server.Tests.HandlersTests
{
    public class UserRegPostHandlerTests
    {
        [Fact]
        public async Task Handle_RegistersUser_WhenValidData()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.FindUserByLogin("Alice")).Returns((UserData?)null);

            var context = new ServerContext(dbMock.Object);

            var handler = new UserRegPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new
            {
                login = "Alice",
                password = "123456"
            });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("Alice", responseBody);
            Assert.Contains("User", responseBody);
            dbMock.Verify(d => d.AddUser(It.Is<UserData>(u => u.Username == "Alice" && u.Personality == "User")), Times.Once);
        }

        [Fact]
        public async Task Handle_RegistersAdmin_WhenAdminKeyValid()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.FindUserByLogin("AdminUser")).Returns((UserData?)null);

            var context = new ServerContext(dbMock.Object);

            var handler = new UserRegPostHandler();

            // Задаем значение переменной ADMIN_KEY через reflection для теста
            var adminKeyField = typeof(UserRegPostHandler).GetField("ADMIN_KEY", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            adminKeyField.SetValue(handler, "SECRET_KEY");

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new
            {
                login = "AdminUser",
                password = "adminpass",
                adminKey = "SECRET_KEY"
            });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("AdminUser", responseBody);
            Assert.Contains("Admin", responseBody);
            dbMock.Verify(d => d.AddUser(It.Is<UserData>(u => u.Username == "AdminUser" && u.Personality == "Admin")), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenUserAlreadyExists()
        {
            // Arrange
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(d => d.FindUserByLogin("Alice")).Returns(new UserData { Username = "Alice" });

            var context = new ServerContext(dbMock.Object);
            var handler = new UserRegPostHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var payloadJson = JsonSerializer.Serialize(new
            {
                login = "Alice",
                password = "123456"
            });
            var payload = JsonDocument.Parse(payloadJson).RootElement;

            // Act
            await handler.Handle(payload, httpContext, context);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Contains("The user already exists", responseBody);
            dbMock.Verify(d => d.AddUser(It.IsAny<UserData>()), Times.Never);
        }
    }
}
