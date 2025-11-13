// Handlers/RateTermHandler.cs
// Команда: RATE_TERM
// Назначение: добавить или удалить оценку сложности термина.
//
// Изменения:
// 1. Вынесено в отдельный класс.
// 2. Код переиспользует JsonHelper.
// 3. Безопасно обновляет оба файла: пользователя и базу терминов.

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using SharedLibrary;

namespace Server.Handlers
{
    public class RateTermHandler : ICommandHandler
    {
        public string Command => "RATE_TERM";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            if (parts.Length < 4) return;

            string username = parts[1];
            string termName = parts[2];
            int rating = int.Parse(parts[3]);
            bool remove = parts.Length > 4 && parts[4] == "REMOVE";

            var userData = UserDataHelper.Load(username);
            if (userData == null) return;

            var termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath);
            if (termList == null) return;

            var term = termList.terms.FirstOrDefault(t => t.term == termName);
            if (term == null) return;

            if (term.difficultyRatings == null)
                term.difficultyRatings = new List<int>();

            if (remove)
            {
                var prev = userData.RatedTerms.FirstOrDefault(r => r.Term == termName);
                if (prev != null)
                {
                    term.difficultyRatings.Remove(prev.Rating);
                    userData.RatedTerms.Remove(prev);
                }
            }
            else
            {
                var existing = userData.RatedTerms.FirstOrDefault(r => r.Term == termName);
                if (existing != null)
                {
                    term.difficultyRatings.Remove(existing.Rating);
                    term.difficultyRatings.Add(rating);
                    existing.Rating = rating;
                }
                else
                {
                    term.difficultyRatings.Add(rating);
                    userData.RatedTerms.Add(new RatedTerm { Term = termName, Rating = rating });
                }
            }

            JsonHelper.WriteJsonFile(context.TermsFilePath, termList);
            UserDataHelper.Save(username, userData);

            TcpServer.SendMessage(stream, "OK");
            Console.WriteLine($"[RATE_TERM] {username} -> {termName} ({rating})");
        }
    }
}
