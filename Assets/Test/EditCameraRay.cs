using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
/// <summary>
/// 编辑器模式下测试使用，/*create by 梁鹏 2021-9-24 */
/// </summary>
public class EditCameraRay : MonoBehaviour
{
    Camera editCamera;
    public LineRenderer line;

    RayPointerHandler _currayPointerHandler;
    RayPointerHandler _lastrayPointerHandler;

    bool isMouseDown;

    RayPointerHandler hitpointhandler;

    public bool canDrag = false;
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR

        if (editCamera == null)
            editCamera = XRCameraManager.Instance.eventCamera;
        Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (line != null)
            line.positionCount = 2;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            if (line != null)
            {
                line.SetPosition(0, ray.origin);
                line.SetPosition(1, hit.point);
            }

            RayPointerHandler hitrayPointerHandler = hit.collider.GetComponent<RayPointerHandler>();
            if (hitrayPointerHandler)
            {
                _currayPointerHandler = hitrayPointerHandler;
                if (_lastrayPointerHandler != _currayPointerHandler)//若这次和上次碰触到的不是一个对象
                {
                    if (_lastrayPointerHandler != null)//若上次的目标不为空,且不是父节点，那么上次目标响应移开事件
                        _lastrayPointerHandler.OnPointerExit();
                    //次目标响应指向事件，这里可以根据自己需求写点击移开指向事件
                    _currayPointerHandler.OnPointerEnter();

                    _lastrayPointerHandler = _currayPointerHandler;//这次目标付给_lastButton
                }
                if (Input.GetMouseButtonDown(0))
                {
                    isMouseDown = true;
                    hitpointhandler = hit.collider.GetComponent<RayPointerHandler>();
                    _currayPointerHandler.OnPinchDown(ray.origin, ray.direction, hit.point);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _currayPointerHandler.OnPinchUp();
                }
            }
            else
            {
                _currayPointerHandler = null;
                if (_lastrayPointerHandler != null)
                {
                    _lastrayPointerHandler.OnPointerExit();
                    _lastrayPointerHandler = null;
                }
            }

            if (canDrag && isMouseDown)
            {
                if (!hitpointhandler.gameObject.activeInHierarchy)
                {
                    isMouseDown = false;
                    hitpointhandler = null;
                }
                if (hitpointhandler)
                {
                    hitpointhandler.OnDragging(ray.origin, ray.direction);
                }
            }
        }
        else
        {
            _currayPointerHandler = null;
            if (_lastrayPointerHandler != null)
            {
                _lastrayPointerHandler.OnPointerExit();
                _lastrayPointerHandler = null;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;

            hitpointhandler = null;
        }
#else

        gameObject.SetActive(false);
#endif
    }
}
