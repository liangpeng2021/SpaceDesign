using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

namespace OXRTK.ARHandTracking
{    
    /// <summary>
    /// The class extending XRWindowBase is to enable full screen.<br>
    /// 该类继承XRWindowBase，用于启用全屏模式。
    /// </summary>
    public class CustomXRWindow : XRWindowBase
    {
        /// <summary>
        /// Enables full screen.<br>
        /// 启用全屏模式。
        /// </summary>
        public override void Create()
        {
            base.Create();
            WindowFullscreen = true;
        }
        public override bool HasCursor()
        {
            return false;
        }
        /// <summary>
        /// Enables GTouch interaction.<br>
        /// 启用GTouch交互。
        /// </summary>
        public override XRRayCastInfo OnRay(XRRay ray, XRFocusState focusState)
        {
            XRRayCastInfo info = base.OnRay(ray, focusState);
            //LaserPointer.instance?.UpdateLaser(ray, info);
            return info;
        }
    }
}

