// Handlers/GetTermsHandler.cs
// Команда: GET_TERMS
// Назначение: отправка клиенту JSON с базой терминов Ethernet.
//
// Изменения:
// 1. Вынесено из switch-case, теперь отдельный обработчик.
// 2. Использует JsonHelper и TcpServer.SendMessage.
// 3. Отправка завершается "__THE_END__" для совместимости с клиентом.

using System;
using System.IO;
using System.Net.Sockets;

namespace Server.Handlers
{
    public class GetTermsHandler : ICommandHandler
    {
        public string Command => "GET_TERMS";

        public void Handle(string[] parts, NetworkStream stream, ServerContext context)
        {
            if (!File.Exists(context.TermsFilePath))
            {
                TcpServer.SendMessage(stream, "ERROR: File not found");
                return;
            }

            try
            {
                string json = File.ReadAllText(context.TermsFilePath);
                TcpServer.SendMessage(stream, json + "__THE_END__");
            }
            catch (Exception ex)
            {
                TcpServer.SendMessage(stream, "ERROR: " + ex.Message);
            }
        }
    }
}
