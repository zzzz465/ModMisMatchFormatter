using Madeline.ModMismatchFormatter.Core;
using UnityEngine;

namespace Madeline.ModMismatchFormatter.UI;

internal static class Palette
{
    public static readonly Color Background = new(0.11f, 0.11f, 0.11f, 1f);
    public static readonly Color Removed = new(0.29f, 0.09f, 0.09f, 1f);
    public static readonly Color Added = new(0.29f, 0.33f, 0.19f, 1f);
    public static readonly Color VersionChanged = new Color32(178, 144, 83, 255);

    public static Color For(RowState state) => state switch
    {
        RowState.Added => Added,
        RowState.Removed => Removed,
        RowState.VersionChanged => VersionChanged,
        _ => Background,
    };
}
