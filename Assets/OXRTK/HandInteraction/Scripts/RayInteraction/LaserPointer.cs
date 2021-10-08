using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for GTouch interaction with ray interaction objects. <br>
    /// 控制GTouch射线远端交互的类。
    /// </summary>
    public class LaserPointer : MonoBehaviour
    {
        bool m_PhysicalHitResult;
        RaycastHit m_HitInfo;
        RayPointerHandler m_CurrentTarget;
        RayPointerHandler m_TargetInInteraction;
        Vector3 m_StartPosition, m_Direction;
        bool m_IsPressed = false;
        bool m_IsActive = false;
        bool m_IsHandMenuOpened = false;
        int m_RaycastLayer = ~Physics.IgnoreRaycastLayer;

        public static LaserPointer instance;

        void Awake()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                this.enabled = false;
                return;
            }

            if (instance != null)
            {
                if (HandTrackingPlugin.debugLevel > 0) Debug.Log("LaserPointer already exist!");
                Destroy(gameObject);
                return;
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            m_IsActive = true;

            if (PointerManager.instance != null)
            {
                PointerManager.instance.onHandMenuChanged += UpdateHandMenuStatus;
            }
        }

        void Update()
        {
            if (!m_IsActive)
                return;
            
            UpdateTouchStatus();
        }

        private void OnDisable()
        {
            Reset();
        }

        //更新射线事件
        void UpdatePointerEvent()
        {
            if (m_IsPressed)
                return;

            RayPointerHandler newTarget = null;
            if (m_PhysicalHitResult && m_HitInfo.transform != null && m_HitInfo.transform.GetComponent<RayPointerHandler>() != null)
            {
                newTarget = m_HitInfo.transform.GetComponent<RayPointerHandler>();
            }

            if (newTarget == null)
            {
                if (m_CurrentTarget != null)
                {
                    m_CurrentTarget.OnPointerExit();
                    if (HandTrackingPlugin.debugLevel > 0)  Debug.Log("PreviousTarget: " + m_CurrentTarget.name + " ---> OnGTouchPointerExit");
                }
            }
            else
            {
                if (m_CurrentTarget != null)
                {
                    if (newTarget != m_CurrentTarget)
                    {
                        m_CurrentTarget.OnPointerExit();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("PreviousTarget: " + m_CurrentTarget.name + " ---> OnGTouchPointerExit");

                        newTarget.OnPointerEnter();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + newTarget.gameObject.name + " ---> OnGTouchPointerEnter");
                        
                    }
                }
                else
                {
                    newTarget.OnPointerEnter();
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + newTarget.gameObject.name + " ---> OnGTouchPointerEnter");

                }
            }
            m_CurrentTarget = newTarget;
        }

        void UpdateTouchStatus()
        {
            if (XRInput.Instance.GetMouseButtonUp(0))
            {
                if (m_TargetInInteraction != null)
                {
                    m_TargetInInteraction.OnPinchUp();
                    m_TargetInInteraction = null;
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + m_CurrentTarget.gameObject.name + " ---> OnGTouchPressUp");
                }
                m_IsPressed = false;
            } 
            else if (XRInput.Instance.GetMouseButtonDown(0))
            {
                if (m_CurrentTarget != null)
                {
                    if (m_CurrentTarget.isInInteraction)
                    {
                        return;
                    }

                    m_TargetInInteraction = m_CurrentTarget;
                    m_TargetInInteraction.OnPinchDown(m_StartPosition, m_Direction, m_HitInfo.point);
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + m_CurrentTarget.gameObject.name + " ---> OnGTouchPressDown");
                }
                m_IsPressed = true;
            } else if (XRInput.Instance.GetMouseButton(0))
            {
                if (m_TargetInInteraction != null)
                    m_TargetInInteraction.OnDragging(m_StartPosition, m_Direction);
            }
        }

        void ResetTarget()
        {
            if (!m_IsActive)
                return;

            if (m_IsPressed)
            {
                if (m_TargetInInteraction != null)
                {
                    m_TargetInInteraction.OnPinchUp();
                    m_TargetInInteraction = null;
                }
            }
            
            if (m_CurrentTarget != null)
            {
                m_CurrentTarget.OnPointerExit();
                m_CurrentTarget = null;
            }

            m_PhysicalHitResult = false;
            m_HitInfo = new RaycastHit();
        }

        /// <summary>
        /// Force reset GTouch laser interaction. <br>
        /// 用来强制重置Gtouch射线交互.
        /// </summary>
        public void Reset()
        {
            ResetTarget();
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Reset GTouch laser pointer");
        }

        /// <summary>
        /// For GTouch laser updates in XRWindow onRay to use only. <br>
        /// 仅供 XRWindow onRay部分的GTouch射线更新使用.
        /// </summary>
        public void UpdateLaser(XRRay ray, XRRayCastInfo info)
        {
            if (!m_IsActive)
                return;

            m_StartPosition = ray.pose.position.ToVector3_FlipZ();
            m_Direction = ray.direction.ToVector3_FlipZ();
            m_PhysicalHitResult = Physics.Raycast(m_StartPosition, m_Direction, out m_HitInfo, float.PositiveInfinity, m_RaycastLayer);

            UpdatePointerEvent();
        }

        void UpdateHandMenuStatus(bool status)
        {
            if (m_IsHandMenuOpened != status)
            {
                m_IsHandMenuOpened = status;
                if (m_IsHandMenuOpened)
                {
                    ResetTarget();
                    m_RaycastLayer = 1 << 15;
                }
                else
                {
                    ResetTarget();
                    m_RaycastLayer = ~Physics.IgnoreRaycastLayer;
                }
            }
        }
    }
}
