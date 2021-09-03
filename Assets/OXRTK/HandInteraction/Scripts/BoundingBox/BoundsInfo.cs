using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    public enum BoundsActionAxis
    {
        X,
        Y,
        Z,
        None
    }

    public enum BoundsAction
    {
        Translate,
        Rotate,
        Scale,
        None
    }

    public enum BoundingBoxActivation
    {
        ActivateOnStart = 0,
        ActivateByProximityAndPoint = 1,
    }
}
