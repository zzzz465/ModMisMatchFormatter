using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public class Mod
    {
        private Mod saveCurrent;

        public bool isPlaceHolder
        {
            get { return (Identifier == null); }
        }
        public int? Order { get; set; }
        public string Identifier { get; set; }
        public string ModName { get; set; }
        public string Version { get; set; }
        public Mod(string Identifier = null, string ModName = null, int? order = null) // Identifier 넣어주기
        {
            this.Order = order;
            this.Identifier = Identifier;
            this.ModName = ModName;
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