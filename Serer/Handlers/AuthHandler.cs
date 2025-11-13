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

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            var data = ParseUserData(parts);
            if (data == null)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Некорректные данные авторизации"));
                return;
            }

            var (role, login, password) = data.Value;
            var user = context.Users.FirstOrDefault(u =>
                u.personality == role && u.login == login && u.password == password);

            if (user != null)
                TcpServer.SendResponse(stream, ServerResponse.Ok("Авторизация успешна", new { login, role }));
            else
                TcpServer.SendResponse(stream, ServerResponse.Error("Неверные учетные данные"));
        }

        private (string Role, string Login, string Password)? ParseUserData(string[] parts)
        {
            try
            {
                string role = parts[1].Split('=')[1];
                string login = parts[2].Split('=')[1];
                string password = parts[3].Split('=')[1];
                return (role, login, password);
            }
            catch
            {
                return null;
            }
        }
    }
}
