using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARRemoteDebug
{
    public class Initiator
    {
        static Initiator()
        {
            GameObject o = new GameObject("ARRemoteDebug.Conduit");
            o.AddComponent<Conduit>(); 
        }
    }
}

