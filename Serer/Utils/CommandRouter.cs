// CommandRouter.cs
// Маршрутизатор команд: хранит словарь команд -> обработчик.
// Изменения:
// - Убирает большой switch-case в пользу extensible routing.
// - Позволяет регистрировать обработчики динамически (в будущем можно внедрить DI).
// - Лёгкий fallback: если обработчик не найден, отправляется информативный ответ.

// CommandRouter.cs (редактирование)
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;

namespace Server
{
    public class CommandRouter
    {
        private readonly Dictionary<string, ICommandHandler> _handlers = new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase);

        public void RegisterHandler(ICommandHandler handler)
        {
            if (handler == null) return;
            _handlers[handler.Command] = handler;
        }

        // Обновлённая сигнатура: добавляем ClientSession session
        public void Route(string command, JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session)
        {
            if (_handlers.TryGetValue(command, out var handler))
                handler.Handle(payload, stream, context, session);
            else
                TcpServer.SendResponse(stream, ServerResponse.Error("Неизвестная команда"));
        }

    }
}
