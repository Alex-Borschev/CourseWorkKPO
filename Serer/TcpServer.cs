// TcpServer.cs (обновлён)
// - ServerContext теперь содержит DatabaseService Db;
// - Регистрация обработчиков остаётся, обработчики могут использовать context.Db.

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Server.Database;
using System.Text.Json;

namespace Server
{
    public class ServerContext
    {
        public string TermsFilePath { get; set; }
        public CommandRouter Router { get; set; }
        public DatabaseService Db { get; set; }

        public ServerContext() { }

        public ServerContext(DatabaseService db)
        {
            Db = db;
            Router = new CommandRouter();
        }
    }

    public static class TcpServer
    {
        private static int clientCounter = 0;
        private const int PORT = 8888;

        public static void Run(Server.Database.DatabaseService dbService = null)
        {
            // Если dbService передан — используем его; иначе создаём по умолчанию.
            var db = dbService ?? new Server.Database.DatabaseService();

            var context = new ServerContext(db)
            {
                Router = new CommandRouter() // Router создаётся внутри контекста, но на всякий случай
            };

            context.Router.RegisterHandler(new Server.Handlers.AuthHandler());
            context.Router.RegisterHandler(new Server.Handlers.RegisterHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetTermsHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetCategoriesHandler());
            context.Router.RegisterHandler(new Server.Handlers.AddTermHandler());
            context.Router.RegisterHandler(new Server.Handlers.DeleteTermHandler());
            context.Router.RegisterHandler(new Server.Handlers.TermVisitedHandler());
            context.Router.RegisterHandler(new Server.Handlers.UpdateFavoriteHandler());
            context.Router.RegisterHandler(new Server.Handlers.AddNoteHandler());
            context.Router.RegisterHandler(new Server.Handlers.DeleteNoteHandler());
            context.Router.RegisterHandler(new Server.Handlers.ClearMessageHandler());
            context.Router.RegisterHandler(new Server.Handlers.SendMessageHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetUserDataHandler());
            context.Router.RegisterHandler(new Server.Handlers.SuggestEditHandler());
            context.Router.RegisterHandler(new Server.Handlers.GetUsersHandler());
            context.Router.RegisterHandler(new Server.Handlers.RateTermHandler());

            TcpListener listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            Console.WriteLine("Server started on port 8888...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(_ => HandleClient(client, context));
                Console.WriteLine("A new user has connected");
            }
        }

        private static void HandleClient(TcpClient client, ServerContext context)
        {
            string clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            var session = new ClientSession(clientAddress); // создаём сессию для клиента

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        string received = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Console.WriteLine(received);
                        var envelope = JsonSerializer.Deserialize<ClientEnvelope>(received);
                        
                        if (envelope == null || envelope.Command == null)
                        {
                            TcpServer.SendResponse(stream, ServerResponse.Error("Некорректный формат сообщения"));
                            continue;
                        }

                        context.Router.Route(
                            envelope.Command,
                            envelope.Payload, // <-- теперь Payload передаётся в Router
                            stream,
                            context,
                            session
                        );

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client {clientAddress} error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        public class ClientEnvelope
        {
            public string Command { get; set; }
            public JsonElement Payload { get; set; }
        }


        // SendResponse оставляем без изменений (используется для сериализации ServerResponse)
        public static void SendResponse(System.Net.Sockets.NetworkStream stream, ServerResponse response)
        {
            if (stream == null || !stream.CanWrite || response == null) return;
            try
            {
                string json = response.ToJson() + '\n';
                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);
            }
            catch (IOException) { }
        }
    }
}
