using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The class for slate ray interaction receiver. <br>
/// 面板接收端射线交互的类。/*create by 梁鹏 2021-9-26 */
/// </summary>

namespace SpaceDesign
{
    public class MySlateRayReceiver : RayPointerHandler
    {
        private MySlateController slateController;
        private bool isActive = true;

        void Start()
        {
            if (gameObject.GetComponent<MySlateController>() != null)
                slateController = gameObject.GetComponent<MySlateController>();
            else
                isActive = false;
        }
        
        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="startPoint">Start point of ray in far interaction. <br>远端射线起点位置.</param>
        /// <param name="direction">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public override void OnPinchDown(Vector3 startPoint, Vector3 direction, Vector3 targetPoint)
        {
            if (!isActive)
                return;
            slateController.UpdatePinchPointerStart(targetPoint);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            if (!isActive)
                return;
            
            slateController.UpdatePinchPointerEnd();
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="startPosition">The start position of laser. <br>射线起点.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public override void OnDragging(Vector3 startPosition, Vector3 direction)
        {
            if (!isActive)
                return;
            Vector3 slateNormal = -transform.forward;
            Vector3 slateFirstPoint = transform.position;
            float res = (Vector3.Dot(slateNormal, slateFirstPoint) - Vector3.Dot(slateNormal, startPosition)) / Vector3.Dot(slateNormal, direction);
            //当射线方向朝向与面板或其延伸平面有焦点时
            if (res > 0)
            {
                slateController.UpdatePinchPointer(startPosition + res * direction);
            }
        }
    }
}

