using System.Net.Sockets;
using SharedLibrary;
using System.Text.Json;
using System;

namespace Server.Handlers
{
    public class AuthHandler : ICommandHandler
    {
        public string Command => "AUTH";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            if (!payload.TryGetProperty("login", out var loginProp) ||
                !payload.TryGetProperty("password", out var passwordProp))
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Отсутствуют обязательные поля"));
                Console.WriteLine("Отсутствуют обязательные поля");
                return;
            }

            string login = loginProp.GetString();
            string password = passwordProp.GetString();

            var user = context.Db.ValidateUser(login, password);
            if (user == null)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Неверные данные"));
                return;
            }

            session.Username = user.Username;
            Console.WriteLine(user);
            TcpServer.SendResponse(
                stream,
                ServerResponse.Ok("Авторизация успешна", new
                {
                    id = user.Id,
                    login = user.Username,
                    role = user.Personality,
                    a = user.Messages
                })

            );
            Console.WriteLine("Авторизация успешна");
        }
    }
}



