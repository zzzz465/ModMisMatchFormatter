using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Verse;

namespace Madeline.ModMismatchFormatter;

/// <summary>
/// Mod versions of the running mods, and the versions recorded in the save's meta header:
/// <code>
/// &lt;meta&gt;
///   &lt;modMetaDatas&gt;
///     &lt;li&gt;&lt;Identifier&gt;packageId&lt;/Identifier&gt;&lt;version&gt;1.2.3&lt;/version&gt;&lt;/li&gt;
///   &lt;/modMetaDatas&gt;
/// &lt;/meta&gt;
/// </code>
/// </summary>
internal static class ModVersions
{
    private const string ListNode = "modMetaDatas";
    private const string IdNode = "Identifier";
    private const string VersionNode = "version";

    // Older releases wrote this placeholder instead of omitting the version.
    private const string LegacyUnknown = "Unknown";

    private static readonly Dictionary<string, string?> RunningVersionCache = [];
    private static Dictionary<string, string> savedVersions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The running version of <paramref name="mod"/>, or <c>null</c> when it does not declare one.</summary>
    public static string? Running(ModContentPack mod)
    {
        if (!RunningVersionCache.TryGetValue(mod.RootDir, out var version))
        {
            version = ReadManifestVersion(mod) ?? NullIfEmpty(mod.ModMetaData?.ModVersion);
            RunningVersionCache[mod.RootDir] = version;
        }

        return version;
    }

    /// <summary>The version recorded in the last loaded meta header, or <c>null</c> when it is not recorded.</summary>
    public static string? Saved(string packageId) =>
        savedVersions.TryGetValue(packageId, out var version) ? version : null;

    /// <summary>Reads the recorded versions from the meta header the scribe loader currently holds.</summary>
    public static void ReadFromLoadedMetaHeader()
    {
        var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var list = Scribe.loader.curXmlParent?["meta"]?[ListNode];

        if (list is not null)
        {
            foreach (XmlNode item in list.ChildNodes)
            {
                var id = item[IdNode]?.InnerText;
                var version = NullIfEmpty(item[VersionNode]?.InnerText);
                if (id is not null && version is not null && version != LegacyUnknown)
                    versions[id] = version;
            }
        }

        savedVersions = versions;
        ModLog.Debug($"Read {versions.Count} mod versions from the meta header");
    }

    /// <summary>Writes the running mod versions. Must be called while the scribe is inside the meta node.</summary>
    public static void WriteToMetaHeader()
    {
        if (!Scribe.EnterNode(ListNode))
            return;

        try
        {
            foreach (var mod in LoadedModManager.RunningMods)
            {
                if (!Scribe.EnterNode("li"))
                    continue;

                try
                {
                    var id = mod.PackageId;
                    var version = Running(mod);
                    Scribe_Values.Look(ref id, IdNode);
                    Scribe_Values.Look(ref version, VersionNode);
                }
                finally
                {
                    Scribe.ExitNode();
                }
            }
        }
        finally
        {
            Scribe.ExitNode();
        }
    }

    private static string? ReadManifestVersion(ModContentPack mod)
    {
        // Manifest.xml is the convention introduced by Fluffy's Mod Manager.
        var path = Path.Combine(mod.RootDir, "About", "Manifest.xml");
        if (!File.Exists(path))
            return null;

        try
        {
            return NullIfEmpty(XDocument.Load(path).Root?.Element("version")?.Value);
        }
        catch (Exception ex)
        {
            ModLog.Error($"Could not read {path}: {ex.Message}");
            return null;
        }
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
}
