using DotNetEnv;
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
            Env.Load();
            try
            {
                var connectionString = Env.GetString("DB_CONNECTION");
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
