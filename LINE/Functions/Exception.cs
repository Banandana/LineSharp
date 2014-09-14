using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Functions
{
    class Exceptions
    {
        internal static void Display(Exception E)
        {
            Console.WriteLine(E.Message);
            Console.WriteLine(E.Source);
            Console.WriteLine(E.StackTrace);
        }
    }
}
