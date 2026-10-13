using System;
using System.Linq;
using HarmonyLib;
using Madeline.ModMismatchFormatter.UI;
using RimWorld;
using Verse;

namespace Madeline.ModMismatchFormatter.Patches;

/// <summary>Replaces the vanilla mod mismatch dialog with <see cref="ModMismatchWindow"/>.</summary>
[HarmonyPatch(typeof(ScribeMetaHeaderUtility), nameof(ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings))]
internal static class MismatchDialogPatch
{
    [HarmonyPriority(9999)] // Ahead of other mods that replace this dialog.
    private static bool Prefix(Action confirmedAction, ref bool __result)
    {
        bool compareVersions = ModMismatchFormatter.Settings.useVersionCompare;

        if (ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out _, out _))
        {
            if (!compareVersions || !AnyVersionChanged())
                return true; // Same mods, same versions: let vanilla check the game version.

            Messages.Message("Checking mismatch for mod and mod versions... please wait", MessageTypeDefOf.SilentInput, false);
        }

        Find.WindowStack.Add(new ModMismatchWindow(confirmedAction, compareVersions));
        __result = true;
        return false;
    }

    private static bool AnyVersionChanged() =>
        LoadedModManager.RunningMods.Any(mod => ModVersions.Saved(mod.PackageId) != ModVersions.Running(mod));
}
