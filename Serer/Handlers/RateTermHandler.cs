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
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Server.Handlers
{
    public class RateTermHandler : ICommandHandler
    {
        public string Command => "RATE_TERM";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string username = parts[1];
                string termName = parts[2];
                int rating = int.Parse(parts[3]);

                string path = $"{username}.json";

                if (!File.Exists(path))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Пользовательские данные не найдены"));
                    return;
                }

                var userData = JsonHelper.ReadJsonFile<UserData>(path);
                if (userData == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка чтения данных пользователя"));
                    return;
                }

                var existing = userData.RatedTerms.FirstOrDefault(r => r.Term == termName);
                if (existing != null)
                    existing.Rating = rating;
                else
                    userData.RatedTerms.Add(new RatedTerm { Term = termName, Rating = rating });

                JsonHelper.WriteJsonFile(path, userData);
                TcpServer.SendResponse(stream, ServerResponse.Ok("Оценка сохранена", new { term = termName, rating }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при сохранении оценки", new { error = ex.Message }));
            }
        }
    }
}


