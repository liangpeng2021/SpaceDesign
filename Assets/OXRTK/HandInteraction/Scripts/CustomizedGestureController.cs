using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The enum of customized gestures.<br>
    /// 自定义手势的枚举。
    /// </summary>
    public enum CustomizedGesture
    {
        Fist = 0,
        PalmBackForBlossom = 1,
        PalmDown = 2,
        PalmForward = 3
        //PinchwithPalmForward = 2,
    }

    /// <summary>
    /// The class for customized gesture management. <br>
    /// 管理自定义手势的类。
    /// </summary>
    public class CustomizedGestureController : MonoBehaviour
    {
        public static CustomizedGestureController instance = null;

        /// <summary>
        /// Callback event when the hand is hiding or showing. <br>
        /// 当手被隐藏或出现时的回调事件。
        /// </summary>
        public Action<HandTrackingPlugin.HandType, bool> onHandDisplayChanged;

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

        Dictionary<CustomizedGesture, bool> leftGesture = new Dictionary<CustomizedGesture, bool>();
        Dictionary<CustomizedGesture, bool> rightGesture = new Dictionary<CustomizedGesture, bool>();

        Transform m_LeftThumbTip, m_LeftIndexTip, m_LeftMiddleTip, m_LeftRingTip, m_LeftPinkyTip,
            m_RightThumbTip, m_RightIndexTip, m_RightMiddleTip, m_RightRingTip, m_RightPinkyTip,
            m_LeftRoot, m_RightRoot, m_LeftIndexRoot, m_RightIndexRoot, m_LeftRingRoot, m_RightRingRoot, m_LeftMiddleRoot, m_RightMiddleRoot;

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

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.MiddleNode1] != null);
                m_LeftMiddleRoot = controller.activeHand.joints[(int)HandJointID.MiddleNode1]; //node 9

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

                yield return new WaitUntil(() => controller.activeHand.joints[(int)HandJointID.MiddleNode1] != null);
                m_RightMiddleRoot = controller.activeHand.joints[(int)HandJointID.MiddleNode1]; //node 9

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

                if (leftGesture.ContainsKey(CustomizedGesture.PalmBackForBlossom))
                    leftGesture[CustomizedGesture.PalmBackForBlossom] = false;
                else
                    leftGesture.Add(CustomizedGesture.PalmBackForBlossom, false);

                if (leftGesture.ContainsKey(CustomizedGesture.PalmDown))
                    leftGesture[CustomizedGesture.PalmDown] = false;
                else
                    leftGesture.Add(CustomizedGesture.PalmDown, false);

                if (leftGesture.ContainsKey(CustomizedGesture.PalmForward))
                    leftGesture[CustomizedGesture.PalmForward] = false;
                else
                    leftGesture.Add(CustomizedGesture.PalmForward, false);
            }
            else
            {
                if (rightGesture.ContainsKey(CustomizedGesture.Fist))
                    rightGesture[CustomizedGesture.Fist] = false;
                else
                    rightGesture.Add(CustomizedGesture.Fist, false);

                if (rightGesture.ContainsKey(CustomizedGesture.PalmBackForBlossom))
                    rightGesture[CustomizedGesture.PalmBackForBlossom] = false;
                else
                    rightGesture.Add(CustomizedGesture.PalmBackForBlossom, false);

                if (rightGesture.ContainsKey(CustomizedGesture.PalmDown))
                    rightGesture[CustomizedGesture.PalmDown] = false;
                else
                    rightGesture.Add(CustomizedGesture.PalmDown, false);

                if (rightGesture.ContainsKey(CustomizedGesture.PalmForward))
                    rightGesture[CustomizedGesture.PalmForward] = false;
                else
                    rightGesture.Add(CustomizedGesture.PalmForward, false);
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
                    leftGesture[CustomizedGesture.PalmBackForBlossom] = true;
                }
                else
                {
                    leftGesture[CustomizedGesture.PalmBackForBlossom] = false;
                }

                if (GetPalmDownStatus(HandTrackingPlugin.HandType.LeftHand))
                {
                    leftGesture[CustomizedGesture.PalmDown] = true;
                }
                else
                {
                    leftGesture[CustomizedGesture.PalmDown] = false;
                }

                if (GetPalmForwardStatus(HandTrackingPlugin.HandType.LeftHand))
                {
                    leftGesture[CustomizedGesture.PalmForward] = true;
                }
                else
                {
                    leftGesture[CustomizedGesture.PalmForward] = false;
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
                    rightGesture[CustomizedGesture.PalmBackForBlossom] = true;
                }
                else
                {
                    rightGesture[CustomizedGesture.PalmBackForBlossom] = false;
                }

                if (GetPalmDownStatus(HandTrackingPlugin.HandType.RightHand))
                {
                    rightGesture[CustomizedGesture.PalmDown] = true;
                }
                else
                {
                    rightGesture[CustomizedGesture.PalmDown] = false;
                }

                if (GetPalmForwardStatus(HandTrackingPlugin.HandType.RightHand))
                {
                    rightGesture[CustomizedGesture.PalmForward] = true;
                }
                else
                {
                    rightGesture[CustomizedGesture.PalmForward] = false;
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
                        onHandDisplayChanged?.Invoke(type, m_IsLeftHandDetected);
                    }
                }
                else
                {
                    if (m_IsLeftHandDetected)
                    {
                        m_IsLeftHandDetected = false;
                        ResetGestureStatus(HandTrackingPlugin.HandType.LeftHand);
                        onHandDisplayChanged?.Invoke(type, m_IsLeftHandDetected);
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
                        onHandDisplayChanged?.Invoke(type, m_IsRightHandDetected);
                    }
                }
                else
                {
                    if (m_IsRightHandDetected)
                    {
                        m_IsRightHandDetected = false;
                        ResetGestureStatus(HandTrackingPlugin.HandType.RightHand);
                        onHandDisplayChanged?.Invoke(type, m_IsRightHandDetected);
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

            if (Vector3.Angle(palmForward, CenterCamera.instance.centerCamera.transform.forward) < 175f &&
                Vector3.Angle(palmForward, CenterCamera.instance.centerCamera.transform.forward) > 145f)
                return true;

            return false;
        }

        bool GetPalmForwardStatus(HandTrackingPlugin.HandType type)
        {
            Vector3 palmForward = GetPalmForward(type);

            if (Vector3.Angle(palmForward, CenterCamera.instance.centerCamera.transform.forward) < 90f)
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

        /// <summary>
        /// Check whether the certain hand is in the certain gesture status.<br>
        /// 检测某个手是否在某一手势状态下。
        /// </summary>
        /// <param name="type">Left/Right hand. <br>左、右手.</param>
        /// <param name="gesture">Customized gesture. <br>自定义手势.</param>
        public bool GetHandStatus(HandTrackingPlugin.HandType type, CustomizedGesture gesture)
        {
            if (!m_IsActive)
                return false;

            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                if (leftGesture.Count > 0)
                {
                    return leftGesture[gesture];
                }
            }
            else
            {
                if (rightGesture.Count > 0)
                {
                    return rightGesture[gesture];
                }
            }
            return false;
        }

        /// <summary>
        /// Check if bloom possible.<br>
        /// 检测是否可以开花
        /// </summary>
        /// <param name="type">Left/Right hand. <br>左、右手.</param>
        /// <returns>Angle off from possible zoon,0 means in the possible zoon. <br>超出可行区域的角度,0表示在可行区域内.</returns>
        public float IsBloomPossible(HandTrackingPlugin.HandType type)
        {
            if (!m_IsActive)
                return 999;

            if (type == HandTrackingPlugin.HandType.LeftHand)
            {
                return IsBloomOk(m_LeftRoot, m_LeftMiddleRoot, m_LeftIndexRoot, m_LeftRingRoot);
            }
            else
            {
                return IsBloomOk(m_RightRoot, m_RightMiddleRoot, m_RightRingRoot, m_RightIndexRoot);
            }
        }
        float IsBloomOk(Transform verticalRoot, Transform verticalTip, Transform horizontalRoot, Transform horizontalTip)
        {
            float xz = PalmRotateAroundXZOK(verticalRoot, verticalTip);
            //Debug.Log("BLOOM - 前倾不OK/时针旋转不OK");
            float y = PalmRotateAroundYOK(horizontalRoot, horizontalTip);
            //Debug.Log("BLOOM - 左右旋转不OK");
            return xz + y;
        }

        float m_XMaxAngle = -40f;
        float m_XMinAngle = -90f;
        float m_YMaxAngle = 30f;
        float m_ZMaxAngle = 40f;
        //Use HandRoot-0 and MiddleRoot-9
        float PalmRotateAroundXZOK(Transform nodeRoot, Transform nodeTip)
        {
            Vector3 rootPos = XR.XRCameraManager.Instance.stereoCamera.transform.InverseTransformPoint(nodeRoot.position);
            Vector3 tipPos = XR.XRCameraManager.Instance.stereoCamera.transform.InverseTransformPoint(nodeTip.position);
            Vector3 rootPosZ = rootPos;
            Vector3 tipPosZ = tipPos;

            if (tipPos.y <= rootPos.y)
                return 999;

            rootPos.x = tipPos.x = 0;
            Vector3 palmDir = (tipPos - rootPos).normalized;
            float angleDiff = Vector3.SignedAngle(Vector3.forward, palmDir, Vector3.right);

            float result = 0;
            if (angleDiff > m_XMaxAngle)
            {
                result += angleDiff - m_XMaxAngle; //相对相机向前倾斜判定, range -40(指尖前倾50) to -90(手竖直)
            }
            if (angleDiff < m_XMinAngle)
            {
                result += m_XMinAngle - angleDiff;
            }

            rootPosZ.z = tipPosZ.z = 0;
            palmDir = (tipPosZ - rootPosZ).normalized;
            angleDiff = Vector3.Angle(Vector3.up, palmDir);
            if (angleDiff > m_ZMaxAngle)
                result += angleDiff - m_ZMaxAngle;//相对相机以Z轴，顺时针/逆时针旋转判定, range 25度以内

            return result;
        }

        //Use RingRoot-5 and IndexRoot-13
        float PalmRotateAroundYOK(Transform nodeRoot, Transform nodeTip)
        {
            //相对相机以Y轴，左/右旋转判定, range 25度以内
            Vector3 rootPos = XR.XRCameraManager.Instance.stereoCamera.transform.InverseTransformPoint(nodeRoot.position);
            Vector3 tipPos = XR.XRCameraManager.Instance.stereoCamera.transform.InverseTransformPoint(nodeTip.position);

            if (rootPos.x == tipPos.x && rootPos.z == tipPos.z)
                return 999;

            rootPos.y = tipPos.y = 0;

            float result = 0;
            Vector3 palmDir = (tipPos - rootPos).normalized;
            float angleDiff = Vector3.Angle(Vector3.right, palmDir);
            if (angleDiff > m_YMaxAngle)
                result = angleDiff - m_YMaxAngle;
            return result;
        }
    }
}
