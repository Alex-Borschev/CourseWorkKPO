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

namespace Server.Tests.HandlersTests
{
    public class CategoriesGetHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsDistinctCategories()
        {
            // Arrange: создаём список терминов
            var terms = new List<Term>
            {
                new Term { id = "1", category = "Networking" },
                new Term { id = "2", category = "Databases" },
                new Term { id = "3", category = "Networking" }, // дубликат
                new Term { id = "4", category = "" }            // пустая категория
            };

            // Мокаем интерфейс IDatabaseService
            var dbMock = new Mock<IDatabaseService>();
            dbMock.Setup(db => db.GetAllTerms()).Returns(terms);

            // Создаём ServerContext с мокнутой базой
            var context = new ServerContext(dbMock.Object as DatabaseService)
            {
                Db = dbMock.Object // заменяем реальный DatabaseService на мок
            };

            var handler = new CategoriesGetHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act: вызываем хендлер
            await handler.Handle(JsonDocument.Parse("{}").RootElement, httpContext, context);

            // Assert: читаем и проверяем ответ
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var categories = JsonSerializer.Deserialize<List<string>>(responseBody);

            Assert.NotNull(categories);
            Assert.Contains("Networking", categories);
            Assert.Contains("Databases", categories);
            Assert.DoesNotContain("", categories);
            Assert.Equal(2, categories.Count); // должно быть только 2 уникальные категории
        }
    }
}
