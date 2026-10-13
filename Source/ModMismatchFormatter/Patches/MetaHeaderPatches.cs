using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Madeline.ModMismatchFormatter.Patches;

/// <summary>Remembers whether the meta header being written belongs to a savegame.</summary>
[HarmonyPatch(typeof(GameDataSaveLoader), nameof(GameDataSaveLoader.SaveGame))]
internal static class SaveGamePatch
{
    public static bool IsSavingGame { get; private set; }

    private static void Prefix() => IsSavingGame = true;

    private static void Finalizer() => IsSavingGame = false;
}

/// <summary>Adds the running mod versions to a savegame's meta header while it is written.</summary>
[HarmonyPatch(typeof(ScribeMetaHeaderUtility), nameof(ScribeMetaHeaderUtility.WriteMetaHeader))]
internal static class WriteMetaHeaderPatch
{
    // WriteMetaHeader is: if (Scribe.EnterNode("meta")) try { ... } finally { Scribe.ExitNode(); }
    // Writing right before that ExitNode puts the versions inside <meta>.
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var result = CallInserter.InsertBefore(
            instructions,
            AccessTools.Method(typeof(Scribe), nameof(Scribe.ExitNode)),
            AccessTools.Method(typeof(WriteMetaHeaderPatch), nameof(WriteModVersions)),
            out bool inserted);

        if (!inserted)
            ModLog.Error("Could not find Scribe.ExitNode in WriteMetaHeader; mod versions will not be saved.");

        return result;
    }

    private static void WriteModVersions()
    {
        if (Scribe.mode != LoadSaveMode.Saving || !SaveGamePatch.IsSavingGame || !ModMismatchFormatter.Settings.saveVersionToSaveFile)
            return;

        try
        {
            ModVersions.WriteToMetaHeader();
        }
        catch (Exception ex)
        {
            // Never break saving: the save is fine, it just will not carry mod versions.
            ModLog.Error($"Could not write mod versions to the save. The save itself is fine.\n{ex}");
        }
    }
}

/// <summary>Reads the recorded mod versions whenever a meta header is loaded.</summary>
[HarmonyPatch(typeof(ScribeMetaHeaderUtility), nameof(ScribeMetaHeaderUtility.LoadGameDataHeader))]
internal static class LoadGameDataHeaderPatch
{
    private static void Postfix()
    {
        if (Scribe.mode != LoadSaveMode.LoadingVars)
            return;

        try
        {
            ModVersions.ReadFromLoadedMetaHeader();
        }
        catch (Exception ex)
        {
            ModLog.Error($"Could not read mod versions from the save.\n{ex}");
        }
    }
}
