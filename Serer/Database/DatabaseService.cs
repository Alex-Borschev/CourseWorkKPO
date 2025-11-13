// DatabaseService.cs
// Фасад для доступа к БД. Инкапсулирует контекст и репозитории.
// Изменения:
// - Интегрирует TermRepository и UserDataRepository.
// - Предоставляет методы, которые удобны для обработчиков.

using SharedLibrary;
using System.Collections.Generic;

namespace Server.Database
{
    public class DatabaseService
    {
        public MongoContext Context { get; }
        public TermRepository Terms { get; }
        public UserDataRepository Users { get; }

        public DatabaseService(string connectionString = "mongodb://localhost:27017", string dbName = "EthernetDictionaryDB")
        {
            Context = new MongoContext(connectionString, dbName);
            Terms = new TermRepository(Context);
            Users = new UserDataRepository(Context);
        }

        // --- Convenience wrappers (можно вызывать из обработчиков) ---
        public List<Term> GetAllTerms() => Terms.GetAll();
        public Term GetTermByName(string name) => Terms.GetByName(name);
        public void AddTerm(Term term) => Terms.Add(term);
        public void DeleteTermByName(string name) => Terms.DeleteByName(name);
        public void UpdateTerm(Term term) => Terms.Replace(term);

        public List<UserData> GetAllUsers() => Users.GetAll();
        public UserData FindUserByLogin(string login) => Users.FindByLogin(login);
        public UserData ValidateUser(string login, string password) => Users.ValidateCredentials(login, password);
        public void AddUser(UserData u) => Users.Add(u);
        public void UpdateUser(UserData u) => Users.Update(u);
        public void DeleteUser(string login) => Users.DeleteByLogin(login);
    }
}
