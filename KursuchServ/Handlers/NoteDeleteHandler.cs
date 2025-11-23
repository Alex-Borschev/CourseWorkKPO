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
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);
                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }

                if (!payload.TryGetProperty("term", out var termProp))
                {
                    await WriteError(http, "Invalid format", 400);
                    return;
                }

                string? term = termProp.GetString() ?? "";
                
                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "User not found", 404);
                    return;
                }

                var existing = user.Notes.FirstOrDefault(n => n.NotedTerm == term);
                if (existing == null)
                {
                    await WriteError(http, "There is no such note", 404);
                    return;
                }

                user.Notes.Remove(existing);
                context.Db.UpdateUser(user);
                await WriteOk(http, new { message = "The note has been deleted" });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error deleting note: {ex}", 500);
            }
        }
    }
}
