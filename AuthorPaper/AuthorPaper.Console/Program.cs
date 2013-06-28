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
            var matches = KNearestNeighbours.FindKNearestPapers(2507, 10);
            foreach (var item in matches)
            {
                System.Console.WriteLine(item.Paper.Id);
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
