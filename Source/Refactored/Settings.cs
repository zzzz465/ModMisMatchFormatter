using System;
using System.Collections.Generic;
using Verse;

namespace Madeline.ModMismatchFormatter
{
    public class Settings : ModSettings
    {
        public bool useVersionCompare;
        public bool saveVersionToSaveFile;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useVersionCompare, "useVersionCompare", true, true);
            Scribe_Values.Look(ref saveVersionToSaveFile, "saveVersionToSaveFile", true, true);
        }
    }
}