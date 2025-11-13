// Handlers/GetTermsHandler.cs
// Команда: GET_TERMS
// Назначение: отправка клиенту JSON с базой терминов Ethernet.
//
// Изменения:
// 1. Вынесено из switch-case, теперь отдельный обработчик.
// 2. Использует JsonHelper и TcpServer.SendMessage.
// 3. Отправка завершается "__THE_END__" для совместимости с клиентом.

using System.Net.Sockets;
using System.Text.Json;

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
}


