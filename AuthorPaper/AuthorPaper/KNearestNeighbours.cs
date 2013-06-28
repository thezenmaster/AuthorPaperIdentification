using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AuthorPaper
{
    public class KNearestNeighbours
    {
        public static List<PaperVector> FindKNearestPapers(int paperId, int nearestK)
        {
            var paperIndex = BuildPaperIndex(paperId);
            NormalizeInputPaperIndex(paperIndex);
            var nearestPapers = new List<PaperVector>();
            using (var context = new AuthorPaperEntities())
            {
                // todo: make sure child entity collections are loaded
                var trainSet = context.papers.Where(p => p.paperkeyword != null && p.paperkeyword.Any());
                foreach (var paperItem in trainSet)
                {
                    double scalarProduct = 0.0;
                    double inputVectorNorm = 0.0;
                    double trainSetItemNorm = 0.0;
                    foreach (var word in paperIndex)
                    {
                        var wordNormalizedCount = word.NormalizedCount;
                        inputVectorNorm += wordNormalizedCount * wordNormalizedCount;
                        var keywordInDb = paperItem.paperkeyword.SingleOrDefault(k => k.keyword.value == word.Value);
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
                            if (nearestPapers.Count < nearestK)
                            {
                                nearestPapers.Add(new PaperVector { Paper = paperItem, Similarity = cosine });
                            }
                            else
                            {
                                nearestPapers = nearestPapers.OrderByDescending(p => p.Similarity).ToList();
                                if (nearestPapers.Last().Similarity < cosine)
                                {
                                    nearestPapers.Remove(nearestPapers.Last());
                                    nearestPapers.Add(new PaperVector { Paper = paperItem, Similarity = cosine });
                                }
                            }
                        }
                    }
                }
            }

            return nearestPapers;
        }

        private static double CalculateNorm(paper paper)
        {
            double norm = 0.0;

            foreach (var item in paper.paperkeyword)
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
                    var keyword = context.keyword.SingleOrDefault(k => k.value == word.Value);
                    if (keyword == null)
                    {
                        //Why is this word missing from dictionary?
                        continue;
                    }
                    if (keyword.count.HasValue)
                    {
                        word.NormalizedCount = (double) word.Count / keyword.count.Value;
                    }
                }
            }
        }

        public static List<Word> BuildPaperIndex(int paperId)
        {
            List<Word> keywords;
            using (var context = new AuthorPaperEntities())
            {
                var paper = context.papers.SingleOrDefault(p => p.id == paperId);
                if (paper == null)
                    return null;
                keywords = LoadPaperKeywords(paper);
            }

            return keywords;
        }

        private static List<Word> LoadPaperKeywords(paper paper)
        {
            var splitChars = new [] { ',', ' ', ';', '.', '!', '?', '"' };
            var keywords = new List<Word>();
            var stopWords = LoadStopWords();
            if (!String.IsNullOrEmpty(paper.title))
            {
                var titleKeywords = paper.title.Trim(new [] { '"' }).Split(splitChars).ToList();
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
            if (!String.IsNullOrEmpty(paper.keyword))
            {
                var paperKeywords = paper.keyword.Trim(new [] { '"' }).Split(splitChars);
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

            const string filePath = @"..\..\..\data\english_stop.txt";
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
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
