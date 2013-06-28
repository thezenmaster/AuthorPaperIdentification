using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthorPaper
{
    public class Word
    {
        public string Keyword { get; set; }
        public int Count { get; set; }
    }

    public static class ListExtensions
    {
        public static bool Contains(this List<Word> list, string value)
        {
            var word = list.SingleOrDefault(w => w.Keyword == value);
            return word != null;
        }
    }
}
