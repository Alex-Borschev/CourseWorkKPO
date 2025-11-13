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
            var filter = Builders<Term>.Filter.Eq(t => t.term, term.term);
            _collection.ReplaceOne(filter, term);
        }
    }
}
