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

    public enum BoundsAvailableAction
    {
        Translation = 0,
        Rotation = 1,
        Rescaling = 2,
        TranslationAndRotation = 3,
        TranslationAndRescaling = 4,
        RotationAndRescaling = 5,
        All = 6
    }
}
