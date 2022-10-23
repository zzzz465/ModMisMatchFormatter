using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace ModMismatchFormatter
{
    public static class Renderer
    {
        private static float ButtonWidth = AccessTools.Field(typeof(Dialog_ModMismatch), "ButtonWidth").GetValue(null).ChangeType<float>();
        private static float ButtonHeight = AccessTools.Field(typeof(Dialog_ModMismatch), "ButtonHeight").GetValue(null).ChangeType<float>();
        private static float ModRowHeight = AccessTools.Field(typeof(Dialog_ModMismatch), "ModRowHeight").GetValue(null).ChangeType<float>();
        private static MethodInfo HandleGoBackClicked = AccessTools.Method(typeof(Dialog_ModMismatch), "HandleGoBackClicked");
        private static MethodInfo HandleSaveCurrentModList = AccessTools.Method(typeof(Dialog_ModMismatch), "HandleSaveCurrentModList");
        private static MethodInfo HandleLoadAnywayClicked = AccessTools.Method(typeof(Dialog_ModMismatch), "HandleLoadAnywayClicked");
        private static MethodInfo HandleChangeLoadedModClicked = AccessTools.Method(typeof(Dialog_ModMismatch), "HandleChangeLoadedModClicked");

        public static void DoWindowContents(Rect root, Dialog_ModMismatch __instance)
        {
            float num = (root.width - 20f) / 3f;
            float num2 = 0f;
            float x = 0f;
            float x2 = num + 10f;
            float x3 = (num + 10f) * 2f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, num2, root.width, Text.LineHeight), "ModsMismatchWarningTitle".Translate());
            num2 += Text.LineHeight + 10f;
            Text.Font = GameFont.Small;
            float height = Text.CalcHeight("ModsMismatchWarningText".Translate(), root.width);
            Rect rect = new Rect(0f, num2, root.width, height);
            Widgets.Label(rect, "ModsMismatchWarningText".Translate());
            num2 += rect.height + 17f;
            float num3 = num2;
            Widgets.Label(new Rect(x, num2, num, Text.LineHeight), "AddedModsList".Translate());
            num2 += Text.LineHeight + 10f;
            float height3 = root.height - num2 - Renderer.ButtonHeight - 10f;
            // this.DoModList(new Rect(x, num2, num, height3), this.addedModsList, ref this.addedModListScrollPosition, new Color?(new Color(0.27f, 0.4f, 0.1f)));
            num2 = num3;
            Widgets.Label(new Rect(x2, num2, num, Text.LineHeight), "MissingModsList".Translate());
            num2 += Text.LineHeight + 10f;
            //this.DoModList(new Rect(x2, num2, num, height3), this.missingModsList, ref this.missingModListScrollPosition, new Color?(new Color(0.38f, 0.07f, 0.09f)));
            num2 = num3;
            Widgets.Label(new Rect(x3, num2, num, Text.LineHeight), "SharedModsList".Translate());
            num2 += Text.LineHeight + 10f;
            //this.DoModList(new Rect(x3, num2, num, height3), this.sharedModsList, ref this.sharedModListScrollPosition, null);
            float y = root.height - Renderer.ButtonHeight;
            Rect rect2 = new Rect(0f, y, Renderer.ButtonWidth, Renderer.ButtonHeight);
            Rect rect3 = new Rect(root.width / 2f - Renderer.ButtonWidth - 4f, y, Renderer.ButtonWidth, Renderer.ButtonHeight);
            Rect rect4 = new Rect(root.width / 2f + 4f, y, Renderer.ButtonWidth, Renderer.ButtonHeight);
            Rect rect5 = new Rect(root.width - Renderer.ButtonWidth, y, Renderer.ButtonWidth, Renderer.ButtonHeight);
            if (Widgets.ButtonText(rect2, "GoBack".Translate(), true, true, true, null))
            {
                Renderer.HandleGoBackClicked.Invoke(__instance, new object[] {});
            }
            if (Widgets.ButtonText(rect3, "SaveModList".Translate(), true, true, true, null))
            {
                Renderer.HandleSaveCurrentModList.Invoke(__instance, new object[] { });
            }
            if (Widgets.ButtonText(rect4, "ChangeLoadedMods".Translate(), true, true, true, null))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmLoadSaveList".Translate(), delegate
                {
                    Renderer.HandleChangeLoadedModClicked.Invoke(__instance, new object[] { });
                }, true, null, WindowLayer.Dialog));
            }
            if (Widgets.ButtonText(rect5, "LoadAnyway".Translate(), true, true, true, null))
            {
                Renderer.HandleLoadAnywayClicked.Invoke(__instance, new object[] { });
            }
        }

        public static void DoModListLeft(Rect rect, List<string> addedModsList, ref Vector2 scrollPosition, Color color)
        {

        }

        public static void DoModListMiddle(Rect rect, List<string> missingModsList, ref Vector2 scrollPosition, Color color)
        {

        }

        public static void DoModListRight(Rect rect, List<string> missingModsList, ref Vector2 scrollPosition, Color color)
        {

        }
    }
}
