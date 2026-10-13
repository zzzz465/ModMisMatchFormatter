using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;

namespace Madeline.ModMismatchFormatter
{
    public class ModListerElementRenderer : iModListerElementRenderer
    {
        public GUIStyle ModStateStyle { get; set; }
        public GUIStyle ModDescriptionStyle { get; set; }
        public GUIStyle ModVersionStyle { get; set; }
        public float LeftListCenterPos { get; protected set; }
        public float RightListCenterPos { get; protected set; }
        public bool useVersionChecking { get; set; }
        enum ModState
        {
            Add,
            Remove,
            VersionChange,
            None
        }
        enum VersionState
        {
            Upversion,
            DownVersion,
            None
        }
        public ModListerElementRenderer()
        {
            InitializeDefaultFont();
        }

        void InitializeDefaultFont()
        {
            ModDescriptionStyle = new GUIStyle();
            ModDescriptionStyle.fontSize = 15;
            ModDescriptionStyle.alignment = TextAnchor.MiddleLeft;
            ModDescriptionStyle.normal.textColor = Color.white;
            ModDescriptionStyle.onNormal.textColor = Color.white;
            ModDescriptionStyle.hover.textColor = Color.white;
            ModDescriptionStyle.onHover.textColor = Color.white;

            ModStateStyle = new GUIStyle();
            ModStateStyle.fontSize = ModDescriptionStyle.fontSize;
            ModStateStyle.alignment = TextAnchor.MiddleLeft;
            ModStateStyle.normal.textColor = Color.white;
            ModStateStyle.onNormal.textColor = Color.white;
            ModStateStyle.hover.textColor = Color.white;
            ModStateStyle.onHover.textColor = Color.white;


            ModVersionStyle = new GUIStyle();
            ModVersionStyle.fontSize = 11;
            ModVersionStyle.alignment = TextAnchor.MiddleLeft;
            ModVersionStyle.normal.textColor = Color.white;
            ModVersionStyle.onNormal.textColor = Color.white;
            ModVersionStyle.hover.textColor = Color.white;
            ModVersionStyle.onHover.textColor = Color.white;

        }

        public void RenderSaveMod(Rect left, ModPair pair)
        {
            var saveMod = pair.Save;
            if(saveMod.isPlaceHolder)
                RenderPlaceHolderMod(left);
            else if(pair.Loaded.isPlaceHolder)
                RenderSingleMod(left, saveMod, ModState.Remove);
            else
                RenderSingleMod(left, saveMod, ModState.None);
        }

        public void RenderActiveMod(Rect right, ModPair pair)
        {
            if(pair.Loaded.isPlaceHolder)
                RenderPlaceHolderMod(right);
            else if(pair.Save.isPlaceHolder)
                RenderSingleMod(right, pair.Loaded, ModState.Add);
            else if(useVersionChecking && pair.Loaded.isVersionDifferent(pair.Save))
                RenderSingleMod(right, pair.Loaded, ModState.VersionChange);
            else
                RenderSingleMod(right, pair.Loaded, ModState.None);
        }

        void RenderPlaceHolderMod(Rect root)
        {
            Widgets.DrawBoxSolid(root, ColorPresets.Background);
        }

        void RenderSingleMod(Rect root, Mod mod, ModState modState)
        {
            Color bgColor;
            if(modState == ModState.Add)
                bgColor = ColorPresets.Green;
            else if(modState == ModState.Remove)
                bgColor = ColorPresets.Red;
            else if(useVersionChecking && modState == ModState.VersionChange)
                bgColor = ColorPresets.Yellow;
            else
                bgColor = ColorPresets.Background;

            Widgets.DrawBoxSolid(root, bgColor);

            float leftBoxWidth = 16f;
            Rect ModStateRect = new Rect(root.x, root.y, leftBoxWidth, root.height);
            // padding
            switch (modState)
            {
                case ModState.Add:
                    ModStateRect.x += 4;
                    break;
                case ModState.Remove:
                    ModStateRect.x += 6;
                    ModStateRect.y -= 2;
                    break;
            }

            RenderModState(ModStateRect, modState);

            Rect DescriptionRect = new Rect(ModStateRect.xMax, root.y, root.width - ModStateRect.width, root.height);
            RenderModDescriptionAndVersion(DescriptionRect, mod);
        }
        void RenderModDescriptionAndVersion(Rect Root, Mod mod)
        {
            string version = mod.Version ?? "Unknown";
            version = $"ver : {version}";
            string description = $"{mod.ModName}";

            var DescriptionTextHeight = ModDescriptionStyle.CalcSize(new GUIContent(description)).y;
            var VersionTextHeight = ModVersionStyle.CalcSize(new GUIContent(version)).y;

            float minimumHeight;
            if(useVersionChecking)
                minimumHeight = DescriptionTextHeight + VersionTextHeight;
            else
                minimumHeight = DescriptionTextHeight;

            if(minimumHeight > Root.height)
                throw new Exception($"Root rect's height is smaller than minimum size {minimumHeight}");
            
            // HACK: somehow text alignment is not correct so move label 3 pixel down
            Rect DescriptionRect = new Rect(Root.x, Root.y + 3, Root.width, DescriptionTextHeight);
            GUI.Label(DescriptionRect, description, ModDescriptionStyle);
            
            if(useVersionChecking)
            {   //TODO - mod을 보고 versionStyle를 넣어주자
                Rect VersionRect = new Rect(Root.x, Root.yMax - VersionTextHeight, Root.width, VersionTextHeight);
                RenderVersionText(VersionRect, version, ModVersionStyle, VersionState.None);
            }
        }

        void RenderVersionText(Rect versionRect, string content, GUIStyle style, VersionState state)
        {
            //TODO - VersionState에 맞는 색깔 넣어주기
            GUI.Label(versionRect, content, style);
        }

        void SetColor(GUIStyle style, Color color)
        { // FIXME - 색깔이 고정되어있음
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            style.onNormal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.onHover.textColor = Color.white;
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