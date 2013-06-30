using System.Collections.Generic;

namespace PreProcessing.IO
{
    public class PaperOutput
    {
        public long PaperId { get; set; }
        public long AuthorId { get; set; }
        public List<MatchedPaperOutput> MatchedPapers { get; set; } 
    }
}