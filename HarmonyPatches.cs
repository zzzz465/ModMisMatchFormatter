using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using Madeline.ModMismatchFormatter;
using System.IO;
using System.Xml.Linq;
using System.Reflection.Emit;

namespace ModMisMatchWindowPatch
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("Patching Madeline.ModMismatchWindow");
            Harmony HMInstance = new Harmony("Madeline.ModMismatchWindowPatch");
            MethodInfo original = AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings");
            MethodInfo prefix = AccessTools.Method(typeof(HarmonyPatches), "Prefix_TryCreateDialogForVersionMismatchWarnings");
            HMInstance.Patch(original, new HarmonyMethod(prefix));

            var SaveGameOriginal = AccessTools.Method(typeof(GameDataSaveLoader), "SaveGame");
            var SaveGamePrefix = AccessTools.Method(typeof(HarmonyPatches), "Prefix_SaveGame");
            var SaveGamePostfix = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Postfix_SaveGame));
            HMInstance.Patch(SaveGameOriginal, new HarmonyMethod(SaveGamePrefix), new HarmonyMethod(SaveGamePostfix));

            var CheckVersionAndLoadOriginal = AccessTools.Method(typeof(PreLoadUtility), "CheckVersionAndLoad");
            var CheckVersionAndLoadPrefix = AccessTools.Method(typeof(MetaHeaderUtility), "UpdateLastAccessedSaveFileInLoadSelection");
            HMInstance.Patch(CheckVersionAndLoadOriginal, new HarmonyMethod(CheckVersionAndLoadPrefix));
        }

        [HarmonyPriority(9999)]
        static bool Prefix_TryCreateDialogForVersionMismatchWarnings(ref bool __result, Action confirmedAction)
        {
            SimpleLog.Log("Checking mod mismatch");
            bool isModListSame = ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out _, out _);
            bool useVersionCompare = ModMismatchFormatter.useVersionCompare;

            if(isModListSame)
            {
                SimpleLog.Log("ModList are same");
                if(useVersionCompare)
                {
                    SimpleLog.Log("use version checking");
                    bool isVersionSame = MetaHeaderUtility.isVersionSame(ModContentPackExtension.GetModsFromSave(useVersionCompare), ModContentPackExtension.GetModsFromActive(useVersionCompare));
                    if(!isVersionSame)
                    {
                        SimpleLog.Log("Version is different...");
                        string warningMessage = "Checking mismatch for mod and mod versions... please wait";
                        Messages.Message(warningMessage, MessageTypeDefOf.SilentInput, false);
                        CreateModMismatchWindow(confirmedAction, useVersionCompare);
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return true;
            }
            else
            {
                SimpleLog.Log("Mod mismatch!");
                CreateModMismatchWindow(confirmedAction, useVersionCompare);
                __result = true;
                return false;
            }
        }

        [HarmonyPriority(9999)]
        static void Prefix_SaveGame(string fileName)
        {
            SimpleLog.Log("Caching last saved file path");
            bool WriteMeta = ModMismatchFormatter.writeMetaToSave;
            if(WriteMeta)
            {
                MetaHeaderUtility.StoreLastSavedFilePath(fileName);
            }
        }

        static void Postfix_SaveGame()
        {
            SimpleLog.Log("Trying to write additional meta headers to save file...");
            bool WriteMeta = ModMismatchFormatter.writeMetaToSave;
            if(WriteMeta)
            {
                try
                {
                    MetaHeaderUtility.UpdateModVersionMetaHeader();
                }
                catch(Exception ex)
                {
                    string report = $"exception was raised and cannot write mod meta headers to the save. mod versions are not saved. your save is fine though.\n==original exception==\n{ex.ToString()}";
                    Log.Error(report);
                }
            }
        }

        static void CreateModMismatchWindow(Action confirmedAction, bool useVersionCompare)
        {
                var renderer = new ModListerElementRenderer();
                renderer.useVersionChecking = useVersionCompare;
                var formatter = new OrderFormatterImpl();
                Find.WindowStack.Add(new Madeline.ModMismatchFormatter.ModMismatchWindow(renderer,
                                                                                         formatter,
                                                                                         confirmedAction, useVersionCompare));
        }
    }
}
