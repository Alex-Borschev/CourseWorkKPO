using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class UserData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }

        public string Personality { get; set; }
        public string Password { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<string> Favorites { get; set; }
        public List<RatedTerm> RatedTerms { get; set; }

        public List<UserNotes> Notes { get; set; }
        public List<MessageEntry> Messages { get; set; }

    }

    public class RatedTerm
    {
        public string Term { get; set; }
        public int Rating { get; set; }
    }

    public class MessageEntry
    {
        public DateTime Timestamp { get; set; }
        public string Theme { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
    }

    public class UserNotes
    {
        public DateTime Timestamp { get; set; }
        public string NotedTerm { get; set; }
        public string NotedData { get; set; }
    }

    public class UserDataList
    {
        public List<UserData> userData { get; set; }
    }

}
