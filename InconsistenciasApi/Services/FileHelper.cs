using InconsistenciasApi.Models.Constants;
using InconsistenciasApi.Models.Inconsistencias.Classes;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace InconsistenciasApi.Services
{
    public static class FileHelper
    {
        public static List<Rule> GetRuleList(IFormFile file)
        {
            string[] lines = file.ReadAsList().ToArray();
            var rules = new List<Rule>();
            foreach (string line in lines)
            {

                var aFrom = line.IndexOf(Constants.RULE_START) + Constants.RULE_START.Length;
                var fullAntecedent = line[aFrom..line.IndexOf(Constants.RULE_DIVIDER)].Trim();

                var cFrom = line.IndexOf(Constants.RULE_DIVIDER) + Constants.RULE_DIVIDER.Length;
                var fullConsequent = line[cFrom..].Trim();

                rules.Add(new Rule { Antecedent = fullAntecedent, Consequent = fullConsequent, Id = rules.Count + 1 });

            }
            return rules;
        }
    }
}
