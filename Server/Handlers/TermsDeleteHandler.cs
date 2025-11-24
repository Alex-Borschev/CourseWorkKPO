using System.Text.Json;

namespace Server.Handlers
{

    public class TermsDeleteHandler : CommandHandler
    {
        public override string Command => "DELETE/api/terms";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                if (!payload.TryGetProperty("term", out var termProp))
                {
                    await WriteError(http, "The term field is missing", 400);
                    return;
                }

                string? termID = termProp.GetString() ?? "";

                var term = context.Db.GetTermByID(termID);
                if (term == null)
                {
                    await WriteError(http, "Term not found", 500);
                    return;
                }

                context.Db.DeleteTermByID(termID);
                await WriteOk(http, new { term = termID });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error deleting term: {ex}");
            }
        }
    }
}
