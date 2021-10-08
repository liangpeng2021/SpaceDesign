using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for pressable button ui interaction receiver. <br>
    /// 可按压按键接收端近场交互的类。<br>
    /// Pressable direction is position direction on Z axis.<br>
    /// 按压方向需要为z轴正向。
    /// </summary>
    public class ButtonTouchableReceiver : ButtonBaseUIReceiver
    {
        /// <summary>
        /// Called when the finger click the button. <br>
        /// 当手指点击按键时触发。
        /// </summary>
        public UnityEvent onClick;

        /// <summary>
        /// Continuously called when the interaction finger is in the checking area of object. <br>
        /// 当用户交互手指在物体检测范围时连续调用。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distance">Distance between interaction finger tip and object on pressable direction <br>当前物体在按压方向上与手的距离.</param>
        public override void OnTouchUpdate(Vector3 tipPoint, float distance)
        {
            //base.OnTouchUpdate(tipPoint, distance);
            if (!m_UpdatePress)
                return;

            if (pressableHandler != null)
            {
                if (distance < m_DistanceThreshold && distance >= 0 && !m_IsLockedDir)
                {
                    if (m_PrevDis <= 0)
                        return;

                    OnTouchEnter();
                    Vector3 newLocalosition =
                        m_HandlerStartPosition + GetLocalScale(pressableHandler, new Vector3(0, 0, (m_DistanceThreshold - distance)));

                    pressableHandler.localPosition = newLocalosition;
                }
                // 按下到负向
                else if (distance < 0)
                {
                    if (m_PrevDis <= 0)
                        return;

                    OnTouchEnter();
                    if (!m_IsClicked)
                    {
                        m_IsClicked = true;
                        onClick?.Invoke();
                    }
                    Vector3 newLocalosition =
                        m_HandlerStartPosition + GetLocalScale(pressableHandler, new Vector3(0, 0, m_DistanceThreshold));
                    pressableHandler.localPosition = newLocalosition;
                    if (!m_IsLockedDir)
                    {
                        m_IsLockedDir = true;
                    }
                }

                if (distance >= m_DistanceThreshold)
                {
                    OnTouchExit();
                }
            }
            if (m_IsLockedDir)
                m_PreviousDistance = Mathf.Min(m_PreviousDistance, distance);
            else
                m_PreviousDistance = distance;
        }
    }
}
