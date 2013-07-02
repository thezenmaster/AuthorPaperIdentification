using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AuthorPaper.Console.Classifier;
using PreProcessing.BuildIndices;
using PreProcessing.IO;

namespace AuthorPaper.Console.IO
{
    public class ParsePaperOutput
    {
        private static List<PaperOutput> ParsePaperResultFile(string path)
        {
            var list = new List<PaperOutput>();

            using (var streamReader = new StreamReader(File.OpenRead(path)))
            {
                var line = streamReader.ReadLine();
                do
                {
                    if(string.IsNullOrEmpty(line)) continue;
                    var items = line.Split(';');
                    long paperId = -1;
                    if (Int64.TryParse(items[0], out paperId))
                    {
                        var newPaperOutput = new PaperOutput
                            {
                                PaperId = paperId,
                                MatchedPapers = new List<MatchedPaperOutput>()
                            };
                        for (var i = 1; i < items.Length; i++)
                        {
                            var matchedItems = items[i].Split(',');
                            long matchedPaperId = -1;
                            double matchedPaperSimiliraty = -1;
                            if (!Int64.TryParse(matchedItems[0], out matchedPaperId) ||
                                !Double.TryParse(matchedItems[1], out matchedPaperSimiliraty)) continue;
                            var matchedPaper = new MatchedPaperOutput{ PaperId = matchedPaperId, Similarity = matchedPaperSimiliraty};
                            newPaperOutput.MatchedPapers.Add(matchedPaper);
                        }
                        list.Add(newPaperOutput);
                    }
                    line = streamReader.ReadLine();
                } while (!string.IsNullOrEmpty(line));
            }
            return list;
        }

        private static void WriteAuthorResultFile(string path, IEnumerable<PaperOutput> paperOutputs)
        {
            //var matchedFirst = 0;
            //var matchedAny = 0;
            var matchedMostCommon = 0;
            var matchedArray = new [] {0,0,0,0,0}; //k = 5
            var totalRows = 0;
            using (var sw = new StreamWriter(!File.Exists(path) ? File.Open(path, FileMode.Create) : File.Open(path, FileMode.Append)))
            {
                var builder = new StringBuilder();
                foreach (var paperVector in paperOutputs)
                {
                    //var firstMatch = paperVector.MatchedPapers.OrderByDescending(r => r.Similarity).FirstOrDefault();
                    //var isMatchedFirst = (firstMatch != null && paperVector.AuthorId == firstMatch.AuthorId);
                    //if (isMatchedFirst) matchedFirst++;
                    //var isMatchedAny = (firstMatch != null &&
                    //                    paperVector.MatchedPapers.Any(mp => mp.AuthorId == paperVector.AuthorId));
                    //if (isMatchedAny) matchedAny++;

                    var matchPosition = -1;
                    var index = 0;
                    foreach (var matchedPaperOutput in paperVector.MatchedPapers.OrderByDescending(r => r.Similarity))
                    {
                        if (matchedPaperOutput.AuthorId == paperVector.AuthorId)
                        {
                            matchedArray[index]++;
                            matchPosition = index;
                            break;
                        }
                        index++;
                    }

                    var matchMostCommon = paperVector.MatchedPapers.GroupBy(b => b.AuthorId)
                                                       .OrderByDescending(m => m.Count())
                                                       .ThenByDescending(r => r.Max(p => p.Similarity)).FirstOrDefault();
                    var isMatchMostCommon = false;
                    if (matchMostCommon != null)
                        isMatchMostCommon = matchMostCommon.Any()
                                            && paperVector.AuthorId == matchMostCommon.First().AuthorId;
                    if (isMatchMostCommon) matchedMostCommon++;
                    builder.AppendFormat("{0},{1},{2},{3};", paperVector.PaperId, paperVector.AuthorId,
                        matchPosition,
                        isMatchMostCommon ? 1 : 0);
                    foreach (var match in paperVector.MatchedPapers)
                    {
                        builder.AppendFormat("{0},{1};", match.PaperId, match.AuthorId);
                    }
                    builder.AppendLine();
                    totalRows++;
                }
                sw.WriteLine(builder.ToString());
                for(var i=0; i< matchedArray.Length; i++)
                {
                    sw.WriteLine("Matched at position i " + i + " papers: " + matchedArray[i]);
                }
                //sw.WriteLine("Matched first: " + matchedFirst);
                //sw.WriteLine("Matched any: " + matchedAny);
                sw.WriteLine("Matched most common: " + matchedMostCommon);
                sw.WriteLine("Total: " + totalRows);
            }
        }

        public static void WritePaperAuthors(string srcPath, string dstPath)
        {
            //const string srcpath = @"..\..\..\..\result130169398164206210.txt";
            //const string dstpath = @"..\..\..\..\resultauthors.txt";
            var paperOutput = ParsePaperResultFile(srcPath);
            PaperAuthors.GetPaperAuthors(paperOutput);
            WriteAuthorResultFile(dstPath, paperOutput);
        }

        public static void WritePaperOutputResults(string path)
        {
            using (var sw = new StreamWriter(!File.Exists(path)
                                         ? File.Open(path, FileMode.Create)
                                         : File.Open(path, FileMode.Append)))
            {
                var builder = new StringBuilder();
                foreach (var testPaperResult in BigStorage.TestPaperResults)
                {
                    builder.AppendFormat("{0};", testPaperResult.Value.PaperId);
                    foreach (var paperVector in testPaperResult.Value.MatchedPapers)
                    {
                        builder.AppendFormat("{0},{1};", paperVector.PaperId, paperVector.Similarity);
                    }
                    sw.WriteLine(builder.ToString());
                    builder.Clear();
                }
            }
        }
    }
}
