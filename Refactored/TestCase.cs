using System;
using System.Collections.Generic;
using System.Linq;

namespace Madeline.ModMismatchFormatter
{
    public static class SorterTester
    {
        public static void DoTest(iOrderFormatter formatter, IEnumerable<Mod> saveMods, IEnumerable<Mod> activeMods)
        {
            var result = formatter.GetFormattedModPairs(saveMods.ToList(), activeMods.ToList());
        }
    }
}