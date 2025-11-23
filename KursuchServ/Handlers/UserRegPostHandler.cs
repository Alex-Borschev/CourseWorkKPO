using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Handlers
{
    public class UserRegPostHandler : CommandHandler
    {
        public override string Command => "POST/api/user/reg";

        private const string ADMIN_KEY = "SECRET_KEY_2025";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            http.Response.ContentType = "application/json";

            try
            {
                if (!payload.TryGetProperty("login", out var loginProp) ||
                    !payload.TryGetProperty("password", out var passProp))
                {
                    await WriteError(http, "Неверный формат", 400);
                    return;
                }

                string login = loginProp.GetString();
                string password = passProp.GetString();

                // Роль пользователя
                string role = "User";

                if (payload.TryGetProperty("adminKey", out var keyProp))
                {
                    string key = keyProp.GetString();

                    if (key == ADMIN_KEY)
                    {
                        role = "Admin";
                    }
                    else
                    {
                        await WriteError(http, "Неверный adminKey", 400);
                        return;
                    }
                }

                // Проверяем, существует ли пользователь
                if (context.Db.FindUserByLogin(login) != null)
                {
                    await WriteError(http, "Пользователь уже существует", 400);
                    return;
                }

                // Создание нового пользователя
                var newUser = new UserData
                {
                    Username = login,
                    Personality = role,
                    Password = password,
                    RegistrationDate = DateTime.Now,
                    Favorites = new List<string>(),
                    RatedTerms = new List<RatedTerm>(),
                    Notes = new List<UserNotes>(),
                    Messages = new List<MessageEntry>()
                };

                context.Db.AddUser(newUser);
                await WriteOk(http, new
                {
                    login = newUser.Username,
                    role = newUser.Personality
                });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка регистрации: {ex}", 500);
                return;
            }
        }
    }
}
