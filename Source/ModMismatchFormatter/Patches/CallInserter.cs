using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Madeline.ModMismatchFormatter.Patches;

internal static class CallInserter
{
    /// <summary>
    /// Inserts a call to <paramref name="insert"/> right before the first call to <paramref name="target"/>.
    /// Labels and exception blocks move to the inserted call, so it also works when the target call
    /// starts a finally block.
    /// </summary>
    public static List<CodeInstruction> InsertBefore(
        IEnumerable<CodeInstruction> instructions, MethodInfo target, MethodInfo insert, out bool inserted)
    {
        var result = new List<CodeInstruction>();
        inserted = false;

        foreach (var instruction in instructions)
        {
            if (!inserted && instruction.Calls(target))
            {
                result.Add(new CodeInstruction(OpCodes.Call, insert).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction));
                inserted = true;
            }

            result.Add(instruction);
        }

        return result;
    }
}
