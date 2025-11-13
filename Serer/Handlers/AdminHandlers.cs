// Handlers/AdminHandlers.cs
// Команды:
// - SUGG_EDIT (предложение правки)
// - GET_USERS (список пользователей)
//
// Изменения:
// 1. Код отправки сообщений администраторам использует SendMessageHandler.AddMessage для переиспользования логики.
// 2. Формат JSON-выдачи списка пользователей идентичен оригиналу.

using SharedLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Server.Handlers
{
    public class SuggestEditHandler : ICommandHandler
    {
        public string Command => "SUGGEST_EDIT";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                string termName = parts[1];
                string suggestion = parts[2];

                string suggestionFile = "suggestions.txt";
                File.AppendAllText(suggestionFile, $"{termName}:{suggestion}{Environment.NewLine}");

                TcpServer.SendResponse(stream, ServerResponse.Ok("Предложение на редактирование отправлено",
                    new { term = termName, suggestion }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при отправке предложения", new { error = ex.Message }));
            }
        }
    }

    public class GetUsersHandler : ICommandHandler
    {
        public string Command => "GET_USERS";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                var users = context.Users.Select(u => new { u.login, u.personality }).ToList();
                TcpServer.SendResponse(stream, ServerResponse.Ok("Список пользователей получен", users));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка при получении списка пользователей", new { error = ex.Message }));
            }
        }
    }
}
