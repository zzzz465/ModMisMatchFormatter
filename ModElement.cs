using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModComparerPatch
{
    public class ModElement
    {
        public bool isPlaceHolder { get; }
        public string ModName { get; }
        public int LoadOrder { get; private set; }
        public bool isActivated { get; private set; }
        public Color ElementBoxColor
        {
            get
            {
                if (isPlaceHolder) // 비활성화 되었을 경우
                    return Color.grey;
                if (isActivated)
                    return Color.green;
                else
                    return Color.red;
            }
        }

        internal ModElement(string ModName, int LoadOrder, bool isActivated, bool isPlaceHolder = true)
        {
            this.isPlaceHolder = isPlaceHolder;
            this.ModName = ModName;
            this.LoadOrder = LoadOrder;
            this.isActivated = isActivated;
        }
    }
}
