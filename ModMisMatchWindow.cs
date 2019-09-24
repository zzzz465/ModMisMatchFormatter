using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Harmony;
using System.Reflection;

namespace ModComparerPatch
{
    public class ModMisMatchWindow : Window
    {
        static ElementContainer ModElementContainer = new ElementContainer(ContainerType.Active);
        static Vector2 scrollPosition = Vector2.zero;
        List<ModElement> ActiveMods = new List<ModElement>();
        List<ModElement> SaveMods = new List<ModElement>();

        public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(650, 700);
			}
		}

		
        public ModMisMatchWindow()
        {
            ActiveMods = new List<ModElement>();
            SaveMods = new List<ModElement>();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            ActiveMods.Clear();
            SaveMods.Clear();

            List<ModElement> ActiveModesToAdd = new List<ModElement>();
            List<ModElement> SaveModsToAdd = new List<ModElement>();

            LoadedModManager.RunningMods.ToList().ForEach(item => ActiveModesToAdd.Add(new ModElement(item.Name, item.loadOrder, true, false)));
            if (ScribeMetaHeaderUtility.loadedModNamesList != null)
            {
                int i = 0;
                ScribeMetaHeaderUtility.loadedModNamesList.ForEach(item =>
                {
                    SaveModsToAdd.Add(new ModElement(item, i, false, false));
                    i++;
                });
            }
            ActiveMods.AddRange(ActiveModesToAdd);
            SaveMods.AddRange(SaveModsToAdd);

            //if(ActiveMods)
            //나중에 하자
            //int itemCount = GetModRectNeededCount(ActiveMods, SaveMods);
            //int k = 0;
            //IEnumerator<ModElement> ActiveEnumerator = ActiveModesToAdd.GetEnumerator();
            //ActiveEnumerator.MoveNext();
            //IEnumerator<ModElement> SaveEnumerator = SaveModsToAdd.GetEnumerator();
            //SaveEnumerator.MoveNext();
            //while(true)
            //{
            //    if(ActiveEnumerator.Current.ModName != SaveEnumerator.Current.ModName)
            //    {
            //
            //    }
            //    else
            //    {
            //
            //    }
            //}
        }
        public override void DoWindowContents(Rect canvas)
        {
            int ItemCount = GetModRectNeededCount(ActiveMods, SaveMods); // 이게 필요한가? 그냥 직접 Rect을 늘리면..안되는건가 안될거같다.
            
            //윗쪽 타이틀 설정
			Rect TextBox = new Rect(canvas.xMin, canvas.yMin, canvas.width, 60f);
            Widgets.Label(TextBox, "ModsMismatchWarningTitle".Translate());

            //1개 모드 사이즈
            float RectWidth = canvas.width / 2 - 40;
            Vector2 SingleItemSize = new Vector2(RectWidth, 20);
            Rect SingleModItemRect = new Rect(Vector2.zero, SingleItemSize);

            //좌측(Save)
            GUI.contentColor = Color.white;
			Rect LeftSaveRect = new Rect((canvas.width / 2) - RectWidth, TextBox.yMax, RectWidth, canvas.height - TextBox.height - 60); // 이걸 가지고 다른 모든 상자들의 길이를 측정함
			Rect LeftinSaveRect = new Rect(LeftSaveRect.xMin, LeftSaveRect.yMin, LeftSaveRect.width - 18f, SingleModItemRect.height * ItemCount);
            //왜 -18f 해줘야 하는거지?
			

            //우측(Active)
            Rect RightActiveRect = new Rect(LeftSaveRect.xMax, LeftSaveRect.yMin, LeftSaveRect.width, LeftSaveRect.height);
            Rect RightinActiveRect = new Rect(RightActiveRect.xMin, RightActiveRect.yMin, RightActiveRect.width, ItemCount * SingleModItemRect.height);

            //개수 안맞으면 PlaceHolder 넣어줘야함


            //Active mod 세팅
            Widgets.BeginScrollView(LeftSaveRect, ref scrollPosition, LeftinSaveRect, true);
			SingleModItemRect.position = new Vector2(LeftinSaveRect.x, LeftinSaveRect.y); // 위치 초기화
			for(int i = 0; i < SaveMods.Count; i++)
			{
				ModElement element = SaveMods.ElementAt(i); // 이걸로 수정
				//ModElement element = new ModElement("test", 0, true, true);
                //Placeholder 처리 해주자
                Widgets.Label(SingleModItemRect, element.ModName);
                SingleModItemRect.y += SingleModItemRect.height;
			}
            Widgets.EndScrollView();

            Widgets.BeginScrollView(RightActiveRect, ref scrollPosition, RightinActiveRect, false);
            SingleModItemRect.position = new Vector2(RightinActiveRect.xMin, RightinActiveRect.yMin); // 위치 초기화
            for(int i = 0; i < ActiveMods.Count; i++)
            {
                ModElement element = ActiveMods.ElementAt(i);
                //ModElement element = new ModElement("Test2", 0, true, true);
                Widgets.Label(SingleModItemRect, element.ModName);
                SingleModItemRect.y += SingleModItemRect.height;
            }
            Widgets.EndScrollView();
		}

        int GetModRectNeededCount(List<ModElement> element1, List<ModElement> element2)
        {
            List<ModElement> element3 = new List<ModElement>();
            element3.AddRange(element2);
            element3.RemoveAll(item => element1.Contains(item));
            return element1.Count + element3.Count;
        }
    }
}
