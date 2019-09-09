using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ModComparerPatch
{
    public class ModMisMatchWindow : Window
    {
        static ElementContainer ModElementContainer = new ElementContainer(ContainerType.Active);
        public ModMisMatchWindow()
        {

        }
        public override void DoWindowContents(Rect inRect)
        {
            //inRect는 기본 사각형 크기
            string name = "ASDF"; // 할당해주자
            //Rect Title = new Rect(inRect.x + 30, inRect.y - 30, Text.CalcSize(name).x, Text.CalcSize(name).y);
            Rect Title = inRect.AtZero();
            Title.x = 10f;
            //Log.Message(string.Format("{0} {1} {2} {3}", Title.x, inRect.x, Title.y, inRect.y));
            Widgets.Label(Title, name);
            //Texture2D texture2D = new Texture2D()

            Rect outRect = inRect.AtZero();
            
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 160f);
            Widgets.BeginScrollView(outRect, ref this.ScrollPosition, viewRect, true);
            float yPos = 0f;
            for(int i = 0; i < 50; i++)
            {
                Rect rect = new Rect(15f, yPos, 50f, 50f);
                Widgets.Label(rect, "Test");
                GUI.Box(rect, GUIContent.none);
                yPos += 15f;
            }
            Widgets.EndScrollView();
            
        }

        public Vector2 ScrollPosition = Vector2.zero;
    }
}
