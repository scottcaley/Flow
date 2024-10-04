using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    public class LogicCaseSet<T> : HashSet<Dictionary<T, bool>>
    {

        public LogicCaseSet() : base() { }


        private HashSet<T> VariableSet()
        {
            HashSet<T> variables = new HashSet<T>();

            foreach (Dictionary<T, bool> map in this)
            {
                foreach (T variable in map.Keys)
                {
                    variables.Add(variable);
                }
            }

            return variables;
        }

        private static Dictionary<T, bool> GenerateCase(List<T> variableList, int caseNumber)
        {
            Dictionary<T, bool> thisCase = new Dictionary<T, bool>();

            for (int index = 0; index < (1 << variableList.Count); index++)
            {
                int digit = (caseNumber >> (variableList.Count - 1 - index));
                bool variableValue = !(digit % 2 == 0);
                thisCase.Add(variableList[index], variableValue);
            }

            return thisCase;
        }

        private static bool IsSubset(Dictionary<T, bool> subset, Dictionary<T, bool> superset)
        {
            foreach (T variable in subset.Keys)
            {
                if (!superset.ContainsKey(variable)) return false;
                if (subset[variable] != superset[variable]) return false;
            }

            return true;
        }

        public void NonRedundantAdd(Dictionary<T, bool> set)
        {
            if (this.Contains(set)) return;

            foreach (Dictionary<T, bool> possibleSuperset in this)
            {
                if (IsSubset(set, possibleSuperset)) return;
            }

            HashSet<Dictionary<T, bool>> subsets = new HashSet<Dictionary<T, bool>>();
            foreach (Dictionary<T, bool> possibleSubset in this)
            {
                if (IsSubset(possibleSubset, set)) subsets.Add(possibleSubset);
            }
            this.ExceptWith(subsets);
        }

        public bool OrAllCases()
        {

            List<T> variableList = VariableSet().ToList();
            for (int caseNumber = 0; caseNumber < (1 << variableList.Count); caseNumber++)
            {
                Dictionary<T, bool> variableCase = GenerateCase(variableList, caseNumber);
                bool solutionFound = false;
                foreach (Dictionary<T, bool> possibleSolution in this)
                {
                    if (IsSubset(possibleSolution, variableCase)) solutionFound = true;
                }
                if (!solutionFound) return false;
            }

            return true;
        }
    }
}
