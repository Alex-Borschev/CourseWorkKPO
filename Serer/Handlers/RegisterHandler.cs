// Handlers/RegisterHandler.cs
// Обработчик команды REGISTER
// Назначение: регистрация нового пользователя.
//
// Изменения по сравнению с исходным кодом:
// 1. Вынесена из switch-case.
// 2. Логика создания JSON-файла пользователя вынесена в отдельный метод CreateUserFile.
// 3. Добавлены проверки и логгирование.
// 4. Код стал короче и полностью изолирован (SRP).

using SharedLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;

namespace Server.Handlers
{
    public class RegisterHandler : ICommandHandler
    {
        public string Command => "REGISTER";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            var data = ParseUserData(parts);
            if (data == null)
            {
                TcpServer.SendMessage(stream, "InvalidData");
                Console.WriteLine("[REGISTER] -> InvalidData");
                return;
            }

            var (role, login, password) = data.Value;

            if (context.Users.Any(u => u.login == login))
            {
                TcpServer.SendMessage(stream, "UserExists");
                Console.WriteLine($"[REGISTER] User '{login}' already exists.");
                return;
            }

            Users newUser = new Users
            {
                personality = role,
                login = login,
                password = password
            };

            context.Users.Add(newUser);
            Users.AppendUserToFile("users.txt", newUser);
            CreateUserFile(login, role);
            TcpServer.SendMessage(stream, "RegisterSuccess");
            Console.WriteLine($"[REGISTER] User '{login}' registered successfully.");
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

        /// <summary>
        /// Создание JSON-файла с данными нового пользователя.
        /// </summary>
        private void CreateUserFile(string username, string role)
        {
            string filePath = $"{username}.json";

            var newUserData = new UserData
            {
                Username = username,
                Personality = role,
                RegistrationDate = DateTime.Now,
                Favorites = new List<string>(),
                RatedTerms = new List<RatedTerm>(),
                Notes = new List<UserNotes>(),
                Messages = new List<MessageEntry>()
            };

            JsonHelper.WriteJsonFile(filePath, newUserData);
        }
    }
}
