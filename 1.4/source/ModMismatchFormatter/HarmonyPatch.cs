using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

            MethodInfo DoWindowContents_original = AccessTools.Method(typeof(RimWorld.Dialog_ModMismatch), "DoWindowContents");
            MethodInfo DoWindowContents_prefix = AccessTools.Method(typeof(HarmonyPatch), "Prefix_DoWindowContents");
            harmony.Patch(DoWindowContents_original, new HarmonyMethod(DoWindowContents_prefix));
        }

        static bool Prefix_TryCreateDialogForVersionMismatchWarnings(ref bool __result, Action confirmedAction)
        {
            Verse.Log.Message("TryCreateDIalogForVersionMismatchWarnings called");
            return true;
        }

        static bool Prefix_DoWindowContents(Rect rect)
        {
            Renderer.DoWindowContents(rect);
            return false;
        }

        // find starting index of given opcode list
        static int FindIndexOf(List<CodeInstruction> instructions, List<OpCode> target)
        {
            for (int i = 0; (i < instructions.Count) && ((i + target.Count) <= instructions.Count); i++)
            {
                if (i < target.Count - 1)
                {
                    continue;
                }

                var slice = instructions.Skip(i - (target.Count - 1)).Take(target.Count);
                if (slice.Count() != target.Count)
                {
                    throw new Exception("target and founded slice's length doesn't match.");
                }

                var zipped = slice.Zip(target, (first, second) => (first, second));

                if (zipped.All((tuple) => tuple.first.opcode == tuple.second))
                {
                    return i - (target.Count - 1);
                }
            }

            return -1;
        }
    }
}
