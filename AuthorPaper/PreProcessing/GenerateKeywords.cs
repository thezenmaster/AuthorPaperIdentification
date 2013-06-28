using System.Collections.Generic;
using System.Linq;
using AuthorPaper;

namespace PreProcessing
{
    public class GenerateKeywords
    {

        public static IEnumerable<Word> GeneratePaperKeywords(Paper paper)
        {
            var paperKeywords = new List<string>();
            paperKeywords.AddRange(GetKeywords(paper.Title, true));
            paperKeywords.AddRange(GetKeywords(paper.Keyword, false));
            // count number of occurrences
            return paperKeywords.GroupBy(pk => pk).Select(g => new Word { Value = g.Key, Count = g.Count()});
        }


        public static List<string> GetKeywords(string keywordList, bool isTitle)
        {
            return Tokenizer.TokenizeList(keywordList, isTitle);
        }
    }
}
