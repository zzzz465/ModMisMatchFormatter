// Verbatim copy of the pre-refactor alignment code (commit bbb3964), kept as a test oracle.
#nullable disable
#pragma warning disable
using System;
using System.Collections.Generic;

namespace Legacy
{
    public class ModPair
    {
        public Mod Save { get; protected set; }
        public Mod Loaded { get; protected set; }

        public ModPair(Mod save, Mod loaded)
        {
            this.Save = save;
            this.Loaded = loaded;
        }
    }
}