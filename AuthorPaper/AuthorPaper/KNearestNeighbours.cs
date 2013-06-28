using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuthorPaper
{
    public class KNearestNeighbours
    {
        public static List<Paper> FindKNearestPapers(int paperId, int k)
        {
            var paperIndex = BuildPaperIndex(paperId);
            return null;
        }

        public static List<Word> BuildPaperIndex(int paperId)
        {
            var keywords = new List<Word>();
            using (var context = new AuthorPaperEntities())
            {
                var paper = context.Papers.SingleOrDefault(p => p.Id == paperId);
                if (paper == null)
                    return null;
                keywords = LoadPaperKeywords(paper);
            }

            return keywords;
        }

        private static List<Word> LoadPaperKeywords(Paper paper)
        {
            var splitChars = new char[] { ',', ' ', ';', '.', '!', '?', '"' };
            var keywords = new List<Word>();
            var stopWords = LoadStopWords();
            if (!String.IsNullOrEmpty(paper.Title))
            {
                var titleKeywords = paper.Title.Trim(new char[] { '"' }).Split(splitChars).ToList();
                foreach (var keyword in titleKeywords)
                {
                    if (!stopWords.Contains(keyword) && !keywords.Contains(keyword))
                    {
                        keywords.Add(new Word { Keyword = keyword, Count = 0 });
                    }
                    else
                    {
                        keywords.Single(w => w.Keyword == keyword).Count++;
                    }
                }
            }
            if (!String.IsNullOrEmpty(paper.Keyword))
            {
                var paperKeywords = paper.Keyword.Trim(new char[] { '"' }).Split(splitChars);
                foreach (var keyword in paperKeywords)
                {
                    if (!stopWords.Contains(keyword) && !keywords.Contains(keyword))
                    {
                        keywords.Add(new Word { Keyword = keyword, Count = 0 });
                    }
                    else
                    {
                        keywords.Single(w => w.Keyword == keyword).Count++;
                    }
                }
            }

            return keywords;
        }

        public static List<string> LoadStopWords()
        {
            var stopWords = new List<string>();

            var filePath = @"..\..\..\data\english_stop.txt";
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        stopWords.Add(line);
                    }
                }
            }
            return stopWords;
        }
    }
}
