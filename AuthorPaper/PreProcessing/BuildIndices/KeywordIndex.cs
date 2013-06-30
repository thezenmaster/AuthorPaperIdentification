using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AuthorPaper;

namespace PreProcessing.BuildIndices
{
    public class KeywordIndex
    {
        public const string KeywordPath = "keywordindex.txt";
        public static void BuildIndex()
        {
            // generate index
            GenerateIndex();
            // store index
            StoreIndex(KeywordPath);
        }

        public static void GenerateIndex()
        {
            // need to generate list of keywords ordered by value
            // get all keywords from db, order them
            // foreach keyword
            Console.WriteLine("start gen index");
            var keywordVectors = new SortedList<string, KeywordVector>();
            var index = 0;
            foreach (var keyword in BigStorage.Keywords)
            {
                var keywordVector = GenerateKeywordVector(keyword);
                if (keywordVectors.ContainsKey(keywordVector.Value))
                {
                    keywordVector.Value += "1";
                }
                keywordVectors.Add(keywordVector.Value, keywordVector);

                index++;
                if (index%1000 == 0)
                {
                    Console.WriteLine("generating for keyword # " + index);
                }
            }
            BigStorage.KeywordIndex = keywordVectors;
            Console.WriteLine("end gen index");
        }

        // todo: new lines should be removed from keywords on insert
        public static string RemoveNewLines(string value)
        {
            if(string.IsNullOrEmpty(value)) return "";
            var result = value.Replace("\t", "");
            if (value.Contains('\r') || value.Contains('\n'))
                return result.Replace('\r', ' ').Replace('\n', ' ') + '1';
            return result;
        }
        
        private static KeywordVector GenerateKeywordVector(Keyword keyword)
        {
            var keywordVector = new KeywordVector
                {
                    KeywordId = keyword.KeywordId,
                    Value = RemoveNewLines(keyword.Value),
                    InvertedPaperFrequency = VectorParameters.CalculateInvertedPaperFreq(keyword),
                    PaperKeywordFrequencies = new SortedList<long, double>()
                };
            // calculate idf/or other
            
            // foreach paper in keyword papers
            foreach (var paperKeyword in keyword.PaperKeywords.GroupBy(p => p.PaperId).OrderBy(x => x.Key))
            {
                // calculate tf/or other
                var firstkeyword = paperKeyword.First();
                var maxCount = BigStorage.Papers[firstkeyword.PaperId].PaperKeywords.Max(m => m.Count);
                var tf = VectorParameters.CalculateKeywordFrequency(keyword.KeywordId,
                    firstkeyword.Count.HasValue ? firstkeyword.Count.Value : 1,
                    maxCount);
                keywordVector.PaperKeywordFrequencies.Add(firstkeyword.PaperId, tf);
            }

            return keywordVector;
        }

        public static void StoreIndex(string path)
        {
            Console.WriteLine("start storing index");
            // open file
            var stringBuilder = new StringBuilder();
            using (var sw = new StreamWriter(!File.Exists(path) ? File.Open(path, FileMode.Create) : File.Open(path, FileMode.Append)))
            {
                // write index (ordered by value)
                foreach (var keywordVector in BigStorage.KeywordIndex)
                {
                    // index format:
                    // value, keywordid, idf(or other params for keywordindex);paperid,tf(or other);paperid,tf;
                    stringBuilder.AppendFormat("\"{0}\",{1},{2};", keywordVector.Key, keywordVector.Value.KeywordId, 
                        keywordVector.Value.InvertedPaperFrequency);
                    foreach (var paperKeywordFrequency in keywordVector.Value.PaperKeywordFrequencies)
                    {
                        stringBuilder.AppendFormat("{0},{1};", paperKeywordFrequency.Key, paperKeywordFrequency.Value);
                    }
                    sw.WriteLine(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
                // close file
            }
            Console.WriteLine("end storing index");
        }

        public static void GetIndex(bool loadFromFile = true, string path = KeywordPath)
        {
            if (loadFromFile)
            {
                var keywordVectors = new SortedList<string, KeywordVector>();
                
                using (var streamReader = new StreamReader(File.OpenRead(path)))
                {
                    var line = streamReader.ReadLine();
                    do
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        var items = line.Split(';');
                        // value, keywordid, idf(or other params for keywordindex);paperid,tf(or other);paperid,tf;
                        var keywordInfo = items[0].Split(',');
                        long keywordId;
                        double idf;
                        if (keywordInfo.Length == 3 && Int64.TryParse(keywordInfo[1], out keywordId)
                            && Double.TryParse(keywordInfo[2], out idf))
                        {
                            var keywordVector = new KeywordVector
                                {
                                    Value = keywordInfo[0].Trim('"'),
                                    KeywordId = keywordId,
                                    InvertedPaperFrequency = idf,
                                    PaperKeywordFrequencies = new SortedList<long, double>()
                                };
                            for (var i = 1; i < items.Length; i++)
                            {
                                var matchedItems = items[i].Split(',');
                                long matchedPaperId;
                                double matchedTf;
                                if (!Int64.TryParse(matchedItems[0], out matchedPaperId) ||
                                    !Double.TryParse(matchedItems[1], out matchedTf)) continue;

                                keywordVector.PaperKeywordFrequencies.Add(matchedPaperId, matchedTf);
                            }
                            keywordVectors.Add(keywordVector.Value, keywordVector);
                        }
                        line = streamReader.ReadLine();
                    } while (!string.IsNullOrEmpty(line));
                }
                BigStorage.KeywordIndex = keywordVectors;
            }
            else
            {
                // or generate in memory
                GenerateIndex();
            }
        }
    }
}
