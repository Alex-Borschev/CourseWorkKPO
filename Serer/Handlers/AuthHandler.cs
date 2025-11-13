// Handlers/AuthHandler.cs
// Обработчик команды AUTH
// Назначение: проверка логина и пароля пользователя.
//
// Изменения по сравнению с исходным кодом:
// 1. Вынесена из switch-case в отдельный класс (паттерн Command).
// 2. Логика проверки вынесена в отдельные приватные методы для читаемости.
// 3. Используется JsonHelper и TcpServer.SendMessage — без дублирования.

using SharedLibrary;
using System;
using System.Linq;
using System.Net.Sockets;

namespace Server.Handlers
{
    public class AuthHandler : ICommandHandler
    {
        public string Command => "AUTH";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context, ClientSession session)
        {
            var data = ParseUserData(parts);
            if (data == null)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Некорректные данные авторизации"));
                return;
            }

            var login = data.Value.Login;
            var password = data.Value.Password;

            var user = context.Db.FindUser(login, password);

            if (user != null)
                TcpServer.SendResponse(stream, ServerResponse.Ok("Авторизация успешна", new { login, role = user.personality }));
            else
                TcpServer.SendResponse(stream, ServerResponse.Error("Неверные учетные данные"));
        }



        private (string Login, string Password)? ParseUserData(string[] parts)
        {
            try
            {
                string login = parts[1].Split('=')[1];
                string password = parts[2].Split('=')[1];
                return (login, password);
            }
            catch
            {
                return null;
            }
        }
    }
}
