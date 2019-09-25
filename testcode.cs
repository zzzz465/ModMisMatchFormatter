using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ModMisMatchWindowPatch
{
    [StaticConstructorOnStartup]
    public class testcode : MonoBehaviour
    {
        static testcode()
        {
            new GameObject().AddComponent(typeof(testcode));
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Find.WindowStack.IsOpen(typeof(ModMisMatchWindow)))
                    Find.WindowStack.TryRemove(typeof(ModMisMatchWindow));
                //else
                    //Find.WindowStack.Add(new ModMisMatchWindow());
            }

            if (Input.GetKeyDown(KeyCode.R))
                Log.Message("R pressed");

        }
    }
}
