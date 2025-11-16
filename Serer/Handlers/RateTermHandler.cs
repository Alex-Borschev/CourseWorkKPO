// Handlers/RateTermHandler.cs
// Команда: RATE_TERM
// Назначение: добавить или удалить оценку сложности термина.
//
// Изменения:
// 1. Вынесено в отдельный класс.
// 2. Код переиспользует JsonHelper.
// 3. Безопасно обновляет оба файла: пользователя и базу терминов.

using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;

namespace Server.Handlers
{
    public class RateTermHandler : ICommandHandler
    {
        public string Command => "RATE_TERM";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                JsonElement termProp;
                JsonElement ratingProp;

                if (!payload.TryGetProperty("term", out termProp) ||
                    !payload.TryGetProperty("rating", out ratingProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Некорректные данные"));
                    return;
                }

                string username = session.Username;
                if (username == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не авторизован"));
                    return;
                }

                string termId = termProp.GetString();
                int rating = ratingProp.GetInt32();

                var user = context.Db.FindUserByLogin(username);
                if (user == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользователь не найден"));
                    return;
                }

                var term = context.Db.GetTermByID(termId);
                if (term == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Термин не найден"));
                    return;
                }

                if (user.RatedTerms == null)
                    user.RatedTerms = new List<RatedTerm>();

                if (term.difficultyRatings == null)
                    term.difficultyRatings = new List<int>();

                // ищем существующую оценку
                var existing = user.RatedTerms.FirstOrDefault(r => r.Term == termId);

                if (rating == 0)
                {
                    // ⛔ УДАЛЕНИЕ ОЦЕНКИ

                    if (existing != null)
                    {
                        // удалить старый рейтинг пользователя
                        int oldRating = existing.Rating;

                        int idx = term.difficultyRatings.IndexOf(oldRating);
                        if (idx >= 0)
                            term.difficultyRatings.RemoveAt(idx);

                        // убрать запись из RatedTerms
                        user.RatedTerms.Remove(existing);
                    }

                    context.Db.UpdateTerm(term);
                    context.Db.UpdateUser(user);

                    TcpServer.SendResponse(stream,
                        ServerResponse.Ok("Оценка удалена", new { term = termId }));

                    return;
                }

                // 🔄 ИЗМЕНЕНИЕ / ДОБАВЛЕНИЕ ОЦЕНКИ
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

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Оценка сохранена", new { term = termId, rating = rating }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при сохранении оценки", new { error = ex.Message }));
            }
        }
    }
}
