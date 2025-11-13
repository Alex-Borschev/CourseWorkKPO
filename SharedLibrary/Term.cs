using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class Term
    {
        public string term { get; set; } 
        public string definition { get; set; }
        public string category { get; set; }
        public List<string> tags { get; set; }
        public int popularity { get; set; }
        public DateTime addedDate { get; set; }

        public DateTime lastAccessed { get; set; }

        public char letter { get; set; }
        public Dictionary<string, string> translations { get; set; }

        public List<int> difficultyRatings { get; set; }

        public string difficultyLevel { get; set; }
        public List<string> relatedTerms { get; set; }
        public List<HistoryEntry> history { get; set; }
        public List<MediaEntry> media { get; set; }
        public string author { get; set; }
        public string source { get; set; }
    }

    public class HistoryEntry
    {
        public DateTime date { get; set; }
        public string author { get; set; }
        public string change { get; set; }
    }

    public class MediaEntry
    {
        public string url { get; set; }
    }

    public class TermList
    {
        public List<Term> terms { get; set; }
    }
}
