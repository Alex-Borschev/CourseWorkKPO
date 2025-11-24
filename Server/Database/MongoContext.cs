// MongoContext.cs
// Контекст подключения к MongoDB: централизованное место получения коллекций.
// Изменения:
// - Добавлена коллекция userdata (UserData).
// - Конструктор с параметрами по умолчанию.

using MongoDB.Driver;
using EntitiesLibrary;

namespace Server.Database
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(string connectionString = "mongodb://localhost:27017", string dbName = "EthernetDictionaryDB")
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<UserData> UserData => _database.GetCollection<UserData>("userdata");
        public IMongoCollection<Term> Terms => _database.GetCollection<Term>("terms");
        // Оставил возможность использовать "users" если нужен отдельный набор (устаревшее)
    }
}
