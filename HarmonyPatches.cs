using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using System.Reflection;
using System.Reflection.Emit;

namespace ModMisMatchWindowPatch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance HMInstance = HarmonyInstance.Create("Madeline");
            MethodInfo original = AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings");
            MethodInfo prefix = AccessTools.Method(typeof(HarmonyPatches), "Prefix");
            HMInstance.Patch(original, new HarmonyMethod(prefix));
        }

        public static bool Prefix(ref bool __result, Action confirmedAction)
        {
            if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out _, out _))
            {
                Find.WindowStack.Add(new ModMisMatchWindow());
                __result = true;
                return false;
            }
            else
            {
                __result = false;
                return true;
            }
        }
    }
}
