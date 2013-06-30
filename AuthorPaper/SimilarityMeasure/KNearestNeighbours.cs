using System.Text;
using System.Threading.Tasks;
using AuthorPaper;
using PreProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PreProcessing.Keywords;

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

        private static bool StoreOutput(List<PaperSimilarity> matches, Paper originalPaper, string path)
        {
            using (var sw = new StreamWriter(!File.Exists(path) ? File.Open(path, FileMode.Create) : File.Open(path, FileMode.Append)))
            {
                var builder = new StringBuilder();
                builder.AppendFormat("{0};", originalPaper.Id);
                foreach (var paperVector in matches)
                {
                    builder.AppendFormat("{0},{1};", paperVector.PaperId, paperVector.Similarity);
                }
                sw.WriteLine(builder.ToString());
                // bool match = false;
                //using (var context = new AuthorPaperEntities())
                //{
                //    var originalAuthorId = context.PaperAuthors.FirstOrDefault(x => x.PaperId == originalPaper.Id);
                //    sw.WriteLine("The real author has ID of: {0}", originalAuthorId.AuthorId);
                //    System.Console.WriteLine("The real author has ID of: {0}", originalAuthorId.AuthorId);
                //    foreach (var item in matches)
                //    {
                //        sw.WriteLine("Paper ID: {0}", item.Paper.PaperId);
                //        System.Console.WriteLine("Paper ID: {0}. ", item.Paper.PaperId);
                //        var authorId = context.PaperAuthors.FirstOrDefault(x => x.PaperId == item.Paper.PaperId);
                //        if (authorId != null && authorId.AuthorId != null)
                //        {
                //            var author = context.Authors.FirstOrDefault(a => a.Id == authorId.AuthorId);
                //            if (author != null && author.Name != null)
                //            {
                //                sw.Write("Author Name={0}; Author Id={1}", author.Name, author.Id);
                //                sw.WriteLine();
                //                System.Console.WriteLine(author.Id);
                //                System.Console.WriteLine(author.Name);

                //                if (author.Id == originalAuthorId.AuthorId)
                //                    match = true;
                //            }
                //        }
                //    }
                //}
                //sw.WriteLine("-----------------------------------------------");
                //sw.WriteLine();
                // System.Console.WriteLine("-----------------------------------------------");
                // System.Console.WriteLine();
            }
            return true;
        }

        private static void GetLasNPercent(int percent)
        {
            string path = @"..\..\..\..\result" + DateTime.Now.ToFileTime() + ".txt";

            using (var context = new AuthorPaperEntities())
            {
                var paperCount = context.ValidPapers.Count() / 10;
                var testPapersCount = (int)Math.Floor(paperCount * 0.1);
                var trainCount = paperCount - testPapersCount;
                System.Console.WriteLine("started loading papers in memory " + DateTime.Now);
                var allPapers = context.ValidPapers.Include("Paper");
                TestPapers =
                    allPapers.OrderByDescending(v => v.PaperId).Take(testPapersCount)
                             .ToList();
                TrainPapers = allPapers.OrderBy(p => p.PaperId)
                    .Take(trainCount)
                    .ToList();
                PaperKeywords = context.PaperKeywords.ToList();
                Keywords = context.Keywords.ToList();
                var matchedCount = 0;
                System.Console.WriteLine("number of all " + paperCount);
                System.Console.WriteLine("number of train set " + trainCount);
                System.Console.WriteLine("number of test set " + testPapersCount);
                System.Console.WriteLine("loaded papers in memory " + DateTime.Now);
                var index = 0;
                foreach (var validPaper in TestPapers)
                {
                    var paper = validPaper.paper;
                    if (paper != null)
                    {
                        var matches = FindKNearestPapersParallel(paper, 5);
                        //var matches = KNearestNeighbours.FindKNearestPapers(paper, 5, trainCount);
                        if (StoreOutput(matches, paper, path))
                            matchedCount++;
                        System.Console.WriteLine("matched paper " + index + " " + DateTime.Now);
                    }
                    index++;
                }
                //sw.WriteLine("Match percentage: {0}", matchedCount / testPapersCount);
                //System.Console.WriteLine("Match percentage: {0}", matchedCount / testPapersCount);
            }
        }

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

        public static List<PaperSimilarity> FindKNearestPapersParallel(Paper paper, int nearestK)
        {
            var batchCount = TrainPapers.Count/MaxDegreeOfParallelization;
            var tasks = new List<Task<List<PaperSimilarity>>>();
            for (var i = 0; i < MaxDegreeOfParallelization; i++)
            {
                var localVar = i;
                tasks.Add(Task.Factory.StartNew(() => FindKNearestPapers(paper, nearestK, localVar * batchCount, batchCount)));
            }

            Task.WaitAll(tasks.ToArray());

            var result = new List<PaperSimilarity>();
            tasks.ForEach(t => 
            {
                if (!t.IsFaulted) result.AddRange(t.Result);
            });
            return GetTopDistinctValues(result, nearestK);
        } 

        public static List<PaperSimilarity> GetTopDistinctValues(List<PaperSimilarity> list, int top)
        {
            var result = list.GroupBy(g => g.PaperId).Select(p => p.First()).OrderByDescending(r => r.Similarity);
            if (result.Count() >= top) return result.Take(top).ToList();
            return result.ToList();
        } 

        public static List<PaperSimilarity> FindKNearestPapers(Paper paper, int nearestK, int startPos, int batchCount)
        {
            var paperIndex = LoadPaperKeywords(paper);
            NormalizeInputPaperIndex(paperIndex);
            var nearestPapers = new List<PaperSimilarity>();
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
                            nearestPapers.Add(new PaperSimilarity { PaperId = paperItem.PaperId.Value, Similarity = cosine });
                        }
                        else
                        {
                            nearestPapers = nearestPapers.OrderByDescending(p => p.Similarity).ToList();
                            if (nearestPapers.Last().Similarity < cosine)
                            {
                                nearestPapers.Remove(nearestPapers.Last());
                                nearestPapers.Add(new PaperSimilarity { PaperId = paperItem.PaperId.Value, Similarity = cosine });
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
