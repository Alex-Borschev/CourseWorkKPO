// TcpServer.cs
// Основная логика сервера: запуск TcpListener, приём клиентов, чтение сообщений и делегирование
// их обработчику (CommandRouter).
//
// Изменения и причины:
// - Вынесено из монолитного Program.cs, теперь класс отвечает только за сетевую часть.
// - Введён CommandRouter для маршрутизации команд (паттерн Command).
// - Введён ClientContext для передачи общих зависимостей обработчикам.
// - Логирование подключений вынесено в FileLogger.
// - JSON-утилиты унифицированы в JsonHelper.
// - Работа с пользователями загружается один раз и передаётся через контекст (минимальная DI).

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using SharedLibrary;

namespace Server
{
    /// <summary>
    /// Контейнер зависимостей и общих данных, которые нужны обработчикам команд.
    /// В дальнейшем сюда можно добавлять репозитории, сервисы и т.д.
    /// </summary>
    public class ServerContext
    {
        public List<Users> Users { get; set; }
        public string TermsFilePath { get; set; }
        public CommandRouter Router { get; set; }
    }

    public static class TcpServer
    {
        private static int clientCounter = 0;
        private const int PORT = 8888;

        // Путь к данным терминов — вынесено в константу, используется в контексте.
        private const string TERMS_JSON = "data2.json";
        private const string USERS_FILE = "users.txt";

        /// <summary>
        /// Запуск сервера (вся инициализация здесь).
        /// </summary>
        public static void Run()
        {
            // Загрузка пользователей один раз при старте
            List<Users> users;
            try
            {
                users = Users.LoadUsersFromFile(USERS_FILE);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке пользователей: " + ex.Message);
                return;
            }

            // Инициализация контекста
            var context = new ServerContext
            {
                Users = users,
                TermsFilePath = TERMS_JSON,
                Router = new CommandRouter()
            };

            // Регистрация обработчиков команд
            context.Router.RegisterHandler(new Server.Handlers.AuthHandler());
            context.Router.RegisterHandler(new Server.Handlers.RegisterHandler());

            // --- Базовые команды ---
            context.Router.RegisterHandler(new Server.Handlers.AuthHandler());
            context.Router.RegisterHandler(new Server.Handlers.RegisterHandler());

            // --- Термины ---
            context.Router.RegisterHandler(new Server.Handlers.GetTermsHandler());
            context.Router.RegisterHandler(new Server.Handlers.AddTermHandler());
            context.Router.RegisterHandler(new Server.Handlers.DeleteTermHandler());
            context.Router.RegisterHandler(new Server.Handlers.TermVisitedHandler());

            // --- Пользовательские данные ---
            context.Router.RegisterHandler(new Server.Handlers.UpdateFavoriteHandler());
            context.Router.RegisterHandler(new Server.Handlers.AddNoteHandler());
            context.Router.RegisterHandler(new Server.Handlers.ClearMessageHandler());
            context.Router.RegisterHandler(new Server.Handlers.SendMessageHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetUserDataHandler());

            // --- Админские ---
            context.Router.RegisterHandler(new Server.Handlers.SuggestEditHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetUsersHandler());

            // --- Оценки ---
            context.Router.RegisterHandler(new Server.Handlers.RateTermHandler());



            // NOTE: регистрация обработчиков команд отложена — их подключим в следующем шаге.
            // Это позволяет разделить ответственность: TcpServer — только сеть и жизненный цикл клиентов.

            TcpListener listener = new TcpListener(IPAddress.Any, PORT);
            listener.Start();
            Console.WriteLine($"Сервер запущен на порту {PORT}...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                int clientId = Interlocked.Increment(ref clientCounter);

                // Каждому клиенту — отдельный поток (как в оригинале)
                Thread clientThread = new Thread(() => HandleClient(client, clientId, context));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }

        /// <summary>
        /// Обработка одного клиента. Считываем сообщения и передаём их в маршрутизатор.
        /// </summary>
        private static void HandleClient(TcpClient client, int clientId, ServerContext context)
        {
            string clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            string connectionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            FileLogger.LogConnection(clientAddress, connectionTime, threadId);

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        string received = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Console.WriteLine($"[{clientAddress}] -> {received}");

                        // Разбор — команда и параметры через ';'
                        string[] parts = received.Split(new[] { ';' }, StringSplitOptions.None);
                        string command = parts.Length > 0 ? parts[0] : string.Empty;

                        // Делаем попытку маршрутизации через CommandRouter. Если никаких обработчиков не зарегистрировано,
                        // отправляется сообщение об ошибке. Это предотвращает громоздкие switch-case и уменьшает дубли.
                        if (context.Router != null)
                        {
                            context.Router.Route(command, parts, stream, context);
                        }
                        else
                        {
                            // fallback: если маршрутизатор отсутствует, логируем и сообщаем клиенту
                            var msg = "ServerError: no router configured";
                            SendMessage(stream, msg);
                            Console.WriteLine(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка клиента {clientAddress} поток {threadId}: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine($"Клиент {clientAddress} поток {threadId} отключился.");
            }
        }

        /// <summary>
        /// Утилитарный метод отправки строки клиенту.
        /// Вынесен отдельно, чтобы не дублировать код.
        /// </summary>
        public static void SendMessage(NetworkStream stream, string message)
        {
            if (stream == null || !stream.CanWrite) return;
            byte[] response = Encoding.UTF8.GetBytes(message);
            try
            {
                stream.Write(response, 0, response.Length);
            }
            catch (IOException) { /* клиент мог отключиться */ }
        }
    }
}
