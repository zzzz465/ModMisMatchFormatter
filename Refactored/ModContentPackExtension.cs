using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Madeline.ModMismatchFormatter
{
    public static class ModContentPackExtension
    {
        static Dictionary<ModContentPack, ModMetaData> pairs = new Dictionary<ModContentPack, ModMetaData>();
        public static ModMetaData GetMetaData(this ModContentPack pack)
        {
            if(!(pairs.ContainsKey(pack)))
            {
                SetModContentPackInDict(pack);
            }

            return pairs[pack];
        }

        static void SetModContentPackInDict(ModContentPack pack)
        {
            var result = from meta in ModsConfig.ActiveModsInLoadOrder
                         where meta.Name == pack.Name
                         select meta;
            pairs[pack] = result.First();
        }

        public static List<Mod> GetModsFromActive(bool useVersionChecking = false)
        {
            List<Mod> activeMods = new List<Mod>();
            foreach(var RimworldMod in LoadedModManager.RunningMods)
            {
                var mod = new Mod(RimworldMod.PackageId, RimworldMod.Name, RimworldMod.loadOrder);
                if(useVersionChecking)
                    mod.Version = MetaHeaderUtility.GetVersionFromManifestFile(RimworldMod);
                activeMods.Add(mod);
            }
            return activeMods;
        }
        public static List<Mod> GetModsFromSave(bool CheckModVersion = false)
        {
            List<Mod> saveMods = new List<Mod>();
            if (ScribeMetaHeaderUtility.loadedModNamesList != null && ScribeMetaHeaderUtility.loadedModIdsList != null)
            {
                CheckSaveHeaderValid();
                saveMods = ReadModsFromSaveHeader(CheckModVersion);
            }
            return saveMods;
        }

        static void CheckSaveHeaderValid()
        {
            var modNamesCount = ScribeMetaHeaderUtility.loadedModNamesList.Count;
            var modIdsCount = ScribeMetaHeaderUtility.loadedModIdsList.Count;
            if(modNamesCount != modIdsCount)
                throw new Exception("Mod Name length and ModID length in savefile is not matched");
        }

        static List<Mod> ReadModsFromSaveHeader(bool readModVersion)
        {
            List<Mod> saveMods = new List<Mod>();
            MetaHeaderUtility.BeginReading(MetaHeaderUtility.LastAccessedSaveFilePathInLoadSelection);
            for(int i = 0; i < ScribeMetaHeaderUtility.loadedModNamesList.Count; i++)
            {
                var mod = new Mod(ScribeMetaHeaderUtility.loadedModIdsList[i], ScribeMetaHeaderUtility.loadedModNamesList[i], i);
                string modName = mod.ModName;
                if(readModVersion)
                    mod.Version = MetaHeaderUtility.GetVersion(modName);
                saveMods.Add(mod);
            }
            MetaHeaderUtility.EndReading();
            return saveMods;
        }
    }
}