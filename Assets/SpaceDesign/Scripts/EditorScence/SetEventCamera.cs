using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
/// <summary>
/// 设置事件相机，/*create by 梁鹏 2021-9-13 */
/// </summary>
public class SetEventCamera : MonoBehaviour
{
    Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
            return;
        if (canvas.worldCamera == null)
            canvas.worldCamera = XRCameraManager.Instance.eventCamera;
    }
}
