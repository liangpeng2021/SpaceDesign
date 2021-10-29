using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XR;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for hand menu relocation.<br>
    /// 控制随手菜单位置重置的类。
    /// </summary>
    public class HandMenuLocator : MonoBehaviour
    {

        /// <summary>
        /// Sets menu face to the user.<br>
        /// 设置菜单朝向用户。
        /// </summary>
        public bool isFaceUser = true;

        /// <summary>
        /// Action to call when menu start displayed or not displayed.<br>
        /// 当菜单开始显示或结束显示时调用。
        /// </summary>
        public Action<bool> onMenuStatusChange;

        List<HandMenuController> m_HandMenuControllers = new List<HandMenuController>();
        bool m_IsPinned;
        private Transform m_MainCameraTransform;
        private Vector3 m_TranslateTarget;
        private Quaternion m_LookRotation;
        private IEnumerator m_TransformAction;
        private bool m_IsRecentering;

        private Vector3 m_LastPos;
        private Vector3 m_LastVDir;
        
        [HideInInspector] public float hideCounter;
        [HideInInspector] public float pinTimerThreshold = 15f;

        UiInteractionPointerHandler[] m_UiHandlers;
        RayPointerHandler[] m_RayHandlers;
        bool m_IsInInteraction = false;

        private void OnEnable()
        {
            onMenuStatusChange?.Invoke(true);
        }

        private void OnDisable()
        {
            Reset();
            onMenuStatusChange?.Invoke(false);
        }

        /// <summary>
        /// Get the number of HandMenuController that control this handmenu.<br>
        /// 获取控制该handmenu的HandMenuController的数量。
        /// </summary>
        public int CheckRoot()
        {
            return m_HandMenuControllers.Count;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitReceiverList();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CloseMenu(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                CloseMenu();
            }

            if (m_MainCameraTransform == null)
            {
                if (CenterCamera.instance.centerCamera != null)
                    m_MainCameraTransform = CenterCamera.instance.centerCamera.transform;
                else
                    return;
            }

            if (m_IsPinned)
            {
                return;
            }

            UpdateInInteraction();

            if (m_IsInInteraction)
                return;

            if (hideCounter > pinTimerThreshold)
            {
                hideCounter = 0;
                CloseMenu(false);
            }
            else
            {
                hideCounter += Time.deltaTime;
            }
                
            if (!m_IsRecentering)
             {
                //Vector3 cameraToTarget = (transform.position - m_MainCameraTransform.position).normalized;
                Vector3 cameraToTarget = m_MainCameraTransform.forward;
                Vector3 projForward = 
                     isFaceUser? cameraToTarget : new Vector3(cameraToTarget.x, 0, cameraToTarget.z).normalized;
            
                 m_TranslateTarget = m_MainCameraTransform.position + projForward * 1.5f;
                 m_LookRotation = Quaternion.LookRotation(projForward, Vector3.up);
             } 

            float td = Vector3.Distance(m_TranslateTarget, transform.position);

            if (td > 1.1f && !m_IsRecentering)
            {
                // Recenter();
                float linearSpeed = Vector3.Distance(m_TranslateTarget, m_LastPos) / Time.deltaTime;
                float anglurSpeed = Vector3.Angle(m_MainCameraTransform.forward, m_LastVDir);
            
                // If the subject moved slowly; e.g.,
                if (linearSpeed <= 0.7f || anglurSpeed <= 2f)
                {
                    Recenter();
                }

                m_LastPos = m_TranslateTarget;
                m_LastVDir = m_MainCameraTransform.forward;
            }
        }

        /// <summary>
        /// Initialize hand menu.<br>
        /// 初始化随手菜单控制器。
        /// </summary>
        public void Init(HandMenuController controller)
        {
            if (m_HandMenuControllers != null && !m_HandMenuControllers.Contains(controller))
            {
                m_HandMenuControllers.Add(controller);
            }
        }

        void InitReceiverList()
        {
            m_RayHandlers = gameObject.GetComponentsInChildren<RayPointerHandler>();
            m_UiHandlers = gameObject.GetComponentsInChildren<UiInteractionPointerHandler>();
        }

        void UpdateInInteraction()
        {
            bool tmpInteractionStatus = false;
            if (m_RayHandlers != null)
            {
                for (int i = 0; i < m_RayHandlers.Length; i++)
                {
                    if (m_RayHandlers[i].isInFocus)
                    {
                        tmpInteractionStatus = true;
                        break;
                    }
                }
            }

            if (!tmpInteractionStatus && m_UiHandlers != null)
            {
                for (int i = 0; i < m_UiHandlers.Length; i++)
                {
                    if (m_UiHandlers[i].isInFocus)
                    {
                        tmpInteractionStatus = true;
                        break;
                    }
                }
            }

            if (tmpInteractionStatus != m_IsInInteraction)
                SetInInteraction(tmpInteractionStatus);
        }

        /// <summary>
        /// Set InInteraction status.<br>
        /// 设置是否在交互中。
        /// </summary>
        public void SetInInteraction(bool status)
        {
            m_IsInInteraction = status;
            if (!status)
                hideCounter = 0;
        }

        /// <summary>
        /// Update toggle status.<br>
        /// 更新Toggle状态。
        /// </summary>
        public void UpdateToggleStatus(bool status)
        {
            m_IsPinned = status;

            if (!m_IsPinned)
            {
                hideCounter = 0;
            }
        }

        /// <summary>
        /// Open the menu.<br>
        /// 打开随手菜单。
        /// </summary>
        public void ShowMenu()
        {
            hideCounter = 0;
            Recenter();
        }

        /// <summary>
        /// Close the menu manually.<br>
        /// 手动关闭随手菜单。
        /// </summary>
        public void CloseMenu(bool closeManually = false)
        {
            if (closeManually)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
            HudManager.instance.HideOverlayMask();
        }


        /// <summary>
        /// The hand menu recenter.<br>
        /// 随手菜单居中。
        /// </summary>
        public void Recenter()
        {
            m_IsRecentering = true;
            if (m_MainCameraTransform == null)
            {
                if (CenterCamera.instance.centerCamera != null)
                    m_MainCameraTransform = CenterCamera.instance.centerCamera.transform;
                else
                    return;
            }

            //Vector3 cameraToTarget = (transform.position - m_MainCameraTransform.position).normalized;
            Vector3 cameraToTarget = m_MainCameraTransform.forward;
            Vector3 projForward =
                     isFaceUser ? cameraToTarget : new Vector3(cameraToTarget.x, 0, cameraToTarget.z).normalized;

            m_TranslateTarget = m_MainCameraTransform.position + projForward * 1.5f;
            m_LookRotation = Quaternion.LookRotation(projForward, Vector3.up);

            if (m_TransformAction != null)
            {
                StopCoroutine(m_TransformAction);
            }

            m_TransformAction = RecenterAction();
            StartCoroutine(m_TransformAction);
        }

        /// <summary>
        /// Calls when hand menu is finish dragged.<br>
        /// 当随手菜单被拖拽结束时调用。
        /// </summary>
        public void OnManualMoveEnd()
        {
            if (m_TransformAction != null)
            {
                StopCoroutine(m_TransformAction);
            }
            m_TransformAction = RecenterAction(true);
            StartCoroutine(m_TransformAction);
        }

        /// <summary>
        /// Calls when hand menu is dragged.<br>
        /// 当随手菜单被拖拽时调用。
        /// </summary>
        public void OnManualMoveStart()
        {
            if (m_MainCameraTransform == null)
            {
                if (CenterCamera.instance.centerCamera != null)
                    m_MainCameraTransform = CenterCamera.instance.centerCamera.transform;
                else
                    return;
            }

            Vector3 cameraToTarget = (transform.position - m_MainCameraTransform.position).normalized;
            Vector3 projForward =
                isFaceUser ? cameraToTarget : new Vector3(cameraToTarget.x, 0, cameraToTarget.z).normalized;

            m_LookRotation = Quaternion.LookRotation(projForward, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, m_LookRotation, 0.25f);
        }

        private IEnumerator RecenterAction(bool rotationOnly = false)
        {
            float elapsedTime = 0;
            float waitTime = 1.2f;
            while (elapsedTime < waitTime)
            //while(Quaternion.Angle(transform.rotation, m_LookRotation) > 5f)
            {
                if (!rotationOnly)
                {
                    transform.position = Vector3.Lerp(transform.position, m_TranslateTarget, 0.15f);
                }
                
                transform.rotation = Quaternion.Lerp(transform.rotation, m_LookRotation, 0.15f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!rotationOnly)
            {
                transform.position = m_TranslateTarget;
            }
            
            transform.rotation = m_LookRotation;
            m_IsRecentering = false;
            yield return null;

        }

        void Reset()
        {
            if (m_TransformAction != null)
            {
                StopCoroutine(m_TransformAction);
            }
            m_IsRecentering = false;

            if (hideCounter != 0)
            {
                hideCounter = 0;
            }
        }

        /// <summary> 
        /// Remove the handmenu controller that controls this handmenu from the dictionary.<br>
        /// 从Dictionary中移除控制该随手菜单的controller。
        /// </summary>
        public void DisconnectController(HandMenuController controller)
        {
            if (m_HandMenuControllers.Contains(controller))
            {
                m_HandMenuControllers.Remove(controller);
            }
        }
    }
}