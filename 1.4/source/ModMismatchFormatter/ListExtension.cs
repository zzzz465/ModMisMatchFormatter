using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMismatchFormatter
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

        public static T PopOrNull<T>(this List<T> list) where T : class
        {
            if (list.Count == 0)
                return null;
            else
                return list.Pop();
        }

        public static string ToString2<T>(this List<T> list)
        {
            return string.Join("\n", list.Select(x => x.ToString()));
        }
    }
}
