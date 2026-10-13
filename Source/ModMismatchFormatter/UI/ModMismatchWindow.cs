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
    private const float Gap = 10f;
    private const float ColumnGap = 16f;
    private const float ToolbarHeight = 24f;
    private const float HeaderHeight = 24f;
    private const float ButtonHeight = 38f;
    private const float MarkWidth = 14f;

    private readonly Action loadGame;
    private readonly bool showVersions;
    private readonly QuickSearchWidget search = new();
    private readonly Dictionary<string, string> truncateCache = [];

    private IReadOnlyList<AlignedRow> rows = [];
    private List<AlignedRow> visibleRows = [];
    private Dictionary<RowState, int> counts = [];
    private int savedCount;
    private int activeCount;
    private Vector2 scrollPosition;

    public ModMismatchWindow(Action loadGame, bool showVersions)
    {
        this.loadGame = loadGame;
        this.showVersions = showVersions;
        doCloseX = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new(860f, 760f);

    private static Settings Settings => ModMismatchFormatter.Settings;

    public override void PreOpen()
    {
        base.PreOpen();

        var saved = SavedMods();
        var active = RunningMods();
        savedCount = saved.Count;
        activeCount = active.Count;
        rows = ModListAligner.Align(saved, active);
        counts = rows.GroupBy(row => row.State).ToDictionary(group => group.Key, group => group.Count());
        RefreshVisibleRows();
    }

    public override void DoWindowContents(Rect inRect)
    {
        float y = inRect.y;

        Text.Font = GameFont.Medium;
        float titleHeight = Text.LineHeight;
        Widgets.Label(new Rect(inRect.x, y, inRect.width, titleHeight), "ModsMismatchWarningTitle".Translate());
        Text.Font = GameFont.Small;
        y += titleHeight + 4f;

        DrawSummary(new Rect(inRect.x, y, inRect.width, Text.LineHeight));
        y += Text.LineHeight + Gap;

        DrawToolbar(new Rect(inRect.x, y, inRect.width, ToolbarHeight));
        y += ToolbarHeight + Gap;

        var buttons = new Rect(inRect.x, inRect.yMax - ButtonHeight, inRect.width, ButtonHeight);
        var list = new Rect(inRect.x, y + HeaderHeight, inRect.width, buttons.y - Gap - (y + HeaderHeight));
        float columnWidth = (list.width - GenUI.ScrollBarWidth - ColumnGap) / 2f;

        Widgets.Label(new Rect(list.x, y, columnWidth, HeaderHeight), $"{"SaveModListTitle".Translate()} ({savedCount})");
        Widgets.Label(new Rect(list.x + columnWidth + ColumnGap, y, columnWidth, HeaderHeight), $"{"ActiveModListTitle".Translate()} ({activeCount})");

        DrawRows(list, columnWidth);
        DrawButtons(buttons);
    }

    // Colored counts that double as the legend.
    private void DrawSummary(Rect rect)
    {
        // A moved mod has one MovedFrom and one MovedTo row; count it once.
        (RowState State, string Key)[] entries =
        [
            (RowState.Added, "ModMismatchFormatter_Added"),
            (RowState.Removed, "ModMismatchFormatter_Removed"),
            (RowState.MovedFrom, "ModMismatchFormatter_Moved"),
            (RowState.VersionChanged, "ModMismatchFormatter_VersionChanged"),
        ];

        float x = rect.x;
        foreach (var (state, key) in entries)
        {
            if (state == RowState.VersionChanged && !showVersions)
                continue;

            string label = key.Translate(counts.TryGetValue(state, out int count) ? count : 0);
            float width = Text.CalcSize(label).x;

            var swatch = new Rect(x, rect.y + (rect.height - 12f) / 2f, 12f, 12f);
            Widgets.DrawBoxSolid(swatch, Palette.For(state));
            Widgets.Label(new Rect(swatch.xMax + 4f, rect.y, width, rect.height), label);
            x = swatch.xMax + 4f + width + 18f;
        }
    }

    private void DrawToolbar(Rect rect)
    {
        search.OnGUI(new Rect(rect.x, rect.y, Mathf.Min(260f, rect.width / 3f), rect.height), RefreshVisibleRows);

        float x = rect.x + Mathf.Min(260f, rect.width / 3f) + 24f;
        bool changed = Toggle(ref x, rect, "ModMismatchFormatter_ShowUnchanged", ref Settings.showUnchanged);
        changed |= Toggle(ref x, rect, "ModMismatchFormatter_ShowOrderChanges", ref Settings.showOrderChanges);

        if (changed)
        {
            Settings.Write();
            RefreshVisibleRows();
        }
    }

    private static bool Toggle(ref float x, Rect toolbar, string key, ref bool value)
    {
        string label = key.Translate();
        float width = Text.CalcSize(label).x + 24f + 6f;
        bool before = value;
        Widgets.CheckboxLabeled(new Rect(x, toolbar.y, width, toolbar.height), label, ref value, placeCheckboxNearText: true);
        x += width + 24f;
        return before != value;
    }

    private void RefreshVisibleRows()
    {
        visibleRows = rows.Where(IsVisible).ToList();
        search.noResultsMatched = search.filter.Active && visibleRows.Count == 0;
    }

    private bool IsVisible(AlignedRow row)
    {
        if (!Settings.showUnchanged && row.State == RowState.Unchanged)
            return false;
        if (!Settings.showOrderChanges && row.State is RowState.MovedFrom or RowState.MovedTo)
            return false;
        if (!search.filter.Active)
            return true;

        return new[] { row.Saved, row.Active }.Any(mod =>
            mod is not null && (search.filter.Matches(mod.Name) || search.filter.Matches(mod.PackageId)));
    }

    private float RowHeight => showVersions
        ? Text.LineHeightOf(GameFont.Small) + Text.LineHeightOf(GameFont.Tiny)
        : Text.LineHeightOf(GameFont.Small) + 2f;

    private void DrawRows(Rect outRect, float columnWidth)
    {
        Widgets.DrawBoxSolid(outRect, Palette.Background);

        float rowHeight = RowHeight;
        var viewRect = new Rect(0f, 0f, outRect.width - GenUI.ScrollBarWidth, visibleRows.Count * rowHeight);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        // Only rows inside the viewport are drawn; mod lists can have well over a thousand entries.
        int first = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / rowHeight));
        int last = Mathf.Min(visibleRows.Count - 1, Mathf.CeilToInt((scrollPosition.y + outRect.height) / rowHeight));
        for (int i = first; i <= last; i++)
        {
            var row = visibleRows[i];
            var rowRect = new Rect(0f, i * rowHeight, viewRect.width, rowHeight);
            if (i % 2 == 0)
                Widgets.DrawLightHighlight(rowRect);

            DrawCell(new Rect(0f, rowRect.y, columnWidth, rowHeight), row.Saved, row, isSavedSide: true);
            DrawCell(new Rect(columnWidth + ColumnGap, rowRect.y, columnWidth, rowHeight), row.Active, row, isSavedSide: false);
        }
        Widgets.EndScrollView();
    }

    private void DrawCell(Rect rect, ModEntry? mod, AlignedRow row, bool isSavedSide)
    {
        if (mod is null)
        {
            Widgets.DrawBoxSolid(rect.ContractedBy(0f, 1f), Palette.EmptyCell);
            return;
        }

        // Each state belongs to one side: removals/old positions on the left, the rest on the right.
        bool ownsState = row.State is RowState.Removed or RowState.MovedFrom ? isSavedSide : !isSavedSide;
        var state = ownsState ? row.State : RowState.Unchanged;
        if (state != RowState.Unchanged)
            Widgets.DrawBoxSolid(rect.ContractedBy(0f, 1f), Palette.For(state));
        Widgets.DrawHighlightIfMouseover(rect);

        float nameHeight = Text.LineHeightOf(GameFont.Small);
        var nameRect = new Rect(rect.x + 4f + MarkWidth, rect.y, rect.width - MarkWidth - 8f, nameHeight);

        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(new Rect(rect.x + 4f, rect.y, MarkWidth, nameHeight), Palette.Mark(state));
        Widgets.Label(nameRect, mod.Name.Truncate(nameRect.width, truncateCache));

        if (showVersions)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(nameRect.x, nameRect.yMax, nameRect.width, rect.height - nameHeight), VersionLabel(mod, row, isSavedSide));
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }
        Text.Anchor = TextAnchor.UpperLeft;

        TooltipHandler.TipRegion(rect, () => Tooltip(mod, state), mod.PackageId.GetHashCode() ^ (isSavedSide ? 1 : 2));
    }

    private static string VersionLabel(ModEntry mod, AlignedRow row, bool isSavedSide)
    {
        string version = mod.Version ?? "Unknown";
        return row.State == RowState.VersionChanged && !isSavedSide
            ? $"ver : {row.Saved!.Version} -> {version}"
            : $"ver : {version}";
    }

    private static string Tooltip(ModEntry mod, RowState state)
    {
        string text = $"{mod.Name}\n{mod.PackageId}";
        if (mod.Version is not null)
            text += $"\nver : {mod.Version}";
        if (state != RowState.Unchanged)
            text += $"\n\n{$"ModMismatchFormatter_State_{state}".Translate()}";
        return text;
    }

    private void DrawButtons(Rect area)
    {
        const int count = 3;
        float width = (area.width - Gap * (count - 1)) / count;
        Rect Button(int i) => new(area.x + i * (width + Gap), area.y, width, area.height);

        if (Widgets.ButtonText(Button(0), "ChangeLoadedMods".Translate()))
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "ConfirmLoadSaveList".Translate(), ChangeLoadedModsToSave, destructive: true));
        }

        if (Widgets.ButtonText(Button(1), "SaveModList".Translate()))
            SaveRunningModList();

        if (Widgets.ButtonText(Button(2), "LoadAnyway".Translate()))
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

    // Mirrors vanilla Dialog_ModMismatch: save the running mod list as a preset.
    private static void SaveRunningModList()
    {
        SoundDefOf.Click.PlayOneShotOnCamera();
        var running = LoadedModManager.RunningMods.ToList();
        var modList = new ModList
        {
            ids = running.Select(mod => mod.PackageId.ToLower()).ToList(),
            names = running.Select(mod => mod.Name).ToList(),
        };
        Find.WindowStack.Add(new Dialog_ModList_Save(modList));
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
            .Select(mod => new ModEntry(mod.PackageId, mod.Name, showVersions ? ModVersions.Running(mod) : null, mod.FolderName))
            .ToList();
}
