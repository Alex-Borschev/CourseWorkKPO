using System;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class TermsVisitedPostHandler : CommandHandler
    {
        public override string Command => "POST/api/terms/visited";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                if (!payload.TryGetProperty("term", out var termProp))
                {
                    await WriteError(http, "The term field is missing", 400);
                    return;
                }

                string termID = termProp.GetString() ?? "";
                var term = context.Db.GetTermByID(termID);
                if (term == null)
                {
                    await WriteError(http, "Term not found");
                    return;
                }
                string host = $"http://{Env.GetString("HOST")}:{Env.GetString("PORT")}";
                term.lastAccessed = DateTime.Now;
                term.popularity = term.popularity + 1;
                context.Db.UpdateTerm(term);

                if (term.media != null && term.media.Count > 0 && !string.IsNullOrEmpty(term.media[0]?.url))
                {
                    term.media[0].url = $"{host}{term.media[0].url}";
                }

                await WriteOk(http, term);
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error updating access: {ex}");
            }
        }
    }
}
