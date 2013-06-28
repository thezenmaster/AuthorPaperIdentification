﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthorPaper
{
    public class Word
    {
        public string Value { get; set; }
        public long Count { get; set; }
        public double NormalizedCount { get; set; }
    }

    public static class ListExtensions
    {
        public static bool Contains(this List<Word> list, string value)
        {
            var word = list.SingleOrDefault(w => w.Value == value);
            return word != null;
        }
    }
}
