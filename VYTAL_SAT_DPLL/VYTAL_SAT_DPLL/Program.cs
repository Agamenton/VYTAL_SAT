
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
Console.WriteLine();
long countCalls = 0;
//Console.WriteLine("\nDPLL says that this Formula " + (DPLL(F, ref countCalls) ? "IS satisfiable" : "is NOT satisfiable"));
//Console.WriteLine("DPLL was called " + countCalls + " times");

HeuristicsTest();


void HeuristicsTest()
{
    Formula f = new();
    

        Console.WriteLine("----------------------------------------------------");

    Formula original = DimacsParser.Parse(filePath);
    
    
    //Console.WriteLine("Formula: [" + f.InputLiterals + "; " + f.InputClauses + "]");
    Console.WriteLine("Is satisfiable: DLIS: " + (DPLL(F.Copy(), ref countCalls, "DLIS") ? "IS satisfiable" : "is NOT satisfiable"));
    Console.WriteLine("Recursive calls: DLIS: " + countCalls);
    countCalls = 0;
    Console.WriteLine();

    Console.WriteLine("Is satisfiable: DLCS: " + (DPLL(F.Copy(), ref countCalls, "DLCS") ? "IS satisfiable" : "is NOT satisfiable"));
    Console.WriteLine("Recursive calls: DLCS: " + countCalls);
    countCalls = 0;
    Console.WriteLine();

    Console.WriteLine("Is satisfiable: MOM: " + (DPLL(F.Copy(), ref countCalls, "MOM") ? "IS satisfiable" : "is NOT satisfiable"));
    Console.WriteLine("Recursive calls: MOM: " + countCalls);
    countCalls = 0;
    Console.WriteLine();

    Console.WriteLine("Is satisfiable: BOHM: " + (DPLL(F.Copy(), ref countCalls, "BOHM") ? "IS satisfiable" : "is NOT satisfiable"));
    Console.WriteLine("Recursive calls: BOHM: " + countCalls);
    countCalls = 0;
    Console.WriteLine();


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