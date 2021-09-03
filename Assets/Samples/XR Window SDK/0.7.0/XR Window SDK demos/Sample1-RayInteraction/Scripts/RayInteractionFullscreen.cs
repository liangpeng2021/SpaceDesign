/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Samples
{
    public class RayInteractionFullscreen : XRWindowFullscreenBase
    {
        public XRRayCastInfo RayInfo { get; private set; }

        public override XRRayCastInfo OnRay(XRRay ray, XRFocusState focusState)
        {
            RayInfo = base.OnRay(ray, focusState);
            return RayInfo;
        }
    }
}