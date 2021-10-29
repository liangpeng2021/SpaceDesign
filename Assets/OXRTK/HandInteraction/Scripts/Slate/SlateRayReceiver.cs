using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for slate ray interaction receiver. <br>
    /// 面板接收端射线交互的类。
    /// </summary>
    public class SlateRayReceiver : RayPointerHandler
    {
        /// <summary>
        /// Called when the laser pinches down on the slate. <br>
        /// 当射线在面板上捏取或松开时触发。
        /// </summary>
        public UnityEvent onPinchDown;
        
        /// <summary>
        /// Called when the laser pinches up on the slate. <br>
        /// 当射线在面板上捏取或松开时触发。
        /// </summary>
        public UnityEvent onPinchUp;

        private SlateController m_SlateController;
        private bool m_IsActive = true;

        void Start()
        {
            if (gameObject.GetComponent<SlateController>() != null)
                m_SlateController = gameObject.GetComponent<SlateController>();
            else
                m_IsActive = false;
        }

        /// <summary>
        /// Called when the laser points to the object. <br>
        /// 当射线打中物体时调用。
        /// </summary>
        public override void OnPointerEnter()
        {
            if (!m_IsActive)
                return;

            base.OnPointerEnter();
        }

        /// <summary>
        /// Called when the laser exit the object. <br>
        /// 当射线离开物体时调用。
        /// </summary>
        public override void OnPointerExit()
        {
            if (!m_IsActive)
                return;

            base.OnPointerExit();
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
            if (!m_IsActive)
                return;

            base.OnPinchDown(startPoint, direction, targetPoint);
            m_SlateController.UpdatePointerUVStartCood(targetPoint);
            onPinchDown?.Invoke();
        }
        
        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="shoulderPoint">Start point of ray in far interaction. <br>远端射线肩膀位置.</param>
        /// <param name="handPoint">Start point of ray in far interaction. <br>远端射线手关键点位置.</param>
        /// <param name="direction">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public override void OnPinchDown(Vector3 shoulderPoint, Vector3 handPoint, Vector3 direction, Vector3 targetPoint)
        {
            if (!m_IsActive)
                return;

            base.OnPinchDown(shoulderPoint, handPoint, direction, targetPoint);
            m_SlateController.UpdatePointerUVStartCood(targetPoint);
            onPinchDown?.Invoke();
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            if (!m_IsActive)
                return;

            base.OnPinchUp();
            onPinchUp?.Invoke();
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="startPosition">The start position of laser. <br>射线起点.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public override void OnDragging(Vector3 startPosition, Vector3 direction)
        {
            if (!m_IsActive)
                return;

            base.OnDragging(startPosition, direction);
            Vector3 slateNormal = -transform.forward;
            Vector3 slateFirstPoint = transform.position;
            float res = (Vector3.Dot(slateNormal, slateFirstPoint) - Vector3.Dot(slateNormal, startPosition)) / Vector3.Dot(slateNormal, direction);
            //当射线方向朝向与面板或其延伸平面有焦点时
            if (res > 0)
            {
                m_SlateController.UpdatePointerUVCoord(startPosition + res * direction, false);
            }
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="shoulderPosition">The shoulder position of ray. <br>射线肩膀位置.</param>
        /// <param name="handPosition">The hand position of ray. <br>射线手关键点位置.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public override void OnDragging(Vector3 shoulderPosition, Vector3 handPosition, Vector3 direction)
        {
            if (!m_IsActive)
                return;

            base.OnDragging(shoulderPosition, handPosition, direction);
            Vector3 slateNormal = -transform.forward;
            Vector3 slateFirstPoint = transform.position;
            float res = (Vector3.Dot(slateNormal, slateFirstPoint) - Vector3.Dot(slateNormal, handPosition)) / Vector3.Dot(slateNormal, direction);
            //当射线方向朝向与面板或其延伸平面有焦点时
            if (res > 0)
            {
                m_SlateController.UpdatePointerUVCoord(handPosition + res * direction, false);
            }
        }
    }
}
