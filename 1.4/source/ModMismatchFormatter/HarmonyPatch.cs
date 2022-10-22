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
            MethodInfo DoWindowContents_transpiler = AccessTools.Method(typeof(HarmonyPatch), "Transpiler_DoWindowContents");
            harmony.Patch(DoWindowContents_original, null, null, new HarmonyMethod(DoWindowContents_transpiler));

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
            return true;
        }

        static IEnumerable<CodeInstruction> Transpiler_DoWindowContents(IEnumerable<CodeInstruction> originalInstructions)
        {
            Log.Message("original insns");
            foreach(var insn in originalInstructions)
            {
                Log.Message(insn.ToString());
            }
            
            var insns = originalInstructions.ToList();
            var result = new List<CodeInstruction>();

            // step 1. find label IL_012A which is our jmp target.
            var IL_00D6_target = new List<OpCode>() { OpCodes.Stloc_1, OpCodes.Ldarg_0, OpCodes.Ldfld, OpCodes.Call, OpCodes.Brtrue_S };
            var off = FindIndexOf(insns, IL_00D6_target);
            if (off == -1)
            {
                throw new Exception("cannot find insn which opcode label is IL_012A");
            }
            var IL_012A_label = insns[off].operand;

            // step 2. find IL_00CB
            var IL_00CB_target = new List<OpCode>() { OpCodes.Ldc_R4, OpCodes.Add, OpCodes.Add, OpCodes.Stloc_1, OpCodes.Ldarg_0 };
            var off2 = FindIndexOf(insns, IL_00CB_target);
            if (off2 == -1)
            {
                throw new Exception("cannot find insn IL_00CB");
            }
            off2 += 4; // because off2 is pointing 4 previous insn.

            var IL_012A_target = new List<OpCode>() { OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_1, OpCodes.Ldloc_0, OpCodes.Call };
            var off3 = FindIndexOf(insns, IL_012A_target);
            if (off3 == -1)
            {
                throw new Exception("cannot find insn IL_012A");
            }

            // insert insns before IL_00CB
            result.AddRange(insns.Take(off2));
            result.Add(new CodeInstruction(OpCodes.Jmp, IL_012A_label));

            // insert remaining ILs

            result.AddRange(insns.Skip(off3));

            Log.Message("patched insns");
            foreach(var insn in result) {
                Log.Message(insn.ToString());
            }

            return result;
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

                Log.Message("found");
                if (zipped.All((tuple) => tuple.first.opcode == tuple.second))
                {
                    return i - (target.Count - 1);
                }
            }

            return -1;
        }
    }
}
