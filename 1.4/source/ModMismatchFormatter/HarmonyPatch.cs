using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModMismatchFormatter
{
    public static class HarmonyPatch
    {
        private static bool patched = false;

        public static void Patch()
        {
            if (patched)
            {
                return;
            }

            Log.Message("Patching Madeline.ModMismatchWindow");

            Harmony HMInstance = new Harmony("Madeline.ModMismatchWindowPatch");
            MethodInfo original = AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings");
            MethodInfo prefix = AccessTools.Method(typeof(HarmonyPatch), "Prefix_TryCreateDialogForVersionMismatchWarnings");
            HMInstance.Patch(original, new HarmonyMethod(prefix));
        }

        static bool Prefix_TryCreateDialogForVersionMismatchWarnings(ref bool __result, Action confirmedAction)
        {
            Verse.Log.Message("TryCreateDIalogForVersionMismatchWarnings called");
            return true;
        }
    }
}
