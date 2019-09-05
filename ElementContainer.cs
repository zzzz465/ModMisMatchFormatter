using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModComparerPatch.Interfaces;
using System.Collections;
using Verse;

namespace ModComparerPatch
{
    public enum ContainerType
    {
        Save = 1, // 세이브 파일 것
        Active = 2 // 현재 로드된 것
    }
    public class ElementContainer
    {
        ContainerType ContainerType { get; }

        public ElementList elementList = new ElementList();
        public ElementContainer(ContainerType containerType)
        {
            this.ContainerType = containerType;
            LoadMods();
        }

        public void Append(ModElement elem)
        {
            elementList.Append(elem);
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

    public class ElementList : IElementEnumerable
    {
        List<ModElement> InnerList = new List<ModElement>();
        IEnumerator ModElementEnumerator;
        public ModElement Before
        {
            get
            {
                int index = InnerList.IndexOf(ModElementEnumerator.Current as ModElement);
                if (index == 0)
                    return null;
                return InnerList[index - 1];
            }
        }
        public ModElement After
        {
            get
            {
                int index = InnerList.IndexOf(ModElementEnumerator.Current as ModElement);
                if (index == InnerList.Count - 1)
                    return null;
                return InnerList[index + 1];
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (ModElementEnumerator == null)
                ModElementEnumerator = InnerList.GetEnumerator();
            return ModElementEnumerator;
        }
        public ElementList()
        {

        }

        public void Append(ModElement element)
        {
            if (!InnerList.Contains(element))
                InnerList.Add(element);
            else
                Log.Error("tried to append Element " + element.ToString() + " but the same item exist.");
        }
    }
}
