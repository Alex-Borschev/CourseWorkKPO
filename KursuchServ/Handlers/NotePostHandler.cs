using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class NotePostHandler : CommandHandler
    {
        public override string Command => "POST/api/note";

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

                if (!payload.TryGetProperty("term", out var termProp) ||
                    !payload.TryGetProperty("note", out var noteProp))
                {
                    await WriteError(http, "Неверный формат", 400);
                    return;
                }

                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "Пользователь не найден", 404);
                    return;
                }

                string term = termProp.GetString();
                string noteData = noteProp.GetString();

                user.Notes ??= new System.Collections.Generic.List<UserNotes>();

                var existing = user.Notes.FirstOrDefault(n => n.NotedTerm == term);
                if (existing != null) user.Notes.Remove(existing);

                user.Notes.Add(new UserNotes
                {
                    Timestamp = DateTime.Now,
                    NotedTerm = term,
                    NotedData = noteData
                });

                context.Db.UpdateUser(user);
                await WriteOk(http, "Заметка обновлена");
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при добавлении заметки: {ex}", 500);
            }
        }
    }
}
