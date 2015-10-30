using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelperBot
{
    class AdminCommands
    {
        public static Dictionary<string, int> Index = new Dictionary<string, int>();

        public static void Initialize()
        {
            Index.Add("help", 0);
            Index.Add("load", 1);
            Index.Add("clear", 2);
            Index.Add("save", 3);
            Index.Add("kick", 4);
            Index.Add("ban", 5);
            Index.Add("unban", 6);
        }
    }
}
