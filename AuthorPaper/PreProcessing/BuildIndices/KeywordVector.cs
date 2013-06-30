using System.Collections.Generic;

namespace PreProcessing.BuildIndices
{
    public class KeywordVector
    {
        public string Value { get; set; }
        public long KeywordId { get; set; }
        public double InvertedPaperFrequency { get; set; }
        public SortedList<long, double> PaperKeywordFrequencies { get; set; }
    }
}