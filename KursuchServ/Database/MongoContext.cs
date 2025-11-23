// MongoContext.cs
// Контекст подключения к MongoDB: централизованное место получения коллекций.
// Изменения:
// - Добавлена коллекция userdata (UserData).
// - Конструктор с параметрами по умолчанию.

using MongoDB.Driver;

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

        public IMongoCollection<SharedLibrary.UserData> UserData => _database.GetCollection<SharedLibrary.UserData>("userdata");
        public IMongoCollection<SharedLibrary.Term> Terms => _database.GetCollection<SharedLibrary.Term>("terms");
        // Оставил возможность использовать "users" если нужен отдельный набор (устаревшее)
    }
}
