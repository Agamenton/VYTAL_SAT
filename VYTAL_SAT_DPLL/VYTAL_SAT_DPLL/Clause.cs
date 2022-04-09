using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VYTAL_SAT_DPLL
{
    public class Clause
    {
        public List<int> Literals { get; set; }

        public Clause()
        {
            Literals = new List<int>();
        }



        public Clause Copy()
        {
            Clause copy = new Clause();
            foreach (int literal in Literals)
            {
                copy.Literals.Add(literal);
            }

            return copy;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (int literal in Literals)
            {
                sb.Append(literal);
                if (literal != Literals.Last())
                {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
