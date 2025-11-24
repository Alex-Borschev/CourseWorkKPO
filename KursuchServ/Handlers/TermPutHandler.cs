using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class TermUpdateHandler : CommandHandler
    {
        public override string Command => "PUT/api/terms";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                // Проверяем необходимые поля
                if (!payload.TryGetProperty("id", out JsonElement idProp) ||
                    !payload.TryGetProperty("newDefinition", out JsonElement defProp))
                {
                    await WriteError(http, "Required fields are missing: id or newDefinition", 400);
                    return;
                }

                string id = idProp.GetString();
                if (string.IsNullOrEmpty(id))
                {
                    await WriteError(http, "Invalid id", 400);
                    return;
                }

                var existing = context.Db.GetTermByID(id);
                if (existing == null)
                {
                    await WriteError(http, "Term not found", 404);
                    return;
                }

                // Обновляем определения (предполагаем, что это JSON массива переводов)
                // newDefinition может быть объектом с { en, ru, de ... }
                var defJson = defProp.GetRawText();
                var newDefs = JsonSerializer.Deserialize<Dictionary<string, string>>(defJson);
                if (newDefs != null)
                {
                    existing.translations = newDefs;
                }

                // Опционально обновляем категорию
                if (payload.TryGetProperty("newCategory", out JsonElement catProp))
                {
                    string cat = catProp.GetString();
                    if (!string.IsNullOrEmpty(cat))
                        existing.category = cat;
                }

                // Опционально обновляем сложность
                if (payload.TryGetProperty("newDifficulty", out JsonElement diffProp))
                {
                    string diff = diffProp.GetString();
                    if (!string.IsNullOrEmpty(diff))
                        existing.difficultyLevel = diff;
                }

                // Опционально обновляем источник
                if (payload.TryGetProperty("newSource", out JsonElement srcProp))
                {
                    string src = srcProp.GetString();
                    existing.source = src; // можно и проверить null
                }

                // Опционально обновляем связанные термины
                if (payload.TryGetProperty("newRelated", out JsonElement relProp) && relProp.ValueKind == JsonValueKind.Array)
                {
                    var relatedList = relProp.EnumerateArray()
                        .Select(el => el.GetString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                    // Допустим, в Term есть List<string> relatedTermIds
                    existing.relatedTerms = relatedList;
                }

                // Обновляем в базе
                context.Db.UpdateTerm(existing);

                await WriteOk(http, new { message = "Term updated", term = existing });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error updating term: {ex}", 500);
            }
        }
    }
}
