using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for ray interaction receiver. <br>
    /// 远端射线交互接收端的类。
    /// </summary>
    public class RayPointerHandler : MonoBehaviour
    {
        protected bool m_IsInInteraction = false;

        /// <summary>
        /// Gets object interaction status. <br>
        /// 获取物体交互状态。
        /// </summary>
        public bool isInInteraction
        {
            get { return m_IsInInteraction; }
        }

        protected bool m_IsInFocus = false;

        /// <summary>
        /// Gets object focus status. <br>
        /// 获取物体聚焦状态。
        /// </summary>
        public bool isInFocus
        {
            get { return m_IsInFocus; }
        }

        protected bool m_IsLockCursor = false;
        /// <summary>
        /// Gets the cursor lock setting for this ray receiver during interaction. <br>
        /// 获取当前射线交互接收端交互时光标是否位置锁定。
        /// </summary>
        public bool isLockCursor
        {
            get { return m_IsLockCursor; }
        }

        /// <summary>
        /// Called when the laser points to the object. <br>
        /// 当射线打中物体时调用。
        /// </summary>
        public virtual void OnPointerEnter() {
            m_IsInFocus = true;
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnPointerEnter: " + gameObject.name);
        }

        /// <summary>
        /// Called when the laser exit the object. <br>
        /// 当射线离开物体时调用。
        /// </summary>
        public virtual void OnPointerExit() {
            m_IsInFocus = false;
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnPointerExit: " + gameObject.name);
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="startPoint">Start point of ray in far interaction. <br>远端射线起点位置.</param>
        /// <param name="direction">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public virtual void OnPinchDown(Vector3 startPoint, Vector3 direction, Vector3 targetPoint)
        {
            m_IsInInteraction = true;
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnPinchDown: " + gameObject.name);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public virtual void OnPinchUp() {
            m_IsInInteraction = false;
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnPinchUp: " + gameObject.name);
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="startPosition">The start position of laser. <br>射线起点.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public virtual void OnDragging(Vector3 startPosition, Vector3 direction)
        {
        }

        /// <summary>
        /// Called when the object is disabled. <br>
        /// 当物体被disable时调用。
        /// </summary>
        public virtual void OnDisable()
        {
            if (m_IsInInteraction)
            {
                OnPinchUp();
            }

            if (m_IsInFocus)
            {
                OnPointerExit();
            }
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Disabled  " + gameObject.name + ", Reset.");
        }
    }
}
