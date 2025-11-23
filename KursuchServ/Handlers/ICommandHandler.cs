using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Server
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Имя команды, например "AUTH", "PING", "REGISTER".
        /// </summary>
        string Command { get; }

        /// <summary>
        /// Обработка команды.
        /// </summary>
        /// <param name="payload">JSON тело запроса.</param>
        /// <param name="http">HttpContext, через него пишем ответ.</param>
        /// <param name="context">Глобальный серверный контекст.</param>
        /// <param name="session">Сессия клиента.</param>
        Task Handle(JsonElement payload, HttpContext http, ServerContext context);
    }
}
