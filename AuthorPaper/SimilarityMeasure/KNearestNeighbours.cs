using System.Threading.Tasks;
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
        public static List<ValidPaper> TestPapers = new List<ValidPaper>();
        public static List<ValidPaper> TrainPapers = new List<ValidPaper>();
        public static Dictionary<long, double> TrainPaperNorm = new Dictionary<long, double>(); 
        public static List<PaperKeyword> PaperKeywords = new List<PaperKeyword>();
        public static List<Keyword> Keywords = new List<Keyword>();

        public const int MaxDegreeOfParallelization = 4;

        public static double GetScalarProduct(List<Word> paperIndex, List<PaperKeyword> paperItemKeywords)
        {
            var scalarProduct = 0.0;

            foreach (var word in paperIndex)
            {
                var wordNormalizedCount = word.NormalizedCount;
                var keywordInDb = paperItemKeywords.FirstOrDefault(k => k.Keyword.Value == word.Value);
                //paperItem.PaperKeywords.SingleOrDefault(k => k.Keyword.Value == word.Value);
                if (keywordInDb != null && keywordInDb.NormalizedCount.HasValue)
                {
                    scalarProduct += wordNormalizedCount * keywordInDb.NormalizedCount.Value;
                }
            }
            return scalarProduct;
        }

        public static List<PaperVector> FindKNearestPapersParallel(Paper paper, int nearestK)
        {
            var batchCount = TrainPapers.Count/MaxDegreeOfParallelization;
            var tasks = new List<Task<List<PaperVector>>>();
            for (var i = 0; i < MaxDegreeOfParallelization; i++)
            {
                var localVar = i;
                tasks.Add(Task.Factory.StartNew(() => FindKNearestPapers(paper, nearestK, localVar * batchCount, batchCount)));
            }

            Task.WaitAll(tasks.ToArray());

            var result = new List<PaperVector>();
            tasks.ForEach(t => 
            {
                if (!t.IsFaulted) result.AddRange(t.Result);
            });
            return GetTopDistinctValues(result, nearestK);
        } 

        public static List<PaperVector> GetTopDistinctValues(List<PaperVector> list, int top)
        {
            var result = list.GroupBy(g => g.Paper.PaperId).Select(p => p.First()).OrderByDescending(r => r.Similarity);
            if (result.Count() >= top) return result.Take(top).ToList();
            return result.ToList();
        } 

        public static List<PaperVector> FindKNearestPapers(Paper paper, int nearestK, int startPos, int batchCount)
        {
            var paperIndex = LoadPaperKeywords(paper);
            NormalizeInputPaperIndex(paperIndex);
            var nearestPapers = new List<PaperVector>();
            double inputVectorNorm = 0.0;
            foreach (var word in paperIndex)
            {
                var wordNormalizedCount = word.NormalizedCount;
                inputVectorNorm += wordNormalizedCount * wordNormalizedCount;
            }

            foreach (var paperItem in TrainPapers.Skip(startPos).Take(batchCount))
            {
                var localPaperItem = paperItem;
                var paperItemKeywords = PaperKeywords.Where(k => k.PaperId == localPaperItem.PaperId).ToList();
                var scalarProduct = GetScalarProduct(paperIndex, paperItemKeywords);

                //It only makes sense to calc cosine if scalar product is > 0
                if (scalarProduct > 0)
                {
                    var trainSetItemNorm = CalculateNorm(paperItem.PaperId.Value, paperItemKeywords);
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
            return nearestPapers;
        }

        private static double CalculateNorm(long paperid, List<PaperKeyword> keywords)
        {
            double result = 0.0;
            lock (TrainPaperNorm)
            {
                if (TrainPaperNorm.ContainsKey(paperid)) return TrainPaperNorm[paperid];
            }
            double norm = 0.0;

            foreach (var item in keywords)
            {
                if (item.NormalizedCount.HasValue)
                {
                    double value = item.NormalizedCount.Value;
                    norm += value * value;
                }
            }
            result = Math.Sqrt(norm);
            lock (TrainPaperNorm)
            {
                if (!TrainPaperNorm.ContainsKey(paperid))
                    TrainPaperNorm.Add(paperid, result);
            }
            return result;
        }

        public static void NormalizeInputPaperIndex(List<Word> index)
        {
           // using (var context = new AuthorPaperEntities())
            {
                foreach (var word in index)
                {
                    var keyword = Keywords.FirstOrDefault(k => k.Value == word.Value);
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
