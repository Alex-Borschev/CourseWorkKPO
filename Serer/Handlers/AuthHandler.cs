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
using System.Collections.Generic;
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
                TcpServer.SendMessage(stream, "InvalidData");
                return;
            }

            var (role, login, password) = data.Value;
            string result = CheckCredentials(context.Users, role, login, password);
            TcpServer.SendMessage(stream, result);
            Console.WriteLine($"[AUTH] {login} -> {result}");
        }

        /// <summary>
        /// Парсинг данных пользователя из формата: AUTH;Role=...;Login=...;Password=...
        /// </summary>
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

        /// <summary>
        /// Проверка логина и пароля.
        /// </summary>
        private string CheckCredentials(List<Users> users, string role, string login, string password)
        {
            bool userExists = users.Any(u =>
                u.personality == role &&
                u.login == login &&
                u.password == password);

            if (role == "Администратор" && userExists)
                return "Admin";
            else if (role == "Пользователь" && userExists)
                return "User";
            else
                return "Invalid";
        }
    }
}
