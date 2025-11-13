using MongoDB.Driver;
using SharedLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Database
{
    public class DatabaseService
    {
        private readonly IMongoDatabase _database;

        public DatabaseService(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<Users> Users => _database.GetCollection<Users>("users");
        public IMongoCollection<Term> Terms => _database.GetCollection<Term>("terms");

        public List<Users> GetAllUsers()
        {
            return Users.Find(_ => true).ToList();
        }

        public void AddUser(Users user)
        {
            Users.InsertOne(user);
        }

        public Users FindUser(string login, string password)
        {
            return Users.Find(u => u.login == login && u.password == password).FirstOrDefault();
        }

        public List<Term> GetAllTerms()
        {
            return Terms.Find(_ => true).ToList();
        }

        public Term GetTermByName(string name)
    => Terms.Find(t => t.term == name).FirstOrDefault();

        public void AddTerm(Term term)
            => Terms.InsertOne(term);

        public void UpdateTerm(Term term)
        {
            var filter = Builders<Term>.Filter.Eq(t => t.term, term.term);
            Terms.ReplaceOne(filter, term);
        }

    }
}
