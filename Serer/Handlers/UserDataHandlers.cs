using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using Server;
using SharedLibrary;

namespace Server.Handlers
{

    public class UpdateFavoriteHandler : ICommandHandler
    {
        public string Command => "UPDATE_FAVORITE";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!session.IsAuthenticated)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                JsonElement termProp;
                JsonElement isFavoriteProp;

                if (!payload.TryGetProperty("term", out termProp) ||
                    !payload.TryGetProperty("isFavorite", out isFavoriteProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Некорректные данные"));
                    return;
                }

                string term = termProp.GetString();
                bool isFavorite = isFavoriteProp.GetBoolean();

                var user = context.Db.FindUserByLogin(session.Username);
                if (user == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                if (user.Favorites == null)
                    user.Favorites = new System.Collections.Generic.List<string>();

                if (isFavorite)
                {
                    if (!user.Favorites.Contains(term))
                        user.Favorites.Add(term);
                }
                else
                {
                    user.Favorites.Remove(term);
                }

                context.Db.UpdateUser(user);

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Избранное обновлено", new { term = term, isFavorite = isFavorite }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при обновлении избранного", new { error = ex.Message }));
            }
        }
    }


    public class AddNoteHandler : ICommandHandler
    {
        public string Command => "ADD_NOTE";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!session.IsAuthenticated)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                if (!payload.TryGetProperty("term", out var termProp) ||
                    !payload.TryGetProperty("note", out var dataProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Неверный формат"));
                    return;
                }

                var user = context.Db.FindUserByLogin(session.Username);
                if (user == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                var term = termProp.GetString();
                var noteData = dataProp.GetString();

                if (user.Notes == null)
                    user.Notes = new List<UserNotes>();

                // ищем уже существующую запись
                var existing = user.Notes.FirstOrDefault(n => n.NotedTerm == term);
                if (existing != null)
                    user.Notes.Remove(existing);

                // добавляем новую
                user.Notes.Add(new UserNotes
                {
                    Timestamp = DateTime.Now,
                    NotedTerm = term,
                    NotedData = noteData
                });

                context.Db.UpdateUser(user);

                TcpServer.SendResponse(stream, ServerResponse.Ok("Заметка обновлена"));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при добавлении заметки", new { error = ex.Message }));
            }
        }
    }

    public class DeleteNoteHandler : ICommandHandler
    {
        public string Command => "DELETE_NOTE";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!session.IsAuthenticated)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                if (!payload.TryGetProperty("term", out var termProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Неверный формат"));
                    return;
                }

                var term = termProp.GetString();

                var user = context.Db.FindUserByLogin(session.Username);
                if (user == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                if (user.Notes == null || user.Notes.Count == 0)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Заметок нет"));
                    return;
                }

                var existing = user.Notes.FirstOrDefault(n => n.NotedTerm == term);
                if (existing == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Такой заметки нет"));
                    return;
                }

                user.Notes.Remove(existing);
                context.Db.UpdateUser(user);

                TcpServer.SendResponse(stream, ServerResponse.Ok("Заметка удалена"));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при удалении заметки", new { error = ex.Message }));
            }
        }
    }



    public class ClearMessageHandler : ICommandHandler
    {
        public string Command => "CLEAR_MESSAGE";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!session.IsAuthenticated)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                var user = context.Db.FindUserByLogin(session.Username);
                if (user == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                if (user.Messages != null)
                    user.Messages.Clear();

                context.Db.UpdateUser(user);

                TcpServer.SendResponse(stream, ServerResponse.Ok("Сообщения очищены"));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при очистке сообщений", new { error = ex.Message }));
            }
        }
    }

    public class SendMessageHandler : ICommandHandler
    {
        public string Command => "SEND_MESSAGE";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!session.IsAuthenticated)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                JsonElement toProp, themeProp, contentProp;

                if (!payload.TryGetProperty("to", out toProp) ||
                    !payload.TryGetProperty("theme", out themeProp) ||
                    !payload.TryGetProperty("content", out contentProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Некорректные данные"));
                    return;
                }

                string recipient = toProp.GetString();
                string theme = themeProp.GetString();
                string content = contentProp.GetString();

                var recipientData = context.Db.FindUserByID(recipient);
                if (recipientData == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                if (recipientData.Messages == null)
                    recipientData.Messages = new System.Collections.Generic.List<MessageEntry>();

                recipientData.Messages.Add(new MessageEntry
                {
                    Timestamp = DateTime.Now,
                    Author = session.Username,
                    Theme = theme,
                    Content = content
                });

                context.Db.UpdateUser(recipientData);

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Сообщение доставлено", new { to = recipient }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при отправке", new { error = ex.Message }));
            }
        }
    }

    public class GetUserDataHandler : ICommandHandler
    {
        public string Command => "GET_USER_DATA";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!session.IsAuthenticated)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                var user = context.Db.FindUserByLogin(session.Username);
                if (user == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Данные пользователя получены", user));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при получении данных", new { error = ex.Message }));
            }
        }
    }
}

