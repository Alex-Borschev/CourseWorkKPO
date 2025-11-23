using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLibrary;

namespace Server.Handlers
{
    public class TermsLikePostHandler : CommandHandler
    {
        public override string Command => "POST/api/terms/like";
        public override async Task Handle(JsonElement payload, HttpContext http, ServerContext context)
        {
            try
            {
                string token = http.Request.Headers["Authorization"].FirstOrDefault();
                var session = context.SessionService.ValidateToken(token);
                if (string.IsNullOrEmpty(session.UserId))
                {
                    await WriteError(http, "Пользователь не авторизован", 401);
                    return;
                }

                if (!payload.TryGetProperty("term", out var termProp) ||
                    !payload.TryGetProperty("isFavorite", out var isFavProp))
                {
                    await WriteError(http, "Некорректные данные", 400);
                    return;
                }

                string term = termProp.GetString();
                bool isFavorite = isFavProp.GetBoolean();

                var user = context.Db.FindUserByID(session.UserId);
                if (user == null)
                {
                    await WriteError(http, "Пользователь не найден", 404);
                    return;
                }

                user.Favorites ??= new System.Collections.Generic.List<string>();

                if (isFavorite)
                {
                    if (!user.Favorites.Contains(term)) user.Favorites.Add(term);
                }
                else
                {
                    user.Favorites.Remove(term);
                }

                context.Db.UpdateUser(user);
                await WriteOk(http, new { term, isFavorite });
            }
            catch (Exception ex)
            {
                await WriteError(http, $"Ошибка при обновлении избранного: {ex}", 500);
            }
        }
    }
}
