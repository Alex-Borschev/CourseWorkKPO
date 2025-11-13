using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;

namespace Server.Handlers
{
    public class RegisterHandler : ICommandHandler
    {
        public string Command { get { return "REGISTER"; } }

        private const string ADMIN_KEY = "SECRET_KEY_2025";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            JsonElement loginProp;
            JsonElement passProp;

            if (!payload.TryGetProperty("login", out loginProp) ||
                !payload.TryGetProperty("password", out passProp))
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Неверный формат"));
                return;
            }

            string login = loginProp.GetString();
            string password = passProp.GetString();

            // определяем роль
            string role = "User";

            JsonElement keyProp;
            if (payload.TryGetProperty("adminKey", out keyProp))
            {
                string key = keyProp.GetString();
                if (key == ADMIN_KEY)
                {
                    role = "Admin";
                }
                else
                {
                    TcpServer.SendResponse(stream,
                        ServerResponse.Error("Неверный adminKey"));
                    return;
                }
            }

            // проверка существования
            if (context.Db.FindUserByLogin(login) != null)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Пользователь уже существует"));
                return;
            }

            // создание пользователя
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

            TcpServer.SendResponse(stream,
                ServerResponse.Ok("Регистрация успешна",
                new
                {
                    login = newUser.Username,
                    role = newUser.Personality
                }));
        }
    }
}
