using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VYTAL_SAT_DPLL
{
    public class DimacsParser
    {


        public static Formula Parse(string path)
        {
            List<Clause> clauses = new List<Clause>();
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("c"))
                {
                    Console.WriteLine(line);
                    continue;
                }
                else if (line.StartsWith("p"))
                {
                    Console.WriteLine(line);
                    continue;
                }
                else if (line.StartsWith("%"))
                {
                    continue;
                }
                else
                {
                    Clause clause = new Clause();
                    string[] literals = line.Split(' ');
                    foreach (string literal in literals)
                    {
                        if (literal.StartsWith("0"))
                        {
                            break;
                        }
                        else
                        {
                            clause.Literals.Add(Convert.ToInt32(literal));
                        }
                    }
                    clauses.Add(clause);
                }
            }
            file.Close();
            return new Formula(clauses);
        }
    }
}
