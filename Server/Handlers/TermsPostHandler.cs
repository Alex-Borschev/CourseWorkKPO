using System.Text.Json;
using EntitiesLibrary;

namespace Server.Handlers
{
    public class TermsPostHandler : CommandHandler
    {
        public override string Command => "POST/api/terms";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);
                if (session == null)
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }

                var user = context.Db.FindUserByID(session.UserId);
                if (user.Personality == "User")
                {
                    await WriteError(http, "The user can not add terms", 403);
                }

                if (!payload.TryGetProperty("termData", out var termJson))
                {
                    await WriteError(http, "The termData field is missing", 400);
                    return;
                }

                var newTerm = JsonSerializer.Deserialize<Term>(termJson.GetRawText());
                if (newTerm == null)
                {
                    await WriteError(http, "Invalid data", 400);
                    return;
                }

                if (context.Db.GetTermByName(newTerm.term) != null)
                {
                    await WriteError(http, "This term already exists", 401);
                    return;
                }

                newTerm.addedDate = DateTime.Now;
                newTerm.lastAccessed = DateTime.MinValue;
                newTerm.author = user.Username;

                context.Db.AddTerm(newTerm);
                await WriteOk(http, new { term = newTerm.term });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error adding term: {ex}");
            }
        }
    }
}
