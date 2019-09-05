using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ModComparerPatch.Interfaces
{
    internal interface IElementEnumerable : IEnumerable
    {
        ModElement Before { get; }
        ModElement After { get; }
    }
}
