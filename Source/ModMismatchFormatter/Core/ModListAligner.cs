using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter.Core;

/// <summary>
/// Lines up the save's mod list against the running mod list so that the same mod
/// appears on the same row, with gaps where a mod was added or removed.
/// </summary>
public static class ModListAligner
{
    /// <summary>
    /// Greedy alignment. When the current mods differ and each reappears later in
    /// the other list, the side whose match is closer to the end of the list is
    /// emitted first.
    /// </summary>
    public static IReadOnlyList<AlignedRow> Align(
        IReadOnlyList<ModEntry> saved,
        IReadOnlyList<ModEntry> active,
        Func<ModEntry, string> key)
    {
        var rows = new List<AlignedRow>(Math.Max(saved.Count, active.Count));
        int s = 0, a = 0;

        while (s < saved.Count && a < active.Count)
        {
            var savedMod = saved[s];
            var activeMod = active[a];

            if (key(savedMod) == key(activeMod))
            {
                rows.Add(new AlignedRow(savedMod, activeMod));
                s++;
                a++;
                continue;
            }

            int savedInActive = LastIndexOf(active, a + 1, key(savedMod), key);
            int activeInSaved = LastIndexOf(saved, s + 1, key(activeMod), key);

            bool takeActive = (savedInActive, activeInSaved) switch
            {
                (< 0, _) => false,
                (_, < 0) => true,
                _ => active.Count - savedInActive > saved.Count - activeInSaved,
            };

            if (takeActive)
                rows.Add(new AlignedRow(null, active[a++]));
            else
                rows.Add(new AlignedRow(saved[s++], null));
        }

        while (s < saved.Count)
            rows.Add(new AlignedRow(saved[s++], null));

        while (a < active.Count)
            rows.Add(new AlignedRow(null, active[a++]));

        return rows;
    }

    private static int LastIndexOf(IReadOnlyList<ModEntry> mods, int start, string value, Func<ModEntry, string> key)
    {
        for (int i = mods.Count - 1; i >= start; i--)
        {
            if (key(mods[i]) == value)
                return i;
        }

        return -1;
    }
}
