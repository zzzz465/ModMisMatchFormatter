namespace Madeline.ModMismatchFormatter.Core;

public enum RowState
{
    /// <summary>The mod is in both lists at this position.</summary>
    Unchanged,

    /// <summary>The mod is in both lists at this position but its version differs.</summary>
    VersionChanged,

    /// <summary>The mod is only in the save (it was removed).</summary>
    Removed,

    /// <summary>The mod is only in the running list (it was added).</summary>
    Added,

    /// <summary>The mod's position in the save; it is still running but loads elsewhere now.</summary>
    MovedFrom,

    /// <summary>The mod's new position in the running list; it was elsewhere in the save.</summary>
    MovedTo,
}

/// <summary>One row of the side-by-side comparison. At least one side is set.</summary>
public readonly record struct AlignedRow(ModEntry? Saved, ModEntry? Active, RowState State)
{
    public bool IsDifference => State != RowState.Unchanged;
}
