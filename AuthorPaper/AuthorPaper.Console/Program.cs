using System;
using System.Linq;
using System.Text;
using PreProcessing;
using SimilarityMeasure;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AuthorPaper.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Test with 10 percent
            GetLasNPercent(10);
            System.Console.ReadLine();
        }

        private static bool StoreOutput(List<PaperVector> matches, Paper originalPaper, string path)
        {
            using (var sw = new StreamWriter(!File.Exists(path) ? File.Open(path, FileMode.Create) : File.Open(path, FileMode.Append)))
                {
                    var builder = new StringBuilder();
                    builder.AppendFormat("{0};", originalPaper.Id);
                    foreach (var paperVector in matches)
                    {
                        builder.AppendFormat("{0},{1};", paperVector.Paper.PaperId, paperVector.Similarity);
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

        private static void GetLasNPercent (int percent)
        {
                string path = @"..\..\..\..\result" + DateTime.Now.ToFileTime() + ".txt";

                using (var context = new AuthorPaperEntities())
                {
                    var paperCount = context.ValidPapers.Count() / 10;
                    var testPapersCount = (int) Math.Floor(paperCount*0.1);
                    var trainCount = paperCount - testPapersCount;
                    System.Console.WriteLine("started loading papers in memory " + DateTime.Now);
                    var allPapers = context.ValidPapers.Include("Paper");
                    KNearestNeighbours.TestPapers =
                        allPapers.OrderByDescending(v => v.PaperId).Take(testPapersCount)
                                 .ToList();
                    KNearestNeighbours.TrainPapers = allPapers.OrderBy(p => p.PaperId)
                        .Take(trainCount)
                        .ToList();
                    KNearestNeighbours.PaperKeywords = context.PaperKeywords.ToList();
                    KNearestNeighbours.Keywords = context.Keywords.ToList();
                    var matchedCount = 0;
                    System.Console.WriteLine("number of all " + paperCount);
                    System.Console.WriteLine("number of train set " + trainCount);
                    System.Console.WriteLine("number of test set " + testPapersCount);
                    System.Console.WriteLine("loaded papers in memory "+ DateTime.Now);
                    var index = 0;
                    foreach (var validPaper in KNearestNeighbours.TestPapers)
                    {
                        var paper = validPaper.paper;
                        if (paper != null)
                        {
                            var matches = KNearestNeighbours.FindKNearestPapersParallel(paper, 5);
                            //var matches = KNearestNeighbours.FindKNearestPapers(paper, 5, trainCount);
                            if (StoreOutput(matches, paper, path))
                                matchedCount++;
                            System.Console.WriteLine("matched paper "+ index + " " + DateTime.Now);
                        }
                        index++;
                    }
                    //sw.WriteLine("Match percentage: {0}", matchedCount / testPapersCount);
                    //System.Console.WriteLine("Match percentage: {0}", matchedCount / testPapersCount);
                }
            }
        }
}

