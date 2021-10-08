using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for toggle receiver in ray interaction. <br>
    /// 切换按键接收端远端交互的类。
    /// </summary>
    [RequireComponent(typeof(ToggleController))]
    public class ToggleRayReceiver : ButtonRayReceiver
    {
        /// <summary>
        /// Event is called when the button is toggled. <br>
        /// 当按键被切换状态时触发的事件。
        /// </summary>
        [Serializable] public class BoolEvent : UnityEvent<bool> { }
        [SerializeField] BoolEvent onToggle;

        bool m_ToggleStatus;
        ToggleController m_ToggleStatusController;

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="startPoint">Start point of ray in far interaction. <br>远端射线起点位置.</param>
        /// <param name="direction">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public override void OnPinchDown(Vector3 start, Vector3 dir, Vector3 targetPoint)
        {
            base.OnPinchDown(start, dir, targetPoint);
            onPinchDown?.Invoke();
            m_ToggleStatusController.SwitchToggle();
        }

        void Start()
        {
            m_ToggleStatusController = gameObject.GetComponent<ToggleController>();
        }

        void Update()
        {
            if(m_ToggleStatus != m_ToggleStatusController.toggleStatus)
            {
                m_ToggleStatus = m_ToggleStatusController.toggleStatus;
                onToggle?.Invoke(m_ToggleStatus);
            }
        }
    }
}
