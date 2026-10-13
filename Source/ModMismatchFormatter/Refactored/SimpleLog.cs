using System;
using System.Linq;
using System.Collections.Generic;
namespace Madeline.ModMismatchFormatter
{
    
    public static class SimpleLog
    {
        public static bool PrintLog = false;
        public static void Log(string msg)
        {
            if(PrintLog)
                Verse.Log.Message(msg);
        }
    }
    
}