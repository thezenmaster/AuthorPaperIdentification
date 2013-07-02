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

            System.Console.WriteLine(SimilarityMeasure.LevenshteinDistance.CalculateDistance("computer", "compute"));
            System.Console.WriteLine(SimilarityMeasure.LevenshteinDistance.CalculateDistance("compile", "decompile"));
            System.Console.WriteLine(SimilarityMeasure.LevenshteinDistance.CalculateDistance("computer", "cAmputer"));
            System.Console.WriteLine(SimilarityMeasure.LevenshteinDistance.CalculateDistance("flomax", "volmax"));

            //KeywordIndex.BuildIndex();
            //KeywordIndex.GetIndex();

            //PaperIndex.BuildIndex();
            //PaperIndex.GetIndex();
            //Classifier.Classifier.ClassifyTestPapers();
            //ParsePaperOutput.WritePaperOutputResults("paperauthors_20130702.txt");
            //ParsePaperOutput.WritePaperAuthors("paperauthors_20130702.txt", "paperauthors_withauthors_20130702_v2.txt");

            var endTime = DateTime.Now;
            System.Console.WriteLine("end building index " + endTime);
            System.Console.WriteLine("it took " + endTime.Subtract(startTime));
            System.Console.ReadLine();
        }
    }
}

