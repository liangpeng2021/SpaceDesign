using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class maintains a transform which mirrors the transform of OppoXR SDK Stereo Camera, so hands are not needed to be put under Stereo Camera.<br>
    /// 该类管理了一个transform使其与OppoXR SDK Stereo Camera一致，所以手不必挂在Stereo Camera之下。
    /// </summary>
    public class CenterCamera : MonoBehaviour
    {
        public static Camera centerCamera;

        void Start()
        {
            if (XRCameraManager.Instance != null)
            {
                centerCamera = XRCameraManager.Instance.stereoCamera.GetComponent<Camera>();
            }
        }

        void Update()
        {
            // Center camera is actually OppoXR SDK's Stereo Camera.
            // The name is to distinguish it from OppoXR's camera.
            // Sync center camera's transform to hands' parent node each frame.
            if (centerCamera != null)
            {
                transform.position = centerCamera.transform.position;
                transform.rotation = centerCamera.transform.rotation;
            }
        }
    }
}

