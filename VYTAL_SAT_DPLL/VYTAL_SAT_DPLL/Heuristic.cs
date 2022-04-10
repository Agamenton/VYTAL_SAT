namespace VYTAL_SAT_DPLL
{
    public class Heuristic
    {

        public static int DLIS(Formula F)
        {
            List<int> allLiterals = F.GetAllLiterals();
            List<int> uniqueLiterals = allLiterals.Distinct().ToList();

            // we take literal and !literal as two different literals in this heuristic
            int literalWithMaxOccurrences = uniqueLiterals[0];
            
            foreach (int literal in uniqueLiterals)
            {
                if (allLiterals.Count(c => c == literal) > allLiterals.Count(c => c == literalWithMaxOccurrences))
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
            List<Trio> literals = new List<Trio>();

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
                            literals.Add(new Trio(literal, F.CountOccurrencesInClausesOfLength(literal, k), F.CountOccurrencesInClausesOfLength(-literal, k)));
                        }
                    }
                }
            }

            // select literal that maximizes this: [fk(x) * fk(!x)]*p + fk(x) * fk(!x)
            int selected = literals[0].literal;
            int lastResult = literals[0].occurrences * literals[0].negativeOccurrences * p + literals[0].occurrences * literals[0].negativeOccurrences;
            foreach (Trio literalWithOccurrences in literals)
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

        private struct Trio
        {
            public int literal;
            public int occurrences;
            public int negativeOccurrences;

            public Trio(int literal, int occurrences, int negativeOccurrences)
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




        public static int BOHM(Formula F)
        {
            List<int> uniqueLiterals = F.UniqueLiterals();
            const int p1 = 1;
            const int p2 = 2;


            List<int> lengths = F.Clauses.Select(c => c.Literals.Count).Distinct().ToList();
            lengths.Sort();

            // Hi(x) is vector of occurrences of x in clauses of length i
            List<Pair> vectors = new List<Pair>();

            foreach (int literal in uniqueLiterals)
            {
                Pair vector = new Pair();
                vector.literal = literal;

                foreach (int length in lengths)
                {
                    // fi(x) is the number of not yet satisfied clauses with i literals that contain the literal x
                    vector.vector += p1 * Math.Max(F.CountClausesOfGivenLengthThatContainLiteral(literal, length), F.CountClausesOfGivenLengthThatContainLiteral(-literal, length))
                                    + p2 * Math.Min(F.CountClausesOfGivenLengthThatContainLiteral(literal, length), F.CountClausesOfGivenLengthThatContainLiteral(-literal, length));
                    vectors.Add(vector);
                }
            }

            int selected = vectors[0].literal;
            int lastResult = vectors[0].vector;
            foreach (Pair vector in vectors)
            {
                int newResult = vector.vector;
                if (newResult > lastResult)
                {
                    selected = vector.literal;
                    lastResult = newResult;
                }
            }
            return selected;
        }



        public struct Pair
        {
            public int literal;
            public int vector;
        }



        public static int FIRSTFIRST(Formula F)
        {
            return F.Clauses.First().Literals.First();
        }



        public static int RANDOM(Formula F)
        {
            Random random = new Random();
            int amountOfClauses = F.Clauses.Count();
            Clause randomClause = F.Clauses[random.Next(amountOfClauses)];

            int amoutOfLiterals = randomClause.Literals.Count();
            int literal = randomClause.Literals[random.Next(amoutOfLiterals)];
            return literal;
        }
    }
}
