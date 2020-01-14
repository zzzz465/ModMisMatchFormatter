using System;
using System.Collections.Generic;

namespace Madeline.ModMismatchFormatter
{
    public interface iOrderFormatter
    {
        void Arrange(ref List<Mod> fromsave, ref List<Mod> loaded);
    }
}