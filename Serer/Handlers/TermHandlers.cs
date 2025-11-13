using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using SharedLibrary;

namespace Server.Handlers
{
    /// <summary>
    /// Обработчик добавления нового термина в базу.
    /// Команда клиента: ADD_TERM;{jsonTerm}
    /// </summary>
    public class AddTermHandler : ICommandHandler
    {
        public string Command => "ADD_TERM";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                if (parts.Length < 2)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Некорректные данные для добавления термина"));
                    return;
                }

                // Объединяем всё после первой части команды в JSON-строку
                string json = string.Join(";", parts.Skip(1));

                // Десериализация нового термина
                Term newTerm = JsonSerializer.Deserialize<Term>(json);
                if (newTerm == null || string.IsNullOrWhiteSpace(newTerm.term))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка чтения данных термина"));
                    return;
                }

                // Загружаем существующую базу
                TermList termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath)
                    ?? new TermList { terms = new List<Term>() };

                // Проверяем наличие дубликатов
                if (termList.terms.Any(t => t.term.Equals(newTerm.term, StringComparison.OrdinalIgnoreCase)))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Термин уже существует"));
                    return;
                }

                // Добавляем новый термин
                newTerm.addedDate = DateTime.Now;
                newTerm.lastAccessed = DateTime.MinValue;
                termList.terms.Add(newTerm);

                // Сохраняем обновлённый список
                JsonHelper.WriteJsonFile(context.TermsFilePath, termList);

                // Отправляем подтверждение
                TcpServer.SendResponse(stream, ServerResponse.Ok("Термин добавлен успешно", new { term = newTerm.term }));

                Console.WriteLine($"[ADD_TERM] {newTerm.term}");
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при добавлении термина", new { error = ex.Message }));
            }
        }
    }
    public class DeleteTermHandler : ICommandHandler
    {
        public string Command => "DELETE_TERM";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string termName = parts[1];
                var termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath);

                var term = termList?.terms?.FirstOrDefault(t =>
                    t.term.Equals(termName, StringComparison.OrdinalIgnoreCase));

                if (term == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Термин не найден"));
                    return;
                }

                termList.terms.Remove(term);
                JsonHelper.WriteJsonFile(context.TermsFilePath, termList);
                TcpServer.SendResponse(stream, ServerResponse.Ok("Термин успешно удалён", new { term = termName }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при удалении термина", new { error = ex.Message }));
            }
        }
    }

    public class TermVisitedHandler : ICommandHandler
    {
        public string Command => "TERM_VISITED";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string termName = parts[1];
                var termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath);

                var term = termList?.terms?.FirstOrDefault(t =>
                    t.term.Equals(termName, StringComparison.OrdinalIgnoreCase));

                if (term == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Термин не найден"));
                    return;
                }

                term.lastAccessed = DateTime.Now; // ✅ исправлено (раньше присваивалась строка)
                JsonHelper.WriteJsonFile(context.TermsFilePath, termList);

                TcpServer.SendResponse(stream, ServerResponse.Ok("Обновлено время последнего доступа",
                    new { term = termName, term.lastAccessed }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при обновлении даты доступа", new { error = ex.Message }));
            }
        }
    }
}
