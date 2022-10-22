using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModMismatchFormatter
{
    public static class ModUtils
    {
        public static List<ModContentPack> GetEnabledMods()
        {
            return LoadedModManager.RunningMods.ToList();
        }
    }
}
