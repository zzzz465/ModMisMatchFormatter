using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public class ModPair
    {
        public Mod Save { get; set; }
        public Mod Loaded { get; set; }

        public ModPair(Mod save, Mod loaded)
        {
            this.Save = save;
            this.Loaded = loaded;
        }
    }
}