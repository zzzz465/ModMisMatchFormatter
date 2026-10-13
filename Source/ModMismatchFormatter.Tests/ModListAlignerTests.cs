using System;
using System.Collections.Generic;
using System.Linq;
using Madeline.ModMismatchFormatter.Core;
using Xunit;

namespace ModMismatchFormatter.Tests;

public class ModListAlignerTests
{
    private static List<ModEntry> Mods(IEnumerable<string> ids) =>
        ids.Select(id => new ModEntry(id, id)).ToList();

    private static List<(string?, string?)> Shape(IEnumerable<AlignedRow> rows) =>
        rows.Select(row => (row.Saved?.PackageId, row.Active?.PackageId)).ToList();

    private static List<(string?, string?)> Shape(IEnumerable<(ModEntry? Saved, ModEntry? Active)> pairs) =>
        pairs.Select(pair => (pair.Saved?.PackageId, pair.Active?.PackageId)).ToList();

    private static int Matched(IEnumerable<AlignedRow> rows) => rows.Count(row => row.Saved is not null && row.Active is not null);

    /// <summary>Independent textbook LCS length, used as the optimum to compare against.</summary>
    private static int LcsLength(IReadOnlyList<ModEntry> a, IReadOnlyList<ModEntry> b)
    {
        var table = new int[a.Count + 1, b.Count + 1];
        for (int i = 1; i <= a.Count; i++)
        for (int j = 1; j <= b.Count; j++)
            table[i, j] = ModEntry.SameMod(a[i - 1], b[j - 1])
                ? table[i - 1, j - 1] + 1
                : Math.Max(table[i - 1, j], table[i, j - 1]);
        return table[a.Count, b.Count];
    }

    private static List<(string?, string?)> LegacyShape(IReadOnlyList<ModEntry> saved, IReadOnlyList<ModEntry> active)
    {
        static List<Legacy.Mod> ToLegacy(IReadOnlyList<ModEntry> mods) =>
            mods.Select((mod, i) => new Legacy.Mod(mod.PackageId, mod.PackageId, i)).ToList();

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

        var rows = ModListAligner.Align(mods, mods);

        Assert.Equal([("a", "a"), ("b", "b"), ("c", "c")], Shape(rows));
        Assert.All(rows, row => Assert.Equal(RowState.Unchanged, row.State));
    }

    [Fact]
    public void Added_and_removed_mods_get_a_gap_on_the_other_side()
    {
        var rows = ModListAligner.Align(Mods(["a", "x", "b"]), Mods(["a", "b", "y"]));

        Assert.Equal([("a", "a"), ("x", null), ("b", "b"), (null, "y")], Shape(rows));
        Assert.Equal(
            [RowState.Unchanged, RowState.Removed, RowState.Unchanged, RowState.Added],
            rows.Select(row => row.State));
    }

    [Fact]
    public void Removed_is_shown_before_added_at_the_same_position()
    {
        var rows = ModListAligner.Align(Mods(["a", "x", "b"]), Mods(["a", "y", "b"]));

        Assert.Equal([("a", "a"), ("x", null), (null, "y"), ("b", "b")], Shape(rows));
    }

    [Fact]
    public void Reordered_mod_is_reported_as_moved_not_added_and_removed()
    {
        var rows = ModListAligner.Align(Mods(["a", "b", "c", "d"]), Mods(["a", "c", "d", "b"]));

        Assert.Equal([("a", "a"), ("b", null), ("c", "c"), ("d", "d"), (null, "b")], Shape(rows));
        Assert.Equal(RowState.MovedFrom, rows[1].State);
        Assert.Equal(RowState.MovedTo, rows[4].State);
    }

    [Fact]
    public void Matches_package_ids_case_insensitively_and_old_saves_by_folder_name()
    {
        var saved = new List<ModEntry> { new("Ludeon.RimWorld", "Core"), new("1234567", "Workshop mod") };
        var active = new List<ModEntry> { new("ludeon.rimworld", "Core"), new("author.mod", "Workshop mod", FolderName: "1234567") };

        var rows = ModListAligner.Align(saved, active);

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Equal(RowState.Unchanged, row.State));
    }

    [Theory]
    [InlineData("1.0", "1.1", RowState.VersionChanged)]
    [InlineData("1.0", "1.0", RowState.Unchanged)]
    [InlineData(null, "1.1", RowState.Unchanged)] // Save has no version record.
    [InlineData("1.0", null, RowState.Unchanged)]
    public void Version_change_needs_both_versions_known(string? savedVersion, string? activeVersion, RowState expected)
    {
        var rows = ModListAligner.Align([new ModEntry("a", "A", savedVersion)], [new ModEntry("a", "A", activeVersion)]);

        Assert.Equal(expected, Assert.Single(rows).State);
    }

    [Fact]
    public void Empty_lists_produce_no_rows()
    {
        Assert.Empty(ModListAligner.Align([], []));
    }

    [Fact]
    public void Sample_mod_lists_align_optimally()
    {
        var saved = Mods(SampleModLists.Saved);
        var active = Mods(SampleModLists.Active);

        var rows = ModListAligner.Align(saved, active);

        Assert.Equal(LcsLength(saved, active), Matched(rows));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Matches_as_many_mods_as_possible_and_never_fewer_than_the_old_algorithm(int seed)
    {
        var random = new Random(seed);

        for (int run = 0; run < 2000; run++)
        {
            var (saved, active) = RandomModLists(random);

            var rows = ModListAligner.Align(saved, active);

            Assert.Equal(LcsLength(saved, active), Matched(rows));
            if (saved.Count > 0 && active.Count > 0) // The old code throws on an empty list.
                Assert.True(Matched(rows) >= LegacyShape(saved, active).Count(p => p.Item1 is not null && p.Item2 is not null));
        }
    }

    [Theory]
    [InlineData(ModListAligner.DefaultMaxTableCells)]
    [InlineData(0)] // Forces the greedy fallback for the middle part.
    public void Every_mod_appears_once_and_in_its_original_order(int maxTableCells)
    {
        var random = new Random(42);

        for (int run = 0; run < 500; run++)
        {
            var (saved, active) = RandomModLists(random);

            var rows = ModListAligner.Align(saved, active, maxTableCells);

            Assert.Equal(saved, rows.Where(row => row.Saved is not null).Select(row => row.Saved!));
            Assert.Equal(active, rows.Where(row => row.Active is not null).Select(row => row.Active!));
            Assert.All(rows.Where(row => row.Saved is not null && row.Active is not null),
                row => Assert.True(ModEntry.SameMod(row.Saved!, row.Active!)));
        }
    }

    [Fact]
    public void Greedy_fallback_is_the_original_algorithm()
    {
        var random = new Random(7);

        for (int run = 0; run < 2000; run++)
        {
            var (saved, active) = RandomModLists(random);
            if (saved.Count == 0 || active.Count == 0)
                continue;

            var pairs = new List<(ModEntry?, ModEntry?)>();
            ModListAligner.AlignGreedy(saved, active, ModEntry.SameMod, pairs);

            Assert.Equal(LegacyShape(saved, active), Shape(pairs));
        }
    }

    /// <summary>Builds a mod list and an edited copy (removals, additions, moves).</summary>
    private static (List<ModEntry> Saved, List<ModEntry> Active) RandomModLists(Random random)
    {
        int count = random.Next(0, 40);
        var saved = Enumerable.Range(0, count).Select(i => $"m{i}").ToList();
        var active = new List<string>(saved);

        int edits = random.Next(0, 10);
        for (int i = 0; i < edits; i++)
        {
            switch (random.Next(3))
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
            }
        }

        return (Mods(saved), Mods(active));
    }
}
