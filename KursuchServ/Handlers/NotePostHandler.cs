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
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);
                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }

                if (!payload.TryGetProperty("term", out var termProp) ||
                    !payload.TryGetProperty("note", out var noteProp))
                {
                    await WriteError(http, "Invalid format", 400);
                    return;
                }

                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "User not found", 404);
                    return;
                }

                string? term = termProp.GetString() ?? "";
                string? noteData = noteProp.GetString() ?? "";

                user.Notes ??= [];

                var existing = user.Notes.FirstOrDefault(n => n.NotedTerm == term);
                if (existing != null) user.Notes.Remove(existing);

                user.Notes.Add(new UserNotes
                {
                    Timestamp = DateTime.Now,
                    NotedTerm = term,
                    NotedData = noteData
                });

                context.Db.UpdateUser(user);
                await WriteOk(http, "The note has been updated.");
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error adding note: {ex}", 500);
            }
        }
    }
}
