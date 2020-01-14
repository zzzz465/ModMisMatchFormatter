using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public class Mod
    {
        public bool isPlaceHolder { get; private set; }
        public int? Order { get; set; }
        public Mod Pair { get; set; }
        public Mod(int? order = null, Mod Pair = null)
        {
            this.Order = order;
            this.Pair = Pair;
        }
    }
}