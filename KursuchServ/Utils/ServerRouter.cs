using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server
{
    public class ServerRouter
    {
        private readonly Dictionary<string, ICommandHandler> _handlers =
            new(StringComparer.OrdinalIgnoreCase);

        public void RegisterHandler(ICommandHandler handler)
        {
            if (handler == null) return;
            _handlers[handler.Command] = handler;
        }

        /// <summary>
        /// Основной метод маршрутизации HTTP-команд.
        /// </summary>
        public async Task Route(
            string command,
            JsonElement payload,
            HttpContext http,
            ServerContext context)
        {
            if (_handlers.TryGetValue(command, out var handler))
            {
                // Запускаем обработчик команды
                await handler.Handle(payload, http, context);
            }
            else
            {
                var response = new
                {
                    message = $"Команда '{command}' не найдена"
                };

                http.Response.StatusCode = 404;
                http.Response.ContentType = "application/json";
                await http.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
