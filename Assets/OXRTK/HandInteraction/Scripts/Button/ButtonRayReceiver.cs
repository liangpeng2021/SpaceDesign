using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for button receiver in ray interaction. <br>
    /// 按键接收端远端交互的类。
    /// </summary>
    public class ButtonRayReceiver : RayPointerHandler
    {
        /// <summary>
        /// Called when the pointer enter the button. <br>
        /// 当射线打中按键时触发。
        /// </summary>
        public UnityEvent onPointerEnter;

        /// <summary>
        /// Called when the pointer exit the button. <br>
        /// 当射线离开按键时触发。
        /// </summary>
        public UnityEvent onPointerExit;

        /// <summary>
        /// Called when the button is pinched down. <br>
        /// 当按键开始被点击时触发。
        /// </summary>
        public UnityEvent onPinchDown;

        /// <summary>
        /// Called when the button is pinched up. <br>
        /// 当按键结束被点击时触发。
        /// </summary>
        public UnityEvent onPinchUp;

        /// <summary>
        /// Called when the laser points to the object. <br>
        /// 当射线打中物体时调用。
        /// </summary>
        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            onPointerEnter?.Invoke();
        }

        /// <summary>
        /// Called when the laser exit the object. <br>
        /// 当射线离开物体时调用。
        /// </summary>
        public override void OnPointerExit()
        {
            base.OnPointerExit();
            onPointerExit?.Invoke();
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="start">Start point of ray in far interaction. <br>远端射线起点位置.</param>
        /// <param name="dir">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public override void OnPinchDown(Vector3 start, Vector3 dir, Vector3 targetPoint)
        {
            base.OnPinchDown(start, dir, targetPoint);

            onPinchDown?.Invoke();
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="shoulderPosition">Start point of ray in far interaction. <br>远端射线肩膀位置.</param>
        /// <param name="handPosition">Start point of ray in far interaction. <br>远端射线手关键点位置.</param>
        /// <param name="dir">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public override void OnPinchDown(Vector3 shoulderPosition, Vector3 handPosition, Vector3 dir, Vector3 targetPoint)
        {
            base.OnPinchDown(shoulderPosition, handPosition, dir, targetPoint);

            onPinchDown?.Invoke();
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            base.OnPinchUp();

            onPinchUp?.Invoke();
        }
    }
}