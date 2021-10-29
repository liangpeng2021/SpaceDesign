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
        public static CenterCamera instance = null;
        public CenterCamera()
        {            
            
        }

        /// <summary>
        /// The camera used to mirror OppoXR SDK Stereo Camera.<br>
        /// 用于镜像OppoXR SDK Stereo Camera的相机。
        /// </summary>
        public Camera centerCamera;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            if (XRCameraManager.Instance != null)
            {
                centerCamera = XRCameraManager.Instance.stereoCamera.GetComponent<Camera>();
            }
        }

        void Update()
        {
            
        }

        /// <summary>
        /// Updates the position and rotation of hands' parent node.<br>
        /// 更新手的母节点位置和旋转信息。
        /// </summary>
        public void UpdateCenterCameraTransform()
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

