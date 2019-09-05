using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace ModComparerPatch.HMPatch
{
    public static class ScribeMetaDataUtilityPatch
    {
        public static void Patch(HarmonyInstance HMInstance)
        {

        }

        public static bool LoadedModsMatchesActiveModsPrefix()
        {
            return false;
        }
    }
}
