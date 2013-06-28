using System;
using System.Linq;
using PreProcessing;
using SimilarityMeasure;


namespace AuthorPaper.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var matches = KNearestNeighbours.FindKNearestPapers(255, 10);
            foreach (var item in matches)
            {
                System.Console.WriteLine(item.Paper.Id);
                using (var context = new AuthorPaperEntities())
                {
                    var authorId = context.PaperAuthors.FirstOrDefault(x => x.PaperId == item.Paper.Id);
                    if (authorId != null && authorId.AuthorId != null)
                    {
                        var author = context.Authors.FirstOrDefault(a => a.Id == authorId.AuthorId);
                        if (author != null && author.Name != null)
                        {
                            System.Console.WriteLine(author.Id);
                            System.Console.WriteLine(author.Name);
                        }
                    }
                }

            }
            System.Console.ReadLine();
            //RemoveDuplicates();
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
