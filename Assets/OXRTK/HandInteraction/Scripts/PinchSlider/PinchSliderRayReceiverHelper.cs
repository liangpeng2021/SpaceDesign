using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for pinch slider ray intetraction. <br>
    /// 非UGUI滑条接收射线交互的类。
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class PinchSliderRayReceiverHelper : RayPointerHandler
    {
        PinchSlider m_PinchSliderRoot;
        float m_Distance;

        private void Start()
        {
            m_IsLockCursor = true;
        }

        /// <summary>
        /// Initialize pinch slider ray handler. <br>
        /// 初始化滑条射线交互接收端。
        /// </summary>
        /// <param name="pinchSlider">Target pinch slider. <br>目标滑条.</param>
        public void Init(PinchSlider pinchSlider)
        {
            m_PinchSliderRoot = pinchSlider;
        }

        /// <summary>
        /// Called when the laser points to the object. <br>
        /// 当射线打中物体时调用。
        /// </summary>
        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            m_PinchSliderRoot.onHighlightStart?.Invoke();
        }

        /// <summary>
        /// Called when the laser exit the object. <br>
        /// 当射线离开物体时调用。
        /// </summary>
        public override void OnPointerExit()
        {
            base.OnPointerExit();
            m_PinchSliderRoot.onHighlightEnd?.Invoke();
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
            base.OnPinchDown(startPoint, direction, targetPoint);
            m_PinchSliderRoot.onInteractionStart?.Invoke();
            m_PinchSliderRoot.UpdateHandlerPosition(targetPoint);
            m_Distance = Vector3.Distance(startPoint, targetPoint);
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
            base.OnPinchDown(shoulderPoint, handPoint, direction, targetPoint);
            m_PinchSliderRoot.onInteractionStart?.Invoke();
            m_PinchSliderRoot.UpdateHandlerPosition(targetPoint);
            m_Distance = Vector3.Distance(handPoint, targetPoint);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            m_PinchSliderRoot.onInteractionEnd?.Invoke();
            base.OnPinchUp();
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="startPosition">The start position of laser. <br>射线起点.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public override void OnDragging(Vector3 startPosition, Vector3 direction)
        {
            base.OnDragging(startPosition, direction);
            Vector3 endPosition = startPosition + direction * m_Distance;
            m_PinchSliderRoot.UpdateHandlerPosition(endPosition);
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
            base.OnDragging(shoulderPosition, handPosition, direction);
            Vector3 endPosition = handPosition + direction * m_Distance;
            m_PinchSliderRoot.UpdateHandlerPosition(endPosition);
        }
    }
}
