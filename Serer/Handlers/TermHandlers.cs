// Handlers/TermHandlers.cs
// Обработчики терминов: добавление, удаление, обновление посещения.
//
// Изменения:
// 1. Объединены в один файл для логической группировки.
// 2. Все операции с JSON используют JsonHelper и TermList.
// 3. Добавлены проверки существования и понятные сообщения.

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Collections.Generic;
using SharedLibrary;

namespace Server.Handlers
{
    // --- ADD_TERM ---
    public class AddTermHandler : ICommandHandler
    {
        public string Command => "ADD_TERM";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            try
            {
                string json = string.Join(";", parts.Skip(1));
                Term newTerm = JsonSerializer.Deserialize<Term>(json);

                TermList termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath) ?? new TermList { terms = new List<Term>() };

                if (termList.terms.Any(t => t.term.Equals(newTerm.term, StringComparison.OrdinalIgnoreCase)))
                {
                    TcpServer.SendMessage(stream, "Термин уже существует");
                    return;
                }

                termList.terms.Add(newTerm);
                JsonHelper.WriteJsonFile(context.TermsFilePath, termList);

                TcpServer.SendMessage(stream, "Термин добавлен успешно");
                Console.WriteLine($"[ADD_TERM] {newTerm.term}");
            }
            catch (Exception ex)
            {
                TcpServer.SendMessage(stream, "Error: " + ex.Message);
            }
        }
    }

    // --- DELETE_TERM ---
    public class DeleteTermHandler : ICommandHandler
    {
        public string Command => "DELETE_TERM";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            if (parts.Length < 2)
            {
                TcpServer.SendMessage(stream, "Отсутствует термин для удаления");
                return;
            }

            string termName = parts[1];
            var termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath);
            if (termList == null)
            {
                TcpServer.SendMessage(stream, "Файл не найден");
                return;
            }

            var termToRemove = termList.terms.FirstOrDefault(t => t.term.Equals(termName, StringComparison.OrdinalIgnoreCase));
            if (termToRemove == null)
            {
                TcpServer.SendMessage(stream, "Термин не существует");
                return;
            }

            termList.terms.Remove(termToRemove);
            JsonHelper.WriteJsonFile(context.TermsFilePath, termList);
            TcpServer.SendMessage(stream, "Термин успешно удалён");
            Console.WriteLine($"[DELETE_TERM] {termName}");
        }
    }

    // --- TERM_VISITED ---
    public class TermVisitedHandler : ICommandHandler
    {
        public string Command => "TERM_VISITED";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            if (parts.Length < 2) return;

            var termList = JsonHelper.ReadJsonFile<TermList>(context.TermsFilePath);
            if (termList == null) return;

            var term = termList.terms.FirstOrDefault(t => t.term.Equals(parts[1], StringComparison.OrdinalIgnoreCase));
            if (term != null)
            {
                term.lastAccessed = DateTime.Now;
                term.popularity++;
                JsonHelper.WriteJsonFile(context.TermsFilePath, termList);
                Console.WriteLine($"[TERM_VISITED] {term.term}");
            }
        }
    }
}
