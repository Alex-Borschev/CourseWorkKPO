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

using SharedLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace Server
{
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
        private const string TERMS_JSON = "data2.json";
        private const string USERS_FILE = "users.txt";

        public static void Run()
        {
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

            var context = new ServerContext
            {
                Users = users,
                TermsFilePath = TERMS_JSON,
                Router = new CommandRouter()
            };

            // Регистрация всех обработчиков
            RegisterAllHandlers(context);

            TcpListener listener = new TcpListener(IPAddress.Any, PORT);
            listener.Start();
            Console.WriteLine($"Сервер запущен на порту {PORT}...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                int clientId = Interlocked.Increment(ref clientCounter);

                Thread clientThread = new Thread(() => HandleClient(client, clientId, context));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }

        private static void RegisterAllHandlers(ServerContext context)
        {
            context.Router.RegisterHandler(new Server.Handlers.AuthHandler());
            context.Router.RegisterHandler(new Server.Handlers.RegisterHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetTermsHandler());
            context.Router.RegisterHandler(new Server.Handlers.AddTermHandler());
            context.Router.RegisterHandler(new Server.Handlers.DeleteTermHandler());
            context.Router.RegisterHandler(new Server.Handlers.TermVisitedHandler());
            context.Router.RegisterHandler(new Server.Handlers.UpdateFavoriteHandler());
            context.Router.RegisterHandler(new Server.Handlers.AddNoteHandler());
            context.Router.RegisterHandler(new Server.Handlers.ClearMessageHandler());
            context.Router.RegisterHandler(new Server.Handlers.SendMessageHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetUserDataHandler());
            context.Router.RegisterHandler(new Server.Handlers.SuggestEditHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetUsersHandler());
            context.Router.RegisterHandler(new Server.Handlers.RateTermHandler());
        }

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

                        string[] parts = received.Split(new[] { ';' }, StringSplitOptions.None);
                        string command = parts.Length > 0 ? parts[0] : string.Empty;

                        context.Router.Route(command, parts, stream, context);
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

        public static void SendResponse(NetworkStream stream, ServerResponse response)
        {
            if (stream == null || !stream.CanWrite || response == null) return;
            try
            {
                string json = response.ToJson();
                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);
            }
            catch (IOException) { }
        }
    }
}
