using ArmIK;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for hand ray interaction with far interaction objects through the laser. <br>
    /// 控制手部射线远端交互的类。<br>
    /// This script depends on HandController.cs. <br>
    /// 脚本依赖于 HandController.cs。
    /// </summary>
    [RequireComponent(typeof(HandController))]
    public class RayInteractionPointer : MonoBehaviour
    {
        #region Public Field
        /// <summary>
        /// Enable ray interaction. <br>
        /// 启用射线交互。<br> 
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
                    if (value)
                    {
                        if (m_IsHandDetected)
                        {
                            if (PointerManager.instance != null)
                            {
                                int res = PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, false);
                            }
                        }
                    }
                }
                m_IsEnabled = value;
            }
        }
        
        /// <summary>
        /// Customized cursor，keep it blank if use a default cursor. <br>
        /// 自定义光标,若使用默认光标则保持空即可。<br> 
        /// If the cursor needs different status, add CursorInfo on it, Set CursorVisual in CursorInfo script and use CursorVisualizationControl to control different cursor status. <br>
        /// 如果光标需要对旋转、移动、缩放进行不同状态显示，需要在光标上挂CursorInfo的脚本, 对CursorVisual赋值, 并使用CursorVisualizationControl来控制光标不同状态显示。
        /// </summary>
        public GameObject cursor;

        /// <summary>
        /// Controls laser line display. If it is true, display line and cursor. If it is false, only cursor shows. <br>
        /// 控制指尖射线显示。如果为true，则使用激光线和光标。如果为false，则只使用光标。<br>
        /// When it is true. An LineRenderer component will appears on the same object. Please use the LineRenderer to set laser visualization. <br>
        /// 当变量为true时，LineRenderer组件会自动出现在当前物体上，请使用该LineRenderer组件设置射线可视化。
        /// </summary>        
        public bool useLaserLine = true;

        /// <summary>
        /// The color gradient of the laser under the normal status.<br>
        /// 射线正常状态下的色彩渐变。<br>
        /// </summary> 
        public Gradient normalLaserCol;

        /// <summary>
        /// The color gradient of the laser under the pinch status.<br>
        /// 射线在pinch状态下的色彩渐变。<br>
        /// </summary> 
        public Gradient pinchedLaserCol;
        
        /// <summary>
        /// The color gradient of the laser under handmenu normal status.<br>
        /// 射线在handMenu正常状态下的色彩渐变。<br>
        /// </summary> 
        public Gradient menuNormalLaserCol;
        
        /// <summary>
        /// The color gradient of the laser under handmenu pinch status.<br>
        /// 射线在handMenu pinch状态下的色彩渐变。<br>
        /// </summary> 
        public Gradient menuPinchLaserCol;
        
        
        //当有手势相关交互发生时触发
        public PointerEvent onActionEvent = new PointerEvent();
        #endregion

        #region Private Field
        HandController m_HandController;
        //用于射线计算的参数
        //射线显示起始点
        Vector3 m_StartPosition,
            //射线方向
            m_Direction,
            //当前射线终点
            m_CurrentTargetPoint;

        //当前射线打到物体结果
        bool m_PhysicalHitResult;
        RaycastHit m_HitInfo;
        //当前射线打到的有效交互物体
        RayPointerHandler m_CurrentTarget;
        RayPointerHandler m_TargetInInteraction;

        CursorInfo m_CursorInfo;
        //左/右手信息
        HandTrackingPlugin.HandInfo m_HandInfo;
        HandTrackingPlugin.HandType m_HandType;
        //当前手指捏合状态
        bool m_IsPinched = false;
        //手是否被检测到，若不是，则隐藏射线
        bool m_IsHandDetected = false;
        //交互初始化状态
        bool m_IsActive = false;
        bool m_PriorityEnabled = false;

        int m_AlgorithmCounterIn = 0;
        int m_AlgorithmCounterOut = 0;

        InteractionPriority m_Priority = InteractionPriority.RayInteraction;
        HandInteractionType m_Type = HandInteractionType.RayInteraction;

        Transform m_IndexRoot, m_ThumbRoot;
        LineRenderer m_PointerLine;

        bool m_LockCursor;
        Vector3 m_RelativeCursorPositionOnPinch;
        Vector3 m_CursorPosition;
        Vector3 CursorPosition
        {
            get { return m_CursorPosition; }
        }

        bool m_IsHandMenuOpened = false;
        bool m_PalmForwardStatus = true;
        bool m_PinchEventStatus = false;
        int m_RaycastLayer = ~Physics.IgnoreRaycastLayer;
        Coroutine m_InitRoutine;
        #endregion

        #region K-filter
        /// <summary>
        /// K-filter Pointer Module
        /// </summary>
        KFilter m_KFilter;
        BezierCurve m_BezierCurve;
        Vector3 m_KStartPosition;
        float m_RayLength = 2.2f;
        #endregion

        #region IK Arm Restriction
        Transform m_IShoulder;
        Transform m_ControllerPivot;
        #endregion

        void OnValidate()
        {
#if UNITY_EDITOR
            if (useLaserLine)
            {
                if (gameObject.GetComponent<LineRenderer>() == null)
                {
                    ApplyLaserPresets applyPresets = gameObject.AddComponent<ApplyLaserPresets>();
                    LineRenderer line = gameObject.AddComponent<LineRenderer>();
                    applyPresets.ApplyLaserVis(line);

                    if (applyPresets != null)
                    {
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            DestroyImmediate(applyPresets);
                        };
                    }

                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Add LineRenderer component for laser. If you want to remove it, turn off UseLaserLine in RayHandPointer.");
                }
            }
            else
            {
                if (gameObject.GetComponent<LineRenderer>() != null)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(gameObject.GetComponent<LineRenderer>());
                    };
                }
            }
#endif
        }

        //需要在手势初始化之后进行
        IEnumerator Start()
        {
            if (m_HandController == null)
                m_HandController = GetComponent<HandController>();

            if (m_HandController == null)
            {
                Debug.LogError("handController Reference Null!");
                yield break;
            }
            yield return StartCoroutine(InitPointer());

            m_CurrentTarget = null;
            m_TargetInInteraction = null;

            HandController.onActiveHandChanged += OnHandStyleChange;

            if (HandTrackingPlugin.instance != null)
            {
                HandTrackingPlugin.instance.onHandDataUpdated += UpdateHandInfo;
            }

            if (PointerManager.instance != null)
            {
                PointerManager.instance.onHandMenuChanged += UpdateHandMenuStatus;
            }
            onActionEvent.AddListener(UpdatePinchEventStatus);
        }

        void LateUpdate()
        {
            if (!m_IsActive)
                return;
            UpdatePointer();
            UpdatePointerEvent();
        }

        void OnHandStyleChange(HandController controller)
        {
            if (controller == m_HandController)
            {
                if (m_InitRoutine != null) StopCoroutine(m_InitRoutine);
                Reset();
                m_IsActive = false;
                m_InitRoutine = StartCoroutine(InitFingerJoints());
            }
        }

        //初始化远端交互射线和手部信息
        IEnumerator InitPointer()
        {
            if (HandTrackingPlugin.instance == null)
            {
                Debug.LogError("NO SDK INIT YET");
                yield break;
            }
            else
            {
                if (useLaserLine)
                {
                    if (gameObject.GetComponent<LineRenderer>() != null)
                    {
                        m_PointerLine = gameObject.GetComponent<LineRenderer>();
                    }
                    else
                    {
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("No laser found for ray interaction, disable line display");
                        useLaserLine = false;
                    }
                }

                if (m_PointerLine != null)
                {
                    m_PointerLine.enabled = false;

                    if (gameObject.GetComponent<BezierCurve>() == null)
                    {
                        gameObject.AddComponent<BezierCurve>();
                    }
                    m_BezierCurve = gameObject.GetComponent<BezierCurve>();
                }

                if (m_HandController == HandTrackingPlugin.instance.rightHandController)
                {
                    m_HandType = HandTrackingPlugin.HandType.RightHand;

                    yield return new WaitUntil(() => m_HandController.activeHand != null);
                    yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.Wrist] != null);
                    m_ControllerPivot = m_HandController.activeHand.joints[(int)HandJointID.Wrist];

                    yield return new WaitUntil(() => PoseManager.Instance != null);
                    PoseManager.Instance.m_RightHand.m_TrackingTarget = m_ControllerPivot;
                    yield return new WaitUntil(() => PoseManager.Instance.m_RShoulder != null);
                    m_IShoulder = PoseManager.Instance.m_RShoulder;

                    // PoseManager.Instance.m_RightHand.m_NullTarget = null;
                }
                else if (m_HandController == HandTrackingPlugin.instance.leftHandController)
                {
                    m_HandType = HandTrackingPlugin.HandType.LeftHand;

                    yield return new WaitUntil(() => m_HandController.activeHand != null);
                    yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.Wrist] != null);
                    m_ControllerPivot = m_HandController.activeHand.joints[(int)HandJointID.Wrist];

                    yield return new WaitUntil(() => PoseManager.Instance != null);
                    PoseManager.Instance.m_LeftHand.m_TrackingTarget = m_ControllerPivot;
                    yield return new WaitUntil(() => PoseManager.Instance.m_LShoulder != null);
                    m_IShoulder = PoseManager.Instance.m_LShoulder;
                    
                    // PoseManager.Instance.m_LeftHand.m_NullTarget = null;
                }
                else
                {
                    yield break;
                }
                yield return m_InitRoutine = StartCoroutine(InitFingerJoints());
            }


            if (gameObject.GetComponent<KFilter>() == null)
            {
                m_KFilter = gameObject.AddComponent<KFilter>();
            }
            else
            {
                m_KFilter = gameObject.GetComponent<KFilter>();
            }

            if (m_KFilter != null)
                m_KFilter.SetSmoothLevel(PointerManager.instance.smoothLevel);

            if (cursor == null)
            {
                GameObject tempC = Resources.Load("Prefabs/DefaultCursor") as GameObject;
                cursor = Instantiate(tempC, transform);
                tempC = null;
                Resources.UnloadUnusedAssets();
                if (HandTrackingPlugin.debugLevel > 0) Debug.Log("No cursor found for ray interaction，use default cursor");
            }

            if (cursor.GetComponent<CursorInfo>() != null)
            {
                m_CursorInfo = cursor.GetComponent<CursorInfo>();
            }
            else
            {
                m_CursorInfo = cursor.AddComponent<CursorInfo>();
            }
            m_CursorInfo.Init(this);
            cursor.SetActive(false);

        }

        //初始化当前手节点
        IEnumerator InitFingerJoints()
        {
            yield return new WaitUntil(() => m_HandController.activeHand != null);

            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.IndexNode1] != null);
            m_IndexRoot = m_HandController.activeHand.joints[(int)HandJointID.IndexNode1];

            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.ThumbNode2] != null);
            m_ThumbRoot = m_HandController.activeHand.joints[(int)HandJointID.ThumbNode2];

            m_IsActive = true;
            m_InitRoutine = null;
        }

        void UpdateHandMenuStatus(bool status)
        {
            if (m_IsHandMenuOpened != status)
            {
                m_IsHandMenuOpened = status;
                if (m_IsHandMenuOpened)
                {
                    if (m_IsEnabled)
                    {
                        ResetTarget();
                    }
                    else
                    {
                        Reset();
                    }
                    m_RaycastLayer = 1 << 15;
                    if (m_PointerLine != null)
                        m_PointerLine.colorGradient = menuNormalLaserCol;
                }
                else
                {
                    if (m_IsEnabled)
                    {
                        ResetTarget();
                    }
                    else
                    {
                        Reset();
                    }
                    m_RaycastLayer = ~Physics.IgnoreRaycastLayer;
                    if (m_PointerLine != null)
                        m_PointerLine.colorGradient = normalLaserCol;
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

            if (!m_PalmForwardStatus)
            {
                if (m_PinchEventStatus)
                {
                    onActionEvent?.Invoke(PointerEventType.OnPinchUp, m_CurrentTarget == null ? null : m_CurrentTarget.gameObject);
                    ResetTarget();
                    m_AlgorithmCounterOut = 0;
                    
                }
                return;
            }

            m_HandInfo = info;
            
            if(m_HandInfo.staticGesture == HandTrackingPlugin.StaticGesture.OK)
            {
                m_AlgorithmCounterOut = 0;
                if (!m_IsPinched)
                {
                    if (m_AlgorithmCounterIn < 3)
                        m_AlgorithmCounterIn += 1;
                    else
                    {
                        onActionEvent?.Invoke(PointerEventType.OnPinchDown, m_CurrentTarget == null ? null : m_CurrentTarget.gameObject);
                        m_IsPinched = true;
                        m_AlgorithmCounterIn = 0;
                        
                        if (!m_IsEnabled && !m_IsHandMenuOpened)
                            return;
                        if (!m_PriorityEnabled)
                            return;

                        Vector3 dir = m_KStartPosition - m_IShoulder.position;
                        UpdatePointerCurve(m_StartPosition, dir, m_CurrentTargetPoint);

                        if (m_CurrentTarget != null)
                        {
                            if (m_CurrentTarget.isInInteraction)
                                return;

                            //m_CurrentTarget.OnPinchDown(m_KStartPosition, m_Direction, m_CurrentTargetPoint);
                            m_CurrentTarget.OnPinchDown(m_IShoulder.position, m_KStartPosition, m_Direction, m_CurrentTargetPoint);
                            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + m_CurrentTarget.gameObject.name + " ---> OnPinchDown");

                            if (m_CurrentTarget != null)
                                m_LockCursor = m_CurrentTarget.isLockCursor;
                            else
                                m_LockCursor = false;
                            if (m_LockCursor)
                            {
                                m_RelativeCursorPositionOnPinch = m_CurrentTarget.transform.InverseTransformPoint(cursor.transform.position);
                            }
                            m_TargetInInteraction = m_CurrentTarget;
                        }
                    }
                }
                else
                {
                    if (!m_IsEnabled && !m_IsHandMenuOpened)
                        return;
                    if (!m_PriorityEnabled)
                        return;

                    if (m_TargetInInteraction != null)
                        //m_TargetInInteraction.OnDragging(m_KStartPosition, m_Direction);
                        m_TargetInInteraction.OnDragging(m_IShoulder.position, m_KStartPosition, m_Direction);
                }
            }
            if (m_HandInfo.staticGesture != HandTrackingPlugin.StaticGesture.OK)
            {
                m_AlgorithmCounterIn = 0;
                if (m_IsPinched)
                {
                    if (m_AlgorithmCounterOut < 4)
                        m_AlgorithmCounterOut += 1;
                    else
                    {
                        onActionEvent?.Invoke(PointerEventType.OnPinchUp, m_CurrentTarget == null ? null : m_CurrentTarget.gameObject);
                        m_IsPinched = false;
                        m_AlgorithmCounterOut = 0;

                        if (!m_IsEnabled && !m_IsHandMenuOpened)
                            return;
                        if (!m_PriorityEnabled)
                            return;

                        if (m_TargetInInteraction != null)
                        {
                            m_TargetInInteraction.OnPinchUp();
                            m_TargetInInteraction = null;
                            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + m_CurrentTarget.gameObject.name + " ---> OnPinchUp");
                        }

                        if (m_LockCursor)
                        {
                            m_LockCursor = false;
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

                    if (m_IsEnabled || m_IsHandMenuOpened)
                    {
                        if (PointerManager.instance != null)
                        {
                            int res = PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, false);
                            if (res < 0)
                            {
                                return;
                            }
                        }
                        //m_PointerLine.enabled = true;
                    }
                }
            }
            else
            {
                if (m_IsHandDetected)
                {
                    m_IsHandDetected = false;

                    if (m_IsEnabled || m_IsHandMenuOpened)
                    {
                        SetVisualizationItems(false);

                        if (PointerManager.instance != null)
                        {
                            PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
                        }
                        ResetTarget();
                    }
                    if (m_PointerLine != null)
                        m_PointerLine.enabled = false;
                    m_PalmForwardStatus = false;
                }
            }
        }

        void UpdatePalmDirection()
        {
            bool palmForwardCheck = true;

            if(CustomizedGestureController.instance != null)
            {
                palmForwardCheck = CustomizedGestureController.instance.GetHandStatus(m_HandType, CustomizedGesture.PalmForward);
            }

            if(m_PalmForwardStatus != palmForwardCheck)
            {
                if(m_PalmForwardStatus && !palmForwardCheck)
                {
                    if (m_IsEnabled || m_IsHandMenuOpened)
                    {
                        SetVisualizationItems(false);

                        if (PointerManager.instance != null)
                        {
                            PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
                        }
                        ResetTarget();
                    }
                    if (m_PointerLine != null)
                        m_PointerLine.enabled = false;
                }
                else
                {
                    if (m_IsEnabled || m_IsHandMenuOpened)
                    {
                        if (PointerManager.instance != null)
                        {
                            int res = PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, false);
                            if (res < 0)
                            {
                                return;
                            }
                        }
                        if(m_PointerLine != null)
                            m_PointerLine.enabled = true;
                        if (cursor != null && !cursor.activeSelf)
                        {
                            cursor.SetActive(true);
                        }
                    }
                }
                m_PalmForwardStatus = palmForwardCheck;
            }
        }

        void UpdatePointer()
        {
            if (!m_IsEnabled && !m_IsHandMenuOpened)
            {
                return;
            }

            if (!m_IsHandDetected)
            {
                return;
            }

            if (!m_PalmForwardStatus)
            {
                return;
            }

            Vector3 shoulderPosition;
            shoulderPosition = m_IShoulder.position;

            if (m_IndexRoot == null && m_ThumbRoot == null)
            {
                return;
            }
            m_StartPosition = (m_IndexRoot.position + m_ThumbRoot.position) / 2;

            m_KStartPosition = m_KFilter.KUpdate(m_StartPosition);
            m_Direction = Vector3.Normalize(m_KStartPosition - shoulderPosition);
            m_PhysicalHitResult = Physics.Raycast(m_KStartPosition, m_Direction, out m_HitInfo, float.PositiveInfinity, m_RaycastLayer);

            if (!m_PriorityEnabled)
            {
                return;
            }

            Vector3 cursorNormal;
            if (m_LockCursor)
            {
                if (m_CurrentTarget != null)
                    m_CursorPosition = m_CurrentTarget.transform.TransformPoint(m_RelativeCursorPositionOnPinch);
                else
                    m_CursorPosition = m_KStartPosition + m_Direction * m_RayLength;
            }
            else
            {
                if (m_PhysicalHitResult)
                {
                    m_CursorPosition = m_HitInfo.point;
                    m_RayLength = Vector3.Distance(m_KStartPosition, m_CurrentTargetPoint);
                }
                else
                {
                    if (m_RayLength < 1.2f)
                    {
                        m_RayLength = Mathf.Lerp(m_RayLength, 1.2f, 0.1f);
                    }

                    m_CursorPosition = m_KStartPosition + m_Direction * m_RayLength;
                }
            }
            cursorNormal = m_Direction * (-1);
            m_CurrentTargetPoint = m_CursorPosition;
            Vector3 dir = m_KStartPosition - shoulderPosition;

            if (m_HandController.hidHandMode)
            {
                if (m_PointerLine != null)
                {
                    if(m_PointerLine.enabled == false)
                    {
                        m_PointerLine.enabled = true;
                    }

                    UpdatePointerCurve(m_StartPosition, dir, m_CurrentTargetPoint);

                    if (!m_IsHandMenuOpened)
                    {
                        if (m_IsPinched)
                        {
                            m_PointerLine.colorGradient = pinchedLaserCol;
                        }
                        else
                        {
                            m_PointerLine.colorGradient = normalLaserCol;
                        }
                    }
                    else
                    {
                        if (m_IsPinched)
                        {
                            m_PointerLine.colorGradient = menuPinchLaserCol;
                        }
                        else
                        {
                            m_PointerLine.colorGradient = menuNormalLaserCol;
                        }
                    }
                }
            }
            else
            {
                if (m_PointerLine != null)
                {
                    if (m_IsPinched)
                    {
                        UpdatePointerCurve(m_StartPosition, dir, m_CurrentTargetPoint);
                        m_PointerLine.enabled = true;
                    }
                    else
                    {
                        m_PointerLine.enabled = false;
                    }
                }
            }

            UpdateCursor(m_CursorPosition, cursorNormal);
        }

        //更新射线事件
        void UpdatePointerEvent()
        {
            if (!m_IsEnabled && !m_IsHandMenuOpened)
                return;

            if (m_IsPinched)
                return;

            RayPointerHandler newTarget = null;
            if (m_PhysicalHitResult && m_HitInfo.transform != null && m_HitInfo.transform.GetComponent<RayPointerHandler>() != null)
            {
                newTarget = m_HitInfo.transform.GetComponent<RayPointerHandler>();
            }

            if (!m_PriorityEnabled)
                return;

            if (newTarget == null)
            {
                if (m_CurrentTarget != null)
                {
                    m_CurrentTarget.OnPointerExit();
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("PreviousTarget: " + m_CurrentTarget.name + " ---> OnPointerExit");
                    onActionEvent?.Invoke(PointerEventType.OnPointerEnd, m_CurrentTarget.gameObject);
                }
            }
            else
            {
                if (m_CurrentTarget != null)
                {
                    if (newTarget != m_CurrentTarget)
                    {
                        m_CurrentTarget.OnPointerExit();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("PreviousTarget: " + m_CurrentTarget.name + " ---> OnPointerExit");
                        onActionEvent?.Invoke(PointerEventType.OnPointerEnd, m_CurrentTarget.gameObject);

                        newTarget.OnPointerEnter();
                        if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + newTarget.gameObject.name + " ---> OnPointerEnter");
                        onActionEvent?.Invoke(PointerEventType.OnPointerStart, newTarget.gameObject);
                    }
                }
                else
                {
                    newTarget.OnPointerEnter();
                    if (HandTrackingPlugin.debugLevel > 0) Debug.Log("CurrentTarget: " + newTarget.gameObject.name + " ---> OnPointerEnter");
                    onActionEvent?.Invoke(PointerEventType.OnPointerStart, newTarget.gameObject);
                }
            }
            m_CurrentTarget = newTarget;
        }

        //更新cursor信息
        void UpdateCursor(Vector3 cPosition, Vector3 cNormal)
        {
            if (m_CursorInfo != null)
            {
                m_CursorInfo.UpdateTransform(cPosition, Quaternion.LookRotation(cNormal));
            }
        }

        void UpdatePointerCurve(Vector3 start, Vector3 dir, Vector3 end)
        {
            if (m_BezierCurve == null)
            {
                if (HandTrackingPlugin.debugLevel > 1) Debug.Log("Unble to calculate curver laser");
                return;
            }
            float length = Vector3.Distance(start, end);
            Vector3 pt1 = start + dir.normalized * length / 3;
            Vector3 pt2 = start + dir.normalized * (length / 3) * 2;

            m_BezierCurve.SetControlPoints(start, pt1, pt2, end);
        }

        void SetVisualizationItems(bool res)
        {
            if (cursor != null)
            {
                if (!m_PalmForwardStatus)
                    cursor.SetActive(false);
                else
                    cursor.SetActive(res);
            }
            if (m_PointerLine != null)
            {
                if (!m_PalmForwardStatus)
                {
                    m_PointerLine.enabled = false;
                }
                else
                {
                    m_PointerLine.enabled = res;
                }
            }
            if (!res && m_CursorInfo != null)
            {
                m_CursorInfo.Reset();
            }
        }

        //识别消失时重置手势信息
        void ResetTarget()
        {
            if (m_IsPinched)
            {
                if (m_TargetInInteraction != null)
                {
                    m_TargetInInteraction.OnPinchUp();
                    m_TargetInInteraction = null;
                }

                if (m_LockCursor)
                {
                    m_LockCursor = false;
                }
            }

            if (m_CurrentTarget != null)
            {
                m_CurrentTarget.OnPointerExit();
                onActionEvent?.Invoke(PointerEventType.OnPointerEnd, m_CurrentTarget.gameObject);
                m_CurrentTarget = null;
            }

            m_PhysicalHitResult = false;
            m_HitInfo = new RaycastHit();
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Hand Lost, Reset ray interaction target");
        }

        //强制重置射线交互
        void Reset()
        {
            if (PointerManager.instance != null)
            {
                PointerManager.instance.UpdatePriorityInteraction(m_HandType, m_Type, m_Priority, true);
            }

            ResetTarget();

            SetVisualizationItems(false);

            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Reset ray hand pointer");
        }

        /// <summary>
        /// Update ray interaction priority when available interaction is updated. <br>
        /// 在可用交互方式有更新时进行交互优先级更新。
        /// </summary>
        public void UpdatePriority(bool isHigher)
        {
            if (!isHigher)
            {
                ResetTarget();
            }
            SetVisualizationItems(isHigher);
            m_PriorityEnabled = isHigher;
        }

        void UpdatePinchEventStatus(PointerEventType pinchevent, GameObject obj)
        {
            if (pinchevent == PointerEventType.OnPinchDown)
            {
                m_PinchEventStatus = true;
            }
            else if (pinchevent == PointerEventType.OnPinchUp)
            {
                m_PinchEventStatus = false;
            }
        }
    }
}
