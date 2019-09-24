using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Verse;

namespace ModMisMatchWindowPatch
{
    public enum ContainerType
    {
        Save = 1, // 세이브 파일 것
        Active = 2 // 현재 로드된 것
    }
    public class ElementContainer
    {
        ContainerType ContainerType { get; }

        public List<ModElement> elementList = new List<ModElement>();
        public ElementContainer(ContainerType containerType)
        {
            this.ContainerType = containerType;
            LoadMods();
        }

        public void Append(ModElement elem)
        {
            elementList.Add(elem);
        }

        public IEnumerable<ModElement> modElements
        {
            get
            {
                foreach (ModElement elem in elementList)
                    yield return elem;
            }
        }
        void LoadMods()
        {
            Log.Message("모드 로드중");
            if(this.ContainerType == ContainerType.Save) //저장파일 꺼를 가져오는 컨테이너라면
            {
                int i = 0;
                foreach(string modName in ScribeMetaHeaderUtility.loadedModNamesList)
                {
                    try
                    {
                        ModElement elem = new ModElement(modName, i, false, false);
                        Append(elem);
                        i++;
                    }
                    catch
                    { }
                }
            }
            else // ContainerType == Active 일때
            {
                foreach (ModContentPack mod in LoadedModManager.RunningMods)
                {
                    try
                    {
                        ModElement elem = new ModElement(mod.Name, mod.loadOrder, true, false);
                        Append(elem);
                    }
                    catch
                    {
                        // 로그 적어주기
                    }
                }
            }
        }
    }
}
