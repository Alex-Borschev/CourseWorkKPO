// Handlers/GetTermsHandler.cs
// Команда: GET_TERMS
// Назначение: отправка клиенту JSON с базой терминов Ethernet.
//
// Изменения:
// 1. Вынесено из switch-case, теперь отдельный обработчик.
// 2. Использует JsonHelper и TcpServer.SendMessage.
// 3. Отправка завершается "__THE_END__" для совместимости с клиентом.

using SharedLibrary;
using System.IO;
using System.Net.Sockets;

namespace Server.Handlers
{
    public class GetTermsHandler : ICommandHandler
    {
        public string Command => "GET_TERMS";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context, ClientSession session)
        {
            var terms = context.Db.GetAllTerms();

            if (terms == null || terms.Count == 0)
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("База терминов пуста или не найдена"));
                return;
            }

            // Отправляем список терминов клиенту
            TcpServer.SendResponse(stream, ServerResponse.Ok("Список терминов получен", terms));
        }
    }
}
