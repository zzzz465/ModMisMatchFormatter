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
            HMInstance.Patch(SaveGameOriginal, new HarmonyMethod(SaveGamePrefix));

            var CheckVersionAndLoadOriginal = AccessTools.Method(typeof(PreLoadUtility), "CheckVersionAndLoad");
            var CheckVersionAndLoadPrefix = AccessTools.Method(typeof(MetaHeaderUtility), "UpdateLastAccessedSaveFileInLoadSelection");
            HMInstance.Patch(CheckVersionAndLoadOriginal, new HarmonyMethod(CheckVersionAndLoadPrefix));
        }

        [HarmonyPriority(9999)]
        static bool Prefix_TryCreateDialogForVersionMismatchWarnings(ref bool __result, Action confirmedAction)
        {
            bool isModListSame = ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out _, out _);
            bool useVersionCompare = ModMismatchFormatter.useVersionCompare;

            if(isModListSame)
            {
                if(useVersionCompare)
                {
                    bool isVersionSame = MetaHeaderUtility.isVersionSame(ModContentPackExtension.GetModsFromSave(), ModContentPackExtension.GetModsFromActive());
                    if(!isVersionSame)
                    {
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
                CreateModMismatchWindow(confirmedAction, useVersionCompare);
                __result = true;
                return false;
            }
        }

        [HarmonyPriority(9999)]
        static bool Prefix_SaveGame(string fileName)
        {
            bool WriteMeta = ModMismatchFormatter.writeMetaToSave;
            if(WriteMeta)
            {
                try
                {
                    MetaHeaderUtility.UpdateModVersionMetaHeader(fileName);
                }
                catch(Exception ex)
                {
                    string report = $"exception was raised and cannot write mod meta headers to the save. mod versions are not saved. your save is fine though.\n==original exception==\n{ex.ToString()}";
                    Log.Error(report);
                }
            }
            return true;
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
