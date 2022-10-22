using Verse;

namespace ModMismatchFormatter
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            Log.Message("Hello World!");

            var enabledMods = ModUtils.GetEnabledMods();

            foreach (var mod in enabledMods)
            {
                Log.Message($"enabled mod: {mod.Name}");
            }
        }
    }
}
