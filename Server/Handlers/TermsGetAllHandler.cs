using System.Text.Json;
using DotNetEnv;

namespace Server.Handlers
{
    public class TermsGetAllHandler : CommandHandler
    {
        public override string Command => "GET/api/terms";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                var terms = context.Db.GetAllTerms();
                string host = $"http://{Env.GetString("HOST")}:{Env.GetString("PORT")}";

                var result = terms.Select(t => new
                {
                    id = t.Id,
                    term = t.term,
                    category = t.category,
                    popularity = t.popularity,
                    difficultyLevel = t.difficultyLevel,
                    addedDate = t.addedDate,
                    difficultyRatings = t.difficultyRatings,

                    mediaUrl = t.media != null && t.media.Count > 0
                        ? host + t.media[0]?.url
                        : null
                }).ToList();

                await WriteOk(http, result);
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error getting terms: {ex}");
            }
        }
    }
}
