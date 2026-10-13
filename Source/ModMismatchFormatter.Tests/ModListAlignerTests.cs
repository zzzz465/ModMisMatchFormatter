using System;
using System.Collections.Generic;
using System.Linq;
using Madeline.ModMismatchFormatter.Core;
using Xunit;

namespace ModMismatchFormatter.Tests;

public class ModListAlignerTests
{
    private static readonly Func<ModEntry, string> ByName = mod => mod.Name;

    private static List<ModEntry> Mods(IEnumerable<string> names) =>
        names.Select(name => new ModEntry(name, name)).ToList();

    private static List<(string?, string?)> Shape(IEnumerable<AlignedRow> rows) =>
        rows.Select(row => (row.Saved?.Name, row.Active?.Name)).ToList();

    private static List<(string?, string?)> LegacyShape(IReadOnlyList<ModEntry> saved, IReadOnlyList<ModEntry> active)
    {
        static List<Legacy.Mod> ToLegacy(IReadOnlyList<ModEntry> mods) =>
            mods.Select((mod, i) => new Legacy.Mod(mod.PackageId, mod.Name, i)).ToList();

        return new Legacy.OrderFormatterImpl()
            .GetFormattedModPairs(ToLegacy(saved), ToLegacy(active))
            .Select(pair => (pair.Save.isPlaceHolder ? null : pair.Save.ModName,
                             pair.Loaded.isPlaceHolder ? null : pair.Loaded.ModName))
            .ToList();
    }

    [Fact]
    public void Identical_lists_align_row_by_row()
    {
        var mods = Mods(["a", "b", "c"]);

        var rows = ModListAligner.Align(mods, mods, ByName);

        Assert.Equal([("a", "a"), ("b", "b"), ("c", "c")], Shape(rows));
        Assert.All(rows, row => Assert.Equal(RowState.Unchanged, row.State));
    }

    [Fact]
    public void Added_and_removed_mods_get_a_gap_on_the_other_side()
    {
        var rows = ModListAligner.Align(Mods(["a", "x", "b"]), Mods(["a", "b", "y"]), ByName);

        Assert.Equal([("a", "a"), ("x", null), ("b", "b"), (null, "y")], Shape(rows));
        Assert.Equal(
            [RowState.Unchanged, RowState.Removed, RowState.Unchanged, RowState.Added],
            rows.Select(row => row.State));
    }

    [Fact]
    public void Empty_lists_produce_no_rows()
    {
        Assert.Empty(ModListAligner.Align([], [], ByName));
    }

    [Fact]
    public void Version_difference_is_reported_on_matched_rows()
    {
        var row = new AlignedRow(new ModEntry("a", "A", "1.0"), new ModEntry("a", "A", "1.1"));

        Assert.Equal(RowState.VersionChanged, row.State);
    }

    [Fact]
    public void Matches_the_legacy_algorithm_on_the_sample_mod_lists()
    {
        var saved = Mods(SampleModLists.Saved);
        var active = Mods(SampleModLists.Active);

        Assert.Equal(LegacyShape(saved, active), Shape(ModListAligner.Align(saved, active, ByName)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Matches_the_legacy_algorithm_on_random_mod_lists(int seed)
    {
        var random = new Random(seed);

        for (int run = 0; run < 2000; run++)
        {
            var (saved, active) = RandomModLists(random);

            // The legacy code throws on an empty list (List.Pop on nothing).
            if (saved.Count == 0 || active.Count == 0)
                continue;

            Assert.Equal(LegacyShape(saved, active), Shape(ModListAligner.Align(saved, active, ByName)));
        }
    }

    [Fact]
    public void Every_mod_appears_once_and_in_its_original_order()
    {
        var random = new Random(42);

        for (int run = 0; run < 500; run++)
        {
            var (saved, active) = RandomModLists(random);

            var rows = ModListAligner.Align(saved, active, ByName);

            Assert.Equal(saved, rows.Where(row => row.Saved is not null).Select(row => row.Saved!));
            Assert.Equal(active, rows.Where(row => row.Active is not null).Select(row => row.Active!));
        }
    }

    /// <summary>Builds a mod list and an edited copy (removals, additions, moves, rare duplicates).</summary>
    private static (List<ModEntry> Saved, List<ModEntry> Active) RandomModLists(Random random)
    {
        int count = random.Next(0, 40);
        var saved = Enumerable.Range(0, count).Select(i => $"m{i}").ToList();
        var active = new List<string>(saved);

        int edits = random.Next(0, 10);
        for (int i = 0; i < edits; i++)
        {
            switch (random.Next(4))
            {
                case 0 when active.Count > 0:
                    active.RemoveAt(random.Next(active.Count));
                    break;
                case 1:
                    active.Insert(random.Next(active.Count + 1), $"new{i}");
                    break;
                case 2 when active.Count > 1:
                    int from = random.Next(active.Count);
                    string moved = active[from];
                    active.RemoveAt(from);
                    active.Insert(random.Next(active.Count + 1), moved);
                    break;
                case 3 when active.Count > 0 && random.Next(5) == 0:
                    active.Insert(random.Next(active.Count + 1), active[random.Next(active.Count)]);
                    break;
            }
        }

        return (Mods(saved), Mods(active));
    }
}
