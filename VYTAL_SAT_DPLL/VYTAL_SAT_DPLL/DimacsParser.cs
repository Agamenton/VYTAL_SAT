using System.Globalization;

namespace VYTAL_SAT_DPLL
{
    public class DimacsParser
    {


        public static Formula Parse(string path)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(path);


            string fileText = "";

            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("c"))
                {
                    continue;
                }
                if (line.StartsWith("p"))
                {
                    continue;
                }
                if (line.StartsWith("%"))
                {
                    continue;
                }

                fileText += " ";
                fileText += line;
            }
            file.Close();
            
            fileText = fileText.Trim();
            fileText = fileText.Replace("\t", " ");

            string[] elements = fileText.Split(' ');

            Formula formula = new Formula();


            Clause clause = new Clause();

            for (long i = 0; i < elements.Length; i++)
            {
                if (elements[i] == "0")
                {
                    formula.Clauses.Add(clause);
                    clause = new Clause();
                }
                else if (elements[i] == "")
                {
                    continue;
                }
                else
                {
                    clause.Literals.Add(Convert.ToInt32(elements[i]));
                }
            }

            return formula;
        }
    }
}
