using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Harmony;
using System.Reflection;

namespace ModMisMatchWindowPatch
{
    public class ModMisMatchWindow : Window
    { // view
        static ElementContainer ModElementContainer = new ElementContainer(ContainerType.Active);
        static Vector2 scrollPosition = Vector2.zero;
        List<ModElement> ActiveMods = new List<ModElement>();
        List<ModElement> SaveMods = new List<ModElement>();
		Action confirmAction;

        public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(650, 700);
			}
		}

		
        public ModMisMatchWindow(Action confirmAction)
        {
            ActiveMods = new List<ModElement>();
            SaveMods = new List<ModElement>();
            this.doCloseX = true;
			this.confirmAction = confirmAction;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            ActiveMods.Clear();
            SaveMods.Clear();

            List<ModElement> ActiveModsToAdd = new List<ModElement>();
            List<ModElement> SaveModsToAdd = new List<ModElement>();

            LoadedModManager.RunningMods.ToList().ForEach(item => ActiveModsToAdd.Add(new ModElement(item.Name, item.loadOrder, true, false)));
            if (ScribeMetaHeaderUtility.loadedModNamesList != null)
            {
                int k = 0;
                ScribeMetaHeaderUtility.loadedModNamesList.ForEach(item =>
                {
                    SaveModsToAdd.Add(new ModElement(item, k, false, false));
                    k++;
                });
            }
            //ActiveMods.AddRange(ActiveModsToAdd);
            //SaveMods.AddRange(SaveModsToAdd);
            //Pop은 뒤에서 부터 가져옴. FIFO 방식 (FILO 아님)
            ActiveModsToAdd.Reverse();
            SaveModsToAdd.Reverse();

            IEnumerator<ModElement> ActiveEnumerator = ActiveModsToAdd.GetEnumerator();
            ActiveEnumerator.MoveNext();
            ModElement SaveCurrent = SaveModsToAdd.Pop();
            ModElement ActiveCurrent = ActiveModsToAdd.Pop();
            int i = 0;

            while(ActiveModsToAdd.Count > 0 && SaveModsToAdd.Count > 0) // 여기서 요소가 있다는 것을 보장함, 대신 다 끝나고 1개 또는 0개가 남을 수 있음
            {
                if(SaveCurrent.ModName == ActiveCurrent.ModName) // 동일할경우(그냥 추가하면 됨)
                {
                    Log.Message(string.Format("Add {0}", SaveCurrent.ModName));
                    ActiveMods.Add(ActiveCurrent);
                    SaveMods.Add(SaveCurrent);
                    ActiveCurrent = ActiveModsToAdd.Pop();
                    SaveCurrent = SaveModsToAdd.Pop();
                    continue;
                }
                //만약 둘다 존재할경우
                int saveCurrentIndex = SaveModsToAdd.IndexOf(SaveCurrent);
                int activeCurrentIndex = ActiveModsToAdd.IndexOf(ActiveCurrent);
                ModElement SaveModInActiveModList = ActiveModsToAdd.AfterIndex(activeCurrentIndex).Where(item => item.ModName == SaveCurrent.ModName).FirstOrDefault(); // SaveModsToAdd.AfterIndex(saveCurrentIndex).Any(item => item.ModName == ActiveCurrent.ModName))
                ModElement ActiveModInSaveModList = SaveModsToAdd.AfterIndex(saveCurrentIndex).Where(item => item.ModName == ActiveCurrent.ModName).FirstOrDefault();
                if (SaveModInActiveModList != null && ActiveModInSaveModList != null)
                {
                    int SaveGapBetweenCurrentAndTarget = ActiveModsToAdd.IndexOf(SaveModInActiveModList) - ActiveModsToAdd.IndexOf(ActiveCurrent);
                    int ActiveGapBetweenCurrentAndTarget = SaveModsToAdd.IndexOf(ActiveModInSaveModList) - SaveModsToAdd.IndexOf(SaveCurrent);

                    if (SaveGapBetweenCurrentAndTarget > ActiveGapBetweenCurrentAndTarget) // 만약 Save의 gap이 더 클 경우 Active을 먼저 더해주자
                    {
                        Log.Message(string.Format("Save : {0} | Active : {1}", "PlaceHolder", ActiveCurrent.ModName));
                        ActiveCurrent.isAddState = true;
                        ActiveMods.Add(ActiveCurrent);
                        SaveModsToAdd.Add(new ModElement(string.Empty, 0, false));
                        ActiveCurrent = ActiveMods.Pop();
                    }
                    else // Active의 Gap이 더 클경우 Save를 먼저 더해주자.
                    {
                        Log.Message(string.Format("Save : {0} | Active : {1}", SaveCurrent.ModName, "PlaceHolder"));
                        ActiveMods.Add(new ModElement(string.Empty, 0, true));
                        SaveCurrent.isAddState = false;
                        SaveMods.Add(SaveCurrent);
                        SaveCurrent = SaveModsToAdd.Pop();
                    }
                }
                //먼저 Active를 띄운다음 그다음 Save를 
                //만약 Save에만 존재할경우
                else if (SaveModInActiveModList == null)
                {
                    Log.Message("Save : " + SaveCurrent.ModName);
                    SaveCurrent.isAddState = false;
                    SaveMods.Add(SaveCurrent);
                    ActiveMods.Add(new ModElement(string.Empty, 0, true, true, false));
                    SaveCurrent = SaveModsToAdd.Pop();
                }
                //만약 Active에만 존재할경우
                else if (ActiveModInSaveModList == null)
                {
                    Log.Message("Active : " + ActiveCurrent.ModName);
                    ActiveCurrent.isAddState = true;
                    ActiveMods.Add(ActiveCurrent);
                    SaveMods.Add(new ModElement(string.Empty, 0, true, false, true));
                    ActiveCurrent = ActiveModsToAdd.Pop();
                }
                else
                {
                    Log.Error(string.Format("Current : {0} Active : {1} 두개가 둘다 null이 아니지만 에러 발생", SaveCurrent.ModName, ActiveCurrent.ModName));
                    ActiveCurrent = ActiveModsToAdd.Pop();
                    SaveCurrent = SaveModsToAdd.Pop();
                }
            }
            while (SaveModsToAdd.Count > 0)
            {
                SaveCurrent = SaveModsToAdd.Pop();
                SaveCurrent.isAddState = false;
                SaveMods.Add(SaveCurrent);
            }
            
            while (ActiveModsToAdd.Count > 0)
            {
                ActiveCurrent = ActiveModsToAdd.Pop();
                ActiveCurrent.isAddState = true;
                ActiveMods.Add(ActiveCurrent);
            }

        }
        public override void DoWindowContents(Rect canvas)
        {
            int ItemCount = SaveMods.Count;
            GUIStyle TitleStyle = MyWidgets.CenterAlignmentStyle;
            
            //윗쪽 타이틀 설정
			Rect TextBox = new Rect(canvas.xMin, canvas.yMin, canvas.width, 60f);
            Widgets.Label(TextBox, "ModsMismatchWarningTitle".Translate());
			GUI.Label(TextBox, "ModsMismatchWarningTitle".Translate(), MyWidgets.CenterAlignmentStyle);

            //1개 모드 사이즈
            float RectWidth = canvas.width / 2 - 40;
            Vector2 SingleItemSize = new Vector2(RectWidth, 22);
            Rect SingleModItemRect = new Rect(Vector2.zero, SingleItemSize);

            //막대기 사이즈
            Rect Bar = new Rect(0, 0, RectWidth, 2f);

			//하위 버튼의 크기
			Vector2 ButtonSize = new Vector2(RectWidth - 50f, 40f);

            //좌측(Save)
            GUI.contentColor = Color.white;
			Rect LeftSaveRect = new Rect((canvas.width / 2) - RectWidth, TextBox.yMax, RectWidth, canvas.height - TextBox.height - 60); // 이걸 가지고 다른 모든 상자들의 길이를 측정함
			Rect LeftinSaveRect = new Rect(LeftSaveRect.xMin, LeftSaveRect.yMin, LeftSaveRect.width - 18f, SingleModItemRect.height * ItemCount);
            Widgets.DrawBoxSolid(LeftSaveRect, MyWidgets.Background);

            Rect LeftTitle = new Rect(LeftSaveRect.xMin, LeftSaveRect.yMin - 26f, RectWidth - 18f, 26f); // 18f는 스크롤바 크기
            GUI.Label(LeftTitle, "SaveModListTitle".Translate(), TitleStyle);
            //Widgets.DrawBoxSolid(LeftTitle, Color.white);
            //MyWidgets.DrawSolidBox(LeftSaveRect, MyWidgets.Background);
            //왜 -18f 해줘야 하는거지?

            //우측(Active)
            Rect RightActiveRect = new Rect(LeftSaveRect.xMax + 16f, LeftSaveRect.yMin, LeftSaveRect.width, LeftSaveRect.height); // LeftSaveRect에 종속
            Rect RightinActiveRect = new Rect(RightActiveRect.xMin, RightActiveRect.yMin, RightActiveRect.width, ItemCount * SingleModItemRect.height); // RightActiveRect에 종속
            Widgets.DrawBoxSolid(RightActiveRect, MyWidgets.Background);

            Rect RightTitle = new Rect(RightActiveRect.xMin, RightActiveRect.yMin - 26f, RectWidth - 18f, 26f);
            GUI.Label(RightTitle, "ActiveModListTitle".Translate(), TitleStyle);
            //개수 안맞으면 PlaceHolder 넣어줘야함


            //Save mod 세팅
            Widgets.BeginScrollView(LeftSaveRect, ref scrollPosition, LeftinSaveRect, true);
			SingleModItemRect.position = new Vector2(LeftinSaveRect.x, LeftinSaveRect.y); // 위치 초기화
            Bar.position = new Vector2(SingleModItemRect.x, SingleModItemRect.yMax + 1f);
			for(int i = 0; i < SaveMods.Count; i++)
			{
				ModElement element = SaveMods.ElementAt(i); // 이걸로 수정
				//ModElement element = new ModElement("test", 0, true, true);
                //Placeholder 처리 해주자
                //Widgets.Label(SingleModItemRect, element.ModName);
                MyWidgets.DoLabelBox(SingleModItemRect, element);
                Widgets.DrawBoxSolid(Bar, Color.grey);
                SingleModItemRect.y += SingleModItemRect.height + Bar.height + 2f;
                Bar.y = SingleModItemRect.yMax + 1f;
                Log.Message("Adding " + element.ModName);
			}
            Widgets.EndScrollView();

			//Active mod 세팅
            Widgets.BeginScrollView(RightActiveRect, ref scrollPosition, RightinActiveRect, false);
            SingleModItemRect.position = new Vector2(RightinActiveRect.xMin, RightinActiveRect.yMin); // 위치 초기화
            Bar.position = new Vector2(SingleModItemRect.x, SingleModItemRect.yMax + 1f);
            for(int i = 0; i < ActiveMods.Count; i++)
            {
                ModElement element = ActiveMods.ElementAt(i);
                //ModElement element = new ModElement("Test2", 0, true, true);
                //Widgets.Label(SingleModItemRect, element.ModName);
                MyWidgets.DoLabelBox(SingleModItemRect, element);
                Widgets.DrawBoxSolid(Bar, Color.grey);
                SingleModItemRect.y += SingleModItemRect.height + Bar.height + 2f;
                Bar.y = SingleModItemRect.yMax + 1f;
            }
            Widgets.EndScrollView();

			//하위 버튼 세팅
			
			//LoadFromSave 버튼 위치
			float ButtonXPos = LeftSaveRect.xMin + ( (LeftSaveRect.width - ButtonSize.x) / 2 ) - 7f; // micro control. I'm lack of math skill.
			float ButtonYPos = canvas.yMax - 10f - ButtonSize.y;

			//LoadFromSave 버튼
			Rect LoadFromSaveButton = new Rect(new Vector2(ButtonXPos, ButtonYPos), ButtonSize);
			if(Widgets.ButtonText(LoadFromSaveButton, "ChangeLoadedMods".Translate(), true, true, true))
			{
				int num = ModLister.InstalledModsListHash(false);
				ModsConfig.SetActiveToList(ScribeMetaHeaderUtility.loadedModIdsList);
				ModsConfig.Save();
				ModsConfig.RestartFromChangedMods();
			}
			
			//LoadAnyway 버튼
			ButtonXPos = RightActiveRect.xMin + ( (RightActiveRect.width - ButtonSize.x) / 2 ) - 7f;
			Rect BackButton = new Rect(new Vector2(ButtonXPos, ButtonYPos), ButtonSize);
			if(Widgets.ButtonText(BackButton, "LoadAnyway".Translate(), true, true, true))
			{
				confirmAction();
			}
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
