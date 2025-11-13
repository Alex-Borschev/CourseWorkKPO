using MongoDB.Driver;
using SharedLibrary;
using System.Collections.Generic;

namespace Server.Database
{
    public class UserRepository
    {
        private readonly IMongoCollection<Users> _collection;

        public UserRepository(MongoContext db)
        {
            _collection = db.Users;
        }

        public List<Users> GetAll()
        {
            return _collection.Find(_ => true).ToList();
        }

        public Users Find(string login, string password)
        {
            return _collection.Find(u => u.login == login && u.password == password).FirstOrDefault();
        }

        public Users FindByLogin(string login)
        {
            return _collection.Find(u => u.login == login).FirstOrDefault();
        }

        public void Add(Users user)
        {
            _collection.InsertOne(user);
        }

        public void Update(Users user)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.login, user.login);
            _collection.ReplaceOne(filter, user);
        }
    }
}
