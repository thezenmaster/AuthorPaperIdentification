using System;
using System.Collections.Generic;
using System.Linq;
using PreProcessing.BuildIndices;
using PreProcessing.IO;

namespace AuthorPaper.Console.Classifier
{
    public class Classifier
    {
        public const int NumberOfNeighbors = 5;
        public static void ClassifyTestPapers()
        {
        // initialize - get indices, get data from db in memory
            Initialize();
            System.Console.WriteLine("start classify");
            var index = 0;
            var testPaperResults = new SortedList<long, PaperOutput>();
            foreach (var testPaper in BigStorage.TestPapers)
            {
                var keywords = PreProcessing.Keywords.GenerateKeywords.GeneratePaperKeywords(testPaper.Value).ToList();
                if (!keywords.Any())
                {
                    System.Console.WriteLine("zero keywords for paperid " + testPaper.Key);
                }
                var simpleKeywords = new List<SimplePaperKeyword>();
                foreach (var keyword in keywords)
                {
                    var escapedKey = KeywordIndex.RemoveNewLines(keyword.Value);
                    if (!BigStorage.KeywordIndex.ContainsKey(escapedKey))
                    {
                        System.Console.WriteLine("keyword not found " + escapedKey + " paperid " + testPaper.Key);
                        continue;
                    }
                    var getFromIndex = BigStorage.KeywordIndex[escapedKey];
                    simpleKeywords.Add(new SimplePaperKeyword
                        {
                            Value = keyword.Value,
                            Count = keyword.Count,
                            KeywordId = getFromIndex.KeywordId
                        });
                }
                testPaper.Value.PaperKeywords = simpleKeywords;
                var paperVector = PaperIndex.GeneratePaperVector(testPaper.Value);

                var paperOutput = ExecuteClassifierAlgorithm(paperVector);

                testPaperResults.Add(paperVector.PaperId, paperOutput);

                index++;
                if (index % 1000 == 0)
                {
                    System.Console.WriteLine("generating for keyword # " + index);
                }
            }
            BigStorage.TestPaperResults = testPaperResults;
            System.Console.WriteLine("end classify");
            
            // store similar papers for all in file
        }

        private static void Initialize()
        {
            // initialize - get indices, get data from db in memory
            KeywordIndex.GetIndex();
            PaperIndex.GetIndex();
        }

        private static PaperOutput ExecuteClassifierAlgorithm(PaperVector paper)
        {
            var paperOutput = new PaperOutput
                {
                    PaperId = paper.PaperId,
                    MatchedPapers = new List<MatchedPaperOutput>()
                };
            
            // in our case Knearest neighbor
            // get list of relevant papers
            foreach (var trainPaper in GetRelevantPapers(paper))
            {
                // foreach paper in relevant list
                var similarity = CalculateSimilarity(paper, trainPaper);
                // store top K results in list
                if (double.IsNaN(similarity)) continue;
                
                var trainPaperMatch = new MatchedPaperOutput
                    {
                        PaperId = trainPaper.PaperId,
                        Similarity = similarity
                    };
                if (paperOutput.MatchedPapers.Count < NumberOfNeighbors)
                {
                    paperOutput.MatchedPapers.Add(trainPaperMatch);
                }
                else
                {
                    var minPaperSimilarity = paperOutput.MatchedPapers.Min(mp => mp.Similarity);
                    if (trainPaperMatch.Similarity > minPaperSimilarity)
                    {
                        paperOutput.MatchedPapers.Add(trainPaperMatch);
                        const double epsilon = 0.0000001;
                        var lastPaper = paperOutput.MatchedPapers.First(mps => Math.Abs(mps.Similarity - minPaperSimilarity) < epsilon);
                        paperOutput.MatchedPapers.Remove(lastPaper);
                    }
                }
            }
            
            // store paper similar papers in list
            return paperOutput;
        }

        private static List<PaperVector> GetRelevantPapers(PaperVector paper)
        {
            // get list of relevant papers
            var result = new List<PaperVector>();
            //foreach (var simplePaper in BigStorage.Papers)
            //{
            //    result.Add(PaperIndex.GeneratePaperVector(simplePaper.Value));
            //}
            var listPaperIds = new List<long>();
            foreach (var keywords in paper.KeywordValues)
            {
                var keywordIndexItem = BigStorage.KeywordIndex[KeywordIndex.RemoveNewLines(keywords.Key)];
                listPaperIds.AddRange(keywordIndexItem.PaperKeywordFrequencies.Select(pk => pk.Key));
            }
            var distinctPaperIds = listPaperIds.Distinct();
            foreach (var distinctPaperId in distinctPaperIds)
            {
                if (!BigStorage.PaperIndex.ContainsKey(distinctPaperId))
                {
                    System.Console.WriteLine("relevant paper not found in big index " + distinctPaperId);
                    continue;
                }

                var relevantPaper = BigStorage.PaperIndex[distinctPaperId];
                result.Add(relevantPaper);
            }

            return result;
        }

        private static double CalculateSimilarity(PaperVector testPaper, PaperVector trainPaper)
        {
            // execute similarity function
            var topValue = 0.0;
            foreach (var weight in testPaper.KeywordWeight)
            {
                var trainWeight = trainPaper.KeywordWeight.ContainsKey(weight.Key);
                topValue += trainWeight ? weight.Value*trainPaper.KeywordWeight[weight.Key] : 0.0;
            }
            var bottomValue = testPaper.ScalarSquare*trainPaper.ScalarSquare;
            return topValue / bottomValue;
        }
    }
}
