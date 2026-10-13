using System;
using System.Linq;
using HarmonyLib;
using Madeline.ModMismatchFormatter.Core;
using Madeline.ModMismatchFormatter.UI;
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

        // Same mods, same versions: let vanilla check the game version.
        if (ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out _, out _) && (!compareVersions || !AnyVersionChanged()))
            return true;

        Find.WindowStack.Add(new ModMismatchWindow(confirmedAction, compareVersions));
        __result = true;
        return false;
    }

    private static bool AnyVersionChanged() =>
        LoadedModManager.RunningMods.Any(mod => ModEntry.VersionChanged(ModVersions.Saved(mod.PackageId), ModVersions.Running(mod)));
}
