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

        public static List<Mod> GetModsFromActive()
        {
            List<Mod> activeMods = new List<Mod>();
            foreach(var RimworldMod in LoadedModManager.RunningMods)
            {
                var mod = new Mod(RimworldMod.Identifier, RimworldMod.Name, RimworldMod.loadOrder);
                mod.Version = MetaHeaderUtility.GetVersionFromManifestFile(RimworldMod);
                activeMods.Add(mod);
            }
            return activeMods;
        }
        public static List<Mod> GetModsFromSave()
        {
            List<Mod> saveMods = new List<Mod>();
            if (ScribeMetaHeaderUtility.loadedModNamesList != null && ScribeMetaHeaderUtility.loadedModIdsList != null)
            { // save
                var modnameList = ScribeMetaHeaderUtility.loadedModNamesList.Count;
                var modIdList = ScribeMetaHeaderUtility.loadedModIdsList.Count;
                if(modnameList != modIdList)
                    throw new Exception("Mod Name length and ModID length in savefile is not matched");

                MetaHeaderUtility.BeginReading(MetaHeaderUtility.LastAccessedSaveFilePathInLoadSelection);
                for(int i = 0; i < modnameList; i++)
                {
                    var mod = new Mod(ScribeMetaHeaderUtility.loadedModIdsList[i], ScribeMetaHeaderUtility.loadedModNamesList[i], i);
                    mod.Version = MetaHeaderUtility.GetVersion(i);
                    saveMods.Add(mod);
                }
                MetaHeaderUtility.EndReading();
            }
            return saveMods;
        }
    }
}