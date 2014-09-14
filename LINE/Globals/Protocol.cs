using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Globals
{
    class Protocol
    {
        internal static string LineApplication
        {
           // get { return "DESKTOPWIN	3.7.0.34	WINDOWS	6.1.7601-7-x64"; }
            get { return "DESKTOPWIN\t3.2.1.83\tWINDOWS\t5.1.2600-XP-x64"; }
        }

        internal static string UserAgent
        {
            get { return "DESKTOP:WINDOWS:6.1.7601-7-x64(3.2.0.76)"; }
        }
    }
}
