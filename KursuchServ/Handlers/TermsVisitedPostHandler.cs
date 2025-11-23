using System;
using System.Text.Json;
using System.Threading.Tasks;
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
                    await WriteError(http, "Отсутствует поле term");
                    return;
                }

                string termID = termProp.GetString();
                var term = context.Db.GetTermByID(termID);
                if (term == null)
                {
                    await WriteError(http, "Термин не найден");
                    return;
                }

                term.lastAccessed = DateTime.Now;
                context.Db.UpdateTerm(term);

                await WriteOk(http, new { term = termID, lastAccessed = term.lastAccessed });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при обновлении доступа: {ex}");
            }
        }
    }
}
