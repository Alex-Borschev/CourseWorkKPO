using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Handlers
{
    public class RatePostHandler : CommandHandler
    {
        public override string Command => "POST/api/rate";

        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            http.Response.ContentType = "application/json";

            try
            {
                if (!payload.TryGetProperty("term", out var termProp) ||
                    !payload.TryGetProperty("rating", out var ratingProp))
                {
                    await WriteError(http, "Invalid data", 500);
                    return;
                }
                string token = http.Request.Headers["Authorization"].ToString();
                var session = context.SessionService.ValidateToken(token);

                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "The user is not authorized", 401);
                    return;
                }

                string? termId = termProp.GetString() ?? "";
                int rating = ratingProp.GetInt32();
                
                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "User not found", 500);
                    return;
                }

                var term = context.Db.GetTermByID(termId);
                if (term == null)
                {
                    await WriteError(http, "Term not found", 500);
                    return;
                }

                if (user.RatedTerms == null)
                    user.RatedTerms = new List<RatedTerm>();

                if (term.difficultyRatings == null)
                    term.difficultyRatings = new List<int>();

                var existing = user.RatedTerms.FirstOrDefault(r => r.Term == termId);

                if (rating == 0)
                {
                    if (existing != null)
                    {
                        int oldRating = existing.Rating;

                        int idx = term.difficultyRatings.IndexOf(oldRating);
                        if (idx >= 0)
                            term.difficultyRatings.RemoveAt(idx);

                        user.RatedTerms.Remove(existing);
                    }

                    context.Db.UpdateTerm(term);
                    context.Db.UpdateUser(user);
                    
                    await WriteOk(http, new { term = termId });

                    return;
                }

                if (existing != null)
                {
                    int old = existing.Rating;

                    int idx = term.difficultyRatings.IndexOf(old);
                    if (idx >= 0)
                        term.difficultyRatings.RemoveAt(idx);

                    existing.Rating = rating;
                }
                else
                {
                    user.RatedTerms.Add(new RatedTerm
                    {
                        Term = termId,
                        Rating = rating
                    });
                }

                term.difficultyRatings.Add(rating);

                context.Db.UpdateTerm(term);
                context.Db.UpdateUser(user);

                await WriteOk(http, new { term = termId, rating });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Error saving rating: {ex}", 500);
            }
        }
    }
}
