using System.Collections.Generic;
using System.Linq;

namespace ModMisMatchWindowPatch
{
    public static class ListExtension
    {
        public static List<ModElement> AfterIndex(this List<ModElement> modlist, int index)
        {
            return modlist.Where(item => modlist.IndexOf(item) > index).ToList();
        }
    }
}