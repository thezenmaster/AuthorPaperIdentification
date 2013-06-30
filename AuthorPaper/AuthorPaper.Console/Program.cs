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
            Classifier.Classifier.ClassifyTestPapers();
            ParsePaperOutput.WritePaperOutputResults("paperauthors_fulllist.txt");
            //ParsePaperOutput.WritePaperAuthors("paperauthors_20130630.txt", "paperauthors_withauthors_20130630.txt");

            var endTime = DateTime.Now;
            System.Console.WriteLine("end building index " + endTime);
            System.Console.WriteLine("it took " + endTime.Subtract(startTime));
            System.Console.ReadLine();
        }
    }
}

