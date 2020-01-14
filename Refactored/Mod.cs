using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public class Mod
    {
        private Mod saveCurrent;

        public bool isPlaceHolder { get; private set; }
        public int? Order { get; set; }
        public string Identifier { get; set; } = "PlaceHolder";
        public string Version { get; set; }
        public Mod(int? order = null) // Identifier 넣어주기
        {
            this.Order = order;
        }

        public bool isVersionDifferent(Mod other)
        {
            if(other.Identifier != Identifier)
                throw new Exception($"Trying to compare version between different mod : {this.Identifier} | {other.Identifier}");

            if(other.Version != Version)
                return true;
            else
                return false;
        }
    }
}