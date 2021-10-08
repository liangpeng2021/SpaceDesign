using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    public enum CustomizedGesture
    {
        Fist = 0,
        PalmBack = 1,
        PalmDown = 2
        //PinchwithPalmForward = 2,
    }

    public class CustomizedGestureController : MonoBehaviour
    {
        public static CustomizedGestureController instance = null;

        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }

       // CustomizedGesture leftGesture = CustomizedGesture.None;
       // CustomizedGesture rightGesture = CustomizedGesture.None;
        Dictionary<CustomizedGesture, bool> leftGesture = new Dictionary<CustomizedGesture, bool>();
        Dictionary<CustomizedGesture, bool> rightGesture = new Dictionary<CustomizedGesture, bool>();

        Transform m_LeftThumbTip, m_LeftIndexTip, m_LeftMiddleTip, m_LeftRingTip, m_LeftPinkyTip,
            m_RightThumbTip, m_RightIndexTip, m_RightMiddleTip, m_RightRingTip, m_RightPinkyTip,
            m_LeftRoot, m_RightRoot, m_LeftIndexRoot, m_RightIndexRoot, m_LeftRingRoot, m_RightRingRoot;

        bool m_IsLeftHandDetected, m_IsRightHandDetected;

        HandController m_LeftHandController, m_RightHandController;
        Coroutine m_InitLeftRoutine, m_InitRightRoutine;
        bool m_IsActive;

        IEnumerator Start()
        {
            if (HandTrackingPlugin.instance != null)
            {
                if (HandTrackingPlugin.instance.leftHandController != null)
                    m_LeftHandController = HandTrackingPlugin.instance.leftHandController;
                if (HandTrackingPlugin.instance.rightHandController != null)
                    m_RightHandController = HandTrackingPlugin.instance.rightHandController;
            }

            if (m_LeftHandController == null || m_RightHandController == null)
            {
                Debug.LogError("handController Reference Null!");
                yield break;
            }

            yield return StartCoroutine(InitHandMenu());

            HandController.onActiveHandChanged += OnHandStyleChange;

            if (HandTrackingPlugin.instance != null)
            {
                HandTrackingPlugin.instance.onHandDataUpdated += UpdateHandInfo;
            }
        }

        IEnumerator InitHandMenu()
        {
            if (HandTrackingPlugin.instance == null)
            {
                Debug.LogError("NO SDK INIT YET");
                yield break;
            }
            else
            {
                yield return m_InitLeftRoutine = StartCoroutine(InitFingerJoints(HandTrackingPlugin.HandType.LeftHand));
                yield return m_InitRightRoutine = StartCoroutine(InitFingerJoints(HandTrackingPlugin.HandType.RightHand));
                ResetGestureStatus(HandTrackingPlugin.HandType.LeftHand);
                ResetGestureStatus(HandTrackingPlugin.HandType.RightHand);
                m_IsActive = true;
            }
        }

        IEnumerator InitFingerJoints(HandTrackingPlugin.HandType type)
        {
            HandController controller;
            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                controller = m_LeftHandController;
                yield return new WaitUntil(() => controller.activeHand != null);

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Thumb] != null);
                m_LeftThumbTip = controller.activeHand.joints[(int)FingerTipJointID.Thumb];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Index] != null);
                m_LeftIndexTip = controller.activeHand.joints[(int)FingerTipJointID.Index];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Middle] != null);
                m_LeftMiddleTip = controller.activeHand.joints[(int)FingerTipJointID.Middle];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Ring] != null);
                m_LeftRingTip = controller.activeHand.joints[(int)FingerTipJointID.Ring];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Pinky] != null);
                m_LeftPinkyTip = controller.activeHand.joints[(int)FingerTipJointID.Pinky];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.Wrist] != null);
                m_LeftRoot = controller.activeHand.joints[(int)HandJointID.Wrist];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.IndexNode1] != null);
                m_LeftIndexRoot = controller.activeHand.joints[(int)HandJointID.IndexNode1];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.RingNode1] != null);
                m_LeftRingRoot = controller.activeHand.joints[(int)HandJointID.RingNode1];

                m_InitLeftRoutine = null;
            }
            else
            {
                controller = m_RightHandController;
                yield return new WaitUntil(() => controller.activeHand != null);

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Thumb] != null);
                m_RightThumbTip = controller.activeHand.joints[(int)FingerTipJointID.Thumb];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Index] != null);
                m_RightIndexTip = controller.activeHand.joints[(int)FingerTipJointID.Index];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Middle] != null);
                m_RightMiddleTip = controller.activeHand.joints[(int)FingerTipJointID.Middle];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Ring] != null);
                m_RightRingTip = controller.activeHand.joints[(int)FingerTipJointID.Ring];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)FingerTipJointID.Pinky] != null);
                m_RightPinkyTip = controller.activeHand.joints[(int)FingerTipJointID.Pinky];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.Wrist] != null);
                m_RightRoot = controller.activeHand.joints[(int)HandJointID.Wrist];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.IndexNode1] != null);
                m_RightIndexRoot = controller.activeHand.joints[(int)HandJointID.IndexNode1];

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.RingNode1] != null);
                m_RightRingRoot = controller.activeHand.joints[(int)HandJointID.RingNode1];

                m_InitRightRoutine = null;
            }
        }

        void ResetGestureStatus(HandTrackingPlugin.HandType type)
        {
            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                if (leftGesture.ContainsKey(CustomizedGesture.Fist))
                    leftGesture[CustomizedGesture.Fist] = false;
                else
                    leftGesture.Add(CustomizedGesture.Fist, false);

                if (leftGesture.ContainsKey(CustomizedGesture.PalmBack))
                    leftGesture[CustomizedGesture.PalmBack] = false;
                else
                    leftGesture.Add(CustomizedGesture.PalmBack, false);
                
                if (leftGesture.ContainsKey(CustomizedGesture.PalmDown))
                    leftGesture[CustomizedGesture.PalmDown] = false;
                else
                    leftGesture.Add(CustomizedGesture.PalmDown, false);

                /*if (leftGesture.ContainsKey(CustomizedGesture.PinchwithPalmForward))
                    leftGesture[CustomizedGesture.PinchwithPalmForward] = false;
                else
                    leftGesture.Add(CustomizedGesture.PinchwithPalmForward, false);*/
            }
            else
            {
                if (rightGesture.ContainsKey(CustomizedGesture.Fist))
                    rightGesture[CustomizedGesture.Fist] = false;
                else
                    rightGesture.Add(CustomizedGesture.Fist, false);

                if (rightGesture.ContainsKey(CustomizedGesture.PalmBack))
                    rightGesture[CustomizedGesture.PalmBack] = false;
                else
                    rightGesture.Add(CustomizedGesture.PalmBack, false);

                if (rightGesture.ContainsKey(CustomizedGesture.PalmDown))
                    rightGesture[CustomizedGesture.PalmDown] = false;
                else
                    rightGesture.Add(CustomizedGesture.PalmDown, false);

                /*if (rightGesture.ContainsKey(CustomizedGesture.PinchwithPalmForward))
                    rightGesture[CustomizedGesture.PinchwithPalmForward] = false;
                else
                    rightGesture.Add(CustomizedGesture.PinchwithPalmForward, false);*/
            }
        }

        void OnHandStyleChange(HandController controller)
        {
            if (controller == m_LeftHandController)
            {
                if (m_InitLeftRoutine != null) StopCoroutine(m_InitLeftRoutine);
                m_IsActive = false;
                m_InitLeftRoutine = StartCoroutine(InitFingerJoints(HandTrackingPlugin.HandType.LeftHand));
                ResetGestureStatus(HandTrackingPlugin.HandType.LeftHand);
                m_IsActive = true;
            }
            else
            {
                if (m_InitRightRoutine != null) StopCoroutine(m_InitRightRoutine);
                m_IsActive = false;
                m_InitRightRoutine = StartCoroutine(InitFingerJoints(HandTrackingPlugin.HandType.RightHand));
                ResetGestureStatus(HandTrackingPlugin.HandType.RightHand);
                m_IsActive = true;
            }
        }

        void UpdateHandInfo()
        {
            if (!m_IsActive)
                return;

            bool leftHandDetected = HandTrackingPlugin.instance.leftHandInfo.handDetected;
            bool rightHandDetected = HandTrackingPlugin.instance.rightHandInfo.handDetected;

            UpdateHandVisible(HandTrackingPlugin.HandType.LeftHand, leftHandDetected);
            UpdateHandVisible(HandTrackingPlugin.HandType.RightHand, rightHandDetected);

            if (m_IsLeftHandDetected) 
            {
                if (GetFistGestureStatus(HandTrackingPlugin.HandType.LeftHand))
                {
                    leftGesture[CustomizedGesture.Fist] = true;
                }
                else
                {
                    leftGesture[CustomizedGesture.Fist] = false;
                }
                
                if (GetPalmBackStatus(HandTrackingPlugin.HandType.LeftHand))
                {
                    leftGesture[CustomizedGesture.PalmBack] = true;
                }
                else
                {
                    leftGesture[CustomizedGesture.PalmBack] = false;
                }
                
                if (GetPalmDownStatus(HandTrackingPlugin.HandType.LeftHand))
                {
                    leftGesture[CustomizedGesture.PalmDown] = true;
                }
                else
                {
                    leftGesture[CustomizedGesture.PalmDown] = false;
                }
                
            }

            if (m_IsRightHandDetected)
            {
                if (GetFistGestureStatus(HandTrackingPlugin.HandType.RightHand))
                {
                    rightGesture[CustomizedGesture.Fist] = true;
                }
                else
                {
                    rightGesture[CustomizedGesture.Fist] = false;
                }

                if (GetPalmBackStatus(HandTrackingPlugin.HandType.RightHand))
                {
                    rightGesture[CustomizedGesture.PalmBack] = true;
                }
                else
                {
                    rightGesture[CustomizedGesture.PalmBack] = false;
                }

                if (GetPalmDownStatus(HandTrackingPlugin.HandType.RightHand))
                {
                    rightGesture[CustomizedGesture.PalmDown] = true;
                }
                else
                {
                    rightGesture[CustomizedGesture.PalmDown] = false;
                }
            }
        }

        void UpdateHandVisible(HandTrackingPlugin.HandType type, bool ishandDetected)
        {

            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                if (ishandDetected)
                {
                    if (!m_IsLeftHandDetected)
                    {
                        m_IsLeftHandDetected = true;
                    }
                }
                else
                {
                    if (m_IsLeftHandDetected)
                    {
                        m_IsLeftHandDetected = false;
                        ResetGestureStatus(HandTrackingPlugin.HandType.LeftHand);
                    }
                }
            }
            else
            {
                if (ishandDetected)
                {
                    if (!m_IsRightHandDetected)
                    {
                        m_IsRightHandDetected = true;
                    }
                }
                else
                {
                    if (m_IsRightHandDetected)
                    {
                        m_IsRightHandDetected = false;
                        ResetGestureStatus(HandTrackingPlugin.HandType.RightHand);
                    }
                }
            }
        }

        bool GetFistGestureStatus(HandTrackingPlugin.HandType type)
        {
            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                Vector3 palmCenter = (m_LeftRoot.position + m_LeftRingRoot.position + m_LeftIndexRoot.position) / 3;
                if (Vector3.Distance(palmCenter, m_LeftIndexTip.position) > 0.06f || Vector3.Distance(m_LeftThumbTip.position, palmCenter) > 0.09f ||
                    Vector3.Distance(palmCenter, m_LeftRingTip.position) > 0.05f || Vector3.Distance(palmCenter, m_LeftPinkyTip.position) > 0.05f ||
                    Vector3.Distance(palmCenter, m_LeftMiddleTip.position) > 0.05f)
                {
                    return false;
                }
                return true;
            }
            else
            {
                Vector3 palmCenter = (m_RightRoot.position + m_RightRingRoot.position + m_RightIndexRoot.position) / 3;
                if (Vector3.Distance(palmCenter, m_RightIndexTip.position) > 0.06f || Vector3.Distance(m_RightThumbTip.position, palmCenter) > 0.09f ||
                    Vector3.Distance(palmCenter, m_RightRingTip.position) > 0.05f || Vector3.Distance(palmCenter, m_RightPinkyTip.position) > 0.05f ||
                    Vector3.Distance(palmCenter, m_RightMiddleTip.position) > 0.05f)
                {
                    return false;
                }
                return true;
            }
        }

        bool GetPalmBackStatus(HandTrackingPlugin.HandType type)
        {
            Vector3 palmForward = GetPalmForward(type);

            if (Vector3.Angle(palmForward, CenterCamera.centerCamera.transform.forward) < 175f &&
                Vector3.Angle(palmForward, CenterCamera.centerCamera.transform.forward) > 145f)
                return true;

            return false;
        }
        
        bool GetPalmDownStatus(HandTrackingPlugin.HandType type)
        {
            Vector3 palmForward = GetPalmForward(type);

            if (Vector3.Angle(palmForward, -Vector3.up) < 30f)
                return true;

            return false;
        }

        Vector3 GetPalmForward(HandTrackingPlugin.HandType type)
        {
            Vector3 rootToIndex, rootToRing;

            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                rootToIndex = m_LeftIndexRoot.position - m_LeftRoot.position;
                rootToRing = m_LeftRingRoot.position - m_LeftRoot.position;
            }
            else
            {
                rootToIndex = m_RightIndexRoot.position - m_RightRoot.position;
                rootToRing = m_RightRingRoot.position - m_RightRoot.position;
            }

            Vector3 palmForward = Vector3.Cross(rootToIndex.normalized, rootToRing.normalized).normalized;

            if (type == HandTrackingPlugin.HandType.RightHand)
                palmForward *= -1f;

            return palmForward;
        }

        public bool GetHandStatus(HandTrackingPlugin.HandType type, CustomizedGesture gesture)
        {
            if (!m_IsActive)
                return false;

            if(type == HandTrackingPlugin.HandType.LeftHand)
            {
                if(leftGesture.Count > 0)
                {
                    return leftGesture[gesture];
                }
            }
            else
            {
                if(rightGesture.Count > 0)
                {
                    return rightGesture[gesture];
                }
            }
            return false;
        }
    }
}
