using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for Hand UI interaction with objects through the index finger tip. <br>
    /// 控制手部食指UI交互的类。<br>
    /// This script depends on HandController.cs, need to be added on the object with HandController.cs. <br>
    /// 脚本依赖于 HandController.cs。需要挂在HandController.cs的物体上。
    /// </summary>
    [RequireComponent(typeof(HandController))]
    public class UiInteractionPointer : MonoBehaviour
    {

        /// <summary>
        /// Enable UI interaction. <br>
        /// 启用UI交互。<br> 
        /// </summary>
        [SerializeField]
        bool m_IsEnabled = false;
        public bool isEnabled
        {
            get { return m_IsEnabled; }
            set
            {
                if (m_IsActive)
                {
                    Reset();
                }
                m_IsEnabled = value;
            }
        }

        /// <summary>
        /// The maximum number of detetable objects in the detection region. This value needs to be set to higher than the number of models one expectes to detect.  <br>
        /// 在检测范围内最多可获取到的物体数量。需要设置该数值大于期待检测到的物体数量。<br>
        /// Returned objects are not sorted by their distance to the detection center (index finger tip). Therefore, if this value is set to lower than the number of expected objects it's likely that the nearest object won't be included in those returned. <br>
        /// 返回的所有物体不按照物体到检测中心（食指指尖）距离进行排序。如果该数值小于期待被检测到的物体数量，有可能出现更近的物体未被检测到的情况。
        /// </summary>
        [Range(30, 150)]
        public int maxDetectableObjectNumber = 30;

        #region Private Field
        HandController m_HandController;
        // 用于检测距交互食指指尖此半径范围内的可交互物体。
        float m_TouchableRadius = 0.2f;
        //用于近场按压交互的食指指尖transform
        Transform m_IndexTip;
        Transform m_IndexRoot;
        Transform m_ThumbSecond;
        //检测范围内的colliders
        Collider[] m_TouchableObjects;
        //交互检测范围内最近的UiInteractionPointerHandler
        UiInteractionPointerHandler m_ClosestObject;
        UiInteractionPointerHandler m_TargetInInteraction;
        //当前手类型
        private HandTrackingPlugin.HandType m_HandType;
        //当前手是否可以被检测
        bool m_IsHandDetected;
        //初始化是否有效，若无效则不进行更新
        bool m_IsActive = false;

        InteractionPriority m_Priority = InteractionPriority.UiIneraction;
        HandInteractionType m_Type = HandInteractionType.UiIneraction;

        //bool m_PriorityEnabled = true;
        bool m_PriorityEnabled = false;
        bool m_IsPinched = false;
        bool m_PalmBackStatus = false;

        bool m_IsHandMenuOpened = false;

        int m_AlgorithmCounterIn = 0;
        int m_AlgorithmCounterOut = 0;

        Coroutine m_InitRoutine;
        #endregion

        //需要在手势初始化之后进行
        IEnumerator Start()
        {
            if (m_HandController == null)
                m_HandController = GetComponent<HandController>();

            if (m_HandController == null)
            {
                Debug.LogError("SkeletonHand Reference Null!");
                yield break;
            }
            yield return StartCoroutine(InitPointer());
            m_TouchableObjects = new Collider[maxDetectableObjectNumber];

            HandController.onActiveHandChanged += OnHandStyleChange;

            if (HandTrackingPlugin.instance != null)
            {
                HandTrackingPlugin.instance.onHandDataUpdated += UpdateHandInfo;
            }

            if(PointerManager.instance != null)
            {
                PointerManager.instance.onHandMenuChanged += UpdateHandMenuStatus;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_IsActive)
                return;
            UpdateClosestObject();
        }

        void OnHandStyleChange(HandController controller)
        {
            if (controller == m_HandController)
            {
                if (m_InitRoutine != null) StopCoroutine(m_InitRoutine);
                Reset();
                m_IsActive = false;
                m_InitRoutine = StartCoroutine(InitFingerJoint());
            }
        }

        void UpdateHandMenuStatus(bool status)
        {
            if (m_IsHandMenuOpened != status)
            {
                m_IsHandMenuOpened = status;
                if (m_IsHandMenuOpened)
                    Reset();
            }
        }

        //初始化近场按压检测手势信息和交互指尖节点
        IEnumerator InitPointer()
        {
            if (HandTrackingPlugin.instance == null)
            {
                Debug.LogError("NO SDK INIT YET");
                yield break;
            }
            else
            {
                if (m_HandController == HandTrackingPlugin.instance.rightHandController)
                {
                    m_HandType = HandTrackingPlugin.HandType.RightHand;
                }
                else if (m_HandController == HandTrackingPlugin.instance.leftHandController)
                {
                    m_HandType = HandTrackingPlugin.HandType.LeftHand;
                }
                else
                {
                    yield break;
                }
                yield return m_InitRoutine = StartCoroutine(InitFingerJoint());
            }
        }

        //初始化当前手节点
        IEnumerator InitFingerJoint()
        {
            yield return new WaitUntil(() => m_HandController.activeHand != null);
            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)FingerTipJointID.Index] != null);
            m_IndexTip = m_HandController.activeHand.joints[(int)FingerTipJointID.Index];
            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.IndexNode1] != null);
            m_IndexRoot = m_HandController.activeHand.joints[(int)HandJointID.IndexNode1];
            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.ThumbNode2] != null);
            m_ThumbSecond = m_HandController.activeHand.joints[(int)HandJointID.ThumbNode2];

            m_IsActive = true;
            m_InitRoutine = null;
        }

        void UpdateClosestObject()
        {
            if (!m_IsEnabled)
                return;
            if (m_IsHandMenuOpened)
                return;
            if (!m_IsHandDetected)
                return;
            if (m_PalmBackStatus)
                return;
            if (m_IsPinched && m_TargetInInteraction != null)
                return;

            UiInteractionPointerHandler currentClosestObject = null;
            List<UiInteractionPointerHandler> availableObjects = new List<UiInteractionPointerHandler>();
            float closestDis = float.PositiveInfinity;
            int objectNum = Physics.OverlapSphereNonAlloc(m_IndexTip.position, m_TouchableRadius, m_TouchableObjects);
            //Get all touchable objects
            for (int i = 0; i < objectNum; i++)
            {
                Collider col = m_TouchableObjects[i];
                if (col.gameObject.GetComponent<UiInteractionPointerHandler>() == null)
                    continue;
                UiInteractionPointerHandler handler = col.GetComponent<UiInteractionPointerHandler>();
                if (handler != null)
                {
                    handler.DistanceToTouchable(m_IndexTip.position);
                    availableObjects.Add(handler);
                }
            }
            //Get the closest & hand in touchable area object
            if (availableObjects != null && availableObjects.Count > 0)
            {
                UiInteractionPointerHandler[] availableObjectsArray = availableObjects.OrderBy(obj => obj.touchableDistance).ToArray();

                for (int i = 0; i < availableObjectsArray.Length; i++)
                {
                    bool isInBounds = availableObjectsArray[i].CheckTouchable(m_IndexTip.position, out float distance, out Vector3 normal);
                    if (isInBounds)
                    {
                        currentClosestObject = availableObjectsArray[i];
                        closestDis = distance;
                        break;
                    }
                }
            }

            if (m_ClosestObject != null)
            {
                if (currentClosestObject != null)
                {
                    if (currentClosestObject != m_ClosestObject)
                    {
                        m_ClosestObject.OnLeaveFar();
                        if (closestDis >= 0)
                        {
                            if (PointerManager.instance != null)
                            {
                                int res = PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, false);
                                if (res < 0)
                                {
                                    return;
                                }
                                else
                                {
                                    if (currentClosestObject.isInInteraction)
                                    {
                                        m_ClosestObject = null;
                                        return;
                                    }
                                    currentClosestObject.OnComeClose();
                                    m_ClosestObject = currentClosestObject;
                                }
                            }
                        }
                        else
                        {
                            m_ClosestObject = null;

                            if (PointerManager.instance != null)
                            {
                                PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
                            }
                        }
                    }
                    else
                    {
                        if (m_PriorityEnabled)
                            m_ClosestObject.OnTouchUpdate(m_IndexTip.position, closestDis);
                    }
                }
                else
                {
                    m_ClosestObject.OnLeaveFar();
                    m_ClosestObject = null;

                    if (PointerManager.instance != null)
                    {
                        PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
                    }
                }
            }
            else
            {
                if (currentClosestObject != null)
                {
                    if (closestDis >= 0)
                    {
                        if (PointerManager.instance != null)
                        {
                            int res = PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, false);
                            if (res < 0)
                            {
                                return;
                            }
                            else
                            {
                                if (currentClosestObject.isInInteraction)
                                {
                                    m_ClosestObject = null;
                                    return;
                                }
                                currentClosestObject.OnComeClose();
                                m_ClosestObject = currentClosestObject;
                            }
                        }
                    }
                }
            }
        }

        void UpdateHandInfo()
        {
            if (!m_IsActive)
                return;

            HandTrackingPlugin.HandInfo info;
            if (m_HandType == HandTrackingPlugin.HandType.LeftHand)
                info = HandTrackingPlugin.instance.leftHandInfo;
            else
                info = HandTrackingPlugin.instance.rightHandInfo;

            UpdateHandVisible(info.handDetected);

            if (!m_IsHandDetected)
                return;

            UpdatePalmDirection();

            if (m_PalmBackStatus)
                return;

            CheckHandPinch(info);
        }

        void CheckHandPinch(HandTrackingPlugin.HandInfo info)
        {
            if (info.staticGesture == HandTrackingPlugin.StaticGesture.OK)
            {
                m_AlgorithmCounterOut = 0;
                if (!m_IsPinched)
                {
                    if (m_AlgorithmCounterIn < 3)
                        m_AlgorithmCounterIn += 1;
                    else
                    {
                        m_IsPinched = true;
                        m_AlgorithmCounterIn = 0;

                        if (!m_IsEnabled)
                            return;
                        if (m_IsHandMenuOpened)
                            return;
                        if (!m_PriorityEnabled)
                            return;

                        if (m_ClosestObject != null)
                        {
                            if (m_ClosestObject.CheckGrabbable())
                            {
                                m_TargetInInteraction = m_ClosestObject;
                                m_TargetInInteraction.OnPinchDown((m_IndexRoot.position + m_ThumbSecond.position) / 2);
                            }
                        }
                    }
                }
                else
                {
                    if (!m_IsEnabled)
                        return;
                    if (m_IsHandMenuOpened)
                        return;
                    if (!m_PriorityEnabled)
                        return;

                    if (m_TargetInInteraction != null)
                    {
                        m_TargetInInteraction.OnDragging((m_IndexRoot.position + m_ThumbSecond.position) / 2);
                    }
                }
            }
            else
            {
                m_AlgorithmCounterIn = 0;
                if (m_IsPinched)
                {
                    if (m_AlgorithmCounterOut < 4)
                        m_AlgorithmCounterOut += 1;
                    else
                    {
                        m_IsPinched = false;
                        m_AlgorithmCounterOut = 0;

                        if (!m_IsEnabled)
                            return;
                        if (m_IsHandMenuOpened)
                            return;
                        if (!m_PriorityEnabled)
                            return;

                        if (m_TargetInInteraction != null)
                        {
                            m_TargetInInteraction.OnPinchUp();
                            m_TargetInInteraction = null;
                        }
                    }
                }
            }
        }

        //更新手的显示隐藏
        void UpdateHandVisible(bool ishandDetected)
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

                    if (m_IsEnabled && !m_IsHandMenuOpened)
                    {
                        if (PointerManager.instance != null)
                        {
                            PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
                        }
                        ResetHandEvent();
                        m_PalmBackStatus = true;
                    }
                }
            }
        }

        void UpdatePalmDirection()
        {
            bool palmBackStatus = false;

            if (CustomizedGestureController.instance != null)
            {
                palmBackStatus = CustomizedGestureController.instance.GetHandStatus(m_HandType, CustomizedGesture.PalmBack);
            }

            if (m_PalmBackStatus != palmBackStatus)
            {
                if (palmBackStatus)
                {
                    if (!m_PalmBackStatus)
                    {
                        m_PalmBackStatus = true;

                        if (m_IsEnabled && !m_IsHandMenuOpened)
                        {
                            if (PointerManager.instance != null)
                            {
                                PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
                            }
                            ResetHandEvent();
                        }
                    }
                }
                else
                {
                    if (m_PalmBackStatus)
                    {
                        m_PalmBackStatus = false;
                    }
                }
                m_PalmBackStatus = palmBackStatus;
            }
        }

        void SetVisualizationItems(bool res)
        {

        }

        //当手消失时重置交互事件
        void ResetHandEvent()
        {
            if (m_IsPinched)
            {
                if (m_TargetInInteraction != null)
                {
                    m_TargetInInteraction.OnPinchUp();
                    m_TargetInInteraction = null;
                }
            }

            if (m_ClosestObject != null)
            {
                m_ClosestObject.OnLeaveFar();
                m_ClosestObject = null;
            }
           
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Hand Lost, Reset UI interaction target");
        }

        // Force reset UI interaction.
        // 用来强制重置UI交互.
        void Reset()
        {
            if (PointerManager.instance != null)
            {
                PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
            }

            ResetHandEvent();

            SetVisualizationItems(false);

            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Reset UI Interaction Pointer");
        }

        /// <summary>
        /// Updates interaction priority when available interaction is updated. <br>
        /// 在可用交互方式有更新时进行交互优先级更新。
        /// </summary>
        public void UpdatePriority(bool isHigher)
        {
            if (!isHigher)
            {
                ResetHandEvent();
            }
            SetVisualizationItems(isHigher);
            m_PriorityEnabled = isHigher;
        }
    }
}
