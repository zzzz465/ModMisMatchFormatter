using System;
using System.Collections.Generic;
using System.Linq;

namespace Madeline.ModMismatchFormatter
{
    public class ModPresenter : iOrderFormatter
    {
        public ModPresenter()
        {

        }

        public IEnumerable<ModPair> GetFormattedModPairs(List<Mod> ModsFromSave, List<Mod> ModsFromActive)
        {
            ModsFromSave = EnsureModIsValidOrderAndOmitPlaceHolder(ModsFromSave);
            ModsFromActive = EnsureModIsValidOrderAndOmitPlaceHolder(ModsFromActive);

            List<ModPair> modPairList = new List<ModPair>();

            var SaveEnumerator = ModsFromSave.GetEnumerator();
            var ActiveEnumerator = ModsFromActive.GetEnumerator();

            while(SaveEnumerator.MoveNext() && ActiveEnumerator.MoveNext())
            {  
                var SaveCurrent = SaveEnumerator.Current; // index is always 0
                var ActiveCurrent = ActiveEnumerator.Current; // index is always 0

                if(ModsFromSave.IndexOf(SaveCurrent) != 0 || ModsFromActive.IndexOf(ActiveCurrent) != 0)
                    throw new Exception("Error in sorting logic.");

                if(SaveCurrent.Identifier == ActiveCurrent.Identifier)
                {
                    modPairList.Add(new ModPair(SaveCurrent, ActiveCurrent));
                    continue;
                }

                var saveModInActiveModList = ModsFromActive.Find(mod => mod.Identifier == SaveCurrent.Identifier);
                var activeModInSaveModList = ModsFromActive.Find(mod => mod.Identifier == ActiveCurrent.Identifier);

                //둘다 존재할경우
                if(saveModInActiveModList != null && activeModInSaveModList != null)
                {
                    int SaveGapBetweenCurrentAndTarget = ModsFromActive.IndexOf(SaveModInActiveModList) - ActiveModsToAdd.IndexOf(ActiveCurrent);
                    int ActiveGapBetweenCurrentAndTarget = SaveModsToAdd.IndexOf(ActiveModInSaveModList) - SaveModsToAdd.IndexOf(SaveCurrent);
                }

                var placeHolder = new Mod();

                //if(SaveCurrentIndexFromActiveMods != null && ActiveCurrentIndexFromSaveMods != null)
                { // 네이밍을 어떻게 해야할까..
                    int ModCountNeeded_ToMatchSave = ModsFromActive.IndexOf(SaveCurrent);
                    int ModCountNeeded_ToMatchActive = ModsFromSave.IndexOf(ActiveCurrent);

                    if(ModCountNeeded_ToMatchActive > ModCountNeeded_ToMatchSave)
                    { // Save를 먼저 올린다
                        modPairList.Add(new ModPair(SaveCurrent, placeHolder));
                        ModsFromSave.RemoveAt(0);
                        SaveCurrent = ModsFromSave.First();
                    }
                    else
                    { // Active를 먼저 올린다
                        modPairList.Add(new ModPair(placeHolder, ActiveCurrent));
                        ModsFromActive.RemoveAt(0);
                        ActiveCurrent = ModsFromActive.First();
                    }
                }
                //한쪽에만 존재할경우
                  // saved에만 존재할경우
                else if(SaveCurrentIndexFromActiveMods != null && ActiveCurrentIndexFromSaveMods == null)
                {
                    modPairList.Add(new ModPair(SaveCurrent, placeHolder));
                    ModsFromSave.RemoveAt(0);
                    SaveCurrent = ModsFromSave.First();
                }
                  // active에만 존재할경우
                else if(ActiveCurrentIndexFromSaveMods != null && SaveCurrentIndexFromActiveMods == null)
                {
                    modPairList.Add(new ModPair(placeHolder, ActiveCurrent));
                    ModsFromActive.RemoveAt(0);
                    ActiveCurrent = ModsFromActive.First();
                }
                else
                { // 서로 1개씩만 남았거나, 기타 예외사항
                    modPairList.Add(new ModPair(SaveCurrent, new Mod()));
                    ModsFromSave.RemoveAt(0);
                    if(ModsFromSave.Count > 0)
                        SaveCurrent = ModsFromSave.First();
                }
            }

            if(ModsFromActive.Count > 0 && ModsFromSave.Count == 0)
            {
                foreach(var ActiveMod in ModsFromActive)
                {
                    modPairList.Add(new ModPair(new Mod(), ActiveMod));
                }
            }
            else if(ModsFromSave.Count > 0 && ModsFromActive.Count == 0)
            {
                foreach(var SaveMod in ModsFromSave)
                {
                    modPairList.Add(new ModPair(SaveMod, new Mod()));
                }
            }
            else
                throw new Exception("Main Logic is ended but both list's count is higher than 0");

            return modPairList;
        }

        private int? GetIndexFromList(Mod mod, List<Mod> mods)
        {
            var result = mods.Find(item => item.Identifier == mod.Identifier);
            if (result != null)
                mods.IndexOf(result);

            return null;
        }

        private List<Mod> EnsureModIsValidOrderAndOmitPlaceHolder(List<Mod> mods)
        {
            return mods.Where(mod => (!mod.isPlaceHolder)).OrderBy(mod => mod.Order).ToList();
        }
    }
}