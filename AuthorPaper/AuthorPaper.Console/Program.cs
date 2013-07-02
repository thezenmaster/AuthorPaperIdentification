using System;
using AuthorPaper.Console.IO;
using PreProcessing.BuildIndices;

namespace AuthorPaper.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var startTime = DateTime.Now;
            System.Console.WriteLine("start building index " + startTime);

            //KeywordIndex.BuildIndex();
            //KeywordIndex.GetIndex();

            //PaperIndex.BuildIndex();
            //PaperIndex.GetIndex();
            //Classifier.Classifier.ClassifyTestPapers();
            //ParsePaperOutput.WritePaperOutputResults("paperauthors_20130702.txt");
            ParsePaperOutput.WritePaperAuthors("paperauthors_20130702.txt", "paperauthors_withauthors_20130702_v2.txt");

            var endTime = DateTime.Now;
            System.Console.WriteLine("end building index " + endTime);
            System.Console.WriteLine("it took " + endTime.Subtract(startTime));
            System.Console.ReadLine();
        }
    }
}

