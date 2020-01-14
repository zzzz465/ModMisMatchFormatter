using UnityEngine;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public interface iModListerElementRenderer 
    {
        GUIStyle ModStateStyle { get; set; }
        GUIStyle ModDescriptionStyle { get; set; }
        GUIStyle ModVersionStyle { get; set; }

        void RenderActiveMod(Rect right, ModPair pair);
        void RenderSaveMod(Rect left, ModPair pair);
    }
}