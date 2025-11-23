using Server;
using Server.Database;
using System;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Ethernet Terms - HTTP Server";
            try
            {
                var connectionString = "mongodb://admin:2342@5.35.94.193:27017/admin";
                var db = new DatabaseService(connectionString, "EthernetDictionary");
                await HttpServer.RunAsync(db);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex.Message);
            }
        }
    }
}
