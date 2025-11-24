using System.Text.Json;
using EntitiesLibrary;

namespace Server.Handlers
{
    public class TermsPutHandler : CommandHandler
    {
        public override string Command => "PUT/api/terms";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                // Проверка авторизации
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);
                if (session == null)
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }

                var user = context.Db.FindUserByID(session.UserId);
                if (user == null || !user.Personality.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    await WriteError(http, "The user is not authorized as admin", 403);
                    return;
                }

                // Проверка обязательных полей
                if (!payload.TryGetProperty("id", out JsonElement idProp) ||
                    !payload.TryGetProperty("newDefinition", out JsonElement defProp) ||
                    !payload.TryGetProperty("changeNote", out JsonElement noteProp))
                {
                    await WriteError(http, "Required fields are missing: id, newDefinition or changeNote", 400);
                    return;
                }

                string id = idProp.GetString() ?? "";
                if (string.IsNullOrEmpty(id))
                {
                    await WriteError(http, "Invalid id", 400);
                    return;
                }

                string changeNote = noteProp.GetString() ?? "";
                if (string.IsNullOrEmpty(changeNote))
                {
                    await WriteError(http, "changeNote cannot be empty", 400);
                    return;
                }

                var existing = context.Db.GetTermByID(id);
                if (existing == null)
                {
                    await WriteError(http, "Term not found", 404);
                    return;
                }

                // Обновление переводов
                var defJson = defProp.GetRawText();
                var newDefs = JsonSerializer.Deserialize<Dictionary<string, string>>(defJson);
                if (newDefs != null)
                {
                    existing.translations = newDefs;
                }

                // Опциональные обновления
                if (payload.TryGetProperty("newCategory", out JsonElement catProp))
                {
                    string cat = catProp.GetString() ?? "";
                    if (!string.IsNullOrEmpty(cat))
                        existing.category = cat;
                }

                if (payload.TryGetProperty("newDifficulty", out JsonElement diffProp))
                {
                    string diff = diffProp.GetString() ?? "";
                    if (!string.IsNullOrEmpty(diff))
                        existing.difficultyLevel = diff;
                }

                if (payload.TryGetProperty("newSource", out JsonElement srcProp))
                {
                    string src = srcProp.GetString() ?? "";
                    existing.source = src;
                }

                if (payload.TryGetProperty("newRelated", out JsonElement relProp) && relProp.ValueKind == JsonValueKind.Array)
                {
                    var relatedList = relProp.EnumerateArray()
                        .Select(el => el.GetString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                    existing.relatedTerms = relatedList;
                }

                // Добавляем запись в history
                if (existing.history == null)
                    existing.history = new System.Collections.Generic.List<HistoryEntry>();

                existing.history.Add(new HistoryEntry
                {
                    date = DateTime.UtcNow,
                    author = user.Username,
                    change = changeNote
                });

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
