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
            string name = null; // 할당해주자
            Rect Title = new Rect(inRect.x + 30, inRect.y - 30, Text.CalcSize(name).x, Text.CalcSize(name).y);
            Widgets.Label(Title, name);

            Rect SaveOutRect = new Rect(inRect.x + 10, inRect.x - inRect.height + 20, 40f, 60f); // 겉 테두리
            Rect viewRect = new Rect(0f, 0f, SaveOutRect.width - 16f, 300);
            Widgets.BeginScrollView(SaveOutRect, ref this.ScrollPosition, viewRect, true);
            float yPos = 6f;
            float num2 = this.ScrollPosition.y - 30f; // ?
            float num3 = this.ScrollPosition.y + SaveOutRect.height;
            foreach (ModElement modElement in ModElementContainer.elementList)
            {
                Rect singleModRect = new Rect(0f, yPos, viewRect.width, 30f);
                Widgets.Label(singleModRect, "Test");
                yPos += 30f;
            }
            Widgets.EndScrollView();
            
        }

        public Vector2 ScrollPosition = Vector2.zero;
    }
}
