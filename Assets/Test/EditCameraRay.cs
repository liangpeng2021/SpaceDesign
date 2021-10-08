using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 编辑器模式下测试使用，/*create by 梁鹏 2021-9-24 */
/// </summary>
public class EditCameraRay : MonoBehaviour
{
    public Camera editCamera;
    public LineRenderer line;
    
    RayPointerHandler _currayPointerHandler;
    RayPointerHandler _lastrayPointerHandler;
    RayPointerHandler hitrayPointerHandler;       //用来临时存储碰触到的对象

    bool isMouseDown;
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (line!=null)
            line.positionCount = 2;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            if (line != null)
            {
                line.SetPosition(0, ray.origin);
                line.SetPosition(1, hit.point);
            }
            //Debug.Log("RayIn");
            hitrayPointerHandler = hit.collider.GetComponent<RayPointerHandler>();
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
                    //Debug.Log("GetMouseButtonDown");
                    isMouseDown = true;
                    _currayPointerHandler.OnPinchDown(ray.origin, ray.direction, hit.point);
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

            if (isMouseDown)
            {
                if (_currayPointerHandler)
                {
                    _currayPointerHandler.OnDragging(ray.origin, ray.direction);
                }
            }
        }
        else
        {
            //Debug.Log("RayOut");
            _currayPointerHandler = null;
            if (_lastrayPointerHandler != null)
            {
                _lastrayPointerHandler.OnPointerExit();
                _lastrayPointerHandler = null;
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("GetMouseButtonUp");
            isMouseDown = false;
            if (_lastrayPointerHandler != null)
            {
                _lastrayPointerHandler.OnPinchUp();
            }
        }
#else
        gameObject.SetActive(false);
#endif
    }
}
