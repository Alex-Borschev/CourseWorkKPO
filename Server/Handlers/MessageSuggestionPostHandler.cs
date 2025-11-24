using System.Text.Json;
using EntitiesLibrary;

namespace Server.Handlers
{
    public class MessageSuggestionPostHandler : CommandHandler
    {
        public override string Command => "POST/api/message/suggestion";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            http.Response.ContentType = "application/json";

            try
            {
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);

                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }
                var user = context.Db.FindUserByID(session.UserId);

                if (!payload.TryGetProperty("termName", out var termNameElement) ||
                    !payload.TryGetProperty("suggestion", out var suggestionElement))
                {
                    await WriteError(http, "Invalid data", 400);
                    return;
                }

                string? termName = termNameElement.GetString() ?? "";
                string? suggestion = suggestionElement.GetString() ?? "";

                string author = user.Username;

                var admins = context.Db.GetAllUsers()
                    .Where(u => u.Personality == "Admin")
                    .ToList();
                
                foreach (var admin in admins)
                {
                    if (admin.Messages == null)
                        admin.Messages = new System.Collections.Generic.List<MessageEntry>();

                    admin.Messages.Add(new MessageEntry
                    {
                        Timestamp = DateTime.Now,
                        Theme = termName,
                        Content = suggestion,
                        Author = author
                    });

                    context.Db.UpdateUser(admin);
                }

                await WriteOk(http, new { message = "Suggestion has been sent" });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при отправке предложения: {ex}", 500);
            }
        }
    }
}
