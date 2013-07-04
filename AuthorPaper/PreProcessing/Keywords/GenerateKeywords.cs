using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AuthorPaper;
using PreProcessing.BuildIndices;

namespace PreProcessing.Keywords
{
    public class GenerateKeywords
    {
        private static readonly List<char> InvalidCharacters = new List<char>
            {
                '$', '\\', '{', '}', '@', '#', '%', '^', '&', '*', '(', ')', '[', ']', ':', ';', '"', '\'', '|', '/',
                '<', '>', ',', '.', '?', '~', '`', '+', '='
            };
        private static readonly string[] Digits = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static readonly Regex NonUtf8Characters = new Regex(@"[\x80-\xFF]");

        private static readonly char[] Punctuation = new[] { ' ', ',', '.', '\'', '"', '(', ')', '-', '_', ';', ':', '[', ']', '?', '!', '`' };
        private static readonly char[] Separators = new[] {';', ',', '|'};
        private static readonly string[] ReplaceCharacters = new [] {"\n", "\r", "\t"};
        private const int MinValidWordLength = 4;
        private const int MaxNumberOfWords = 4;

        public static IEnumerable<Word> GeneratePaperKeywords(SimplePaper paper)
        {
            return GenerateAllKeywords(paper.Title, paper.Keywords);
        }

        public static IEnumerable<Word> GeneratePaperKeywords(Paper paper)
        {
            return GenerateAllKeywords(paper.Title, paper.Keyword);
        }
       
        private static IEnumerable<Word> GenerateAllKeywords(string title, string keywords)
        {
            var paperKeywords = new List<string>();
            paperKeywords.AddRange(GetKeywords(title, true));
            paperKeywords.AddRange(GetKeywords(keywords, false));
            // count number of occurrences
            return paperKeywords.Where(pk => !string.IsNullOrEmpty(pk))
                .Select(Stemmer.GetStemmedWord)
                .GroupBy(pk => pk).Select(g => new Word { Value = g.Key, Count = g.Count() });
        }

        public static bool IsValidKeyword(string keyword)
        {
            return !string.IsNullOrEmpty(keyword) && keyword.Length >= MinValidWordLength
                && !Digits.Any(keyword.StartsWith)
                && !NonUtf8Characters.IsMatch(keyword)
                && InvalidCharacters.All(ic => !keyword.Contains(ic))
                && !StopWordsContainer.StopWords.Contains(keyword);
        }

        public static string TrimKeyword(string keyword)
        {
            var result = keyword.Trim(Punctuation);
            result = ReplaceCharacters.Aggregate(result, (current, t) => current.Replace(t, ""));

            // trim html tags
            if (result.StartsWith("<"))
            {
                var startTagEndIndex = result.IndexOf(">", StringComparison.Ordinal);
                if(startTagEndIndex > -1)
                    result = result.Substring(startTagEndIndex + 1);
            }
            if (result.EndsWith(">"))
            {
                var endTagStartIndex = result.LastIndexOf("<", StringComparison.Ordinal);
                if (endTagStartIndex > 0)
                    result = result.Substring(0, endTagStartIndex - 1);
            }

            return result;
        }

        public static List<string> GetKeywords(string keywordList, bool isTitle)
        {
            if (!string.IsNullOrEmpty(keywordList))
            {
                var keywordLower = keywordList.ToLowerInvariant();
                List<string> splitTitle;
                if (isTitle)
                {
                    splitTitle = keywordLower.Split(' ').ToList();
                }
                else
                {
                    splitTitle = keywordLower.Split(Separators).ToList();
                    
                    if (splitTitle.Count() == 1)
                    {
                        splitTitle = keywordLower.Split(' ').ToList();
                    }
                    else
                    {
                        var localSplit = new List<string>();
                        localSplit.AddRange(splitTitle);
                        
                        for(var index = 0; index < localSplit.Count; index++)
                        {
                            var part = localSplit[index];
                            var splitBySpace = part.Split(' ').ToList();
                            if (splitBySpace.Count <= MaxNumberOfWords) continue;
                            splitTitle.RemoveAt(index);
                            splitTitle.AddRange(splitBySpace);
                        }
                    }
                }
                return splitTitle.Select(TrimKeyword).Where(IsValidKeyword).ToList();
            }
            return new List<string>();
        }
    }
}
