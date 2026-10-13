using System;
using System.Collections.Generic;
using System.Linq;

namespace Madeline.ModMismatchFormatter.Core;

/// <summary>
/// Lines up the save's mod list against the running mod list so that the same mod
/// appears on the same row, with gaps where a mod was added or removed.
/// </summary>
public static class ModListAligner
{
    // An LCS table of this many cells is 16 MB. Bigger differences fall back to the greedy pass.
    internal const int DefaultMaxTableCells = 4_000_000;

    /// <summary>
    /// Aligns the lists so that as many mods as possible share a row (longest common subsequence),
    /// matching mods with <see cref="ModEntry.SameMod"/>. Mods present in both lists but in a
    /// different order are reported as <see cref="RowState.MovedFrom"/>/<see cref="RowState.MovedTo"/>.
    /// </summary>
    public static IReadOnlyList<AlignedRow> Align(IReadOnlyList<ModEntry> saved, IReadOnlyList<ModEntry> active) =>
        Align(saved, active, DefaultMaxTableCells);

    internal static IReadOnlyList<AlignedRow> Align(IReadOnlyList<ModEntry> saved, IReadOnlyList<ModEntry> active, int maxTableCells)
    {
        var pairs = new List<(ModEntry? Saved, ModEntry? Active)>(Math.Max(saved.Count, active.Count));

        // Mod lists usually share long runs at both ends; only the middle needs the table.
        int prefix = 0;
        while (prefix < saved.Count && prefix < active.Count && ModEntry.SameMod(saved[prefix], active[prefix]))
            prefix++;

        int suffix = 0;
        while (suffix < saved.Count - prefix && suffix < active.Count - prefix
               && ModEntry.SameMod(saved[saved.Count - 1 - suffix], active[active.Count - 1 - suffix]))
            suffix++;

        for (int i = 0; i < prefix; i++)
            pairs.Add((saved[i], active[i]));

        var savedMiddle = Slice(saved, prefix, saved.Count - prefix - suffix);
        var activeMiddle = Slice(active, prefix, active.Count - prefix - suffix);

        if ((long)(savedMiddle.Count + 1) * (activeMiddle.Count + 1) <= maxTableCells)
            AlignLcs(savedMiddle, activeMiddle, pairs);
        else
            AlignGreedy(savedMiddle, activeMiddle, ModEntry.SameMod, pairs);

        for (int i = suffix; i > 0; i--)
            pairs.Add((saved[saved.Count - i], active[active.Count - i]));

        return Classify(pairs);
    }

    private static void AlignLcs(IReadOnlyList<ModEntry> saved, IReadOnlyList<ModEntry> active, List<(ModEntry?, ModEntry?)> pairs)
    {
        int n = saved.Count, m = active.Count, width = m + 1;

        // lcs[i * width + j] = length of the LCS of saved[i..] and active[j..].
        var lcs = new int[(n + 1) * width];
        for (int i = n - 1; i >= 0; i--)
        {
            for (int j = m - 1; j >= 0; j--)
            {
                lcs[i * width + j] = ModEntry.SameMod(saved[i], active[j])
                    ? lcs[(i + 1) * width + j + 1] + 1
                    : Math.Max(lcs[(i + 1) * width + j], lcs[i * width + j + 1]);
            }
        }

        int s = 0, a = 0;
        while (s < n && a < m)
        {
            if (ModEntry.SameMod(saved[s], active[a]))
                pairs.Add((saved[s++], active[a++]));
            else if (lcs[(s + 1) * width + a] >= lcs[s * width + a + 1])
                pairs.Add((saved[s++], null)); // On ties, show what was removed before what was added.
            else
                pairs.Add((null, active[a++]));
        }

        while (s < n)
            pairs.Add((saved[s++], null));
        while (a < m)
            pairs.Add((null, active[a++]));
    }

    /// <summary>
    /// The original (pre-1.2) greedy alignment: when the current mods differ and each reappears
    /// later in the other list, the side whose match is closer to the end of the list is emitted first.
    /// Linear in memory, so it is kept for lists too different for the LCS table.
    /// </summary>
    internal static void AlignGreedy(
        IReadOnlyList<ModEntry> saved, IReadOnlyList<ModEntry> active,
        Func<ModEntry, ModEntry, bool> same, List<(ModEntry?, ModEntry?)> pairs)
    {
        int s = 0, a = 0;

        while (s < saved.Count && a < active.Count)
        {
            var savedMod = saved[s];
            var activeMod = active[a];

            if (same(savedMod, activeMod))
            {
                pairs.Add((savedMod, activeMod));
                s++;
                a++;
                continue;
            }

            int savedInActive = LastIndex(active, a + 1, mod => same(savedMod, mod));
            int activeInSaved = LastIndex(saved, s + 1, mod => same(mod, activeMod));

            bool takeActive = (savedInActive, activeInSaved) switch
            {
                (< 0, _) => false,
                (_, < 0) => true,
                _ => active.Count - savedInActive > saved.Count - activeInSaved,
            };

            if (takeActive)
                pairs.Add((null, active[a++]));
            else
                pairs.Add((saved[s++], null));
        }

        while (s < saved.Count)
            pairs.Add((saved[s++], null));
        while (a < active.Count)
            pairs.Add((null, active[a++]));
    }

    private static IReadOnlyList<AlignedRow> Classify(List<(ModEntry? Saved, ModEntry? Active)> pairs)
    {
        // A mod left unmatched on both sides is still running; only its load order changed.
        var unmatchedActive = pairs.Where(p => p.Saved is null).Select(p => p.Active!).ToList();
        var unmatchedSaved = pairs.Where(p => p.Active is null).Select(p => p.Saved!).ToList();

        return pairs.Select(pair => pair switch
        {
            (null, { } active) => new AlignedRow(null, active,
                unmatchedSaved.Any(saved => ModEntry.SameMod(saved, active)) ? RowState.MovedTo : RowState.Added),
            ({ } saved, null) => new AlignedRow(saved, null,
                unmatchedActive.Any(active => ModEntry.SameMod(saved, active)) ? RowState.MovedFrom : RowState.Removed),
            ({ } saved, { } active) => new AlignedRow(saved, active,
                ModEntry.VersionChanged(saved.Version, active.Version) ? RowState.VersionChanged : RowState.Unchanged),
            _ => throw new InvalidOperationException("A row must have at least one mod."),
        }).ToList();
    }

    private static IReadOnlyList<ModEntry> Slice(IReadOnlyList<ModEntry> mods, int start, int count)
    {
        var slice = new ModEntry[count];
        for (int i = 0; i < count; i++)
            slice[i] = mods[start + i];
        return slice;
    }

    private static int LastIndex(IReadOnlyList<ModEntry> mods, int start, Func<ModEntry, bool> match)
    {
        for (int i = mods.Count - 1; i >= start; i--)
        {
            if (match(mods[i]))
                return i;
        }

        return -1;
    }
}
