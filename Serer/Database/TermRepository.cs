// TermRepository.cs
// CRUD для терминов в коллекции "terms".
// Изменения:
// - Поддержка Id (BsonId в модели Term).
// - Реализованы базовые методы: GetAll, GetByName, GetById, Add, DeleteByName, Replace.

using MongoDB.Driver;
using SharedLibrary;
using System.Collections.Generic;

namespace Server.Database
{
    public class TermRepository
    {
        private readonly IMongoCollection<Term> _collection;

        public TermRepository(MongoContext db)
        {
            _collection = db.Terms;
        }

        public List<Term> GetAll()
        {
            return _collection.Find(_ => true).ToList();
        }

        public Term GetByName(string name)
        {
            return _collection.Find(t => t.term == name).FirstOrDefault();
        }

        public Term GetById(string id)
        {
            return _collection.Find(t => t.Id == id).FirstOrDefault();
        }

        public void Add(Term term)
        {
            _collection.InsertOne(term);
        }

        public void DeleteByName(string name)
        {
            _collection.DeleteOne(t => t.term == name);
        }

        public void Replace(Term term)
        {
            var filter = Builders<Term>.Filter.Eq(t => t.Id, term.Id);
            _collection.ReplaceOne(filter, term);
        }
    }
}
