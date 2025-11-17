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

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!payload.TryGetProperty("termData", out var termJson))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Отсутствует поле termData"));
                    return;
                }

                var newTerm = JsonSerializer.Deserialize<Term>(termJson.GetRawText());
                if (newTerm == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Ошибка десериализации термина"));
                    return;
                }

                if (context.Db.GetTermByName(newTerm.term) != null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Такой термин уже существует"));
                    return;
                }

                newTerm.addedDate = DateTime.Now;
                newTerm.lastAccessed = DateTime.MinValue;

                context.Db.AddTerm(newTerm);

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Термин добавлен", new { term = newTerm.term }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при добавлении термина", new { error = ex.Message }));
            }
        }
    }



    public class DeleteTermHandler : ICommandHandler
    {
        public string Command => "DELETE_TERM";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!payload.TryGetProperty("term", out var termProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Отсутствует поле term"));
                    return;
                }

                string termID = termProp.GetString();

                var term = context.Db.GetTermByID(termID);
                if (term == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Термин не найден"));
                    return;
                }

                context.Db.DeleteTermByID(termID);

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Термин успешно удалён", new { term = termID }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при удалении термина", new { error = ex.Message }));
            }
        }
    }



    public class TermVisitedHandler : ICommandHandler
    {
        public string Command => "TERM_VISITED";

        public void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            try
            {
                if (!payload.TryGetProperty("term", out var termProp))
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Отсутствует поле term"));
                    return;
                }

                string termID = termProp.GetString();

                var term = context.Db.GetTermByID(termID);
                if (term == null)
                {
                    TcpServer.SendResponse(stream, ServerResponse.Error("Термин не найден"));
                    return;
                }

                term.lastAccessed = DateTime.Now;
                context.Db.UpdateTerm(term);

                TcpServer.SendResponse(stream,
                    ServerResponse.Ok("Время последнего доступа обновлено",
                        new { term = termID, lastAccessed = term.lastAccessed }));
            }
            catch (Exception ex)
            {
                TcpServer.SendResponse(stream,
                    ServerResponse.Error("Ошибка при обновлении доступа", new { error = ex.Message }));
            }
        }
    }
}


