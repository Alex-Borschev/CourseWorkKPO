// CommandRouter.cs
// Маршрутизатор команд: хранит словарь команд -> обработчик.
// Изменения:
// - Убирает большой switch-case в пользу extensible routing.
// - Позволяет регистрировать обработчики динамически (в будущем можно внедрить DI).
// - Лёгкий fallback: если обработчик не найден, отправляется информативный ответ.

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server
{
    public class CommandRouter
    {
        private readonly Dictionary<string, ICommandHandler> handlers = new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Регистрация обработчика для конкретной команды.
        /// Можно вызывать несколько раз при инициализации (например, в Main или TcpServer).
        /// </summary>
        public void RegisterHandler(ICommandHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (string.IsNullOrWhiteSpace(handler.Command)) throw new ArgumentException("handler.Command is required");

            handlers[handler.Command] = handler;
        }

        /// <summary>
        /// Маршрутизация команды на соответствующий обработчик.
        /// </summary>
        public void Route(string command, string[] parts, NetworkStream stream, ServerContext context)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("Invalid command"));
                return;
            }

            ICommandHandler handler;
            if (handlers.TryGetValue(command, out handler))
            {
                try
                {
                    handler.Handle(parts, stream, context);
                }
                catch (Exception ex)
                {
                    // Единая обработка ошибок для обработчиков
                    var msg = "ERROR:" + ex.Message;
                    TcpServer.SendResponse(stream, ServerResponse.Error(msg));
                    Console.WriteLine("Handler error for command " + command + ": " + ex.Message);
                }
            }
            else
            {
                TcpServer.SendResponse(stream, ServerResponse.Error("UnknownCommand"));
            }
        }
    }
}
