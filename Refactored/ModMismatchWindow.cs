using System.Collections.Generic;
using Verse;
using System.Linq;
using System;
using UnityEngine;

namespace Madeline.ModMismatchFormatter
{
    public class ModMismatchWindow : Window
    {
        iModListerElementRenderer renderer;
        iOrderFormatter formatter;
        List<ModPair> pairs;
        Action confirmAction;
        Vector2 scrollPosition = Vector2.zero;
        public override Vector2 InitialSize
        {
            get { return new Vector2(650, 700); }
        }

        public ModMismatchWindow(iModListerElementRenderer renderer, iOrderFormatter formatter, Action confirmAction)
        { // 3항 연산자는..??
            this.renderer = renderer;
            this.formatter = formatter;
            this.confirmAction = confirmAction;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            InitializePair();
        }

        void InitializePair()
        {
            List<Mod> activeMods = GetModsFromActive();
            List<Mod> saveMods = GetModsFromSave();
            var result = formatter.GetFormattedModPairs(saveMods, activeMods);
            pairs = result.ToList();
        }

        List<Mod> GetModsFromActive()
        {
            List<Mod> activeMods = new List<Mod>();
            foreach(var mod in LoadedModManager.RunningMods)
            {
                activeMods.Add(new Mod(mod.Identifier, mod.Name, mod.loadOrder));
            }
            return activeMods;
        }

        List<Mod> GetModsFromSave()
        {
            List<Mod> saveMods = new List<Mod>();
            if (ScribeMetaHeaderUtility.loadedModNamesList != null && ScribeMetaHeaderUtility.loadedModIdsList != null)
            { // save
                var modnameList = ScribeMetaHeaderUtility.loadedModNamesList.Count;
                var modIdList = ScribeMetaHeaderUtility.loadedModIdsList.Count;
                if(modnameList != modIdList)
                    throw new Exception("Mod Name length and ModID length in savefile is not matched");

                for(int i = 0; i < modnameList; i++)
                {
                    saveMods.Add(new Mod(ScribeMetaHeaderUtility.loadedModIdsList[i], ScribeMetaHeaderUtility.loadedModNamesList[i], i));
                }
            }
            return saveMods;
        }

        public override void DoWindowContents(Rect canvas)
        {
            int ItemCount = pairs.Count;
            GUIStyle TitleStyle = new GUIStyle() { fontSize = 12, alignment = TextAnchor.MiddleCenter};

            //윗쪽 타이틀 설정
			Rect TextBox = new Rect(canvas.xMin, canvas.yMin, canvas.width, 60f);
            Widgets.Label(TextBox, "ModsMismatchWarningTitle".Translate());

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
            Widgets.DrawBoxSolid(LeftSaveRect, ColorPresets.Background);

            Rect LeftTitle = new Rect(LeftSaveRect.xMin, LeftSaveRect.yMin - 26f, RectWidth - 18f, 26f); // 18f는 스크롤바 크기
            GUI.Label(LeftTitle, "SaveModListTitle".Translate(), TitleStyle);

            //우측(Active)
            Rect RightActiveRect = new Rect(LeftSaveRect.xMax + 16f, LeftSaveRect.yMin, LeftSaveRect.width, LeftSaveRect.height); // LeftSaveRect에 종속
            Rect RightinActiveRect = new Rect(RightActiveRect.xMin, RightActiveRect.yMin, RightActiveRect.width, ItemCount * SingleModItemRect.height); // RightActiveRect에 종속
            Widgets.DrawBoxSolid(RightActiveRect, ColorPresets.Background);

            Rect RightTitle = new Rect(RightActiveRect.xMin, RightActiveRect.yMin - 26f, RectWidth - 18f, 26f);
            GUI.Label(RightTitle, "ActiveModListTitle".Translate(), TitleStyle);
            //개수 안맞으면 PlaceHolder 넣어줘야함

            Widgets.BeginScrollView(LeftSaveRect, ref scrollPosition, LeftinSaveRect, true);
            SingleModItemRect.position = new Vector2(LeftinSaveRect.x, LeftinSaveRect.y); // 위치 초기화
            foreach(var pair in pairs)
            {
                renderer.RenderSaveMod(SingleModItemRect, pair);
            }
            Widgets.EndScrollView();

            Widgets.BeginScrollView(RightActiveRect, ref scrollPosition, RightinActiveRect, false);
            SingleModItemRect.position = new Vector2(RightinActiveRect.xMin, RightinActiveRect.yMin); // 위치 초기화
            Bar.position = new Vector2(SingleModItemRect.x, SingleModItemRect.yMax + 1f);
            foreach(var pair in pairs)
            {
                renderer.RenderActiveMod(SingleModItemRect, pair);
            }
            Widgets.EndScrollView();

            float ButtonXPos = LeftSaveRect.xMin + ( (LeftSaveRect.width - ButtonSize.x) / 2 ) - 7f; // micro control. I'm lack of math skill.
			float ButtonYPos = canvas.yMax - 10f - ButtonSize.y;
            Rect LoadFromSaveButton = new Rect(new Vector2(ButtonXPos, ButtonYPos), ButtonSize);
            DrawLoadFromSaveButton(LoadFromSaveButton);

            ButtonXPos = RightActiveRect.xMin + ( (RightActiveRect.width - ButtonSize.x) / 2 ) - 7f;
			Rect LoadAnywayButton = new Rect(new Vector2(ButtonXPos, ButtonYPos), ButtonSize);
            DrawLoadAnywayButton(LoadAnywayButton);
        }

        void DrawLoadAnywayButton(Rect LoadAnywayRect)
        {
            if (Widgets.ButtonText(LoadAnywayRect, "LoadAnyway".Translate()))
            {
                confirmAction();
            }
        }

        void DrawLoadFromSaveButton(Rect LoadFromSaveRect)
        {
            if (Widgets.ButtonText(LoadFromSaveRect, "ChangeLoadedMods".Translate()))
            {
                ModsConfig.SetActiveToList(ScribeMetaHeaderUtility.loadedModIdsList);
                ModsConfig.Save();
                ModsConfig.RestartFromChangedMods();
            }
        }
    }
}