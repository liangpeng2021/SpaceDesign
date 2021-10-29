using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Hand menu progress step. <br>
    /// 随手菜单触发步骤。
    /// </summary>
    public enum HandMenuStep
    {
        None = 0,
        InPreparing = 1,
        Prepared = 2
        //Prepared = 2
    }

    /// <summary>
    /// Hand menu indicator display status. <br>
    /// 随手菜单指示器显示状态。
    /// </summary>
    public enum ShowingStatus
    {
        Showing = 0,
        Success = 1,
        Fail = 2,
        Off = 3
    }

    /// <summary>
    /// The class for hand menu interaction. <br>
    /// 控制随手菜单交互的类。
    /// </summary>
    public class HandMenuController : MonoBehaviour
    {
        /// <summary>
        /// The hand menu object.<br>
        /// 随手菜单的GameObject。
        /// </summary>
        public GameObject handMenu;

        /// <summary>
        /// The hand menu hand indicator.<br>
        /// 随手菜单指示器。
        /// </summary>
        public HandPrepIndicator prepIndicator;

        /// <summary>
        /// The hand to control menu.<br>
        /// 控制随手菜单的手。
        /// </summary>
        public HandTrackingPlugin.HandType handToFollow;

        HandMenuLocator m_HandMenuLocator;
        HandMenuStep m_CurrentStep;
        HandController m_HandController;

        float m_LockTimerThreshold = 1f;
        float m_CompleteCounter;
        bool m_IsHandDetected;
        bool m_IsLocked = false;
        bool m_IsActive = false;
        float m_HandMenuReadyTimeEnd = 0;

        void OnValidate()
        {
            if (handToFollow == HandTrackingPlugin.HandType.LeftHand)
            {
                HandMenuController[] allHandMenu = FindObjectsOfType<HandMenuController>();
                if (allHandMenu.Length > 2)
                {
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("The number of Hand Menu exceeds the hand number!");
                }
                for (int i = 0; i < allHandMenu.Length; i++)
                {
                    if (allHandMenu[i] == this)
                        continue;
                    if (allHandMenu[i].handToFollow == handToFollow)
                    {
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Hand menu for left hand already existed!");
                        break;
                    }
                }
            }
            else
            {
                HandMenuController[] allHandMenu = FindObjectsOfType<HandMenuController>();
                if (allHandMenu.Length > 2)
                {
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("The number of Hand Menu exceeds the hand number!");
                }
                for (int i = 0; i < allHandMenu.Length; i++)
                {
                    if (allHandMenu[i] == this)
                        continue;
                    if (allHandMenu[i].handToFollow == handToFollow)
                    {
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Hand menu for right hand already existed!");
                        break;
                    }
                }
            }

            if (handMenu != null || m_HandMenuLocator != null)
            {
                if (handMenu != null && m_HandMenuLocator != null)
                {
                    if (handMenu != m_HandMenuLocator.gameObject)
                    {
                        m_HandMenuLocator.DisconnectController(this);
                        if (m_HandMenuLocator.CheckRoot() <= 0)
                        {
                            HandMenuLocator temp = m_HandMenuLocator;
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.delayCall += () =>
                            {
                                DestroyImmediate(temp);
                            };
#endif
                        }
                        if (handMenu.GetComponent<HandMenuLocator>() != null)
                        {
                            m_HandMenuLocator = handMenu.GetComponent<HandMenuLocator>();
                        }
                        else
                        {
                            m_HandMenuLocator = handMenu.AddComponent<HandMenuLocator>();
                        }
                        m_HandMenuLocator.Init(this);
                    }
                }
                else if (handMenu == null && m_HandMenuLocator != null)
                {
                    m_HandMenuLocator.DisconnectController(this);
                    if (m_HandMenuLocator.CheckRoot() <= 0)
                    {
                        HandMenuLocator temp = m_HandMenuLocator;
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            DestroyImmediate(temp);
                        };
#endif
                        m_HandMenuLocator = null;
                    }
                }
                else if (handMenu != null && m_HandMenuLocator == null)
                {
                    if (handMenu.GetComponent<HandMenuLocator>() != null)
                    {
                        m_HandMenuLocator = handMenu.GetComponent<HandMenuLocator>();
                    }
                    else
                    {
                        m_HandMenuLocator = handMenu.AddComponent<HandMenuLocator>();
                    }
                    m_HandMenuLocator.Init(this);
                }
            }
        }

        void Start()
        //IEnumerator Start()
        {
#if !UNITY_EDITOR
            if (handMenu != null || m_HandMenuLocator != null)
            {
                if (handMenu != null && m_HandMenuLocator != null)
                {
                    if (handMenu != m_HandMenuLocator.gameObject)
                    {
                        m_HandMenuLocator.DisconnectController(this);
                        if (m_HandMenuLocator.CheckRoot() <= 0)
                        {
                            HandMenuLocator temp = m_HandMenuLocator;

                            DestroyImmediate(temp);
                        }
                        if (handMenu.GetComponent<HandMenuLocator>() != null)
                        {
                            m_HandMenuLocator = handMenu.GetComponent<HandMenuLocator>();
                        }
                        else
                        {
                            m_HandMenuLocator = handMenu.AddComponent<HandMenuLocator>();
                        }
                        m_HandMenuLocator.Init(this);
                    }
                }
                else if (handMenu == null && m_HandMenuLocator != null)
                {
                    m_HandMenuLocator.DisconnectController(this);
                    if (m_HandMenuLocator.CheckRoot() <= 0)
                    {
                        HandMenuLocator temp = m_HandMenuLocator;

                        DestroyImmediate(temp);
                        m_HandMenuLocator = null;
                    }
                }
                else if (handMenu != null && m_HandMenuLocator == null)
                {
                    if (handMenu.GetComponent<HandMenuLocator>() != null)
                    {
                        m_HandMenuLocator = handMenu.GetComponent<HandMenuLocator>();
                    }
                    else
                    {
                        m_HandMenuLocator = handMenu.AddComponent<HandMenuLocator>();
                    }
                    m_HandMenuLocator.Init(this);
                }
            }
#endif

            if (HandTrackingPlugin.instance != null)
            {
                if (handToFollow == HandTrackingPlugin.HandType.LeftHand && HandTrackingPlugin.instance.leftHandController != null)
                    m_HandController = HandTrackingPlugin.instance.leftHandController;
                if (handToFollow == HandTrackingPlugin.HandType.RightHand && HandTrackingPlugin.instance.rightHandController != null)
                    m_HandController = HandTrackingPlugin.instance.rightHandController;
            }

            if (m_HandController == null)
            {
                Debug.LogError("handController Reference Null!");
                return;
                //yield return 0;
            }
            InitHandMenu();

            if (HandTrackingPlugin.instance != null)
            {
                HandTrackingPlugin.instance.onHandDataUpdated += UpdateHandInfo;
            }

            // yield return new WaitUntil(() => HandDisplayModeSwtich.rightHandControllerLocated && HandDisplayModeSwtich.leftHandControllerLocated);
            if (handMenu != null)
                handMenu.SetActive(false);
        }

        private void OnDestroy()
        {
            if (m_HandMenuLocator != null)
            {
                m_HandMenuLocator.DisconnectController(this);
            }
            if (HandTrackingPlugin.instance != null)
            {
                HandTrackingPlugin.instance.onHandDataUpdated -= UpdateHandInfo;
            }
        }

        void InitHandMenu()
        {
            if (HandTrackingPlugin.instance == null)
            {
                Debug.LogError("NO SDK INIT YET");
                return;
            }
            else
            {
                m_CurrentStep = HandMenuStep.None;
                m_IsActive = true;
            }
        }

        void UpdateHandInfo()
        {
            if (!m_IsActive)
                return;

            bool handDetected;
            if (handToFollow == HandTrackingPlugin.HandType.LeftHand)
            {
                handDetected = HandTrackingPlugin.instance.leftHandInfo.handDetected;
            }
            else
            {
                handDetected = HandTrackingPlugin.instance.rightHandInfo.handDetected;
            }

            UpdateHandVisible(handToFollow, handDetected);

            if (handDetected)
            {
                UpdateHandStep();
            }
        }

        void UpdateHandStep()
        {
            // Update the prep indicator's pose
            UpdateHandPrepPos(m_HandController, prepIndicator);

            if (m_IsLocked)
            {
                if (m_CompleteCounter < m_LockTimerThreshold)
                {
                    m_CompleteCounter += Time.deltaTime;
                    return;
                }
                else
                {
                    m_IsLocked = false;
                    m_CompleteCounter = 0;
                }
            }

            if (CustomizedGestureController.instance == null)
            {
                if (HandTrackingPlugin.instance != null)
                    HandTrackingPlugin.instance.gameObject.AddComponent<CustomizedGestureController>();
                else
                    return;
            }

            bool fistCheck = CustomizedGestureController.instance.GetHandStatus(handToFollow, CustomizedGesture.Fist);
            //bool palmBackwardCheck = CustomizedGestureController.instance.GetHandStatus(handToFollow, CustomizedGesture.PalmBackForBlossom);
            float palmReadyOffsetAngle = CustomizedGestureController.instance.IsBloomPossible(handToFollow);

            if (handToFollow == HandTrackingPlugin.HandType.LeftHand)
            {
                if (HandTrackingPlugin.debugLevel > 1) Debug.Log("Left hand " + HandTrackingPlugin.instance.leftHandInfo.dynamicGesture + ", currentStep = " + m_CurrentStep);
            }

            if (handToFollow == HandTrackingPlugin.HandType.RightHand)
            {
                if (HandTrackingPlugin.debugLevel > 1) Debug.Log("Right hand " + HandTrackingPlugin.instance.rightHandInfo.dynamicGesture + ", currentStep = " + m_CurrentStep);
            }

            if (m_CurrentStep != HandMenuStep.None)
            {
                if ((handToFollow == HandTrackingPlugin.HandType.LeftHand &&
                    HandTrackingPlugin.instance.leftHandInfo.dynamicGesture == HandTrackingPlugin.DynamicGesture.Blossom) ||
                    (handToFollow == HandTrackingPlugin.HandType.RightHand &&
                    HandTrackingPlugin.instance.rightHandInfo.dynamicGesture == HandTrackingPlugin.DynamicGesture.Blossom))
                {
                    if (HandTrackingPlugin.debugLevel > 1) Debug.Log("In preparing && " + handToFollow + " is blossom, show handMenu");
                    m_CompleteCounter = 0;
                    m_IsLocked = true;
                    ShowMenu();
                    VisualizationOnPrepare(handToFollow, ShowingStatus.Success);
                    m_CurrentStep = HandMenuStep.None;
                    return;
                }
            }

            if (fistCheck && palmReadyOffsetAngle == 0)
            {
                if (m_CurrentStep != HandMenuStep.InPreparing)
                {
                    if (HandTrackingPlugin.debugLevel > 1) Debug.Log("Start in preparing");
                    m_CurrentStep = HandMenuStep.InPreparing;
                    VisualizationOnPrepare(handToFollow, ShowingStatus.Showing);
                    return;
                }
            }
            else if (palmReadyOffsetAngle != 0)
            {
                if (m_CurrentStep != HandMenuStep.None)
                {
                    if (HandTrackingPlugin.debugLevel > 1) Debug.Log("PalmBackwardCheck is false , step = " + m_CurrentStep);
                    if (m_CurrentStep == HandMenuStep.InPreparing )
                    {
                        if (palmReadyOffsetAngle > 5)
                        {
                            VisualizationOnPrepare(handToFollow, ShowingStatus.Fail);
                            m_CurrentStep = HandMenuStep.None;
                        }
                        else
                        {
                            m_CurrentStep = HandMenuStep.Prepared;
                            m_HandMenuReadyTimeEnd = Time.time + 0.2f;
                        }
                    }
                    else if (m_CurrentStep == HandMenuStep.Prepared)
                    {
                        if (Time.time > m_HandMenuReadyTimeEnd || palmReadyOffsetAngle > 15)
                        {
                            VisualizationOnPrepare(handToFollow, ShowingStatus.Fail);
                            m_CurrentStep = HandMenuStep.None;
                        }
                    }
                }
            }
        }

        void VisualizationOnPrepare(HandTrackingPlugin.HandType type, ShowingStatus showingStatus)
        {
            switch (showingStatus)
            {
                case ShowingStatus.Showing:
                    if (prepIndicator != null && prepIndicator.isShow != ShowingStatus.Showing)
                    {
                        prepIndicator.Show();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log(type + " Hand SHOW Completed");
                    }
                    break;
                case ShowingStatus.Fail:
                    if (prepIndicator != null && prepIndicator.isShow == ShowingStatus.Showing)
                    {
                        prepIndicator.Fail();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log(type + " Hand FAIL Completed");
                    }
                    break;
                case ShowingStatus.Success:
                    if (prepIndicator != null && prepIndicator.isShow == ShowingStatus.Showing)
                    {
                        prepIndicator.Success();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log(type + " Hand SUCCESS Completed");
                    }
                    break;
                case ShowingStatus.Off:
                    if (prepIndicator != null && prepIndicator.isShow == ShowingStatus.Showing)
                    {
                        prepIndicator.Off();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log(type + " Hand OFF Completed");
                    }
                    break;
            }
        }

        void UpdateHandPrepPos(HandController handController, HandPrepIndicator indicator)
        {
            Vector3 palmPos = (handController.activeHand.joints[0].transform.position +
                               handController.activeHand.joints[9].transform.position) / 2;

            indicator.transform.rotation = Quaternion.LookRotation(
                handController.activeHand.joints[0].transform.up,
                handController.activeHand.joints[0].transform.right);
            indicator.transform.position = palmPos - handController.activeHand.joints[0].transform.up * 0.035f;
        }

        //更新手的显示隐藏
        void UpdateHandVisible(HandTrackingPlugin.HandType type, bool ishandDetected)
        {
            if (ishandDetected)
            {
                if (!m_IsHandDetected)
                {
                    m_IsHandDetected = true;
                }
            }
            else
            {
                if (m_IsHandDetected)
                {
                    m_IsHandDetected = false;
                    m_CurrentStep = HandMenuStep.None;

                    m_IsLocked = false;
                    m_CompleteCounter = 0;

                    VisualizationOnPrepare(type, ShowingStatus.Off);
                }
            }
        }

        void ShowMenu()
        {
            if (handMenu != null && !handMenu.activeSelf)
            {
                handMenu.SetActive(true);
                HudManager.instance.ShowOverlayMask();
            }
            if (m_HandMenuLocator != null)
                m_HandMenuLocator.ShowMenu();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (handToFollow == HandTrackingPlugin.HandType.RightHand)
                {
                    ShowMenu();
                }
            }
        }
    }
}