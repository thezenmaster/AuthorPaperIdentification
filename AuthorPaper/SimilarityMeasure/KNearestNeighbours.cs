using AuthorPaper;
using PreProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimilarityMeasure
{
    public class KNearestNeighbours
    {
        public static List<PaperVector> FindKNearestPapers(Paper paper, int nearestK, int trainCount)
        {
            var paperIndex = LoadPaperKeywords(paper);
            NormalizeInputPaperIndex(paperIndex);
            var nearestPapers = new List<PaperVector>();
            using (var context = new AuthorPaperEntities())
            {
                // todo: make sure child entity collections are loaded
                var trainSet = context.ValidPapers.OrderBy(v => v.PaperId).Take(trainCount);
                foreach (var paperItem in trainSet)
                {
                    var paperItemKeywords = context.PaperKeywords.Where(k => k.PaperId == paperItem.PaperId).ToList();
                    double scalarProduct = 0.0;
                    double inputVectorNorm = 0.0;
                    double trainSetItemNorm = 0.0;
                    foreach (var word in paperIndex)
                    {
                        var wordNormalizedCount = word.NormalizedCount;
                        inputVectorNorm += wordNormalizedCount * wordNormalizedCount;
                        var keywordInDb = paperItemKeywords.FirstOrDefault(k => k.Keyword.Value == word.Value);
                            //paperItem.PaperKeywords.SingleOrDefault(k => k.Keyword.Value == word.Value);
                        if (keywordInDb != null && keywordInDb.NormalizedCount.HasValue)
                        {
                            scalarProduct += wordNormalizedCount * keywordInDb.NormalizedCount.Value;
                        }
                    }

                    //It only makes sense to calc cosine if scalar product is > 0
                    if (scalarProduct > 0)
                    {
                        trainSetItemNorm = CalculateNorm(paperItemKeywords, context);
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

        private static double CalculateNorm(List<PaperKeyword> keywords, AuthorPaperEntities context)
        {
            double norm = 0.0;

            foreach (var item in keywords)
            {
                if (item.NormalizedCount.HasValue)
                {
                    double value = item.NormalizedCount.Value;
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
                    var keyword = context.Keywords.FirstOrDefault(k => k.Value == word.Value);
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

        private static List<Word> LoadPaperKeywords(Paper paper)
        {
            var splitChars = new [] { ',', ' ', ';', '.', '!', '?', '"' };
            var keywords = GenerateKeywords.GeneratePaperKeywords(paper);
            
            return keywords.ToList();
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
