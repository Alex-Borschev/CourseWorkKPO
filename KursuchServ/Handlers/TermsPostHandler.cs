using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

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
                    await WriteError(http, "Отсутствует поле termData");
                    return;
                }

                var newTerm = JsonSerializer.Deserialize<Term>(termJson.GetRawText());
                if (newTerm == null)
                {
                    await WriteError(http, "Ошибка десериализации термина");
                    return;
                }

                if (context.Db.GetTermByName(newTerm.term) != null)
                {
                    await WriteError(http, "Такой термин уже существует");
                    return;
                }

                newTerm.addedDate = DateTime.Now;
                newTerm.lastAccessed = DateTime.MinValue;

                context.Db.AddTerm(newTerm);

                await WriteOk(http, new { term = newTerm.term });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при добавлении термина: {ex}");
            }
        }
    }
}
