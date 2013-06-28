using System;
using System.Linq;
using PreProcessing;

namespace AuthorPaper.Console
{
    class Program
    {
        static void Main(string[] args)
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
