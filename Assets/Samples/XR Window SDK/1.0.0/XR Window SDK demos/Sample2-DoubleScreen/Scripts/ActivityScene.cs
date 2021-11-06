/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Samples
{
    public class ActivityScene : MonoBehaviour
    {
        private void Reset()
        {
            int layer = LayerMask.NameToLayer("ActivityPlayer");
            if (layer < 0)
            {
                Debug.LogError("不存在layer: ActivityPlayer，请添加此layer！！！");
                return;
            }
            SetObjectLayer(transform,layer);
            if(gameObject.GetComponentInChildren<Camera>() == null)
            {
                Debug.LogError("请添加Camera，并设置为该对象的子对象！");
                return;
            }
            gameObject.GetComponentInChildren<Camera>().cullingMask = 1 << layer;
            if (XRCameraManager.Instance)
                XRCameraManager.Instance.stereoCamera.GetComponent<Camera>().cullingMask &= ~(1 << layer);
            Debug.Log("ActivityScene渲染对象设置成功");
        }

        private void SetObjectLayer(Transform parent, int layer)
        {
            parent.gameObject.layer = layer;
            foreach (Transform item in parent)
            {
                SetObjectLayer(item, layer);
            }
        }
    }
}