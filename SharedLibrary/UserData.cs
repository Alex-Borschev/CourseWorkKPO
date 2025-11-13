using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class UserData
    {
        public string Username { get; set; }

        public string Personality { get; set; }
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

}
