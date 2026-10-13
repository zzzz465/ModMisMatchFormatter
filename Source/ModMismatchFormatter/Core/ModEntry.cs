namespace Madeline.ModMismatchFormatter.Core;

/// <summary>A mod as recorded in a save file or as currently running.</summary>
/// <param name="PackageId">The mod's package id.</param>
/// <param name="Name">Display name.</param>
/// <param name="Version">Mod version, or <c>null</c> when it is not known.</param>
public sealed record ModEntry(string PackageId, string Name, string? Version = null);
