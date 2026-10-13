using System;

namespace Madeline.ModMismatchFormatter.Core;

/// <summary>A mod as recorded in a save file or as currently running.</summary>
/// <param name="PackageId">The mod's package id. Saves made before RimWorld 1.1 store the folder name here.</param>
/// <param name="Name">Display name.</param>
/// <param name="Version">Mod version, or <c>null</c> when it is not known.</param>
/// <param name="FolderName">Folder name of a running mod, used to match pre-1.1 saves.</param>
public sealed record ModEntry(string PackageId, string Name, string? Version = null, string? FolderName = null)
{
    /// <summary>Whether a mod from the save and a running mod are the same mod (ignoring version).</summary>
    public static bool SameMod(ModEntry saved, ModEntry active) =>
        string.Equals(saved.PackageId, active.PackageId, StringComparison.OrdinalIgnoreCase)
        || (active.FolderName is not null && string.Equals(saved.PackageId, active.FolderName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Versions differ only when both are known; a save without version data proves nothing.</summary>
    public static bool VersionChanged(string? saved, string? active) =>
        saved is not null && active is not null && saved != active;
}
