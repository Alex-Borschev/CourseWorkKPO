// Handlers/UserDataHandlers.cs
// Группа обработчиков, работающих с файлами пользователей:
// - UPDATE_FAVORITE
// - ADD_NOTE
// - CLEAR_MESSAGE
// - SEND_MESSAGE
// - GET_USER_DATA
//
// Изменения:
// 1. Код разделён на отдельные классы внутри файла для логической близости.
// 2. Все операции с JSON теперь безопасно выполняются через JsonHelper.
// 3. Повторяющийся код чтения UserData объединён.

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.Json;
using SharedLibrary;

namespace Server.Handlers
{
    // Базовый вспомогательный класс для доступа к данным пользователя
    internal static class UserDataHelper
    {
        public static UserData Load(string username)
        {
            string path = $"{username}.json";
            return JsonHelper.ReadJsonFile<UserData>(path);
        }

        public static void Save(string username, UserData data)
        {
            string path = $"{username}.json";
            JsonHelper.WriteJsonFile(path, data);
        }
    }

    // --- UPDATE_FAVORITE ---
    public class UpdateFavoriteHandler : ICommandHandler
    {
        public string Command => "UPDATE_FAVORITE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            if (parts.Length < 4) return;
            string username = parts[1];
            string term = parts[2];
            bool isAdding = Convert.ToBoolean(parts[3]);

            var data = UserDataHelper.Load(username);
            if (data == null) return;

            if (isAdding)
            {
                if (!data.Favorites.Contains(term))
                    data.Favorites.Add(term);
            }
            else data.Favorites.Remove(term);

            UserDataHelper.Save(username, data);
            Console.WriteLine($"[UPDATE_FAVORITE] {username} -> {term}");
        }
    }

    // --- ADD_NOTE ---
    public class AddNoteHandler : ICommandHandler
    {
        public string Command => "ADD_NOTE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            if (parts.Length < 4) return;

            string username = parts[1];
            string term = parts[2];
            string content = parts[3];
            bool isRemoving = parts.Length > 4 && parts[4] == "REMOVE";
            bool isRemovingAll = parts.Length > 4 && parts[4] == "REMOVE_ALL";

            var data = UserDataHelper.Load(username);
            if (data == null) return;

            if (data.Notes == null)
                data.Notes = new List<UserNotes>();

            if (isRemovingAll)
                data.Notes.Clear();
            else if (isRemoving)
                data.Notes.RemoveAll(n => n.NotedTerm == term && n.NotedData == content);
            else
                data.Notes.Add(new UserNotes { Timestamp = DateTime.Now, NotedTerm = term, NotedData = content });

            UserDataHelper.Save(username, data);
            TcpServer.SendMessage(stream, "OK");
            Console.WriteLine($"[ADD_NOTE] {username}");
        }
    }

    // --- CLEAR_MESSAGE ---
    public class ClearMessageHandler : ICommandHandler
    {
        public string Command => "CLEAR_MESSAGE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            string username = parts[1];
            string theme = parts[2];
            string content = parts[3];
            bool removeAll = parts.Length > 4 && parts[4] == "REMOVE_ALL";

            var data = UserDataHelper.Load(username);
            if (data == null) return;

            if (removeAll) data.Messages.Clear();
            else data.Messages.RemoveAll(m => m.Theme == theme && m.Content == content);

            UserDataHelper.Save(username, data);
            TcpServer.SendMessage(stream, "OK");
            Console.WriteLine($"[CLEAR_MESSAGE] {username}");
        }
    }

    // --- SEND_MESSAGE ---
    public class SendMessageHandler : ICommandHandler
    {
        public string Command => "SEND_MESSAGE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            string loginFrom = parts[1];
            string loginTo = parts[2];
            string theme = parts[3];
            string content = parts[4];

            if (loginTo == "ALL_ADMINS")
            {
                foreach (var admin in context.Users.Where(u => u.personality == "Администратор"))
                    AddMessage(admin.login, loginFrom, theme, content);
            }
            else AddMessage(loginTo, loginFrom, theme, content);

            TcpServer.SendMessage(stream, "Доставлено");
            Console.WriteLine($"[SEND_MESSAGE] {loginFrom} -> {loginTo}");
        }

        private void AddMessage(string toUser, string fromUser, string theme, string content)
        {
            var data = UserDataHelper.Load(toUser);
            if (data == null) return;

            data.Messages.Add(new MessageEntry
            {
                Timestamp = DateTime.Now,
                Theme = theme,
                Content = content,
                Author = fromUser
            });

            UserDataHelper.Save(toUser, data);
        }
    }

    // --- GET_USER_DATA ---
    public class GetUserDataHandler : ICommandHandler
    {
        public string Command => "GET_USER_DATA";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            string username = parts[1];
            var data = UserDataHelper.Load(username);
            if (data == null) return;

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            TcpServer.SendMessage(stream, json + "__THE_END__");
        }
    }
}
