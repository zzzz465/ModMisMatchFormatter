using System;
using System.Collections.Generic;
using UnityEngine;
#if RELEASE
using Verse;
#endif
using System.Linq;

namespace Madeline.ModMismatchFormatter
{
    public class ModListerElementRenderer : iModListerElementRenderer
    {
        public GUIStyle ModStateStyle { get; set; } = new GUIStyle() { fontSize = 12 };
        public GUIStyle ModDescriptionStyle { get; set; } = new GUIStyle() { fontSize = 12, alignment = TextAnchor.MiddleLeft };
        public GUIStyle ModVersionStyle { get; set; } = new GUIStyle() { fontSize = 8, alignment = TextAnchor.MiddleLeft };
        public float LeftListCenterPos { get; protected set; }
        public float RightListCenterPos { get; protected set; }
        enum ModState
        {
            Add,
            Remove,
            VersionChange,
            None
        }
        /*
        void RenderElementList(Rect root, IEnumerable<ModPair> pairs)
        {
            Rect SingleItemRect = new Rect(root.x, root.y, root.width, 22f);
            Rect SaveModsViewRect = new Rect(root.x, root.y, root.width, SingleItemRect.height * pairs.Count());
            Rect ActiveModsViewRect = new Rect(SaveModsViewRect);
            float scrollBarWidth = verticalScrollBarStyle.fixedWidth;
            Rect ScrollRect = new Rect(root.x + ((root.width - scrollBarWidth) / 2), root.y, scrollBarWidth, SingleItemRect.height);
            
            scrollPosition = GUI.BeginScrollView(root, scrollPosition, ViewRect);
            foreach(var pair in pairs)
            {
                RenderSinglePair(SingleItemRect, ScrollRect, pair);
                SingleItemRect.y = SingleItemRect.y + SingleItemRect.height;
                ScrollRect.y = SingleItemRect.y;
            }
            GUI.EndScrollView();
            
        }
        */

        public void RenderSaveMod(Rect left, ModPair pair)
        {
            var saveMod = pair.Save;
            if(saveMod.isPlaceHolder)
                RenderPlaceHolderMod(left);
            else
                RenderSingleMod(left, saveMod, ModState.None);
        }

        public void RenderActiveMod(Rect right, ModPair pair)
        {
            if(pair.Loaded.isPlaceHolder)
                RenderPlaceHolderMod(right);
            else if(pair.Loaded.isVersionDifferent(pair.Save))
                RenderSingleMod(right, pair.Loaded, ModState.VersionChange);
            else
                RenderSingleMod(right, pair.Loaded, ModState.None);
        }

        void RenderPlaceHolderMod(Rect root)
        { // Complete
            #if RELEASE
            Widgets.DrawBoxSolid(root, Background);
            #endif
        }

        void RenderSingleMod(Rect root, Mod mod, ModState modState)
        { // 다 적었나?
            Color bgColor;
            if(modState == ModState.Add)
                bgColor = ColorPresets.Green;
            else if(modState == ModState.Remove)
                bgColor = ColorPresets.Red;
            else
                bgColor = ColorPresets.Background;

            #if RELEASE
            Widgets.DrawBoxSolid(root, bgColor); // 이것때문에 테스트가 불가능해진다고?
            #endif

            float leftBoxWidth = 16f;
            Rect ModStateRect = new Rect(root.x, root.y, leftBoxWidth, root.height);
            RenderModState(ModStateRect, modState);

            Rect DescriptionRect = new Rect(ModStateRect.xMax, root.y, root.width - ModStateRect.width, root.height);
            RenderModDescriptionAndVersion(DescriptionRect, mod);
        }
        void RenderModDescriptionAndVersion(Rect Root, Mod mod)
        {
            string version = mod.Version ?? "Unknown";
            string description = $"{mod.ModName}";
            
            var size = ModVersionStyle.CalcSize(new GUIContent(version));
            if(Root.height - size.y <= 0)
                throw new Exception("Root rect is smaller than version text.");
            
            Rect DescriptionRect = new Rect(Root.x, Root.y, Root.width, Root.height - size.y);
            GUI.Label(DescriptionRect, description, ModDescriptionStyle);

            Rect VersionRect = new Rect(Root.x, DescriptionRect.yMax, Root.width, size.y);
            GUI.Label(VersionRect, version, ModVersionStyle);   
        }

        void RenderModState(Rect root, ModState state)
        {
            GUIStyle style = ModStateStyle;

            string content = string.Empty;
            if(state == ModState.Add)
                content = "+";
            else if(state == ModState.Remove)
                content = "-";
            else if(state == ModState.VersionChange)
                content = "?";
            else if(state == ModState.None)
                content = " ";
            else
                throw new Exception($"ModState {state.ToString()} is not supported yet. please contact to modder.");
            
            GUI.Label(root, content, style);
        }
    }
}