using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using SharedLibrary;

namespace Server.Handlers
{
    public class UpdateFavoriteHandler : ICommandHandler
    {
        public string Command => "UPDATE_FAVORITE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string username = parts[1];
                string term = parts[2];
                bool isFavorite = bool.Parse(parts[3]);

                string path = $"{username}.json";
                var data = JsonHelper.ReadJsonFile<UserData>(path) ?? new UserData
                {
                    Username = username,
                    Favorites = new List<string>(),
                    RatedTerms = new List<RatedTerm>(),
                    Notes = new List<UserNotes>(),
                    Messages = new List<MessageEntry>()
                };

                if (isFavorite)
                {
                    if (!data.Favorites.Contains(term))
                        data.Favorites.Add(term);
                }
                else
                {
                    data.Favorites.Remove(term);
                }

                JsonHelper.WriteJsonFile(path, data);
                TcpServer.SendResponse(stream, ServerResponse.Ok("Избранное обновлено", new { term, isFavorite }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при обновлении избранного", new { error = ex.Message }));
            }
        }
    }

    public class AddNoteHandler : ICommandHandler
    {
        public string Command => "ADD_NOTE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string username = parts[1];
                string notedTerm = parts[2];
                string notedData = parts[3];

                string path = $"{username}.json";
                var data = JsonHelper.ReadJsonFile<UserData>(path) ?? new UserData
                {
                    Username = username,
                    Notes = new List<UserNotes>()
                };

                data.Notes.Add(new UserNotes
                {
                    Timestamp = DateTime.Now,
                    NotedTerm = notedTerm,
                    NotedData = notedData
                });

                JsonHelper.WriteJsonFile(path, data);
                TcpServer.SendResponse(stream, ServerResponse.Ok("Заметка добавлена", new { term = notedTerm, note = notedData }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при добавлении заметки", new { error = ex.Message }));
            }
        }
    }

    public class ClearMessageHandler : ICommandHandler
    {
        public string Command => "CLEAR_MESSAGE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string username = parts[1];
                string path = $"{username}.json";

                var data = JsonHelper.ReadJsonFile<UserData>(path);
                if (data == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Файл пользователя не найден"));
                    return;
                }

                data.Messages.Clear();
                JsonHelper.WriteJsonFile(path, data);
                TcpServer.SendResponse(stream, ServerResponse.Ok("Сообщения очищены"));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при очистке сообщений", new { error = ex.Message }));
            }
        }
    }

    public class SendMessageHandler : ICommandHandler
    {
        public string Command => "SEND_MESSAGE";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string author = parts[1];
                string recipient = parts[2];
                string theme = parts[3];
                string content = parts[4];

                string recipientPath = $"{recipient}.json";
                var data = JsonHelper.ReadJsonFile<UserData>(recipientPath) ?? new UserData
                {
                    Username = recipient,
                    Messages = new List<MessageEntry>()
                };

                data.Messages.Add(new MessageEntry
                {
                    Author = author,
                    Theme = theme,
                    Content = content,
                    Timestamp = DateTime.Now
                });

                JsonHelper.WriteJsonFile(recipientPath, data);

                TcpServer.SendResponse(stream, ServerResponse.Ok("Сообщение отправлено",
                    new { to = recipient, theme, content }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при отправке сообщения", new { error = ex.Message }));
            }
        }
    }

    public class GetUserDataHandler : ICommandHandler
    {
        public string Command => "GET_USER_DATA";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string username = parts[1];
                string path = $"{username}.json";

                if (!File.Exists(path))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Файл пользователя не найден"));
                    return;
                }

                var data = JsonHelper.ReadJsonFile<UserData>(path);
                TcpServer.SendResponse(stream, ServerResponse.Ok("Данные пользователя получены", data));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при получении данных пользователя", new { error = ex.Message }));
            }
        }
    }
}
