using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public interface iOrderFormatter
    {
        IEnumerable<ModPair> GetFormattedModPairs(List<Mod> fromSave, List<Mod> fromLoaded);
    }
}