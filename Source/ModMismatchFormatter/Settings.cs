using Verse;

namespace Madeline.ModMismatchFormatter;

// The type name is written into the settings file as Class="..."; do not rename.
// Both options start off on a fresh install (no settings file yet), as they always have.
public sealed class Settings : ModSettings
{
    /// <summary>Compare each mod's version with the one recorded in the save.</summary>
    public bool useVersionCompare;

    /// <summary>Record running mod versions in the save's meta header.</summary>
    public bool saveVersionToSaveFile;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref useVersionCompare, "useVersionCompare", true, true);
        Scribe_Values.Look(ref saveVersionToSaveFile, "saveVersionToSaveFile", true, true);
    }
}
