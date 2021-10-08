using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking 
{
    /// <summary>
    /// The class for toggle receiver in UI interaction. <br>
    /// 切换按键接收端UI交互的类。
    /// </summary>
    [RequireComponent(typeof(ToggleController))]
    public class ToggleTouchableReceiver : ButtonBaseUIReceiver
    {
        /// <summary>
        /// Event is called when the button is toggled. <br>
        /// 当按键被切换状态时触发的事件。
        /// </summary>
        [Serializable] public class BoolEvent : UnityEvent<bool> { }
        [SerializeField] BoolEvent onToggle;

        bool m_ToggleStatus = false;
        ToggleController m_ToggleStatusController;

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
                        m_ToggleStatusController.SwitchToggle();
                        //m_ToggleStatus = !m_ToggleStatus;
                        //onToggle?.Invoke(m_ToggleStatus);
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

        protected override void Start()
        {
            base.Start();
            m_ToggleStatusController = gameObject.GetComponent<ToggleController>();
        }

        void Update()
        {
            if (m_ToggleStatus != m_ToggleStatusController.toggleStatus)
            {
                m_ToggleStatus = m_ToggleStatusController.toggleStatus;
                onToggle?.Invoke(m_ToggleStatus);
            }
        }
    } 
}
