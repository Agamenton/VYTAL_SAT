using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VYTAL_SAT_DPLL
{
    public class Formula
    {
        public List<Clause> Clauses { get; set; }
        public int Variables { get; set; }
        public int ClausesCount { get; set; }

        public Formula()
        {
            Clauses = new List<Clause>();
        }


        public Formula(List<Clause> clauses)
        {
            Clauses = clauses;
        }


        public int? GetPureLiteral()
        {

            // get all literals in formula
            List<int> allLiterals = GetAllLiterals();

            foreach (int literal in allLiterals)
            {
                if (!allLiterals.Contains(-literal))
                {
                    return literal;
                }
            }

            return null;
        }
        
        

        public List<int> GetAllLiterals()
        {
            List<int> literals = new List<int>();
            foreach (Clause clause in Clauses)
            {
                foreach (int literal in clause.Literals)
                {
                    literals.Add(literal);
                }
            }

            return literals;
        }

        public int CountOccurrencesInClausesOfLength(int literal, int length)
        {
            int count = 0;
            foreach (Clause clause in Clauses)
            {
                if (clause.Literals.Contains(literal) && clause.Literals.Count == length)
                {
                    count++;
                }
            }

            return count;
        }


        public int CountClausesOfGivenLengthThatContainLiteral(int literal, int length)
        {
            int count = 0;
            foreach (Clause clause in Clauses)
            {
                if (clause.Literals.Contains(literal) && clause.Literals.Count == length)
                {
                    count++;
                }
            }

            return count;
        }


        public int CountOccurrences(int literal)
        {
            return Clauses.Count(c => c.Literals.Contains(literal));
        }


        public Formula Copy()
        {
            Formula copy = new Formula();
            foreach (Clause clause in Clauses)
            {
                copy.Clauses.Add(clause.Copy());
            }

            return copy;
        }



        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (Clause clause in Clauses)
            {
                sb.Append(clause.ToString());
            }
            sb.Append("}");
            return sb.ToString();
        }


        public void RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(int literal)
        {
            // remove all clauses containing the same literal as the one in the unitClause
            Clauses.RemoveAll(c => c.Literals.Any(l => l == literal));

            // and remove all instances of negation of the literal in all Clauses
            Clauses.ForEach(c => c.Literals.RemoveAll(l => l == -literal));
        }
    }
}
