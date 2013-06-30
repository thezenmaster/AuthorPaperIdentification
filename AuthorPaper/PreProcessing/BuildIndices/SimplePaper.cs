using System.Collections.Generic;

namespace PreProcessing.BuildIndices
{
    public class SimplePaper
    {
        public long Id { get; set; }
        public IEnumerable<SimplePaperKeyword> PaperKeywords { get; set; } 
    }
}