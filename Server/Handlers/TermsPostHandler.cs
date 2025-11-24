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
