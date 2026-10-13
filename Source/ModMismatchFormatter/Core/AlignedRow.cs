using System;

namespace Madeline.ModMismatchFormatter.Core;

public enum RowState
{
    /// <summary>The mod is in both lists at this position.</summary>
    Unchanged,

    /// <summary>The mod is in both lists but its version differs.</summary>
    VersionChanged,

    /// <summary>The mod is only in the save (it was removed).</summary>
    Removed,

    /// <summary>The mod is only in the running list (it was added).</summary>
    Added,
}

/// <summary>One row of the side-by-side comparison. At least one side is set.</summary>
public readonly record struct AlignedRow(ModEntry? Saved, ModEntry? Active)
{
    public RowState State => (Saved, Active) switch
    {
        (not null, null) => RowState.Removed,
        (null, not null) => RowState.Added,
        (not null, not null) when Saved.Version != Active.Version => RowState.VersionChanged,
        (not null, not null) => RowState.Unchanged,
        _ => throw new InvalidOperationException("A row must have at least one mod."),
    };
}
