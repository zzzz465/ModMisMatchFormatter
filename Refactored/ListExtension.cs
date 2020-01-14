using System;
using System.Collections.Generic;
using System.Linq;

namespace Madeline.ModMismatchFormatter
{
    public static class ListExtension
    {
        public static List<T> AfterIndex<T>(this List<T> modlist, int index)
        {
            return modlist.Where(item => modlist.IndexOf(item) > index).ToList();
        }

        public static T Pop<T>(this List<T> list)
        {
            var elem = list.Last();
            list.RemoveAt(list.Count - 1);
            return elem;
        }
    }
}