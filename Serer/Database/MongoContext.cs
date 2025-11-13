using MongoDB.Driver;
using SharedLibrary;

namespace Server.Database
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        // Конструктор с параметрами по умолчанию
        public MongoContext(string connectionString = "mongodb://localhost:27017", string dbName = "EthernetDictionaryDB")
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<Users> Users => _database.GetCollection<Users>("users");
        public IMongoCollection<Term> Terms => _database.GetCollection<Term>("terms");
    }
}
