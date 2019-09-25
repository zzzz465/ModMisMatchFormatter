using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using System.IO;

namespace ModMisMatchWindowPatch
{
    public static class MyWidgets
    {
        static GUIStyle PlusOrMinusStyle;
        static GUIStyle ModNameStyle;
        public static GUIStyle CenterAlignmentStyle;
        static Texture2D RedTexture; // #4b1818
        static Texture2D GreenTexture; // #373d29
        static Texture2D BackgroundTexture; // #1e1e1e
        static Texture2D PlaceHolderTexture;
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

            CenterAlignmentStyle = Text.CurFontStyle;
            CenterAlignmentStyle.alignment = TextAnchor.MiddleCenter;
            //에러가 날수도 있나?
            
            PlaceHolderTexture = new Texture2D(100, 100);
            string FolderPath = Path.Combine(LoadedModManager.RunningMods.ToList().Where(mod => mod.Name == "Readable SaveModList").Select(item => item.RootDir).FirstOrDefault(), "Resources");
            byte[] ImageData = File.ReadAllBytes(Path.Combine(FolderPath, "PlaceHolderImage.png"));
            PlaceHolderTexture.LoadImage(ImageData);

        }
        public static void DoLabelBox(Rect rect, ModElement element)
        {
            //Color OldColor = GUI.backgroundColor;
            //GUI.backgroundColor = Background;
            //GUI.Label(rect, GUIContent.none);
            bool OverrideColor = element.color != null;
            
            float width = PlusOrMinusStyle.CalcSize(new GUIContent("x")).x;
            Rect LeftBox = new Rect(rect.xMin, rect.yMin, width, rect.height);
            if (element.isAddState == true)
            {
                if(OverrideColor)
                    Widgets.DrawBoxSolid(rect, (Color)element.color);
                else
                    Widgets.DrawBoxSolid(rect, Green);
                GUI.Label(LeftBox, "+", PlusOrMinusStyle);
            }
            else if (element.isAddState == false)
            {
                if(OverrideColor)
                    Widgets.DrawBoxSolid(rect, (Color)element.color);
                else
                    Widgets.DrawBoxSolid(rect, Red);
                GUI.Label(LeftBox, "-", PlusOrMinusStyle);
            }

            if(element.isPlaceHolder)
            {
                //GUI.Box(rect, BackgroundTexture);
            }

            Rect RightBox = new Rect(LeftBox.xMax, rect.yMin, rect.width - LeftBox.width, rect.height); // rect와 LeftBox에 종속
            
            GUI.Label(RightBox, element.ModName, ModNameStyle);
            
        }
    }
}
