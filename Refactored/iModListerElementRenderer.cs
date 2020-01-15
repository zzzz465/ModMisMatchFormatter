using UnityEngine;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public interface iModListerElementRenderer 
    {
        void RenderActiveMod(Rect right, ModPair pair);
        void RenderSaveMod(Rect left, ModPair pair);
    }
}