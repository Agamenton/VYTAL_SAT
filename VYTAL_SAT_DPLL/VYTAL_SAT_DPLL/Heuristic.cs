using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VYTAL_SAT_DPLL
{
    public class Heuristic
    {

        public static int DLIS(Formula F)
        {
            List<int> allLiterals = F.GetAllLiterals();
            List<int> uniqueLiterals = allLiterals.Distinct().ToList();

            int literalWithMaxOccurrences = uniqueLiterals[0];
            foreach (int literal in uniqueLiterals)
            {
                if (allLiterals.Count(c => c == literal) > literalWithMaxOccurrences)
                {
                    literalWithMaxOccurrences = literal;
                }
            }
            return literalWithMaxOccurrences;
        }

        public static int MOM(Formula F)
        {
            Clause shortest = F.Clauses.OrderBy(c => c.Literals.Count).First();
            int p = F.Clauses.SelectMany(c => c.Literals.Distinct()).Count();
            p = p * p + 1;

            // length of shortest clause
            int k = shortest.Literals.Count;

            // save here how many times each literal appears in the clauses of length k
            List<Tlist> literals = new List<Tlist>();
            
            // for each clause
            foreach (Clause clause in F.Clauses)
            {
                // of length "shortest"
                if (clause.Literals.Count == k)
                {
                    // for each literal in that clause
                    foreach (int literal in clause.Literals)
                    {
                        // if we haven't processed this literal yet
                        if (literals.All(l => l.literal != literal || l.literal != -literal))
                        {
                            literals.Add(new Tlist(literal, F.CountOccurrencesInClausesOfLength(literal, k), F.CountOccurrencesInClausesOfLength(-literal, k)));
                        }
                    }
                }
            }

            // select literal that maximizes this: [fk(x) * fk(!x)]*p + fk(x) * fk(!x)
            int selected = literals[0].literal;
            int lastResult = literals[0].occurrences * literals[0].negativeOccurrences * p + literals[0].occurrences * literals[0].negativeOccurrences;
            foreach (Tlist literalWithOccurrences in literals)
            {
                int newResult = literalWithOccurrences.occurrences * literalWithOccurrences.negativeOccurrences * p +
                                literalWithOccurrences.occurrences * literalWithOccurrences.negativeOccurrences;
                if (newResult > lastResult)
                {
                    selected = literalWithOccurrences.literal;
                    lastResult = newResult;
                }
            }

            return selected;
        }

        private struct Tlist
        {
            public int literal;
            public int occurrences;
            public int negativeOccurrences;
            
            public Tlist(int literal, int occurrences, int negativeOccurrences)
            {
                this.literal = literal;
                this.occurrences = occurrences;
                this.negativeOccurrences = negativeOccurrences;
            }
        }


        public static int DLCS(Formula F)
        {
            List<int> literals = F.Clauses.SelectMany(c => c.Literals.Distinct()).ToList();
            literals.RemoveAll(l => l == -l);

            int selected = literals[0];
            int lastResult = F.CountOccurrences(selected) + F.CountOccurrences(-selected);
            foreach (int literal in literals)
            {
                int newResult = F.CountOccurrences(literal) + F.CountOccurrences(-literal);
                if (newResult > lastResult)
                {
                    selected = literal;
                    lastResult = newResult;
                }
            }

            if (F.CountOccurrences(selected) > F.CountOccurrences(-selected))
            {
                return selected;
            }
            else
            {
                return -selected;
            }
        }



        public static int Custom(Formula F)
        {
            return 0;
        }
    }
}
