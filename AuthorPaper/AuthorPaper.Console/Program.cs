using System;
using System.Linq;
using PreProcessing;
using SimilarityMeasure;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AuthorPaper.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test with 10 percent
            GetLasNPercent(10);
            System.Console.ReadLine();
        }

        private static bool StoreOutput(List<PaperVector> matches, Paper originalPaper, StreamWriter sw)
        {
            bool match = false;
            using (var context = new AuthorPaperEntities())
            {
                var originalAuthorId = context.PaperAuthors.FirstOrDefault(x => x.PaperId == originalPaper.Id);
                sw.WriteLine("The real author has ID of: {0}", originalAuthorId.AuthorId);
                System.Console.WriteLine("The real author has ID of: {0}", originalAuthorId.AuthorId);
                foreach (var item in matches)
                {
                    sw.WriteLine("Paper ID: {0}", item.Paper.PaperId);
                    System.Console.WriteLine("Paper ID: {0}. ", item.Paper.PaperId);
                    var authorId = context.PaperAuthors.FirstOrDefault(x => x.PaperId == item.Paper.PaperId);
                    if (authorId != null && authorId.AuthorId != null)
                    {
                        var author = context.Authors.FirstOrDefault(a => a.Id == authorId.AuthorId);
                        if (author != null && author.Name != null)
                        {
                            sw.Write("Author Name={0}; Author Id={1}", author.Name, author.Id);
                            sw.WriteLine();
                            System.Console.WriteLine(author.Id);
                            System.Console.WriteLine(author.Name);

                            if (author.Id == originalAuthorId.AuthorId)
                                match = true;
                        }
                    }
                }
            }
            sw.WriteLine("-----------------------------------------------");
            sw.WriteLine();
            System.Console.WriteLine("-----------------------------------------------");
            System.Console.WriteLine();
            return match;
        }

        private static void GetLasNPercent(int percent)
        {
            string path = @"..\..\..\..\result" + DateTime.Now.ToFileTime() + ".txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    using (var context = new AuthorPaperEntities())
                    {
                        var testPapersCount = (int)Math.Floor(context.ValidPapers.Count() * 0.1);
                        var trainCount = (int)Math.Floor(context.ValidPapers.Count() * 0.9);
                        var testPapers = context.ValidPapers.OrderByDescending(v => v.PaperId).Take(testPapersCount);
                        int matchedCount = 0;
                        foreach (var validPaper in testPapers)
                        {
                            var paper = context.Papers.FirstOrDefault(p => p.Id == validPaper.PaperId);
                            if (paper != null)
                            {
                                var matches = KNearestNeighbours.FindKNearestPapers(paper, 5, trainCount);
                                if (StoreOutput(matches, paper, sw))
                                    matchedCount++;
                            }
                            else
                            {

                            }
                        }
                        sw.WriteLine("Match percentage: {0}", matchedCount / testPapersCount);
                        System.Console.WriteLine("Match percentage: {0}", matchedCount / testPapersCount);
                    }
                }
            }
        }

        private static void RemoveDuplicates()
        {
            var startTime = DateTime.Now;
            System.Console.WriteLine("start time " + (startTime));

            //InsertKeywords.InsertKeywordsForPapers();
            // InsertKeywords.RunParallelInserts();
            InsertKeywords.RemoveDuplicatingKeywords();
            var endTime = DateTime.Now;

            System.Console.WriteLine("total time " + endTime.Subtract(startTime));
        }
    }
}
