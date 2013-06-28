using System;
using System.Collections.Generic;

namespace PreProcessing
{
    public class Rule
    {
        public int Order { get; set; }
        public string InitialSuffix { get; set; }
        public string SubstituteSuffix { get; set; }

        public Func<string, int, int, bool> Condition { get; set; }

        public Func<string, Rule, string> ApplyRule { get; set; }
        public List<int> DownstreamRuleGroups { get; set; }
        public bool IsDownstreamRule { get; set; }
        public int RuleGroup { get; set; }
    }
}