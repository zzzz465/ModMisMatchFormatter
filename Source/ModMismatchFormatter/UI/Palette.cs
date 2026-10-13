using Madeline.ModMismatchFormatter.Core;
using UnityEngine;

namespace Madeline.ModMismatchFormatter.UI;

internal static class Palette
{
    public static readonly Color Background = new(0.11f, 0.11f, 0.11f, 1f);
    public static readonly Color EmptyCell = new(0.07f, 0.07f, 0.07f, 1f);
    public static readonly Color Removed = new(0.29f, 0.09f, 0.09f, 1f);
    public static readonly Color Added = new(0.29f, 0.33f, 0.19f, 1f);
    // As dark as the other state colors so white text stays readable on it.
    public static readonly Color VersionChanged = new(0.40f, 0.31f, 0.12f, 1f);
    public static readonly Color Moved = new(0.16f, 0.24f, 0.36f, 1f);

    /// <summary>Secondary text on a state-colored cell, where plain gray would blend in.</summary>
    public static readonly Color TextOnState = new(1f, 1f, 1f, 0.8f);

    public static Color For(RowState state) => state switch
    {
        RowState.Added => Added,
        RowState.Removed => Removed,
        RowState.VersionChanged => VersionChanged,
        RowState.MovedFrom or RowState.MovedTo => Moved,
        _ => Background,
    };

    public static string Mark(RowState state) => state switch
    {
        RowState.Added => "+",
        RowState.Removed => "-",
        RowState.VersionChanged => "?",
        RowState.MovedFrom or RowState.MovedTo => "~",
        _ => "",
    };
}
