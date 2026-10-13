using HarmonyLib;
using Verse;

namespace Madeline.ModMismatchFormatter.Patches;

[StaticConstructorOnStartup]
internal static class HarmonyBootstrap
{
    static HarmonyBootstrap()
    {
        new Harmony("Madeline.ModMismatchWindowPatch").PatchAll(typeof(HarmonyBootstrap).Assembly);
        ModLog.Debug("Patched");
    }
}
