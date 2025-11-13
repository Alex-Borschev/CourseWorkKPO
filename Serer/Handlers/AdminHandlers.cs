// Handlers/AdminHandlers.cs
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using SharedLibrary;

namespace Server.Handlers
{
    // Предложение правки: доставляется всем администраторам как сообщение
    public class SuggestEditHandler : ICommandHandler
    {
        public string Command => "SUGG_EDIT"; // используем единообразно с клиентом

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!payload.TryGetProperty("termName", out var termNameElement) ||
                    !payload.TryGetProperty("suggestion", out var suggestionElement))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Недостаточно данных для предложения правки"));
                    return;
                }

                string termName = termNameElement.GetString();
                string suggestion = suggestionElement.GetString();

                string theme = "Предложение правки: " + termName;
                string content = suggestion;
                string author = session.IsAuthenticated ? session.Username : "anonymous";

                var admins = context.Db.GetAllUsers().Where(u => u.Personality == "Администратор").ToList();
                foreach (var admin in admins)
                {
                    if (admin.Messages == null)
                        admin.Messages = new System.Collections.Generic.List<MessageEntry>();

                    admin.Messages.Add(new MessageEntry
                    {
                        Timestamp = DateTime.Now,
                        Theme = theme,
                        Content = content,
                        Author = author
                    });

                    context.Db.UpdateUser(admin);
                }

                TcpServer.SendResponse(stream, ServerResponse.Ok("Предложение на редактирование отправлено", new { term = termName }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при отправке предложения", new { error = ex.Message }));
            }
        }
    }

    // Выдать список пользователей из userdata (Username + Personality)
    public class GetUsersHandler : ICommandHandler
    {
        public string Command => "GET_USERS";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                var users = context.Db.GetAllUsers()
                    .Select(u => new { login = u.Username, role = u.Personality })
                    .ToList();

                TcpServer.SendResponse(stream, ServerResponse.Ok("Список пользователей получен", users));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при получении списка пользователей", new { error = ex.Message }));
            }
        }
    }
}
