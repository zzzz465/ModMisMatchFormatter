using UnityEngine;
using Verse;

namespace Madeline.ModMismatchFormatter;

// The class name is part of the settings file name (Mod_<packageId>_ModMismatchFormatter.xml); do not rename.
public sealed class ModMismatchFormatter : Mod
{
    public ModMismatchFormatter(ModContentPack content) : base(content)
    {
        Settings = GetSettings<Settings>();
    }

    internal static Settings Settings { get; private set; } = null!;

    public override string SettingsCategory() => "SettingsLabel".Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.CheckboxLabeled(
            "EnableVersionChecking".Translate(),
            ref Settings.useVersionCompare,
            "useVersionCompareDescription".Translate());
        listing.CheckboxLabeled(
            "EnableWritingCustomVersionHeader".Translate(),
            ref Settings.saveVersionToSaveFile,
            "writeMetaDescription".Translate());
        listing.End();

        base.DoSettingsWindowContents(inRect);
    }
}
