using System;
using System.Collections.Generic;
using Verse;

namespace Madeline.ModMismatchFormatter
{
    public class Settings : ModSettings
    {
        public bool useVersionCompare;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useVersionCompare, "useVersionCompare", true, false);
        }
    }
}