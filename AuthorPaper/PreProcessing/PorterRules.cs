using System;
using System.Collections.Generic;
using System.Linq;

namespace PreProcessing
{
    public class PorterRules
    {
        public static string SubstituteString(string inputString, Rule rule)
        {
            var lastIndexOfSuffix = inputString.LastIndexOf(rule.InitialSuffix, StringComparison.Ordinal);
            if (lastIndexOfSuffix > -1)
                inputString = inputString.Substring(0, lastIndexOfSuffix) + rule.SubstituteSuffix;
            return inputString;
        }
        public static bool TestCvc(string str, int p1, int p2)
        {
            if (p1 > -1 && p2 == -1)
            {
                if (str.EndsWith("w") || str.EndsWith("x") || str.EndsWith("y")) return false;
                var strArray = str.ToCharArray();
                var arrayCount = strArray.Count();
                if (arrayCount >= 3 &&
                    !Stemmer.Vowels.Contains(strArray[arrayCount - 3]) &&
                    Stemmer.Vowels.Contains(strArray[arrayCount - 2]) &&
                    !Stemmer.Vowels.Contains(strArray[arrayCount - 1]))
                    return true;
            }
            return false;
        }
        public static List<Rule> Rules = new List<Rule>
            {
                #region Rule 1
                // 1a
                new Rule
                    {
                        Order = 0,
                        InitialSuffix = "sses",
                        SubstituteSuffix = "ss",
                        Condition = (str, p1, p2) => str.EndsWith("sses"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 0
                    },
                new Rule
                    {
                        Order = 1,
                        InitialSuffix = "ies",
                        SubstituteSuffix = "i",
                        Condition = (str, p1, p2) => str.EndsWith("ies"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 0
                    },
                new Rule
                    {
                        Order = 2,
                        InitialSuffix = "ss",
                        SubstituteSuffix = "ss",
                        Condition = (str, p1, p2) => str.EndsWith("ss"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 0
                    },
                new Rule
                    {
                        Order = 3,
                        InitialSuffix = "s",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => str.EndsWith("s"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 0
                    },
                // 1b
                new Rule
                    {
                        Order = 4,
                        InitialSuffix = "eed",
                        SubstituteSuffix = "ee",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 3 && str.EndsWith("eed"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 1
                    },
                new Rule
                    {
                        Order = 5,
                        InitialSuffix = "ed",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 2 && str.EndsWith("ed"),
                        ApplyRule = SubstituteString,
                        DownstreamRuleGroups = new List<int> {2},
                        RuleGroup = 1
                    },
                new Rule
                    {
                        Order = 6,
                        InitialSuffix = "ing",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p1) > 3 && str.EndsWith("ing"),
                        ApplyRule = SubstituteString,
                        DownstreamRuleGroups = new List<int> {2},
                        RuleGroup = 1
                    },
                // 1b extension
                new Rule
                    {
                        Order = 7,
                        InitialSuffix = "at",
                        SubstituteSuffix = "ate",
                        Condition = (str, p1, p2) => str.EndsWith("at"),
                        ApplyRule = SubstituteString,
                        IsDownstreamRule = true,
                        RuleGroup = 2
                    },
                new Rule
                    {
                        Order = 8,
                        InitialSuffix = "bl",
                        SubstituteSuffix = "ble",
                        Condition = (str, p1, p2) => str.EndsWith("bl"),
                        ApplyRule = SubstituteString,
                        IsDownstreamRule = true,
                        RuleGroup = 2
                    },
                new Rule
                    {
                        Order = 9,
                        InitialSuffix = "iz",
                        SubstituteSuffix = "ize",
                        Condition = (str, p1, p2) => str.EndsWith("iz"),
                        ApplyRule = SubstituteString,
                        IsDownstreamRule = true,
                        RuleGroup = 2
                    },
                new Rule
                    {
                        Order = 10,
                        InitialSuffix = "",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) =>
                            {
                                var endings = new[] {"bb", "dd", "ff", "gg", "mm", "nn", "pp", "rr", "tt"};
                                return endings.Any(ending => str.EndsWith(ending));
                            },
                        IsDownstreamRule = true,
                        RuleGroup = 2,
                        ApplyRule = (str, rule) => str.Substring(0, str.Length - 1)
                    },
                new Rule
                    {
                        Order = 11,
                        InitialSuffix = "",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => TestCvc(str, p1, p2),
                        IsDownstreamRule = true,
                        RuleGroup = 2,
                        ApplyRule = (str, rule) => str + "e"
                    },
                //1c
                new Rule
                    {
                        Order = 12,
                        InitialSuffix = "y",
                        SubstituteSuffix = "i",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 1 && str.EndsWith("y"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 3
                    },
                #endregion
                #region Rule 2  
                new Rule
                    {
                        Order = 13,
                        InitialSuffix = "ational",
                        SubstituteSuffix = "ate",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 7 && str.EndsWith("ational"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 14,
                        InitialSuffix = "tional",
                        SubstituteSuffix = "tion",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 6 && str.EndsWith("tional"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 15,
                        InitialSuffix = "enci",
                        SubstituteSuffix = "ence",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 4 && str.EndsWith("enci"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 16,
                        InitialSuffix = "anci",
                        SubstituteSuffix = "ance",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 4 && str.EndsWith("anci"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 17,
                        InitialSuffix = "izer",
                        SubstituteSuffix = "ize",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 4 && str.EndsWith("izer"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 18,
                        InitialSuffix = "abli",
                        SubstituteSuffix = "able",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 4 && str.EndsWith("abli"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 19,
                        InitialSuffix = "alli",
                        SubstituteSuffix = "al",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 4 && str.EndsWith("alli"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 20,
                        InitialSuffix = "entli",
                        SubstituteSuffix = "ent",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("entli"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 21,
                        InitialSuffix = "eli",
                        SubstituteSuffix = "e",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 3 && str.EndsWith("eli"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 22,
                        InitialSuffix = "ousli",
                        SubstituteSuffix = "ous",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("ousli"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 23,
                        InitialSuffix = "ization",
                        SubstituteSuffix = "ize",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 7 && str.EndsWith("ization"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 24,
                        InitialSuffix = "ation",
                        SubstituteSuffix = "ate",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("ation"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 25,
                        InitialSuffix = "ator",
                        SubstituteSuffix = "ate",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 4 && str.EndsWith("ator"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 26,
                        InitialSuffix = "alism",
                        SubstituteSuffix = "al",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 5 && str.EndsWith("alism"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 27,
                        InitialSuffix = "iveness",
                        SubstituteSuffix = "ive",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 7 && str.EndsWith("iveness"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 28,
                        InitialSuffix = "fulness",
                        SubstituteSuffix = "ful",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 7 && str.EndsWith("fulness"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 29,
                        InitialSuffix = "ousness",
                        SubstituteSuffix = "ous",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 7 && str.EndsWith("ousness"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 30,
                        InitialSuffix = "aliti",
                        SubstituteSuffix = "al",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("aliti"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 31,
                        InitialSuffix = "iviti",
                        SubstituteSuffix = "ive",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("iviti"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                new Rule
                    {
                        Order = 32,
                        InitialSuffix = "biliti",
                        SubstituteSuffix = "ble",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 6 && str.EndsWith("biliti"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 4
                    },
                #endregion
                #region Rule 3
                new Rule
                    {
                        Order = 33,
                        InitialSuffix = "icate",
                        SubstituteSuffix = "ic",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("icate"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                new Rule
                    {
                        Order = 34,
                        InitialSuffix = "ative",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p1 > -1  && (str.Length - p1) > 5 && str.EndsWith("ative"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                new Rule
                    {
                        Order = 35,
                        InitialSuffix = "alize",
                        SubstituteSuffix = "al",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("alize"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                new Rule
                    {
                        Order = 36,
                        InitialSuffix = "iciti",
                        SubstituteSuffix = "ic",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 5 && str.EndsWith("iciti"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                new Rule
                    {
                        Order = 37,
                        InitialSuffix = "ical",
                        SubstituteSuffix = "ic",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 4 && str.EndsWith("ical"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                new Rule
                    {
                        Order = 38,
                        InitialSuffix = "full",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 4 && str.EndsWith("full"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                new Rule
                    {
                        Order = 39,
                        InitialSuffix = "ness",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p1 > -1 && (str.Length - p1) > 4 && str.EndsWith("ness"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 5
                    },
                #endregion
                #region Rule 4
                new Rule
                    {
                        Order = 40,
                        InitialSuffix = "al",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1  && (str.Length - p2) > 2 && str.EndsWith("al"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 41,
                        InitialSuffix = "ance",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 4 && str.EndsWith("ance"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 42,
                        InitialSuffix = "ence",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1  && (str.Length - p2) > 4 && str.EndsWith("ence"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 43,
                        InitialSuffix = "er",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 2 && str.EndsWith("er"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 44,
                        InitialSuffix = "ic",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 2 && str.EndsWith("ic"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 45,
                        InitialSuffix = "able",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 4 && str.EndsWith("able"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 46,
                        InitialSuffix = "ible",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 4 && str.EndsWith("ible"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 47,
                        InitialSuffix = "ant",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ant"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 48,
                        InitialSuffix = "ement",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 5 && str.EndsWith("ement"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 49,
                        InitialSuffix = "ment",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 4 && str.EndsWith("ment"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 50,
                        InitialSuffix = "ent",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ent"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 51,
                        InitialSuffix = "ion",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && (str.EndsWith("sion") || str.EndsWith("tion")), 
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 52,
                        InitialSuffix = "ou",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 2 && str.EndsWith("ou"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 53,
                        InitialSuffix = "ism",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ism"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 54,
                        InitialSuffix = "ate",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ate"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 55,
                        InitialSuffix = "iti",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("iti"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 56,
                        InitialSuffix = "ous",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ous"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 57,
                        InitialSuffix = "ive",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ive"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                new Rule
                    {
                        Order = 58,
                        InitialSuffix = "ize",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2) > 3 && str.EndsWith("ize"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 6
                    },
                #endregion  
                #region Rule 5
                new Rule
                    {
                        Order = 59,
                        InitialSuffix = "e",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && (str.Length - p2 > 1) && str.EndsWith("e"),
                        ApplyRule = SubstituteString,
                        RuleGroup = 7
                    },
                new Rule
                    {
                        Order = 60,
                        InitialSuffix = "e",
                        SubstituteSuffix = "",
                        Condition = (str, p1, p2) => p2 > -1 && str.EndsWith("e") && !TestCvc(str.Substring(0, str.Length - 1), p1, p2),
                        ApplyRule = SubstituteString,
                        RuleGroup = 7
                    },
                new Rule
                    {
                        Order = 61,
                        InitialSuffix = "ll",
                        SubstituteSuffix = "l",
                        Condition = (str, p1, p2) => str.EndsWith("ll") && p2 > -1,
                        ApplyRule = SubstituteString,
                        RuleGroup = 8
                    },
                #endregion
            };
    }
}