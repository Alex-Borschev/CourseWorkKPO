using EntitiesLibrary;
using System.Collections.Generic;

namespace Server.Database
{
    public interface IDatabaseService
    {
        List<Term> GetAllTerms();
        Term GetTermByName(string name);
        Term GetTermByID(string id);
        void AddTerm(Term term);
        void DeleteTermByName(string name);
        void DeleteTermByID(string id);
        void UpdateTerm(Term term);

        List<UserData> GetAllUsers();
        UserData FindUserByLogin(string login);
        UserData FindUserByID(string id);
        UserData ValidateUser(string login, string password);
        void AddUser(UserData u);
        void UpdateUser(UserData u);
        void DeleteUser(string login);
    }
}
