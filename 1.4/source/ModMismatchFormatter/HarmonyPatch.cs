using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

            Harmony harmony = new Harmony("Madeline.ModMismatchWindowPatch");
            MethodInfo original = AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings");
            MethodInfo prefix = AccessTools.Method(typeof(HarmonyPatch), "Prefix_TryCreateDialogForVersionMismatchWarnings");
            harmony.Patch(original, new HarmonyMethod(prefix));

            MethodInfo DoModList_original = AccessTools.Method(typeof(RimWorld.Dialog_ModMismatch), "DoModList");
            MethodInfo DoModList_prefix = AccessTools.Method(typeof(HarmonyPatch), "Prefix_DoModList");
            harmony.Patch(DoModList_original, new HarmonyMethod(DoModList_prefix));
        }

        static bool Prefix_TryCreateDialogForVersionMismatchWarnings(ref bool __result, Action confirmedAction)
        {
            Verse.Log.Message("TryCreateDIalogForVersionMismatchWarnings called");
            return true;
        }

        static bool Prefix_DoModList(ref Rect r, ref List<string> modList, ref Vector2 scrollPos, ref Color? rowColor)
        {
            Log.Message("prefix_domodlist called");
            return false;
        }
    }
}
