// ICommandHandler.cs
// Интерфейс для обработчиков команд (паттерн Command).
// Изменения:
// - Введён единый контракт для всех обработчиков, чтобы упростить регистрацию и тестирование.
// - Каждый обработчик получает: parts (разобранные аргументы), stream (чтобы отправлять ответ),
//   и контекст (общие данные / зависимости).

using System.Net.Sockets;
using System.Text.Json;

namespace Server
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Команда, которую обрабатывает этот handler, например "AUTH".
        /// </summary>

        /// <summary>
        /// Обработать команду.
        /// </summary>
        /// <param name="parts">Массив частей сообщения (разделитель ';')</param>
        /// <param name="stream">NetworkStream, можно писать ответ</param>
        /// <param name="context">Серверный контекст (пользователи, пути и т.д.)</param>

       // ICommandHandler.cs


        string Command { get; }
        void Handle(JsonElement payload, NetworkStream stream, ServerContext context, ClientSession session);


    }



}
