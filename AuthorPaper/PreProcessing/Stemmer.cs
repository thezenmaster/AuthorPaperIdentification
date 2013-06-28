using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PreProcessing
{
    public class Stemmer
    {
        public static readonly char[] Vowels = new[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        
        public static int GetNextPosition(char[] inputArray, int startPosition)
        {
            var firstVowel = -1;
            var firstConsonant = -1;
            var pos1 = -1;
            for (var i = startPosition; i < inputArray.Length; i++)
            {
                if (firstVowel < 0)
                {
                    if (Vowels.Contains(inputArray[i])) firstVowel = i;
                    continue;
                }
                if (firstConsonant < 0 && Vowels.Contains(inputArray[i])) continue;
                if (firstConsonant < 0)
                {
                    firstConsonant = i;
                    continue;
                }
                if (!Vowels.Contains(inputArray[i])) continue;
                pos1 = i - 1;
                break;
            }
            if (firstConsonant > -1 && pos1 < 0) pos1 = inputArray.Length - 1; // last letter is consonant
            return pos1;
        }

        public static void CalculateMeasurePoints(string inputString, out int p1, out int p2)
        {
            var inputArray = inputString.ToCharArray();
            p1 = GetNextPosition(inputArray, 0);
            if (p1 > -1)
                p2 = GetNextPosition(inputArray, p1);
            else
                p2 = -1;
        }

        public static string ApplyGroupRules(string inputString, IGrouping<int, Rule> ruleGroup, int p1, int p2)
        {
            var downStreamGroups = new List<int>();
            foreach (var rule in ruleGroup)
            {
                if (!rule.Condition(inputString, p1, p2)) continue;
                inputString = rule.ApplyRule(inputString, rule);
                if (rule.DownstreamRuleGroups != null && rule.DownstreamRuleGroups.Any())
                {
                    downStreamGroups.AddRange(rule.DownstreamRuleGroups);
                }
                break;
            }

            downStreamGroups = downStreamGroups.Distinct().ToList();
            if (!downStreamGroups.Any()) return inputString;

            foreach (var downstreamGroup in downStreamGroups)
            {
                var currentGroup = downstreamGroup;
                var group = PorterRules.Rules.GroupBy(g => g.RuleGroup).Single(g => g.Key == currentGroup);
                inputString = ApplyGroupRules(inputString, group, p1, p2);
            }
            return inputString;
        }

        public static string GetStemmedWord(string inputString)
        {
            foreach (var ruleGroup in PorterRules.Rules.Where(r => !r.IsDownstreamRule)
                .GroupBy(r => r.RuleGroup))
            {
                int p1 = -1, p2 = -1;
                CalculateMeasurePoints(inputString, out p1, out p2);
                inputString = ApplyGroupRules(inputString, ruleGroup, p1, p2);
            }

            return inputString;
        }
    }
}
