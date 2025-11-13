// Program.cs
// Точка входа приложения — минимальна: только старт сервера.
// Изменения:
// 1) Убрал всю логику из старого Program.cs — теперь логика сервера в TcpServer.
// 2) Это улучшает читабельность и тестируемость (SRP).

using Server;
using Server.Database;
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Ethernet Terms - TCP Server";
            try
            {
                TcpServer.Run(); // Всё управление сервером перенесено в TcpServer.
                var db = new DatabaseService("mongodb://localhost:27017", "EthernetDictionary");
                var context = new ServerContext(db);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex.Message);
            }
        }
    }
}
