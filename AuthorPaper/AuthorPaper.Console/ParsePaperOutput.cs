using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuthorPaper.Console
{
    public class PaperOutput
    {
        public long PaperId { get; set; }
        public long AuthorId { get; set; }
        public List<MatchedPaperOutput> MatchedPapers { get; set; } 
    }
    public class MatchedPaperOutput
    {
        public long PaperId { get; set; }
        public long AuthorId { get; set; }
        public double Similarity { get; set; }
    }
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

        private static void WriteAuthorResultFile(string path, List<PaperOutput> paperOutputs)
        {
            var matchedFirst = 0;
            var matchedAny = 0;
            var matchedMostCommon = 0;
            var totalRows = 0;
            using (var sw = new StreamWriter(!File.Exists(path) ? File.Open(path, FileMode.Create) : File.Open(path, FileMode.Append)))
            {
                var builder = new StringBuilder();
                foreach (var paperVector in paperOutputs)
                {
                    var firstMatch = paperVector.MatchedPapers.OrderByDescending(r => r.Similarity).FirstOrDefault();
                    var isMatchedFirst = (firstMatch != null && paperVector.AuthorId == firstMatch.AuthorId);
                    if (isMatchedFirst) matchedFirst++;
                    var isMatchedAny = (firstMatch != null &&
                                        paperVector.MatchedPapers.Any(mp => mp.AuthorId == paperVector.AuthorId));
                    if (isMatchedAny) matchedAny++;
                    var matchMostCommon = paperVector.MatchedPapers.GroupBy(b => b.AuthorId)
                                                       .OrderByDescending(m => m.Count()).FirstOrDefault();
                    var isMatchMostCommon = false;
                    if (matchMostCommon != null)
                        isMatchMostCommon = matchMostCommon.Any()
                                            && paperVector.AuthorId == matchMostCommon.First().AuthorId;
                    if (isMatchMostCommon) matchedMostCommon++;
                    builder.AppendFormat("{0},{1},{2},{3},{4};", paperVector.PaperId, paperVector.AuthorId,
                        isMatchedFirst ? 1 : 0,
                        isMatchedAny ? 1 : 0,
                        isMatchMostCommon ? 1 : 0);
                    foreach (var match in paperVector.MatchedPapers)
                    {
                        builder.AppendFormat("{0},{1};", match.PaperId, match.AuthorId);
                    }
                    builder.AppendLine();
                    totalRows++;
                }
                sw.WriteLine(builder.ToString());
                sw.WriteLine("Matched first: " + matchedFirst);
                sw.WriteLine("Matched any: " + matchedAny);
                sw.WriteLine("Matched most common: " + matchedMostCommon);
                sw.WriteLine("Total: " + totalRows);
            }
        }

        public static void GetPaperAuthors()
        {
            const string srcpath = @"..\..\..\..\result130169398164206210.txt";
            const string dstpath = @"..\..\..\..\resultauthors.txt";
            using (var context = new AuthorPaperEntities())
            {
                var validPapers = context.ValidPapers.ToList();

                var paperOutput = ParsePaperResultFile(srcpath);
                foreach (var output in paperOutput)
                {
                    var validPaper = validPapers.FirstOrDefault(p => p.PaperId == output.PaperId);
                    if (validPaper != null)
                    {
                        output.AuthorId = validPaper.AuthorId.HasValue ? validPaper.AuthorId.Value : -1;
                    }
                    foreach (var matched in output.MatchedPapers)
                    {
                        var validMatchedPaper = validPapers.FirstOrDefault(p => p.PaperId == matched.PaperId);
                        if (validMatchedPaper != null)
                        {
                            matched.AuthorId = validMatchedPaper.AuthorId.HasValue ? validMatchedPaper.AuthorId.Value : -1;
                        }
                    }
                }
                WriteAuthorResultFile(dstpath, paperOutput);
            }
        }
    }
}
