using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AuthorPaper;
using PreProcessing.Keywords;

namespace PreProcessing.BuildIndices
{
    public class PaperVector
    {
        // paperid, similarity term calculation (math.sqrt(sum((tf*idf)^2))); 
        // keywordid, tf*idf(or other params for keywordindex);keywordid, tf*idf;
        public long PaperId { get; set; }
        public double ScalarSquare { get; set; }
        public SortedList<long, double> KeywordWeight { get; set; }
        public SortedList<string, long> KeywordValues { get; set; }
        public SortedList<long, KeywordVector> KeywordVectors { get; set; } 
    }
    public class PaperIndex
    {
        public const string KeywordPath = "paperindex_new.txt";
        public static void BuildIndex()
        {
            // generate index
            GenerateIndex();
            // store index
            StoreIndex(KeywordPath);
            KeywordIndex.StoreIndex("keyword_new.txt");
        }
        
        public static void GenerateIndex()
        {
            Console.WriteLine("start gen index");
            var index = 0;
            var paperVectors = new SortedList<long, PaperVector>();
            foreach (var validPaper in BigStorage.TrainPapers)
            {
               var keywords = GenerateKeywords.GeneratePaperKeywords(validPaper.Value);
               var paperKeywords = new List<SimplePaperKeyword>();
                foreach (var keyword in keywords)
                {
                    KeywordVector keywordIndexItem = null;
                    if(BigStorage.KeywordIndex.ContainsKey(keyword.Value))
                    {
                        keywordIndexItem = BigStorage.KeywordIndex[keyword.Value];
                    }
                    else
                    {
                        // new keyword - generate id and add it to index
                        keywordIndexItem = new KeywordVector
                                               {
                                                   KeywordId = KeywordIndex.GetNextKeywordId(),
                                                   Value = keyword.Value,
                                                   PaperKeywordFrequencies = new SortedList<long, double>(),
                                                   InvertedPaperFrequency = 1.0
                                               };
                        BigStorage.KeywordIndex.Add(keyword.Value, keywordIndexItem);
                    }
                    keywordIndexItem.PaperKeywordFrequencies.Add(validPaper.Value.Id, (int)keyword.Count);
                    // store keyword count
                    paperKeywords.Add(new SimplePaperKeyword
                                          {
                                              KeywordId = keywordIndexItem.KeywordId,
                                              Count = keyword.Count,
                                              Value = keyword.Value,
                                              PaperKeywordId = KeywordIndex.GetNextPaperKeywordId()
                                          });
                }
                validPaper.Value.PaperKeywords = paperKeywords;
                
                var paperVector = GeneratePaperVector(validPaper.Value);
                paperVectors.Add(paperVector.PaperId, paperVector);
                // calculate tf for each paper keyword

                index++;
                if (index % 1000 == 0)
                {
                    Console.WriteLine("generating for keyword # " + index);
                }
            }

            BigStorage.PaperIndex = paperVectors;

            // calculate idf for each keyword
            foreach (var keywordVector in BigStorage.KeywordIndex)
            {
                keywordVector.Value.InvertedPaperFrequency =
                    VectorParameters.CalculateInvertedPaperFreq(keywordVector.Value);
                
                foreach (var paperKeyword in keywordVector.Value.PaperKeywordFrequencies.OrderBy(x => x.Key))
                {
                    // calculate tf/or other
                    var keywords = BigStorage.Papers[paperKeyword.Key].PaperKeywords;
                    var maxCount = keywords != null && keywords.Any() ? keywords.Max(m => m.Count) : 1;
                    var tf = VectorParameters.CalculateKeywordFrequency(paperKeyword.Key,
                        (long)paperKeyword.Value,
                        maxCount);
                    keywordVector.Value.PaperKeywordFrequencies[paperKeyword.Key] = tf;
                }
            }
            // calculate tf*idf + scalar square for each paper
            foreach (var paperVector in BigStorage.PaperIndex)
            {
                GeneratePaperVectorParams(paperVector.Value);
            }
           
            Console.WriteLine("end gen index");
        }

        public static void GenerateIndexFromDb()
        {
            // need to generate list of papers ordered by id
            // get train papers from validpaper ordered by id
            // foreach paper
            Console.WriteLine("start gen index");
            var index = 0;
            var paperVectors = new SortedList<long, PaperVector>();
            foreach (var validPaper in BigStorage.TrainPapers)
            {
                var paperVector = GeneratePaperVector(validPaper.Value);
                paperVectors.Add(paperVector.PaperId, paperVector);
                
                index++;
                if (index % 1000 == 0)
                {
                    Console.WriteLine("generating for keyword # " + index);
                }
            }
            BigStorage.PaperIndex = paperVectors;
            Console.WriteLine("end gen index");
        }

        public static void GeneratePaperVectorParamsForTestPapers(SimplePaper validPaper, PaperVector paperVector)
        {
            // get paper keywords list
            // foreach keyword
            if(validPaper.PaperKeywords == null || !validPaper.PaperKeywords.Any()) return;
            var maxCount = validPaper.PaperKeywords.Max(m => m.Count);
            maxCount = maxCount == 0 ? 1 : maxCount;
            foreach (var paperKeyword in validPaper.PaperKeywords)
            {
                if (string.IsNullOrEmpty(paperKeyword.Value)) continue;
                // calculate tf*idf
                var keywordWeight = VectorParameters.CalculateWeight(paperVector.PaperId, paperKeyword.Value, (double)paperKeyword.Count / maxCount);
                // store in list
                paperVector.KeywordWeight.Add(paperKeyword.KeywordId, keywordWeight);
                paperVector.KeywordValues.Add(paperKeyword.Value, paperKeyword.KeywordId);
            }
            // calculate similarity term (math.sqrt(sum((tf*idf)^2)))
            paperVector.ScalarSquare = VectorParameters.CalculateScalarSquare(paperVector);
        }

        public static void GeneratePaperVectorParams(PaperVector paperVector)
        {
            // get paper keywords list
            // foreach keyword
            if (paperVector.KeywordVectors == null || !paperVector.KeywordVectors.Any()) return;
            foreach (var paperKeyword in paperVector.KeywordVectors)
            {
                // calculate tf*idf
                var keywordWeight = VectorParameters.CalculateWeight(paperVector.PaperId, paperKeyword.Value.Value);
                // store in list
                paperVector.KeywordWeight.Add(paperKeyword.Value.KeywordId, keywordWeight);
                paperVector.KeywordValues.Add(paperKeyword.Value.Value, paperKeyword.Value.KeywordId);
            }
            // calculate similarity term (math.sqrt(sum((tf*idf)^2)))
            paperVector.ScalarSquare = VectorParameters.CalculateScalarSquare(paperVector);
        }

        public static void GeneratePaperVectorParams(SimplePaper validPaper, PaperVector paperVector)
        {
            // get paper keywords list
            // foreach keyword
            foreach (var paperKeyword in validPaper.PaperKeywords.GroupBy(pk => pk.KeywordId).OrderBy(k => k.Key))
            {
                var firstKeyword = paperKeyword.First();
                if (string.IsNullOrEmpty(firstKeyword.Value)) continue;
                // calculate tf*idf
                var keywordWeight = VectorParameters.CalculateWeight(paperVector.PaperId, firstKeyword.Value);
                // store in list
                paperVector.KeywordWeight.Add(firstKeyword.KeywordId, keywordWeight);
                paperVector.KeywordValues.Add(firstKeyword.Value, firstKeyword.KeywordId);
            }
            // calculate similarity term (math.sqrt(sum((tf*idf)^2)))
            paperVector.ScalarSquare = VectorParameters.CalculateScalarSquare(paperVector);
        }

        public static PaperVector GeneratePaperVectorForTestPapers(SimplePaper validPaper)
        {
            var paperVector = new PaperVector
            {
                PaperId = validPaper.Id,
                KeywordWeight = new SortedList<long, double>(),
                KeywordValues = new SortedList<string, long>()
            };

            GeneratePaperVectorParamsForTestPapers(validPaper, paperVector);

            return paperVector;
        }

        public static PaperVector GeneratePaperVector(SimplePaper validPaper)
        {
            var paperVector = new PaperVector
                {
                    PaperId = validPaper.Id,
                    KeywordWeight = new SortedList<long, double>(),
                    KeywordValues = new SortedList<string, long>()
                };

            GeneratePaperVectorParams(validPaper, paperVector);
           
            return paperVector;
        }

        public static void StoreIndex(string path)
        {
            // write index (ordered by paperid)
            Console.WriteLine("start storing index");
            // open file
            var stringBuilder = new StringBuilder();
            using (var sw = new StreamWriter(!File.Exists(path) ? File.Open(path, FileMode.Create) : File.Open(path, FileMode.Append)))
            {
                foreach (var paperdVector in BigStorage.PaperIndex)
                {
                    // index format:
                    // paperid, similarity term calculation (math.sqrt(sum((tf*idf)^2))); 
                    stringBuilder.AppendFormat("{0},{1};", paperdVector.Key, paperdVector.Value.ScalarSquare);
                    // keywordid, tf*idf(or other params for keywordindex);keywordid, tf*idf;
                    foreach (var paperKeywords in paperdVector.Value.KeywordWeight)
                    {
                        stringBuilder.AppendFormat("{0},{1};", paperKeywords.Key, paperKeywords.Value);
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
                var paperVectors = new SortedList<long, PaperVector>();

                using (var streamReader = new StreamReader(File.OpenRead(path)))
                {
                    var line = streamReader.ReadLine();
                    do
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        var items = line.Split(';');
                        // value, keywordid, idf(or other params for keywordindex);paperid,tf(or other);paperid,tf;
                        var paperInfo = items[0].Split(',');
                        long paperId;
                        double scalarSquare;
                        if (paperInfo.Length == 2 && Int64.TryParse(paperInfo[0], out paperId)
                            && Double.TryParse(paperInfo[1], out scalarSquare))
                        {
                            var paperVector = new PaperVector
                            {
                                PaperId = paperId,
                                ScalarSquare = scalarSquare,
                                KeywordWeight = new SortedList<long, double>()
                            };
                            for (var i = 1; i < items.Length; i++)
                            {
                                var matchedItems = items[i].Split(',');
                                long matchedKeywordId;
                                double matchedWeight;
                                if (!Int64.TryParse(matchedItems[0], out matchedKeywordId) ||
                                    !Double.TryParse(matchedItems[1], out matchedWeight)) continue;

                                paperVector.KeywordWeight.Add(matchedKeywordId, matchedWeight);
                            }
                            paperVectors.Add(paperVector.PaperId, paperVector);
                        }
                        line = streamReader.ReadLine();
                    } while (!string.IsNullOrEmpty(line));
                }
                BigStorage.PaperIndex = paperVectors;
            }
            else
            {
                // or generate in memory
                GenerateIndex();
            }
        }
    }
}
