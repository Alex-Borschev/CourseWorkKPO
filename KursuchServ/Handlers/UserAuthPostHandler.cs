using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class UserAuthPostHandler : CommandHandler
    {
        public override string Command => "POST/api/user/auth";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            http.Response.ContentType = "application/json";

            // Проверка на обязательные поля
            if (!payload.TryGetProperty("login", out var loginProp) ||
                !payload.TryGetProperty("password", out var passwordProp))
            {
                await WriteError(http, "Отсутствуют обязательные поля", 500);
                Console.WriteLine("Отсутствуют обязательные поля");
                return;
            }

            string login = loginProp.GetString();
            string password = passwordProp.GetString();

            // Проверка пользователя
            var user = context.Db.ValidateUser(login, password);
            if (user == null)
            {
                await WriteError(http, "Неверные данные", 500);
                return;
            }

            string newToken = context.SessionService.CreateSession(user.Id);
            Console.WriteLine($"Пользователь авторизован: {user.Username}");

            
            await WriteOk(http, new
            {
                id = user.Id,
                login = user.Username,
                role = user.Personality,
                messages = user.Messages,
                token = newToken
            });
            Console.WriteLine("Авторизация успешна");
        }
    }
}
