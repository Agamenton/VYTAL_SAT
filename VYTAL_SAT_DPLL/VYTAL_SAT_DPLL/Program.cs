
using System.Diagnostics;
using System.Globalization;
using VYTAL_SAT_DPLL;




class Program
{

    // list of all implemented heuristics
    static List<string> Heuristics = new List<string>() { "DLIS", "DLCS", "MOM", "BOHM", "FIRSTFIRST" };

    // each heuristic will count to it's own counter, (this is because each heuristic is running in a separate thread)
    static Dictionary<string, long> GlobalCounter = new Dictionary<string, long>();


    /* ============================================================
     * MAIN
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("Program Started !");
        Console.Write("Heuristics: [");
        Heuristics.ForEach(h =>
        {
            Console.Write(h);
            if (Heuristics.Last() != h)
            {
                Console.Write("; ");
            }
        });
        Console.WriteLine("]");
        
        // init dictionary
        InitDictionary();

        // get input file
        string filePath = TestPath("test.dimacs");
        Console.WriteLine("File: " + filePath);

        // load formula from that file
        Formula F = LoadFormula(filePath);

        // run heuristics
        AsyncDPLL(F);

        // Print common data
        Console.WriteLine("\n================================");
        Console.WriteLine("Amount of Variables: " + F.UniqueLiterals().Count);
        Console.WriteLine("Amount of Clauses:   " + F.Clauses.Count);
        Console.WriteLine("================================\n");

        Console.WriteLine("Program Finished !");
    }
    /*
     * END OF MAIN
     * ============================================================ */



    // For each heuristic init counter to 1
    static void InitDictionary()
    {
        foreach (string heuristic in Heuristics)
        {
            GlobalCounter.Add(heuristic, 1);
        }
    }

    // Only tests if there is a dimacs file in the current directory
    // and if not, then ask for name (full path would also work)
    static string TestPath(string filePath)
    {
        string result = filePath;

        while (!File.Exists(result))
        {
            Console.Write("Could not find file: ");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(result);
            Console.ResetColor();

            Console.Write(" at: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Directory.GetCurrentDirectory() + "\\");
            Console.ResetColor();

            Console.Write("Please provide filename that can be found at ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Directory.GetCurrentDirectory() + "\\");
            Console.ResetColor();

            Console.Write(":> ");
            string filename = Console.ReadLine();
            if (filename == null)
            {
                continue;
            }
            
            if (filename.Split('.').Length == 2)
            {
                result = filename;
            }
            else
            {
                result = filename + ".dimacs";
            }
        }

        return result;
    }

    // Practically only calls DimacsParser, but also catches exceptions from the Parser
    // (primarily for debugging purposes)
    static Formula LoadFormula(string filePath)
    {
        try
        {
            return DimacsParser.Parse(filePath);
        }
        catch (IOException e)
        {
            Console.WriteLine("IO error: Could not read file: " + filePath);
            Console.WriteLine(e);
            throw e;
        }
        catch (FormatException e)
        {
            Console.WriteLine("DimcasParser error: Could not parse file: " + filePath);
            Console.WriteLine("Tried to parse char to int");
            Console.WriteLine(e);
            throw e;
        }
        catch (OverflowException e)
        {
            Console.WriteLine("DimacsParser error: Could not parse file: " + filePath);
            Console.WriteLine("Tried to parse int, that would overflow " + Int32.MaxValue);
            Console.WriteLine(e);
            throw e;
        }
    }


    // For each heuristic create new Thread and call DPLL
    static void AsyncDPLL(Formula F)
    {

        Task[] tasks = new Task[Heuristics.Count];
        int heuristicIterator = 0;
        foreach (string heuristic in Heuristics)
        {
            tasks[heuristicIterator] = Task.Run(() =>
            {
                TestDPLL(F.Copy(), heuristic);
                return 0;
            });
            if (heuristicIterator < Heuristics.Count - 1)   // DEV-NOTE: yes, this IF is necessary (it would create OutOfBounds Exception for the array of threads)
            {
                heuristicIterator++;
            }
        }

        Task.WaitAll(tasks);    // wait for all threads to finish before the Program can end
    }



    // Helper method, that calls DPLL with the given heuristic and counts time, after DPLL finishes, it gets printed
    static void TestDPLL(Formula F, string H)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        bool result = DPLL(F, H);
        sw.Stop();
        Log(result, sw, H, "console");
    }


    // Helper method, that prints the result of DPLL to the console or file
    static void Log(bool result, Stopwatch sw, string heuristic, string whereTo)
    {
        if (whereTo.ToLower() == "console")
        {
            Console.WriteLine("\n--------------------------------\n" +
                              "Used Heuristic: " + heuristic + "\n" +
                              "Satisfiable?:   " + (result ? "YES" : "NO") + "\n" +
                              "DPLL calls:     " + GlobalCounter[heuristic] + "\n" +
                              "Time elapsed:   " + sw.Elapsed.Minutes + "min " + sw.Elapsed.Seconds + "s " + sw.Elapsed.Milliseconds + "ms");
        }
        else
        {
            // TODO find file path
        }
    }


    /* ============================================================
     * DPLL
     */
    static bool DPLL(Formula F, string H)
    {
        lock (GlobalCounter)
        {
            GlobalCounter[H]++;
        }



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

            return DPLL(F, H);
        }

        // find pureLiteral and if it exists, remove all clauses containing this literal
        int? pureLiteral = F.GetPureLiteral();
        if (pureLiteral != null)
        {
            F.Clauses.RemoveAll(c => c.Literals.Any(l => l == pureLiteral));
            return DPLL(F, H);
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
            case "FIRSTFIRST":
                selected = Heuristic.FIRSTFIRST(F);
                break;
            case "RANDOM":
                selected = Heuristic.RANDOM(F);
                break;
            default:
                throw new Exception("Heuristic not found");
        }
        Formula Fcopy = F.Copy();



        // test if removing negation of this literal resulted in SAT
        Fcopy.RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(-selected);


        // for the first 2 recursions, run DPLL for selected and -selected on different threads
        // then select the one that results in SAT
        if (GlobalCounter[H] < 3)
        {
            Task<bool> task = Task.Run(() => DPLL(Fcopy, H));

            F.RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(selected);
            bool result = DPLL(F, H);

            // if one thread finished faster, wait for the second thread
            task.Wait();
            if (task.Result)
            {
                return true;
            }
            else
            {
                return result;
            }
        }
        // for deeper recursions run on single thread
        else
        {
            if (DPLL(Fcopy, H))
            {
                return true;
            }
            // otherwise remove not-negative instance of this literal
            else
            {
                F.RemoveClausesContainingLiteralAndRemoveNegationOfLiteralFromAllOtherClauses(selected);
                return DPLL(F, H);
            }
        }
    }
}


