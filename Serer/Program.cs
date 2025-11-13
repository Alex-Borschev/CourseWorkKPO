// Program.cs
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
                // Инициализируем DB и передаём его в TcpServer.Run (вариант с DI-lite).
                var db = new DatabaseService("mongodb://localhost:27017", "EthernetDictionary");

                // Запускаем сервер, передав db (я покажу ниже перегрузку Run)
                TcpServer.Run(db);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex.Message);
            }
        }
    }
}
