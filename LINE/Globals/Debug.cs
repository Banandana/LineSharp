using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Globals
{
    class Debug
    {
        internal static void Print(string a)
        {
#if DEBUG
            Console.WriteLine(a);
#endif
        }
    }
}
