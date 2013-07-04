using System;
using AuthorPaper;

namespace PreProcessing.BuildIndices
{
    public class VectorParameters
    {
        private static double TryFindKeywordInIndex(long paperId, string keywordValue)
        {
            if (BigStorage.KeywordIndex.ContainsKey(keywordValue))
            {
                var keywordFromIndex = BigStorage.KeywordIndex[keywordValue];
                var idf = keywordFromIndex.InvertedPaperFrequency;
                if (keywordFromIndex.PaperKeywordFrequencies.ContainsKey(paperId))
                {
                    var tf = keywordFromIndex.PaperKeywordFrequencies[paperId];
                    return idf * tf;
                }
            }
            return -1.0;
        }

        public static double CalculateWeight(long paperId, string keywordValue, double tf)
        {
            if (BigStorage.KeywordIndex.ContainsKey(keywordValue))
            {
                var keywordFromIndex = BigStorage.KeywordIndex[keywordValue];
                var idf = keywordFromIndex.InvertedPaperFrequency;
                return idf * tf;
            }
            return 0.00001;
        }

        public static double CalculateWeight(long paperId, string keywordValue)
        {
            // todo: remove hack for removing new lines
            var weight = TryFindKeywordInIndex(paperId, keywordValue); //KeywordIndex.RemoveNewLines(keywordValue)
            return weight > -1.0 ? weight : 0.00001;
        }
        public static double CalculateInvertedPaperFreq(Keyword keyword)
        {
            var numberOfPapersContainingKeyword = keyword.PaperKeywords.Count;
            var result = BigStorage.TrainPaperCount / (numberOfPapersContainingKeyword + 1.0);
            return Math.Log(result);
        }

        public static double CalculateInvertedPaperFreq(KeywordVector keyword)
        {
            var numberOfPapersContainingKeyword = keyword.PaperKeywordFrequencies.Count;
            var result = BigStorage.TrainPaperCount / (numberOfPapersContainingKeyword + 1.0);
            return Math.Log(result);
        }
        
        public static double CalculateKeywordFrequency(long keywordId, long countKeyword, long? maxCount)
        {
            const double a = 0.4;
            var normalizedTf = a + (1 - a)* countKeyword / (maxCount.HasValue ? maxCount.Value : 1.0);
            return normalizedTf;
        }

        public static double CalculateScalarSquare(PaperVector vector)
        {
            // calculate similarity term (math.sqrt(sum((tf*idf)^2)))
            var result = 0.0;
            foreach (var weight in vector.KeywordWeight)
            {
                result += weight.Value*weight.Value;
            }
            return Math.Sqrt(result);
        }
       
    }
}
