using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Globals
{
    class URL
    {
        //https://54.251.6.74/
        //https://gd2.line.naver.jp:443/
        //gd2u.line.naver.jp:443
        internal static string Root
        {
            get { return "https://gd2u.line.naver.jp:443"; }
        }

        internal static string Key
        {
            get { return Root + "/authct/v1/keys/line"; }
        }

        internal static string TalkService
        {
            get { return Root + "/api/v4/TalkService.do"; }
        }

        internal static string P
        {
            get { return Root + "/P4"; }
        }

        internal static string S
        {
            get { return Root + "/S4"; }
        }

        internal static string Q
        {
            get { return Root + "/Q"; }
        }
    }
}
