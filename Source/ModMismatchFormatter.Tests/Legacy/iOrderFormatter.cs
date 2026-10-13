// Verbatim copy of the pre-refactor alignment code (commit bbb3964), kept as a test oracle.
#nullable disable
#pragma warning disable
using System;
using System.Collections.Generic;

namespace Legacy
{
    public interface iOrderFormatter
    {
        IEnumerable<ModPair> GetFormattedModPairs(List<Mod> fromSave, List<Mod> fromLoaded);
    }
}