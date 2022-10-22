using Verse;

namespace ModMismatchFormatter
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            Log.Message("Hello World!");
        }
    }
}
