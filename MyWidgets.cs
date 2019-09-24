using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace ModMisMatchWindowPatch
{
    public static class MyWidgets
    {
        static GUIStyle PlusOrMinusStyle;
        static GUIStyle ModNameStyle;
        static Texture2D RedTexture; // #4b1818
        static Texture2D GreenTexture; // #373d29
        static Texture2D BackgroundTexture; // #1e1e1e
        public static Color Red = new Color(0.29f, 0.09f, 0.09f, 1);
        public static Color Green = new Color(0.29f, 0.33f, 0.19f, 1);
        public static Color Background = new Color(0.11f, 0.11f, 0.11f, 1);
        //static 

        static MyWidgets()
        {
            PlusOrMinusStyle = Text.CurFontStyle;
            PlusOrMinusStyle.alignment = TextAnchor.MiddleCenter;

            ModNameStyle = Text.CurFontStyle;
            ModNameStyle.alignment = TextAnchor.MiddleLeft;
            //PlusOrMinusStyle.fontSize = 16; // 이거 수정해줘야함

            //텍스쳐들 초기화 코드 여기에 넣어주자
            /*
            RedTexture = new Texture2D(1,1);
            RedTexture.SetPixel(0, 0, new Color32(75, 24, 24, 1));
            RedTexture.Apply();
            RedTexture.Resize(100, 100);

            GreenTexture = new Texture2D(1,1);
            GreenTexture.SetPixel(0, 0, new Color32(75, 86, 50, 1));
            GreenTexture.Apply();
            GreenTexture.Resize(100, 100);

            BackgroundTexture = new Texture2D(1,1);
            BackgroundTexture.SetPixel(0, 0, new Color32(30, 30, 30, 1));
            BackgroundTexture.Apply();
            BackgroundTexture.Resize(100, 100);
            */
        }
        public static void DoLabelBox(Rect rect, ModElement element)
        {
            //Color OldColor = GUI.backgroundColor;
            //GUI.backgroundColor = Background;
            //GUI.Label(rect, GUIContent.none);
            
            float width = PlusOrMinusStyle.CalcSize(new GUIContent("x")).x;
            Rect LeftBox = new Rect(rect.xMin, rect.yMin, width, rect.height);
            if (element.isAddState == true)
            {
                Widgets.DrawBoxSolid(rect, Green);
                GUI.Label(LeftBox, "+", PlusOrMinusStyle);
            }
            else if (element.isAddState == false)
            {
                Widgets.DrawBoxSolid(rect, Red);
                GUI.Label(LeftBox, "-", PlusOrMinusStyle);
            }
            else
            {

            }

            Rect RightBox = new Rect(LeftBox.xMax, rect.yMin, rect.width - LeftBox.width, rect.height); // rect와 LeftBox에 종속
            
            GUI.Label(RightBox, element.ModName, ModNameStyle);
            
        }
    }
}
