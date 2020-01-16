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

            List<Mod> FormattedSaveMods = new List<Mod>();
            List<Mod> FormattedActiveMods = new List<Mod>();

            List<ModPair> modPairList = new List<ModPair>();

            ModsFromSave.Reverse();
            ModsFromActive.Reverse();
            Mod SaveCurrent = ModsFromSave.Pop();
            Mod ActiveCurrent = ModsFromActive.Pop();

            while(ModsFromSave.Count > 0 && ModsFromActive.Count > 0) // 여기서 요소가 있다는 것을 보장함, 대신 다 끝나고 1개 또는 0개가 남을 수 있음
            {
                if(SaveCurrent.ModName == ActiveCurrent.ModName) // 동일할경우(그냥 추가하면 됨)
                {
                    FormattedActiveMods.Add(ActiveCurrent);
                    FormattedSaveMods.Add(SaveCurrent);
                    SaveCurrent = ModsFromSave.Pop();
                    ActiveCurrent = ModsFromActive.Pop();
                    continue;
                }

                //만약 둘다 존재할경우
                int saveCurrentIndex = ModsFromSave.IndexOf(SaveCurrent);
                int activeCurrentIndex = ModsFromActive.IndexOf(ActiveCurrent);
                Mod SaveModInActiveModList = ModsFromActive.AfterIndex(activeCurrentIndex).Where(item => item.ModName == SaveCurrent.ModName).FirstOrDefault(); // SaveModsToAdd.AfterIndex(saveCurrentIndex).Any(item => item.ModName == ActiveCurrent.ModName))
                Mod ActiveModInSaveModList = ModsFromSave.AfterIndex(saveCurrentIndex).Where(item => item.ModName == ActiveCurrent.ModName).FirstOrDefault();
                if (SaveModInActiveModList != null && ActiveModInSaveModList != null)
                {
                    int SaveGapBetweenCurrentAndTarget = ModsFromActive.IndexOf(SaveModInActiveModList) - ModsFromActive.IndexOf(ActiveCurrent);
                    int ActiveGapBetweenCurrentAndTarget = ModsFromSave.IndexOf(ActiveModInSaveModList) - ModsFromSave.IndexOf(SaveCurrent);

                    if (SaveGapBetweenCurrentAndTarget > ActiveGapBetweenCurrentAndTarget) // 만약 Save의 gap이 더 클 경우 Active을 먼저 더해주자
                    {
                        FormattedActiveMods.Add(ActiveCurrent);
                        FormattedSaveMods.Add(new Mod());
                        ActiveCurrent = ModsFromActive.Pop();
                    }
                    else // Active의 Gap이 더 클경우 Save를 먼저 더해주자.
                    {
                        FormattedSaveMods.Add(SaveCurrent);
                        FormattedActiveMods.Add(new Mod());
                        SaveCurrent = ModsFromSave.Pop();
                    }
                }
                //먼저 Active를 띄운다음 그다음 Save를 
                //만약 Save에만 존재할경우
                else if (SaveModInActiveModList == null)
                {
                    FormattedSaveMods.Add(SaveCurrent);
                    FormattedActiveMods.Add(new Mod());
                    SaveCurrent = ModsFromSave.Pop();
                }
                //만약 Active에만 존재할경우
                else if (ActiveModInSaveModList == null)
                {
                    FormattedActiveMods.Add(ActiveCurrent);
                    FormattedSaveMods.Add(new Mod());
                    ActiveCurrent = ModsFromActive.Pop();
                }
                else
                {
                    throw new Exception("Error in main Logic");
                }
            }
            
            while(ModsFromSave.Count > 0)
            {
                FormattedSaveMods.Add(ModsFromSave.Pop());
                FormattedActiveMods.Add(new Mod());
            }

            while(ModsFromActive.Count > 0)
            {
                FormattedActiveMods.Add(ModsFromActive.Pop());
                FormattedSaveMods.Add(new Mod());
            }

            if(FormattedSaveMods.Count != FormattedActiveMods.Count)
                throw new Exception("Exception in Main Logic 2");
            
            for(int i = 0; i < FormattedSaveMods.Count; i++)
            {
                modPairList.Add(new ModPair(FormattedSaveMods[i], FormattedActiveMods[i]));
            }

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
            return mods.Where(mod => !(mod.isPlaceHolder)).OrderBy(mod => mod.Order).ToList();
        }
    }
}