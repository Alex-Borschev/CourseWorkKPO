using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class NoteDeleteHandler : CommandHandler
    {
        public override string Command => "DELETE/api/note";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                string token = http.Request.Headers["Authorization"];
                var session = context.SessionService.ValidateToken(token);
                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "Пользователь не авторизован", 401);
                    return;
                }

                if (!payload.TryGetProperty("term", out var termProp))
                {
                    await WriteError(http, "Неверный формат", 400);
                    return;
                }

                string term = termProp.GetString();
                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "Пользователь не найден", 404);
                    return;
                }

                var existing = user.Notes?.FirstOrDefault(n => n.NotedTerm == term);
                if (existing == null)
                {
                    await WriteError(http, "Такой заметки нет", 404);
                    return;
                }

                user.Notes.Remove(existing);
                context.Db.UpdateUser(user);
                await WriteOk(http, "Заметка удалена");
            }
            catch (Exception ex)
            {
                await WriteError(http, "Ошибка при удалении заметки", 500);
            }
        }
    }
}
