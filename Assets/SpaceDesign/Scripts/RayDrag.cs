using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 根据手势射线进行拖拽，/*create by 梁鹏 2021-8-12 */
/// </summary>
public class RayDrag : MonoBehaviour
{
    ButtonRayReceiver buttonRayReceiver;
    bool isDrag;
    // Start is called before the first frame update
    void Start()
    {
        buttonRayReceiver.onPinchDown.AddListener(BeginDrag);
        buttonRayReceiver.onPinchUp.AddListener(EndDrag);
    }

    private void Update()
    {
        if (isDrag)
        {
            Draging();
        }
    }

    /// <summary>
    /// 开始拖拽
    /// </summary>
    void BeginDrag()
    {
        isDrag = true;
    }
    /// <summary>
    /// 拖拽中
    /// </summary>
    void Draging()
    {

    }

    void EndDrag()
    {
        isDrag = false;
    }
}
