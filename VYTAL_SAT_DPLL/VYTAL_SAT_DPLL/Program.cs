
using System.Diagnostics;
using System.Diagnostics.Metrics;
using VYTAL_SAT_DPLL;

/*
 * MAIN
 */

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

RunDPLL(F);




/*
 * END OF MAIN
 */





static void RunDPLL(Formula F)
{
    List<string> heuristics = new List<string>()
        {"DLIS", "DLCS", "MOM", "BOHM", "CUSTOM"};

    Task[] tasks = new Task[heuristics.Count];
    int heuristicIterator = 0;
    foreach (string heuristic in heuristics)
    {
        tasks[heuristicIterator] = Task.Run(() =>
        {
            TestDPLL(F.Copy(), heuristic);
            return 0;
        });
        if (heuristicIterator < heuristics.Count-1)
        {
            heuristicIterator++;
        }
    }

    Task.WaitAll(tasks);
}



static void TestDPLL(Formula F, string H)
{
    Stopwatch sw = new Stopwatch();
    long counter = 0;
    sw.Start();
    bool result = DPLL(F, ref counter, H);
    sw.Stop();
    Console.WriteLine("\n--------------------------------\n" +
                      "Running Heuristic: " + H + "\n" +
                      "Satisfiable?: " + (result ? "YES" : "NO") +"\n" +
                      "DPLL calls:   " + counter + "\n"+
                      "Time elapsed: " + sw.Elapsed.Minutes +"min " + sw.Elapsed.Seconds+"s " + sw.Elapsed.Milliseconds +"ms");
}



static bool DPLL(Formula F, ref long countCalls, string H)
{
    countCalls += 1;
    // if an empty clause is present then NOT-SAT
    if (F.Clauses.Any(c => c.Literals.Count == 0))
        return false;
    
    // if there are no clauses then SAT
    if (F.Clauses.Count == 0)
        return true;

    // find unit clause and if it exists, 
    Clause? unitClause = F.Clauses.FirstOrDefault(c => c.Literals.Count() == 1);
    if (unitClause != null)
    {
        F.RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(unitClause.Literals.First());

        return DPLL(F, ref countCalls, H);
    }

    // find pureLiteral and if it exists, remove all clauses containing this literal
    int? pureLiteral = F.GetPureLiteral();
    if (pureLiteral != null)
    {
        F.Clauses.RemoveAll(c => c.Literals.Any(l => l == pureLiteral));
        return DPLL(F, ref countCalls, H);
    }


    // else, use Heuristic to select a literal 
    int selected;
    switch (H)
    {
        case "DLIS":
            selected = Heuristic.DLIS(F);
            break;
        case "DLCS":
            selected = Heuristic.DLCS(F);
            break;
        case "MOM":
            selected = Heuristic.MOM(F);
            break;
        case "BOHM":
            selected = Heuristic.BOHM(F);
            break;
        case "CUSTOM":
            selected = Heuristic.CUSTOM(F);
            break;
        case "RANDOM":
            selected = Heuristic.RANDOM(F);
            break;
        default:
            throw new Exception("Heuristic not found");
    }
    Heuristic.BOHM(F);
    Formula Fcopy = F.Copy();

    // test if removing negation of this literal resulted in SAT
    Fcopy.RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(-selected);
    
    if (DPLL(Fcopy, ref countCalls, H))
    {
        return true;
    }
    // otherwise remove not-negative instance of this literal
    else
    {
        F.RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(selected);
        return DPLL(F, ref countCalls, H);
    }


}