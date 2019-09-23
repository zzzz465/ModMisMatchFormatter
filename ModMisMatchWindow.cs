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
		Vector2 SingleItemSize = new Vector2(40, 20);

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(700, 800);
			}
		}

		
        public ModMisMatchWindow()
        {

        }
        public override void DoWindowContents(Rect canvas)
        {
			/*
			//inRect는 기본 사각형 크기
			Rect outRect = new Rect(canvas.xMin, canvas.yMin, canvas.width, canvas.height);
			Texture2D texture = new Texture2D((int)outRect.width, (int)outRect.height);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
			GUI.skin.box.normal.background = texture;
			GUI.Box(outRect, GUIContent.none);
			int itemCount = 50;
			Rect inRect = new Rect(canvas.xMin, canvas.yMin, 80, itemCount * 50f);

			Rect button = new Rect(outRect.xMin, outRect.yMin, outRect.width, 50f);
			Widgets.BeginScrollView(outRect, ref ScrollPosition, inRect, true);
			for(int i = 0; i < itemCount; i++)
			{
				Widgets.Label(button, "Test");
				button.y += 50f;
			}
			Widgets.EndScrollView();*/
			Rect TextBox = new Rect(canvas.xMin, canvas.yMin, canvas.width, 30f);
			Rect OutActiveRect = new Rect(canvas.xMin, TextBox.yMin, canvas.width / 2, canvas.height - TextBox.height);
			int ItemCount = 100; // 나중에 계산해서 둠
			Rect SingleModItemBox = new Rect(Vector2.zero, SingleItemSize);
			Rect inActiveRect = new Rect(OutActiveRect.xMin, OutActiveRect.yMin, OutActiveRect.width, SingleModItemBox.height * ItemCount);

			List<ModElement> ActiveMods = new List<ModElement>(); // 다른걸로 초기화해주자.
			SingleModItemBox.position = new Vector2(inActiveRect.x, )
			for(int i = 0; i < ItemCount; i++)
			{
				//ModElement element = ActiveMods.ElementAt(i); // 이걸로 수정
				ModElement element = new ModElement("test", 0, true, true);
				//Placeholder 처리 해주자
				
			}

		}
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
