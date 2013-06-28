using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuthorPaper
{
    public class KNearestNeighbours
    {
        public static List<Paper> FindKNearestPapers(int paperId, int nearestK)
        {
            var paperIndex = BuildPaperIndex(paperId);
            NormalizeInputPaperIndex(paperIndex);
            var nearestPapers = new List<Paper>();
            using (var context = new AuthorPaperEntities())
            {
                var trainSet = context.Papers.Where(p => p.keywords != null && p.keywords.Count > 0);
                foreach (var paperItem in trainSet)
                {
                    double scalarProduct = 0.0;
                    double inputVectorNorm = 0.0;
                    double trainSetItemNorm = 0.0;
                    foreach (var word in paperIndex)
                    {
                        var wordNormalizedCount = word.NormalizedCount;
                        inputVectorNorm += wordNormalizedCount * wordNormalizedCount;
                        var keywordInDb = paperItem.keywords.SingleOrDefault(k => k.Value == word.Value);
                        if (keywordInDb != null && keywordInDb.normalizedcount.HasValue)
                        {
                            scalarProduct += wordNormalizedCount * keywordInDb.normalizedcount.Value;
                        }
                    }

                    //It only makes sense to calc cosine if scalar product is > 0
                    if (scalarProduct > 0)
                    {
                        trainSetItemNorm = CalculateNorm(paperItem);
                        if (inputVectorNorm > 0 && trainSetItemNorm > 0)
                        {
                            inputVectorNorm = Math.Sqrt(inputVectorNorm);
                            double cosine = scalarProduct / (inputVectorNorm * trainSetItemNorm);
                        }
                    }
                }
            }

            return null;
        }

        private static double CalculateNorm(Paper paper)
        {
            double norm = 0.0;

            foreach (var item in paper.keywords)
            {
                if (item.normalizedcount.HasValue)
                {
                    double value = item.normalizedcount.Value;
                    norm += value*value;
                }
            }

            return Math.Sqrt(norm);
        }

        public static void NormalizeInputPaperIndex(List<Word> index)
        {
            using (var context = new AuthorPaperEntities())
            {
                foreach (var word in index)
                {
                    var keyword = context.Keywords.SingleOrDefault(k => k.Value == word.Value);
                    if (keyword == null)
                    {
                        //Why is this word missing from dictionary?
                        continue;
                    }
                    if (keyword.Count.HasValue)
                    {
                        word.NormalizedCount = (double) word.Count / keyword.Count.Value;
                    }
                }
            }
        }

        public static List<Word> BuildPaperIndex(int paperId)
        {
            var keywords = new List<Word>();
            using (var context = new AuthorPaperEntities())
            {
                var paper = context.Papers.SingleOrDefault(p => p.Id == paperId);
                if (paper == null)
                    return null;
                keywords = LoadPaperKeywords(paper);
            }

            return keywords;
        }

        private static List<Word> LoadPaperKeywords(Paper paper)
        {
            var splitChars = new char[] { ',', ' ', ';', '.', '!', '?', '"' };
            var keywords = new List<Word>();
            var stopWords = LoadStopWords();
            if (!String.IsNullOrEmpty(paper.Title))
            {
                var titleKeywords = paper.Title.Trim(new char[] { '"' }).Split(splitChars).ToList();
                foreach (var keyword in titleKeywords)
                {
                    if (!stopWords.Contains(keyword) && !keywords.Contains(keyword))
                    {
                        keywords.Add(new Word { Value = keyword, Count = 0, NormalizedCount = 0.0 });
                    }
                    else
                    {
                        keywords.Single(w => w.Value == keyword).Count++;
                    }
                }
            }
            if (!String.IsNullOrEmpty(paper.Keyword))
            {
                var paperKeywords = paper.Keyword.Trim(new char[] { '"' }).Split(splitChars);
                foreach (var keyword in paperKeywords)
                {
                    if (!stopWords.Contains(keyword) && !keywords.Contains(keyword))
                    {
                        keywords.Add(new Word { Value = keyword, Count = 0, NormalizedCount = 0.0 });
                    }
                    else
                    {
                        keywords.Single(w => w.Value == keyword).Count++;
                    }
                }
            }

            return keywords;
        }

        public static List<string> LoadStopWords()
        {
            var stopWords = new List<string>();

            var filePath = @"..\..\..\data\english_stop.txt";
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        stopWords.Add(line);
                    }
                }
            }
            return stopWords;
        }
    }
}
