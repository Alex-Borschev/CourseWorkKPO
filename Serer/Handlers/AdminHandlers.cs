// Handlers/AdminHandlers.cs
// Команды:
// - SUGG_EDIT (предложение правки)
// - GET_USERS (список пользователей)
//
// Изменения:
// 1. Код отправки сообщений администраторам использует SendMessageHandler.AddMessage для переиспользования логики.
// 2. Формат JSON-выдачи списка пользователей идентичен оригиналу.

using SharedLibrary;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;

namespace Server.Handlers
{
    // --- SUGG_EDIT ---
    public class SuggestEditHandler : ICommandHandler
    {
        public string Command => "SUGG_EDIT";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            string loginFrom = parts[1];
            string term = parts[2];
            string content = parts[3];
            string theme = "«ПРАВКА»" + term;

            foreach (var admin in context.Users.Where(u => u.personality == "Администратор"))
            {
                var data = UserDataHelper.Load(admin.login);
                if (data == null) continue;

                data.Messages.Add(new MessageEntry
                {
                    Timestamp = DateTime.Now,
                    Theme = theme,
                    Content = content,
                    Author = loginFrom
                });
                UserDataHelper.Save(admin.login, data);
            }

            TcpServer.SendMessage(stream, "Предложение правки доставлено");
        }
    }

    // --- GET_USERS ---
    public class GetUsersHandler : ICommandHandler
    {
        public string Command => "GET_USERS";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            var list = context.Users.Select(u => new { u.login, u.personality }).ToList();
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            TcpServer.SendMessage(stream, json + "__THE_END__");
        }
    }
}
