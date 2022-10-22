using Verse;

namespace ModMismatchFormatter
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            Log.Message("Hello World!");

            HarmonyPatch.Patch();

            var enabledMods = ModUtils.GetEnabledMods();

            foreach (var mod in enabledMods)
            {
                Log.Message($"enabled mod: {mod.Name}");
            }
        }
    }
}
