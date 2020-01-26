using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
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
            HarmonyInstance HMInstance = HarmonyInstance.Create("Madeline");
            MethodInfo original = AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings");
            MethodInfo prefix = AccessTools.Method(typeof(HarmonyPatches), "Prefix");
            HMInstance.Patch(original, new HarmonyMethod(prefix));

            var SaveGameOriginal = AccessTools.Method(typeof(GameDataSaveLoader), "SaveGame");
            var SaveGamePostfix = AccessTools.Method(typeof(MetaHeaderUtility), "UpdateModVersionMetaHeader");
            HMInstance.Patch(SaveGameOriginal, null, new HarmonyMethod(SaveGamePostfix));

            var CheckVersionAndLoadOriginal = AccessTools.Method(typeof(PreLoadUtility), "CheckVersionAndLoad");
            var CheckVersionAndLoadPrefix = AccessTools.Method(typeof(MetaHeaderUtility), "UpdateLastAccessedSaveFileInLoadSelection");
            HMInstance.Patch(CheckVersionAndLoadOriginal, new HarmonyMethod(CheckVersionAndLoadPrefix));
        }

        [HarmonyPriority(9999)]
        static bool Prefix(ref bool __result, Action confirmedAction)
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
                        CreateModMismatchWindow(confirmedAction);
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return true;
            }
            else
            {
                CreateModMismatchWindow(confirmedAction);
                __result = true;
                return false;
            }
        }

        static void CreateModMismatchWindow(Action confirmedAction)
        {
                var renderer = new ModListerElementRenderer();
                var formatter = new OrderFormatterImpl();
                Find.WindowStack.Add(new Madeline.ModMismatchFormatter.ModMismatchWindow(renderer,
                                                                                         formatter,
                                                                                         confirmedAction));
        }
    }
}
