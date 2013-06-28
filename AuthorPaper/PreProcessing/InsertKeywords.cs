using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthorPaper;

namespace PreProcessing
{
    public class InsertKeywords
    {
        private const int BatchSize = 500;
        private const int MaxDegreeOfParallelization = 4;

        public static void WriteFile(string file, StringBuilder storage)
        {
            using (var outfile = new StreamWriter(file + ".sql", true))
            {
                outfile.Write(storage.ToString());
            }
            storage.Clear();
        }

        public static string GetEscapedString(string value)
        {
            return value != null ? value.Replace("'", "''") : string.Empty;
        }

        public static void InsertScript(int iteration, Paper paper)
        {
            var builder = new StringBuilder();
            var keywords = GenerateKeywords.GeneratePaperKeywords(paper);
            foreach (var keyword in keywords)
            {
                builder.AppendLine(
                    string.Format("INSERT INTO keyword(count, value) VALUES ({0}, '{1}');", keyword.Count, GetEscapedString(keyword.Value)));
            }
            WriteFile("keywords" + iteration, builder);
        }

        public static void InsertKeywordsForPaper(AuthorPaperEntities context, Paper paper)
        {
            var keywords = GenerateKeywords.GeneratePaperKeywords(paper);
            foreach (var keyword in keywords)
            {
                // some keywords will duplicate
                //var contextKeyword = context.Keywords.FirstOrDefault(k => k.Value == keyword.Item1);
                
                //if (contextKeyword != null)
                //{
                //    contextKeyword.Count += keyword.Item2;
                //}
                //else
                //{
                //    contextKeyword = new Keyword
                //        {
                //            Value = keyword.Item1,
                //            Count = keyword.Item2
                //        };
                //    context.AddToKeywords(contextKeyword);
                //}

                // duplicate records will be inserted, they have to be grouped afterwards
                var contextKeyword = new Keyword
                        {
                            Value = keyword.Value,
                            Count = keyword.Count
                        };
                context.AddToKeywords(contextKeyword);
                var paperKeyword = new PaperKeyword
                {
                    Count = keyword.Count,
                    Paper = paper,
                    Keyword = contextKeyword
                };
                context.AddToPaperKeywords(paperKeyword);
            }
        }

        public static void RunParallelInserts()
        {
            var context = new AuthorPaperEntities();
            var totalCount = (int)Math.Floor(context.ValidPapers.Count() * 0.9);
            var totalIterations = totalCount/BatchSize;

            for (var outerIteration = 0; outerIteration <= totalIterations / MaxDegreeOfParallelization; outerIteration++)
            {
                var tasks = new List<Task>();
                for (var iteration = 0; iteration < MaxDegreeOfParallelization; iteration++)
                {
                    var currentIteration = iteration + MaxDegreeOfParallelization * outerIteration;
                    if (currentIteration > totalIterations) break;
                    tasks.Add(Task.Factory.StartNew(() => InsertKeywordsForPapers(currentIteration*BatchSize, totalCount)));
                }
                Task.WaitAll(tasks.ToArray());
            }
        }

        public static void InsertKeywordsForPapers(int skipItems, int totalCount)
        {
            //var context = new AuthorPaperEntities();
            //var iteration = 0;
            //var totalCount = context.papers.Count();

           // while (totalCount > iteration * BatchSize)
           // {
                using (var context2 = new AuthorPaperEntities())
                {
                    //var skipItems = iteration*BatchSize;
                    var paperList = skipItems > 0
                                        ? context2.ValidPapers.Include("Paper")
                                                    .OrderBy(p => p.PaperId)
                                                    .Skip(skipItems)
                                                    .Take((totalCount - skipItems - BatchSize < 0)
                                                            ? (totalCount - skipItems) // last item in the list
                                                            : BatchSize)
                                        : context2.ValidPapers.Include("Paper").OrderBy(p => p.PaperId).Take(BatchSize);

                    foreach (var paper in paperList)
                    {
                        //InsertScript(skipItems/BatchSize, paper);
                        InsertKeywordsForPaper(context2, paper.paper);
                    }

                    context2.SaveChanges();
                }
                
                Console.WriteLine("saved iteration " + skipItems /BatchSize + DateTime.Now);
             //   iteration++;
           // }
        }

        public static void RemoveDuplicatingKeywords()
        {
            int total;
            var iteration = 0;
            const int batchsize = 100;

            do
            {
                var context = new AuthorPaperEntities();

                var duplicateKeywordsGroup = context.Keywords.Include("paperkeyword").GroupBy(g => g.Value)
                                                    .Where(g => g.Count() > 1)
                                                    .OrderBy(g => g.Key);
                total = duplicateKeywordsGroup.Count();

                var duplicateKeywords = duplicateKeywordsGroup.Take((total < batchsize)
                                                                        ? total
                                                                        : batchsize).ToList();

                foreach (var duplicateKeyword in duplicateKeywords)
                {
                    var totalCount = duplicateKeyword.Sum(dk => dk.Count);
                    var firstKeyword = duplicateKeyword.First();
                    firstKeyword.Count = totalCount;

                    foreach (
                        var keyword in duplicateKeyword.Where(keyword => keyword.KeywordId != firstKeyword.KeywordId))
                    {
                        foreach (var paperkeyword in keyword.PaperKeywords)
                        {
                            context.AddToPaperKeywords(new PaperKeyword
                                {
                                    PaperId = paperkeyword.PaperId,
                                    KeywordId = firstKeyword.KeywordId,
                                    Count = paperkeyword.Count
                                });
                            // fk on delete cascade
                            //context.DeleteObject(paperkeyword);
                        }
                        context.DeleteObject(keyword);
                    }
                }

                context.SaveChanges();
                Console.WriteLine("saved iteration " + iteration + DateTime.Now);
                iteration++;
               
            } while (total > 0);
        }
    }
}
