using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects.DataClasses;
using System.Linq;
using AuthorPaper;
using PreProcessing.IO;

namespace PreProcessing.BuildIndices
{
    // store database and index collections in a single place
    public class BigStorage
    {
        public const int TrainPaperCount = 78079;
        private static SortedList<long, ValidPaper> _validPapers;
        public static SortedList<long, ValidPaper> ValidPapers
        {
            get
            {
                if (_validPapers == null)
                {
                    using (var context = new AuthorPaperEntities())
                    {
                        _validPapers = new SortedList<long, ValidPaper>();
                        var validPapers = context.ValidPapers.OrderBy(p => p.PaperId).ToList();
                        validPapers.ForEach(f => { if (f.PaperId.HasValue) _validPapers.Add(f.PaperId.Value, f); });
                    }
                }
                return _validPapers;
            }
            // use setter to clear validpapers from memory
            set { _validPapers = value; }
        }

        private static SortedList<long, SimplePaper> _trainPapers;
        public static SortedList<long, SimplePaper> TrainPapers
        {
            get
            {
                if (_trainPapers == null)
                {
                    using (var context = new AuthorPaperEntities())
                    {
                        _trainPapers = new SortedList<long, SimplePaper>();
                        var trainPapers = context.ValidPapers.Include("paper.PaperKeywords.Keyword")
                            .OrderBy(p => p.PaperId).Take(TrainPaperCount)
                            .Select(p => new SimplePaper
                            {
                                Id = p.PaperId.Value,
                                PaperKeywords =
                                    p.paper.PaperKeywords.Select(pk => new SimplePaperKeyword
                                    {
                                        Count = pk.Count.HasValue ? pk.Count.Value : 1,
                                        PaperKeywordId = pk.PaperKeywordId,
                                        KeywordId = pk.KeywordId,
                                        Value = pk.Keyword.Value
                                    })
                            })
                            .ToList();
                        trainPapers.ForEach(f =>  _trainPapers.Add(f.Id, f));
                    }
                }
                return _trainPapers;
            }
            // use setter to clear validpapers from memory
            set { _trainPapers = value; }
        }

        private static SortedList<long, SimplePaper> _testPapers;
        public static SortedList<long, SimplePaper> TestPapers
        {
            get
            {
                if (_testPapers == null)
                {
                    using (var context = new AuthorPaperEntities())
                    {
                        _testPapers = new SortedList<long, SimplePaper>();
                        var testPapers = context.ValidPapers.Include("paper.PaperKeywords.Keyword")
                            .OrderBy(p => p.PaperId).Skip(TrainPaperCount)
                            //.Take(1) // todo: remove
                            .Select(p => new SimplePaper
                            {
                                Id = p.PaperId.Value,
                                PaperKeywords =
                                    p.paper.PaperKeywords.Select(pk => new SimplePaperKeyword
                                    {
                                        Count = pk.Count.HasValue ? pk.Count.Value : 1,
                                        PaperKeywordId = pk.PaperKeywordId,
                                        KeywordId = pk.KeywordId,
                                        Value = pk.Keyword.Value
                                    })
                            })
                            .ToList();
                        testPapers.ForEach(f => _testPapers.Add(f.Id, f));
                    }
                }
                return _testPapers;
            }
            // use setter to clear validpapers from memory
            set { _testPapers = value; }
        }

        private static SortedList<long, SimplePaper> _papers;
        public static SortedList<long, SimplePaper> Papers
        {
            get
            {
                if (_papers == null)
                {
                    Console.WriteLine("loading papers");
                    using (var context = new AuthorPaperEntities())
                    {
                        _papers = new SortedList<long, SimplePaper>();
                        var papers = context.Papers
                            .Include("PaperKeywords")
                            .OrderBy(p => p.Id)
                            .Select(p => new SimplePaper
                                {
                                    Id = p.Id,
                                    PaperKeywords =
                                        p.PaperKeywords.Select(pk => new SimplePaperKeyword()
                                        {
                                            Count = pk.Count.HasValue ? pk.Count.Value : 1,
                                            PaperKeywordId = pk.PaperKeywordId
                                        })
                                }).ToList();
                        papers.ForEach(f => _papers.Add(f.Id, f));
                    }
                    Console.WriteLine("loaded papers");
                }
                return _papers;
            }
            // use setter to clear validpapers from memory
            set { _papers = value; }
        }

        public static SortedList<long, PaperOutput> TestPaperResults { get; set; } 

        private static SortedList<string, KeywordVector> _keywordIndex;
        public static SortedList<string, KeywordVector> KeywordIndex
        {
            get
            {
                if (_keywordIndex == null)
                {
                    //todo: get keyword index
                }
                return _keywordIndex;
            }
            set { _keywordIndex = value;  }
        }

        private static SortedList<long, PaperVector> _paperIndex;
        public static SortedList<long, PaperVector> PaperIndex
        {
            get
            {
                if (_paperIndex == null)
                {
                    //todo: get paper index
                }
                return _paperIndex;
            }
            set { _paperIndex = value; }
        }

        private static List<Keyword> _keywords;
        public static List<Keyword> Keywords
        {
            get
            {
                if (_keywords == null)
                {
                    Console.WriteLine("loading keywords");
                    using (var context = new AuthorPaperEntities())
                    {
                        _keywords = context.Keywords.Include("PaperKeywords").OrderBy(k => k.Value).ToList();
                    }
                    Console.WriteLine("loaded keywords");
                }
                return _keywords;
            }
            // use setter to clear validpapers from memory
            set { _keywords = value; }
        }
    }
}
