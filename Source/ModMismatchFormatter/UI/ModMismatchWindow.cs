using System;
using System.Collections.Generic;
using System.Linq;
using Madeline.ModMismatchFormatter.Core;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Madeline.ModMismatchFormatter.UI;

/// <summary>Shows the save's mod list and the running mod list side by side, aligned row by row.</summary>
public sealed class ModMismatchWindow : Window
{
    private const float TitleHeight = 36f;
    private const float HeaderHeight = 26f;
    private const float ColumnGap = 16f;
    private const float SeparatorHeight = 2f;
    private const float ButtonHeight = 40f;
    private const float ButtonMargin = 10f;

    private readonly Action loadGame;
    private readonly bool showVersions;
    private readonly Dictionary<string, string> truncateCache = [];
    private IReadOnlyList<AlignedRow> rows = [];
    private Vector2 scrollPosition;

    public ModMismatchWindow(Action loadGame, bool showVersions)
    {
        this.loadGame = loadGame;
        this.showVersions = showVersions;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new(650f, 700f);

    public override void PreOpen()
    {
        base.PreOpen();
        rows = ModListAligner.Align(SavedMods(), RunningMods(), mod => mod.Name);
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, TitleHeight), "ModsMismatchWarningTitle".Translate());

        var buttons = new Rect(inRect.x, inRect.yMax - ButtonHeight - ButtonMargin, inRect.width, ButtonHeight);
        var list = new Rect(inRect.x, inRect.y + TitleHeight + HeaderHeight, inRect.width, buttons.y - ButtonMargin - (inRect.y + TitleHeight + HeaderHeight));
        float columnWidth = (list.width - GenUI.ScrollBarWidth - ColumnGap) / 2f;

        Widgets.Label(new Rect(list.x, list.y - HeaderHeight, columnWidth, HeaderHeight), "SaveModListTitle".Translate());
        Widgets.Label(new Rect(list.x + columnWidth + ColumnGap, list.y - HeaderHeight, columnWidth, HeaderHeight), "ActiveModListTitle".Translate());

        DrawRows(list, columnWidth);
        DrawButtons(buttons, columnWidth);
    }

    private float RowHeight => showVersions
        ? Text.LineHeightOf(GameFont.Small) + Text.LineHeightOf(GameFont.Tiny)
        : Text.LineHeightOf(GameFont.Small);

    private void DrawRows(Rect outRect, float columnWidth)
    {
        Widgets.DrawBoxSolid(outRect, Palette.Background);

        float stride = RowHeight + SeparatorHeight;
        var viewRect = new Rect(0f, 0f, outRect.width - GenUI.ScrollBarWidth, rows.Count * stride);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        for (int i = 0; i < rows.Count; i++)
        {
            float y = i * stride;

            // Only draw what is visible; mod lists can be very long.
            if (y + stride < scrollPosition.y || y > scrollPosition.y + outRect.height)
                continue;

            var row = rows[i];
            DrawCell(new Rect(0f, y, columnWidth, RowHeight), row.Saved, row.State == RowState.Removed ? RowState.Removed : RowState.Unchanged);
            DrawCell(new Rect(columnWidth + ColumnGap, y, columnWidth, RowHeight), row.Active, row.State == RowState.Removed ? RowState.Unchanged : row.State);

            Widgets.DrawBoxSolid(new Rect(0f, y + RowHeight, columnWidth, SeparatorHeight), Color.grey);
            Widgets.DrawBoxSolid(new Rect(columnWidth + ColumnGap, y + RowHeight, columnWidth, SeparatorHeight), Color.grey);
        }
        Widgets.EndScrollView();
    }

    private void DrawCell(Rect rect, ModEntry? mod, RowState state)
    {
        if (mod is null)
        {
            Widgets.DrawBoxSolid(rect, Palette.Background);
            return;
        }

        Widgets.DrawBoxSolid(rect, Palette.For(state));

        var markRect = new Rect(rect.x + 4f, rect.y, 12f, rect.height);
        var textRect = new Rect(markRect.xMax + 2f, rect.y, rect.xMax - markRect.xMax - 4f, rect.height);
        float nameHeight = Text.LineHeightOf(GameFont.Small);

        Text.Anchor = TextAnchor.MiddleLeft;
        Text.Font = GameFont.Small;
        Widgets.Label(new Rect(markRect.x, rect.y, markRect.width, nameHeight), Mark(state));
        Widgets.Label(new Rect(textRect.x, rect.y, textRect.width, nameHeight), mod.Name.Truncate(textRect.width, truncateCache));

        if (showVersions)
        {
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(textRect.x, rect.y + nameHeight, textRect.width, rect.height - nameHeight), $"ver : {mod.Version ?? "Unknown"}");
        }

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;

        TooltipHandler.TipRegion(rect, $"{mod.Name}\n{mod.PackageId}");
    }

    private static string Mark(RowState state) => state switch
    {
        RowState.Added => "+",
        RowState.Removed => "-",
        RowState.VersionChanged => "?",
        _ => "",
    };

    private void DrawButtons(Rect area, float columnWidth)
    {
        float buttonWidth = columnWidth - 50f;
        float leftX = area.x + (columnWidth - buttonWidth) / 2f;
        float rightX = area.x + columnWidth + ColumnGap + (columnWidth - buttonWidth) / 2f;

        if (Widgets.ButtonText(new Rect(leftX, area.y, buttonWidth, area.height), "ChangeLoadedMods".Translate()))
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "ConfirmLoadSaveList".Translate(), ChangeLoadedModsToSave, destructive: true));
        }

        if (Widgets.ButtonText(new Rect(rightX, area.y, buttonWidth, area.height), "LoadAnyway".Translate()))
            LoadAnyway();
    }

    public override void OnAcceptKeyPressed() => LoadAnyway();

    private void LoadAnyway()
    {
        SoundDefOf.Click.PlayOneShotOnCamera();
        loadGame();
        Close();
    }

    // Mirrors vanilla Dialog_ModMismatch: activate the save's mod list and restart.
    private static void ChangeLoadedModsToSave()
    {
        var ids = ScribeMetaHeaderUtility.loadedModIdsList.Select(id => id.ToLower()).ToList();
        var names = ScribeMetaHeaderUtility.loadedModNamesList;

        if (Current.ProgramState == ProgramState.Entry)
            ModsConfig.SetActiveToList(ids);
        ModsConfig.SaveFromList(ids);

        var missing = ids
            .Select((id, i) => (id, name: i < names.Count ? names[i] : id))
            .Where(mod => ModLister.GetModWithIdentifier(mod.id) is null)
            .Select(mod => mod.name)
            .ToList();
        if (missing.Count > 0)
            Messages.Message($"{"MissingMods".Translate()}: {missing.ToCommaList()}", MessageTypeDefOf.RejectInput, historical: false);

        ModsConfig.RestartFromChangedMods();
    }

    private List<ModEntry> SavedMods()
    {
        var ids = ScribeMetaHeaderUtility.loadedModIdsList ?? [];
        var names = ScribeMetaHeaderUtility.loadedModNamesList ?? [];

        return ids
            .Select((id, i) => new ModEntry(id, i < names.Count ? names[i] : id, showVersions ? ModVersions.Saved(id) : null))
            .ToList();
    }

    private List<ModEntry> RunningMods() =>
        LoadedModManager.RunningMods
            .Select(mod => new ModEntry(mod.PackageId, mod.Name, showVersions ? ModVersions.Running(mod) : null))
            .ToList();
}
