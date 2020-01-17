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
            this.doCloseX = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            InitializePair();
            SetDrawerFonts();
        }

        void SetDrawerFonts()
        {
            //renderer.ModDescriptionStyle = Text.CurFontStyle;
            //renderer.ModStateStyle = Text.CurFontStyle;
            ////renderer.ModVersionStyle = new GUIStyle(Text.CurFontStyle) { fontSize = Text.CurFontStyle.fontSize - 3 };

            Log.Message(string.Format("DescriptionStyle Font size : {0}", renderer.ModDescriptionStyle.fontSize));
        }

        void InitializePair()
        {
            List<Mod> activeMods = ModContentPackExtension.GetModsFromActive();
            List<Mod> saveMods = ModContentPackExtension.GetModsFromSave();
            Log.Message($"activeMods count : {activeMods.Count} | saveMods count : {saveMods.Count}");
            var result = formatter.GetFormattedModPairs(saveMods, activeMods);
            pairs = result.ToList();
        }

        public override void DoWindowContents(Rect canvas)
        {
            int ItemCount = pairs.Count + 1;
            //윗쪽 타이틀 설정
			Rect TitleRect = new Rect(canvas.xMin, canvas.yMin, canvas.width, 60f);
            Widgets.Label(TitleRect, "ModsMismatchWarningTitle".Translate());

            //1개 모드 사이즈
            float RectWidth = canvas.width / 2 - 40;
            Vector2 SingleItemSize = new Vector2(RectWidth, 34);
            Rect SingleModItemRect = new Rect(Vector2.zero, SingleItemSize);

            //막대기 사이즈
            Rect Bar = new Rect(0, 0, RectWidth, 2f);

			//하위 버튼의 크기
			Vector2 ButtonSize = new Vector2(RectWidth - 50f, 40f);

            //좌측(Save)
            GUI.contentColor = Color.white;
			Rect LeftSaveRect = new Rect((canvas.width / 2) - RectWidth, TitleRect.yMax, RectWidth, canvas.height - TitleRect.height - 60); // 이걸 가지고 다른 모든 상자들의 길이를 측정함
			Rect LeftinSaveRect = new Rect(LeftSaveRect.xMin, LeftSaveRect.yMin, LeftSaveRect.width - 18f, SingleModItemRect.height * ItemCount + Bar.height * (ItemCount - 1));
            Widgets.DrawBoxSolid(LeftSaveRect, ColorPresets.Background);

            Rect LeftTitle = new Rect(LeftSaveRect.xMin, LeftSaveRect.yMin - 26f, RectWidth - 18f, 26f); // 18f는 스크롤바 크기
            Widgets.Label(LeftTitle, "SaveModListTitle".Translate());

            //우측(Active)
            Rect RightActiveRect = new Rect(LeftSaveRect.xMax + 16f, LeftSaveRect.yMin, LeftSaveRect.width, LeftSaveRect.height); // LeftSaveRect에 종속
            Rect RightinActiveRect = new Rect(RightActiveRect.xMin, RightActiveRect.yMin, RightActiveRect.width, LeftinSaveRect.height); // RightActiveRect에 종속
            Widgets.DrawBoxSolid(RightActiveRect, ColorPresets.Background);

            Rect RightTitle = new Rect(RightActiveRect.xMin, RightActiveRect.yMin - 26f, RectWidth - 18f, 26f);
            Widgets.Label(RightTitle, "ActiveModListTitle".Translate());
            //개수 안맞으면 PlaceHolder 넣어줘야함

            
            Widgets.BeginScrollView(LeftSaveRect, ref scrollPosition, LeftinSaveRect, true);
            SingleModItemRect.position = new Vector2(LeftinSaveRect.x, LeftinSaveRect.y); // 위치 초기화
            Bar.position = new Vector2(SingleModItemRect.x, SingleModItemRect.yMax + 1f);
            Bar.width = SingleModItemRect.xMax - SingleModItemRect.xMin;
            foreach(var pair in pairs)
            {
                renderer.RenderSaveMod(SingleModItemRect, pair);
                Bar.y = SingleModItemRect.yMax;
                RenderSeperationLine(Bar);
                SingleModItemRect.y = Bar.yMax;

            }
            Widgets.EndScrollView();

            Widgets.BeginScrollView(RightActiveRect, ref scrollPosition, RightinActiveRect, false);
            SingleModItemRect.position = new Vector2(RightinActiveRect.xMin, RightinActiveRect.yMin); // 위치 초기화
            Bar.position = new Vector2(SingleModItemRect.x, SingleModItemRect.yMax + 1f);
            Bar.width = SingleModItemRect.xMax - SingleModItemRect.xMin;
            foreach(var pair in pairs)
            {
                renderer.RenderActiveMod(SingleModItemRect, pair);
                Bar.y = SingleModItemRect.yMax;
                RenderSeperationLine(Bar);
                SingleModItemRect.y = Bar.yMax;
            }
            Widgets.EndScrollView();
            

            float ButtonXPos = LeftSaveRect.xMin + ( (LeftSaveRect.width - ButtonSize.x) / 2 ) - 7f; // micro control. I'm lack of math skill.
			float ButtonYPos = canvas.yMax - 10f - ButtonSize.y;
            Rect LoadFromSaveButton = new Rect(new Vector2(ButtonXPos, ButtonYPos), ButtonSize);
            RenderLoadFromSaveButton(LoadFromSaveButton);

            ButtonXPos = RightActiveRect.xMin + ( (RightActiveRect.width - ButtonSize.x) / 2 ) - 7f;
			Rect LoadAnywayButton = new Rect(new Vector2(ButtonXPos, ButtonYPos), ButtonSize);
            RenderLoadAnywayButton(LoadAnywayButton);
        }

        void RenderSeperationLine(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, Color.grey);
        }

        void RenderLoadAnywayButton(Rect LoadAnywayRect)
        {
            if (Widgets.ButtonText(LoadAnywayRect, "LoadAnyway".Translate()))
            {
                confirmAction();
            }
        }

        void RenderLoadFromSaveButton(Rect LoadFromSaveRect)
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