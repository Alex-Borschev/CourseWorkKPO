// Handlers/GetTermsHandler.cs
// Команда: GET_TERMS
// Назначение: отправка клиенту JSON с базой терминов Ethernet.
//
// Изменения:
// 1. Вынесено из switch-case, теперь отдельный обработчик.
// 2. Использует JsonHelper и TcpServer.SendMessage.
// 3. Отправка завершается "__THE_END__" для совместимости с клиентом.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Linq;

namespace Server.Handlers
{
    public class GetTermsHandler : ICommandHandler
    {
        public string Command => "GET_TERMS";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            var terms = context.Db.GetAllTerms();
            TcpServer.SendResponse(stream, ServerResponse.Ok("Список терминов", terms));
        }
    }

    public class GetCategoriesHandler : ICommandHandler
    {
        public string Command => "GET_CATEGORIES";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                // Получаем все термины
                var terms = context.Db.GetAllTerms();

                // Собираем уникальные категории
                var categories = terms
                    .Where(t => !string.IsNullOrEmpty(t.category))
                    .Select(t => t.category)
                    .Distinct()
                    .ToList();

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Список категорий", categories));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при получении категорий", new { error = ex.Message }));
            }
        }
    }
}


