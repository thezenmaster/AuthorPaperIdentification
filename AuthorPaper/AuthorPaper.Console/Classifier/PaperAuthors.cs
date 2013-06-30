using System.Collections.Generic;
using System.Linq;
using AuthorPaper.Console.IO;
using PreProcessing.BuildIndices;
using PreProcessing.IO;

namespace AuthorPaper.Console.Classifier
{
    public class PaperAuthors
    {
        public static void GetPaperAuthors(List<PaperOutput> sourcePapers)
        {
            var validPapers = BigStorage.ValidPapers;
                
            foreach (var output in sourcePapers)
            {
                var validPaper = validPapers[output.PaperId];   
                if (validPaper != null)
                {
                    output.AuthorId = validPaper.AuthorId.HasValue ? validPaper.AuthorId.Value : -1;
                }
                foreach (var matched in output.MatchedPapers)
                {
                    var validMatchedPaper = validPapers.ContainsKey(matched.PaperId)
                                                ? validPapers[matched.PaperId]
                                                : null;
                    if (validMatchedPaper != null)
                    {
                        matched.AuthorId = validMatchedPaper.AuthorId.HasValue ? validMatchedPaper.AuthorId.Value : -1;
                    }
                }
            }
        }
    }
}

