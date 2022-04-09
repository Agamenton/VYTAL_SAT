// See https://aka.ms/new-console-template for more information

using VYTAL_SAT_DPLL;


string filePath = "test.dimacs";
Formula F;
try
{ 
    F = DimacsParser.Parse(filePath);
}
catch (IOException e)
{
    Console.WriteLine("Could not read file: " + filePath);
    Console.WriteLine(e);
    throw e;
}
catch (FormatException e)
{
    Console.WriteLine("Could not parse file: " + filePath);
    Console.WriteLine(e);
    throw e;
}
catch (OverflowException e )
{
    Console.WriteLine("Could not parse file: " + filePath);
    Console.WriteLine(e);
    throw e;
}


Console.WriteLine(F);
long countCalls = 0;
Console.WriteLine("\nDPLL says that this Formula " + (DPLL(F, ref countCalls) ? "IS satisfiable" : "is NOT satisfiable"));
Console.WriteLine("DPLL was called " + countCalls + " times");




static bool DPLL(Formula F, ref long countCalls)
{
    countCalls += 1;
    // if an empty clause is present then NOT-SAT
    if (F.Clauses.Any(c => c.Literals.Count == 0))
        return false;
    
    // if there are no clauses then SAT
    if (F.Clauses.Count == 0)
        return true;

    // find pureLiteral and if it exists, remove all clauses containing this literal
    int? pureLiteral = F.GetPureLiteral();
    if (pureLiteral != null)
    {
        F.Clauses.RemoveAll(c => c.Literals.Any(l => l == pureLiteral));
        return DPLL(F, ref countCalls);
    }

    // find unit clause and if it exists, 
    Clause? unitClause = F.Clauses.FirstOrDefault(c => c.Literals.Count() == 1);
    if (unitClause != null)
    {
        // remove all clauses containing the same literal as the one in the unitClause
        F.Clauses.RemoveAll(c => c.Literals.Any(l => l == unitClause.Literals.First()));

        // and remove all instances of negation of the literal in all Clauses
        F.Clauses.ForEach(c => c.Literals.RemoveAll(l => l == -unitClause.Literals.First()));

        return DPLL(F, ref countCalls);
    }

    // else, use Heuristic to select a literal 
    int selected = Heuristic.DLCS(F);   // TODO change heuristic by parameter
    Formula Fcopy = F.Copy();

    // test if removing negation of this literal resulted in SAT
    Fcopy.Clauses.RemoveAll(c => c.Literals.Any(l => l == -selected));
    Fcopy.Clauses.ForEach(c => c.Literals.RemoveAll(l => l == selected));
    if (DPLL(Fcopy, ref countCalls))
    {
        return true;
    }
    // otherwise remove not-negative instance of this literal
    else
    {
        F.Clauses.RemoveAll(c => c.Literals.Any(l => l == selected));
        F.Clauses.ForEach(c => c.Literals.RemoveAll(l => l == -selected));
        return DPLL(F, ref countCalls);
    }


}