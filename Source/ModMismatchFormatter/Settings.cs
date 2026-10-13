using Verse;

namespace Madeline.ModMismatchFormatter;

// The type name is written into the settings file as Class="..."; do not rename.
public sealed class Settings : ModSettings
{
    /// <summary>Compare each mod's version with the one recorded in the save. Off on a fresh install.</summary>
    public bool useVersionCompare;

    /// <summary>Record running mod versions in the save's meta header. Cheap since 1.2, so on by default.</summary>
    public bool saveVersionToSaveFile = true;

    /// <summary>Mismatch window: list mods that did not change.</summary>
    public bool showUnchanged = true;

    /// <summary>Mismatch window: list mods whose load order changed.</summary>
    public bool showOrderChanges = true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref useVersionCompare, "useVersionCompare", true, true);
        Scribe_Values.Look(ref saveVersionToSaveFile, "saveVersionToSaveFile", true, true);
        Scribe_Values.Look(ref showUnchanged, "showUnchanged", true);
        Scribe_Values.Look(ref showOrderChanges, "showOrderChanges", true);
    }
}
