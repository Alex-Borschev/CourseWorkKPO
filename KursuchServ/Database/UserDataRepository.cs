// UserDataRepository.cs
// CRUD для данных пользователей — коллекция "userdata".
// Изменения:
// - Репозиторий работает с SharedLibrary.UserData (включает пароль, роль и др.).
// - Методы: GetAll, FindByLogin (по Username), ValidateCredentials, Add, Update, Delete.


using MongoDB.Driver;
using SharedLibrary;
using System.Collections.Generic;
using Server;

namespace Server.Database
{
    public class UserDataRepository
    {
        private readonly IMongoCollection<UserData> _collection;

        public UserDataRepository(MongoContext db)
        {
            _collection = db.UserData;
        }

        public List<UserData> GetAll()
        {
            return _collection.Find(_ => true).ToList();
        }

        public UserData FindByLogin(string username)
        {
            return _collection.Find(u => u.Username == username).FirstOrDefault();
        }

        public UserData FindByID(string id)
        {
            return _collection.Find(u => u.Id == id).FirstOrDefault();
        }

        // Проверка логина/пароля. В текущем виде сравнивает plain-text (нужно хэширование).
        public UserData ValidateCredentials(string username, string password)
        {
            var user = _collection.Find(u => u.Username == username).FirstOrDefault();

            if (user == null)
                return null;

            bool valid = PasswordHasher.VerifyPassword(password, user.Password);

            return valid ? user : null;
        }

        public void RegisterUser(UserData user)
        {
            user.Password = PasswordHasher.HashPassword(user.Password);
            _collection.InsertOne(user);
        }

        public void Update(UserData user)
        {
            var filter = Builders<UserData>.Filter.Eq(u => u.Id, user.Id);
            _collection.ReplaceOne(filter, user);
        }

        public void DeleteByLogin(string username)
        {
            _collection.DeleteOne(u => u.Username == username);
        }
    }
}
