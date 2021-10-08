using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for bounding box interaction. <br>
    /// 包围盒交互的类。
    /// </summary>
    public class BoundingBox : MonoBehaviour
    {
        #region Public Field
        /// <summary>
        /// The activation type of bounding box. <br>
        /// 包围盒激活方式。
        /// </summary>
        public BoundingBoxActivation activationType;
        
        /// <summary>
        /// The available interactions of bounding box. <br>
        /// 包围盒可用交互方式。
        /// </summary>
        public BoundsAvailableAction availableInteraction = BoundsAvailableAction.All;

        /// <summary>
        /// The timer threshold before bounding box hiden for ActivateByProximityAndPoint type. <br>
        /// 在ActivateByProximityAndPoint类型下，包围盒在不被交互状态下的隐藏时长阈值。<br>
        /// If activationType is not ActivateByProximityAndPoint, hideThreshold value doesn't affect the result. <br>
        /// 如果activationType不是 ActivateByProximityAndPoint 的类型，hideThreshold 数值不用于计算。
        /// </summary>
        [Min(0)]
        public float hideThreshold = 3f;

        /// <summary>
        /// The boxCollider used to initialize the bouding box. The box collider should be on the target object which use this bounding box to interact with. Leave it blank if using the boxCollider on current object to calculate. <br>
        /// 用于初始化包围盒的boxCollider。boxCollider需要挂在当前包围盒的目标交互物体上。如果想使用另一物体boxCollider的话在此处设置，如果使用物体自身boxCollider计算则预留此处为空。
        /// </summary>
        public BoxCollider boundsOverride;

        /// <summary>
        /// The object used for moving the bounding box, leave it blank if using the same object with boundsOverride object. <br>
        /// 用于包围盒拖拽移动交互的物体，如果为boundsOverride物体则预留此处为空。
        /// </summary>
        public GameObject translateOverride;

        /// <summary>
        /// The prefab used to initializing bounding box corner to make rescaling interaction, leave it blank if using the default corner. <br>
        /// 用于初始化包围盒角点控制缩放的预制件，如果使用默认角点则预留此处为空。
        /// </summary>
        public GameObject scalePrefab;

        /// <summary>
        /// The prefab used to initializing bounding box edge to make rotation interaction, leave it blank if using the default edge. <br>
        /// 用于初始化包围盒边缘控制旋转的预制件，如果使用默认边缘则预留此处为空。
        /// </summary>
        public GameObject rotatePrefab;

        /// <summary>
        /// Whether the object is a flat object. If yes (such as slate), it only has X and Y control. <br>
        /// 包围盒控制物体是否为平面物体。如果是的话（如面板）则只有X, Y轴交互控制。
        /// </summary>
        public bool isFlat = false;

        /// <summary>
        /// The max multiple value and min multiple value of the object rescaling based on the object start size. <br>
        /// 物体缩放相较于初始大小的最大倍数，最小倍数。
        /// </summary>
        public float scaleMin = 0.25f, scaleMax = 2f;

        /// <summary>
        /// Callback event for when the object starts to move. <br>
        /// 当物体开始移动的回调事件。
        /// </summary>
        public UnityEvent onTranslateStart;

        /// <summary>
        /// Callback event during the object is moving. <br>
        /// 当物体正在移动的回调事件。
        /// </summary>
        public UnityEvent onTranslateUpdate;

        /// <summary>
        /// Callback event for when the object ends to move. <br>
        /// 当物体结束移动时的回调事件。
        /// </summary>
        public UnityEvent onTranslateEnd;

        /// <summary>
        /// Callback event for when the object starts to rotate. <br>
        /// 当物体开始旋转的回调事件。
        /// </summary>
        public UnityEvent onRotateStart;

        /// <summary>
        /// Callback event during the object is rotating. <br>
        /// 当物体正在旋转的回调事件。
        /// </summary>
        public UnityEvent onRotateUpdate;

        /// <summary>
        /// Callback event for when the object ends to rotate. <br>
        /// 当物体结束旋转时的回调事件。
        /// </summary>
        public UnityEvent onRotateEnd;

        /// <summary>
        /// Callback event for when the object starts to rescale. <br>
        /// 当物体开始缩放的回调事件。
        /// </summary>
        public UnityEvent onScaleStart;

        /// <summary>
        /// Callback event during the object is scaling. <br>
        /// 当物体正在缩放的回调事件。
        /// </summary>
        public UnityEvent onScaleUpdate;

        /// <summary>
        /// Callback event for when the object ends to rescale. <br>
        /// 当物体结束缩放时的回调事件。
        /// </summary>
        public UnityEvent onScaleEnd;

        public Action<bool> onStatusChange;
        public Action onBoundsInitFinished;
        #endregion

        #region Private Field

        /// <summary>
        /// The target object that used for bounding box translation, leave it blank if using the same object with the one have this script. <br>
        /// 用于包围盒控制交互的目标物体，如果为当前物体自身则预留此处为空。
        /// </summary>
        Transform m_TargetOverride;

        Transform m_BoundingBoxRoot;
        GameObject[] m_Corners;
        public GameObject[] cornerObjects
        {
            get { return m_Corners; }
        }

        GameObject[] m_CornerVisulizations;
        GameObject[] m_Edges;
        public GameObject[] edgeObjects
        {
            get { return m_Edges; }
        }

        GameObject[] m_EdgeVisulizations;

        BoundsAction m_CurrentAction;
        BoundsActionAxis m_CurrentActionAxis;

        //For translation
        float m_DistanceToObject;
        //Move
        Vector3 m_OriginalPosition, m_CursorToCenter;
        //Rotation
        Quaternion m_OriginalRotation;
        Vector3 m_OriginalTowardCamera;
        Vector3 m_CurrentAxis, m_OriginalCenterToHandlerOnTargetAxis;
        Vector3 m_PreviousEndPosition, m_CurrentRotationDirection;
        //Scale
        Vector3 m_OriginalLocalScale, m_LocalScaleAtInit, m_CenterToHandlerAtBeginning, m_GlobalScaleAtInit;
        Vector2 m_CornerRescaleDirection;

        float m_TimerForFocus;
        bool m_IsFocusOn;

        #endregion

        void Start()
        {
            if (scalePrefab == null)
            {
                scalePrefab = Resources.Load("Prefabs/BoundsCorner") as GameObject;
            }
            if (rotatePrefab == null)
            {
                rotatePrefab = Resources.Load("Prefabs/BoundsEdge") as GameObject;
            }
            if (boundsOverride == null)
            {
                if (gameObject.GetComponent<BoxCollider>() != null)
                {
                    boundsOverride = gameObject.GetComponent<BoxCollider>();
                }
                else
                {
                    boundsOverride = AddBoundsToAllChildren(transform, new Vector3(1.1f, 1.1f, 1.1f));
                    if (!isFlat)
                    {
                        float maxEdgeLength = Mathf.Max(Mathf.Max(boundsOverride.size.x, boundsOverride.size.y), boundsOverride.size.z);
                        float newEdgeLengthX = (maxEdgeLength > 2.0f * boundsOverride.size.x) ? 0.5f * maxEdgeLength : boundsOverride.size.x;
                        float newEdgeLengthY = (maxEdgeLength > 2.0f * boundsOverride.size.y) ? 0.5f * maxEdgeLength : boundsOverride.size.y;
                        float newEdgeLengthZ = (maxEdgeLength > 2.0f * boundsOverride.size.z) ? 0.5f * maxEdgeLength : boundsOverride.size.z;
                        boundsOverride.size = new Vector3(newEdgeLengthX, newEdgeLengthY, newEdgeLengthZ);
                    }
                }

            }

            if (isFlat)
                boundsOverride.size = new Vector3(boundsOverride.size.x, boundsOverride.size.y, 0);
            m_TargetOverride = boundsOverride.transform;
            m_LocalScaleAtInit = m_TargetOverride.localScale;
            m_GlobalScaleAtInit = m_TargetOverride.lossyScale;

            InitBounds();

            if (activationType != BoundingBoxActivation.ActivateOnStart)
            {
                m_IsFocusOn = false;
                if (m_BoundingBoxRoot != null)
                    m_BoundingBoxRoot.gameObject.SetActive(false);
            }
            else
            {
                m_IsFocusOn = true;
            }
            onBoundsInitFinished?.Invoke();
        }

        private void Update()
        {
            if (!m_IsFocusOn && m_BoundingBoxRoot != null && m_BoundingBoxRoot.gameObject.activeSelf)
            {
                if (Time.realtimeSinceStartup - m_TimerForFocus > hideThreshold)
                {
                    m_BoundingBoxRoot.gameObject.SetActive(false);
                }
            }
        }

        void InitBounds()
        {
            GameObject boundsPrefab = null, boundsVisualization = null;
            if (availableInteraction != BoundsAvailableAction.Translation)
            {
                boundsPrefab = Resources.Load("Prefabs/BoundPref") as GameObject;
                boundsVisualization = Instantiate(boundsPrefab, m_TargetOverride);
                boundsVisualization.name = "BoundsVisualization";
            }

            GameObject boundsRootObj = new GameObject("BoundingBox");
            boundsRootObj.transform.SetParent(m_TargetOverride);
            boundsRootObj.transform.localPosition = Vector3.zero;
            if (boundsVisualization != null)
                boundsVisualization.transform.localPosition = boundsOverride.center;
            boundsRootObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            if (boundsVisualization != null)
                boundsVisualization.transform.localRotation = Quaternion.Euler(Vector3.zero);
            boundsRootObj.transform.localScale = Vector3.one;
            if (boundsVisualization != null)
            {
                if (!isFlat)
                    boundsVisualization.transform.localScale = boundsOverride.size;
                else
                    boundsVisualization.transform.localScale = new Vector3(boundsOverride.size.x, boundsOverride.size.y, 0f);
            }
            boundsRootObj.AddComponent<BoundsScaleController>();

            m_BoundingBoxRoot = boundsRootObj.transform;
            if (boundsVisualization != null)
                boundsVisualization.transform.SetParent(m_BoundingBoxRoot.transform);

            if (availableInteraction == BoundsAvailableAction.Translation || availableInteraction == BoundsAvailableAction.TranslationAndRescaling
                || availableInteraction == BoundsAvailableAction.TranslationAndRotation || availableInteraction == BoundsAvailableAction.All)
            {
                if (translateOverride == null)
                {
                    translateOverride = m_TargetOverride.gameObject;
                }

                BoundingBoxRayReceiverHelper receiver = translateOverride.AddComponent<BoundingBoxRayReceiverHelper>();
                receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Translate);
                BoundingBoxTouchableReceiverHelper touchableReceiver = translateOverride.AddComponent<BoundingBoxTouchableReceiverHelper>();
                touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Translate);
            }

            if (availableInteraction == BoundsAvailableAction.Rescaling || availableInteraction == BoundsAvailableAction.RotationAndRescaling
                || availableInteraction == BoundsAvailableAction.TranslationAndRescaling || availableInteraction == BoundsAvailableAction.All)
            {
                InitCorner();
            }

            if (availableInteraction == BoundsAvailableAction.Rotation || availableInteraction == BoundsAvailableAction.RotationAndRescaling
                || availableInteraction == BoundsAvailableAction.TranslationAndRotation || availableInteraction == BoundsAvailableAction.All)
            {
                InitEdge();
            }

            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().SetBoundCollider(boundsOverride);

            scalePrefab = null;
            rotatePrefab = null;
            if(boundsPrefab != null)
                boundsPrefab = null;
            Resources.UnloadUnusedAssets();
        }

        void InitCorner()
        {
            if (isFlat)
            {
                m_Corners = new GameObject[4];
                m_CornerVisulizations = new GameObject[4];
            }
            else
            {
                m_Corners = new GameObject[8];
                m_CornerVisulizations = new GameObject[8];
            }


            for (int i = 0; i < m_Corners.Length; i++)
            {
                GameObject cornerObjRoot = new GameObject("RescaleHandler_" + i);
                cornerObjRoot.transform.SetParent(m_BoundingBoxRoot, false);
                GameObject cornerObj = Instantiate(scalePrefab, cornerObjRoot.transform);

                m_Corners[i] = cornerObjRoot;
                m_CornerVisulizations[i] = cornerObj;

            }
            if (isFlat)
            {
                //x, y, 0
                m_Corners[0].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, boundsOverride.size.y, 0) * 0.5f;
                m_CornerVisulizations[0].transform.rotation = Quaternion.LookRotation(-transform.up, -transform.forward);
                //x, -y, 0
                m_Corners[1].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, -boundsOverride.size.y, 0) * 0.5f;
                m_CornerVisulizations[1].transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up);
                //-x, y, 0
                m_Corners[2].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, boundsOverride.size.y, 0) * 0.5f;
                m_CornerVisulizations[2].transform.rotation = Quaternion.LookRotation(-transform.forward, -transform.up);
                //-x, -y, 0
                m_Corners[3].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, -boundsOverride.size.y, 0) * 0.5f;
                m_CornerVisulizations[3].transform.rotation = Quaternion.LookRotation(transform.up, -transform.forward);
            }
            else
            {
                //x, y, z
                m_Corners[0].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, boundsOverride.size.y, boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[0].transform.rotation = Quaternion.LookRotation(-transform.up, -transform.forward);
                //x, y, -z
                m_Corners[1].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, boundsOverride.size.y, -boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[1].transform.rotation = Quaternion.LookRotation(transform.forward, -transform.up);
                //x, -y, z
                m_Corners[2].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, -boundsOverride.size.y, boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[2].transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up);
                //x, -y, -z
                m_Corners[3].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, -boundsOverride.size.y, -boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[3].transform.rotation = Quaternion.LookRotation(transform.up, transform.forward);
                //-x, y, z
                m_Corners[4].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, boundsOverride.size.y, boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[4].transform.rotation = Quaternion.LookRotation(-transform.forward, -transform.up);
                //-x, y, -z
                m_Corners[5].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, boundsOverride.size.y, -boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[5].transform.rotation = Quaternion.LookRotation(-transform.up, transform.forward);
                //-x, -y, z
                m_Corners[6].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, -boundsOverride.size.y, boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[6].transform.rotation = Quaternion.LookRotation(transform.up, -transform.forward);
                //-x, -y, -z
                m_Corners[7].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, -boundsOverride.size.y, -boundsOverride.size.z) * 0.5f;
                m_CornerVisulizations[7].transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
            }
            for (int i = 0; i < m_Corners.Length; i++)
            {
                if (m_Corners[i].GetComponent<Collider>() == null)
                {
                    AddBoundsToAllChildren(m_Corners[i].transform, new Vector3(2f, 2f, 2f));
                }
                BoundingBoxRayReceiverHelper receiver = m_Corners[i].AddComponent<BoundingBoxRayReceiverHelper>();
                receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Scale);
                BoundingBoxTouchableReceiverHelper touchableReceiver = m_Corners[i].AddComponent<BoundingBoxTouchableReceiverHelper>();
                touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Scale);

                UnScale unscaleCorner = m_Corners[i].AddComponent<UnScale>();
                unscaleCorner.Init();
                unscaleCorner.SetMaxModelScaleRatio(2f);
                unscaleCorner.SetMinWrapperScaleRatio(scaleMin);
                m_BoundingBoxRoot.GetComponent<BoundsScaleController>().AddCorner(unscaleCorner); 
            }
        }

        void InitEdge()
        {
            if (isFlat)
            {
                m_Edges = new GameObject[4];
                m_EdgeVisulizations = new GameObject[4];
            }
            else
            {
                m_Edges = new GameObject[12];
                m_EdgeVisulizations = new GameObject[12];
            }

            for (int i = 0; i < m_Edges.Length; i++)
            {
                GameObject edgeObjRoot = new GameObject("RotateHandler_" + i);
                edgeObjRoot.transform.SetParent(m_BoundingBoxRoot, false);
                GameObject edgeObj = Instantiate(rotatePrefab, edgeObjRoot.transform);
                m_Edges[i] = edgeObjRoot;
                m_EdgeVisulizations[i] = edgeObj;
            }

            if (isFlat)
            {
                //X-Axis: y, 0
                m_Edges[0].transform.localPosition = boundsOverride.center + new Vector3(0, boundsOverride.size.y, 0) * 0.5f;
                m_Edges[0].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.up);
                //-y, 0
                m_Edges[1].transform.localPosition = boundsOverride.center + new Vector3(0, -boundsOverride.size.y, 0) * 0.5f;
                m_Edges[1].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.up);

                //Y-Axis: x, 0
                m_Edges[2].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, 0, 0) * 0.5f;
                m_Edges[2].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.right);
                //-x, 0
                m_Edges[3].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, 0, 0) * 0.5f;
                m_Edges[3].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.right);
            }
            else
            {
                //X-Axis: y, z
                m_Edges[0].transform.localPosition = boundsOverride.center + new Vector3(0, boundsOverride.size.y, boundsOverride.size.z) * 0.5f;
                m_Edges[0].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.up);
                //y, -z
                m_Edges[1].transform.localPosition = boundsOverride.center + new Vector3(0, boundsOverride.size.y, -boundsOverride.size.z) * 0.5f;
                m_Edges[1].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.up);
                //-y, z
                m_Edges[2].transform.localPosition = boundsOverride.center + new Vector3(0, -boundsOverride.size.y, boundsOverride.size.z) * 0.5f;
                m_Edges[2].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.up);
                //-y, -z
                m_Edges[3].transform.localPosition = boundsOverride.center + new Vector3(0, -boundsOverride.size.y, -boundsOverride.size.z) * 0.5f;
                m_Edges[3].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.up);

                //Y-Axis: x, z
                m_Edges[4].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, 0, boundsOverride.size.z) * 0.5f;
                m_Edges[4].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.right);
                //x, -z
                m_Edges[5].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, 0, -boundsOverride.size.z) * 0.5f;
                m_Edges[5].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.right);
                //-x, z
                m_Edges[6].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, 0, boundsOverride.size.z) * 0.5f;
                m_Edges[6].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.right);
                //-x, -z
                m_Edges[7].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, 0, -boundsOverride.size.z) * 0.5f;
                m_Edges[7].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.forward, m_BoundingBoxRoot.right);

                //Z-Axis: x, y
                m_Edges[8].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, boundsOverride.size.y, 0) * 0.5f;
                m_Edges[8].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.right, m_BoundingBoxRoot.up);
                //x, -y
                m_Edges[9].transform.localPosition = boundsOverride.center + new Vector3(boundsOverride.size.x, -boundsOverride.size.y, 0) * 0.5f;
                m_Edges[9].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.right, m_BoundingBoxRoot.up);
                //-x, y
                m_Edges[10].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, boundsOverride.size.y, 0) * 0.5f;
                m_Edges[10].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.right, m_BoundingBoxRoot.up);
                //-x, -y
                m_Edges[11].transform.localPosition = boundsOverride.center + new Vector3(-boundsOverride.size.x, -boundsOverride.size.y, 0) * 0.5f;
                m_Edges[11].transform.rotation = Quaternion.LookRotation(m_BoundingBoxRoot.right, m_BoundingBoxRoot.up);
            }


            for (int i = 0; i < m_Edges.Length; i++)
            {
                if (m_Edges[i].GetComponent<Collider>() == null)
                {
                    AddBoundsToAllChildren(m_Edges[i].transform, new Vector3(4f, 4f, 4f));
                }
                BoundingBoxRayReceiverHelper receiver = m_Edges[i].AddComponent<BoundingBoxRayReceiverHelper>();
                BoundingBoxTouchableReceiverHelper touchableReceiver = m_Edges[i].AddComponent<BoundingBoxTouchableReceiverHelper>();

                if (isFlat)
                {
                    if (i < 2)
                    {
                        receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.X);
                        touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.X);
                    }
                    else if (i < 4)
                    {
                        receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.Y);
                        touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.Y);
                    }
                }
                else
                {
                    if (i < 4)
                    {
                        receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.X);
                        touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.X);
                    }
                    else if (i < 8)
                    {
                        receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.Y);
                        touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.Y);
                    }
                    else
                    {
                        receiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.Z);
                        touchableReceiver.Init(m_BoundingBoxRoot, this, BoundsAction.Rotate, BoundsActionAxis.Z);
                    }
                }
                UnScale unscaleEdge = m_Edges[i].AddComponent<UnScale>();
                unscaleEdge.Init();
                unscaleEdge.SetMaxModelScaleRatio(4f);
                unscaleEdge.SetMinWrapperScaleRatio(scaleMin);
                m_BoundingBoxRoot.GetComponent<BoundsScaleController>().AddEdge(unscaleEdge); 
            }
        }

        /// <summary>
        /// Starts bounding box ray interaction. <br>
        /// 开始bounding box远端交互。
        /// </summary>
        /// <param name="action">The action to start. <br>开始交互的类型.</param>
        /// <param name="axis">Current interaction axis. <br>当前交互轴.</param>
        /// <param name="startPoint">Start point of ray in far interaction. <br>远端射线起点位置.</param>
        /// <param name="direction">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="endPosition">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public void StartRayAction(BoundsAction action, BoundsActionAxis axis = BoundsActionAxis.None,
            Vector3 startPoint = default(Vector3), Vector3 direction = default(Vector3), Vector3 endPosition = default(Vector3))
        {
            m_CurrentAction = action;
            m_CurrentActionAxis = axis;
            m_DistanceToObject = Vector3.Distance(startPoint, endPosition);
            switch (m_CurrentAction)
            {
                case BoundsAction.Translate:
                    m_OriginalPosition = m_TargetOverride.position;
                    m_CursorToCenter = m_OriginalPosition - endPosition;
                    m_OriginalTowardCamera = Vector3.ProjectOnPlane(Vector3.Normalize(CenterCamera.centerCamera.transform.position - m_TargetOverride.position), Vector3.up);
                    onTranslateStart?.Invoke();
                    break;

                case BoundsAction.Rotate:
                    switch (m_CurrentActionAxis)
                    {
                        case BoundsActionAxis.X:
                            m_CurrentAxis = m_TargetOverride.right;
                            break;
                        case BoundsActionAxis.Y:
                            m_CurrentAxis = m_TargetOverride.up;
                            break;
                        case BoundsActionAxis.Z:
                            m_CurrentAxis = m_TargetOverride.forward;
                            break;
                        default:
                            break;
                    }
                    m_OriginalCenterToHandlerOnTargetAxis = Vector3.ProjectOnPlane(endPosition - m_TargetOverride.position, m_CurrentAxis).normalized;
                    m_PreviousEndPosition = endPosition;
                    m_CurrentRotationDirection = Vector3.Cross(Vector3.forward, m_CurrentAxis);
                    m_OriginalRotation = m_TargetOverride.rotation;
                    onRotateStart?.Invoke();
                    break;
                case BoundsAction.Scale:
                    m_OriginalLocalScale = m_TargetOverride.localScale;
                    Vector3 pointOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(endPosition);
                    Vector3 objectOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(m_TargetOverride.position);
                    m_CornerRescaleDirection = new Vector2(Mathf.Sign((pointOnCamera - objectOnCamera).x), Mathf.Sign((pointOnCamera - objectOnCamera).y));
                    m_CenterToHandlerAtBeginning = pointOnCamera - objectOnCamera;

                    onScaleStart?.Invoke();
                    break;
                default:
                    break;
            }
        }
        
        /// <summary>
        /// Starts bounding box ui interaction. <br>
        /// 开始bounding box近场交互。
        /// </summary>
        /// <param name="action">The action to start. <br>开始交互的类型.</param>
        /// <param name="axis">Current interaction axis. <br>当前交互轴.</param>
        /// <param name="pinchPosition">Start pinch point. <br>近场交互捏取点.</param>
        public void StartUiAction(BoundsAction action, BoundsActionAxis axis = BoundsActionAxis.None, Vector3 pinchPosition = default(Vector3))
        {
            m_CurrentAction = action;
            m_CurrentActionAxis = axis;
            switch (m_CurrentAction)
            {
                case BoundsAction.Translate:
                    m_OriginalPosition = m_TargetOverride.position;
                    m_CursorToCenter = m_OriginalPosition - pinchPosition;
                    m_OriginalTowardCamera = Vector3.ProjectOnPlane(Vector3.Normalize(CenterCamera.centerCamera.transform.position - m_TargetOverride.position), Vector3.up);
                    onTranslateStart?.Invoke();
                    break;

                case BoundsAction.Rotate:
                    switch (m_CurrentActionAxis)
                    {
                        case BoundsActionAxis.X:
                            m_CurrentAxis = m_TargetOverride.right;
                            break;
                        case BoundsActionAxis.Y:
                            m_CurrentAxis = m_TargetOverride.up;
                            break;
                        case BoundsActionAxis.Z:
                            m_CurrentAxis = m_TargetOverride.forward;
                            break;
                        default:
                            break;
                    }
                    m_OriginalCenterToHandlerOnTargetAxis = Vector3.ProjectOnPlane(pinchPosition - m_TargetOverride.position, m_CurrentAxis).normalized;
                    m_OriginalRotation = m_TargetOverride.rotation;
                    onRotateStart?.Invoke();
                    break;
                case BoundsAction.Scale:
                    m_OriginalLocalScale = m_TargetOverride.localScale;
                    m_CenterToHandlerAtBeginning = pinchPosition - m_TargetOverride.position;
                   /* Vector3 pointOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(pinchPosition);
                    Vector3 objectOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(m_TargetOverride.position);
                    m_CornerRescaleDirection = new Vector2(Mathf.Sign((pointOnCamera - objectOnCamera).x), Mathf.Sign((pointOnCamera - objectOnCamera).y));
                    m_CenterToHandlerAtBeginning = pointOnCamera - objectOnCamera;
                   */
                    onScaleStart?.Invoke();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Ends bounding box interaction. <br>
        /// 结束bounding box交互。
        /// </summary>
        /// <param name="action">The action to end. <br>结束交互的类型.</param>
        public void EndAction(BoundsAction action)
        {
            m_DistanceToObject = 0;
            if (m_CurrentAction == action)
            {
                switch (m_CurrentAction)
                {
                    case BoundsAction.Translate:
                        onTranslateEnd?.Invoke();
                        m_OriginalPosition = default(Vector3);
                        m_CursorToCenter = default(Vector3);
                        m_OriginalTowardCamera = default(Vector3);
                        break;
                    case BoundsAction.Rotate:
                        onRotateEnd?.Invoke();
                        m_OriginalCenterToHandlerOnTargetAxis = default(Vector3);
                        m_PreviousEndPosition = default(Vector3);
                        m_CurrentAxis = default(Vector3);
                        m_OriginalRotation = default(Quaternion);
                        m_CurrentRotationDirection = default(Vector3);
                        m_BoundingBoxRoot.GetComponent<BoundsScaleController>().SetIsRotation(false);
                        break;
                    case BoundsAction.Scale:
                        onScaleEnd?.Invoke();
                        m_CenterToHandlerAtBeginning = default(Vector3);
                        m_OriginalLocalScale = default(Vector3);
                        m_CornerRescaleDirection = default(Vector2);
                        break;
                    default:
                        break;
                }
                m_CurrentAction = BoundsAction.None;
            }
        }

        /// <summary>
        /// Updates far interaction information. <br>
        /// 交互过程中，更新bounding box远端交互。
        /// </summary>
        /// <param name="startPosition">The start position of laser. <br>射线起点.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public void UpdateRayAction(Vector3 startPosition, Vector3 direction)
        {
            Vector3 endPosition;
            switch (m_CurrentAction)
            {
                case BoundsAction.Translate:
                    endPosition = startPosition + direction * m_DistanceToObject + m_CursorToCenter;
                    m_TargetOverride.position = Vector3.Lerp(m_TargetOverride.position, endPosition, 0.3f);
                    if (isFlat)
                    {
                        Vector3 objectTowardCamera = Vector3.ProjectOnPlane(Vector3.Normalize(CenterCamera.centerCamera.transform.position - m_TargetOverride.position), Vector3.up);
                        Quaternion newRot = Quaternion.FromToRotation(m_OriginalTowardCamera, objectTowardCamera);
                        m_TargetOverride.rotation = newRot * m_TargetOverride.rotation;
                        m_OriginalTowardCamera = objectTowardCamera;
                    }
                    onTranslateUpdate?.Invoke();
                    break;
                case BoundsAction.Rotate:
                    endPosition = startPosition + direction * m_DistanceToObject + m_CursorToCenter;
                    Vector3 endPositionShift = endPosition - m_PreviousEndPosition;
                    endPositionShift = new Vector3(endPositionShift.x, endPositionShift.y, endPositionShift.z);
                    Vector3 temp = CenterCamera.centerCamera.transform.InverseTransformDirection(endPositionShift);
                    temp = new Vector3(temp.x, temp.y * 1.5f, temp.z);
                    endPositionShift = CenterCamera.centerCamera.transform.TransformDirection(temp);
                    Vector3 forwardVector = CenterCamera.centerCamera.transform.forward;
                    switch (m_CurrentActionAxis)
                    {
                        case BoundsActionAxis.Y:
                            if (forwardVector == m_TargetOverride.transform.up)
                            {
                                m_CurrentRotationDirection = -CenterCamera.centerCamera.transform.up;
                            }
                            else if (forwardVector == -m_TargetOverride.transform.up)
                            {
                                m_CurrentRotationDirection = CenterCamera.centerCamera.transform.up;
                            }
                            else
                            {
                                m_CurrentRotationDirection = Vector3.Cross(forwardVector, m_TargetOverride.transform.up);
                                m_CurrentRotationDirection = m_CurrentRotationDirection.normalized;
                            }
                            m_TargetOverride.transform.Rotate(m_CurrentAxis, Vector3.Dot(m_CurrentRotationDirection, endPositionShift) * 150, Space.World);
                            break;
                        case BoundsActionAxis.X:
                            if (forwardVector == m_TargetOverride.transform.right)
                            {
                                m_CurrentRotationDirection = -CenterCamera.centerCamera.transform.up;
                            }
                            else if (forwardVector == -m_TargetOverride.transform.right)
                            {
                                m_CurrentRotationDirection = CenterCamera.centerCamera.transform.up;
                            }
                            else
                            {
                                m_CurrentRotationDirection = Vector3.Cross(forwardVector, m_TargetOverride.transform.right);
                                m_CurrentRotationDirection = m_CurrentRotationDirection.normalized;
                            }
                            m_TargetOverride.transform.Rotate(m_CurrentAxis, Vector3.Dot(m_CurrentRotationDirection, endPositionShift) * 150, Space.World);
                            break;
                        case BoundsActionAxis.Z:
                            if (forwardVector == m_TargetOverride.transform.forward)
                            {
                                m_CurrentRotationDirection = -CenterCamera.centerCamera.transform.up;
                            }
                            else if (forwardVector == -m_TargetOverride.transform.forward)
                            {
                                m_CurrentRotationDirection = CenterCamera.centerCamera.transform.up;
                            }
                            else
                            {
                                m_CurrentRotationDirection = Vector3.Cross(forwardVector, m_TargetOverride.transform.forward);
                                m_CurrentRotationDirection = m_CurrentRotationDirection.normalized;
                            }
                            m_TargetOverride.transform.Rotate(m_CurrentAxis, Vector3.Dot(m_CurrentRotationDirection, endPositionShift) * 150, Space.World);
                            break;
                    }
                    m_PreviousEndPosition = endPosition;
                    m_BoundingBoxRoot.GetComponent<BoundsScaleController>().SetIsRotation(true);
                    onRotateUpdate?.Invoke();
                    break;
                case BoundsAction.Scale:
                    endPosition = startPosition + direction * m_DistanceToObject;
                    Vector3 pointOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(endPosition);
                    Vector3 objectOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(m_TargetOverride.position);
                    Vector3 centerToHandler = pointOnCamera - objectOnCamera;

                    float xWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.x) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y));
                    float yWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.y) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y));

                    Vector2 newPositionDiff = new Vector2((centerToHandler - m_CenterToHandlerAtBeginning).x, (centerToHandler - m_CenterToHandlerAtBeginning).y);

                    float xMul = xWeight * newPositionDiff.x * m_CornerRescaleDirection.x / 1500;
                    float yMul = yWeight * newPositionDiff.y * m_CornerRescaleDirection.y / 1500;
                    float finalMulResult = (xMul + yMul) / (xWeight + yWeight);

                    float scaleSize = (finalMulResult * m_GlobalScaleAtInit.magnitude + m_OriginalLocalScale.x) / m_LocalScaleAtInit.x;

                    if (scaleSize > scaleMin && scaleSize < scaleMax)
                    {
                        Vector3 tempScale = m_LocalScaleAtInit * scaleSize;
                        if (isFlat)
                        {
                            m_TargetOverride.localScale = Vector3.Lerp(m_TargetOverride.localScale,
                                new Vector3(tempScale.x, tempScale.y, m_TargetOverride.localScale.z), 0.4f);
                            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().UpdateFlatZScale(scaleSize);
                        }

                        else
                        {
                            m_TargetOverride.localScale = Vector3.Lerp(m_TargetOverride.localScale, tempScale, 0.4f);
                        }
                    }
                    else if (scaleSize <= scaleMin)
                    {
                        Vector3 tempScale = scaleMin * m_LocalScaleAtInit;
                        if (isFlat)
                        {
                            m_TargetOverride.localScale = new Vector3(tempScale.x, tempScale.y, m_TargetOverride.localScale.z);
                            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().UpdateFlatZScale(scaleMin);
                        }
                        else
                            m_TargetOverride.localScale = tempScale;
                    }
                    else
                    {
                        Vector3 tempScale = scaleMax * m_LocalScaleAtInit;
                        if (isFlat)
                        {
                            m_TargetOverride.localScale = new Vector3(tempScale.x, tempScale.y, m_TargetOverride.localScale.z);
                            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().UpdateFlatZScale(scaleMax);
                        }
                        else
                            m_TargetOverride.localScale = tempScale;
                    }
                    onScaleUpdate?.Invoke();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates ui interaction information. <br>
        /// 交互过程中，更新bounding box近场交互。
        /// </summary>
        /// <param name="fingerPosition">Pinch position. <br>捏取位置.</param>
        public void UpdateUiAction(Vector3 fingerPosition)
        {
            Vector3 endPosition = fingerPosition + m_CursorToCenter;
            switch (m_CurrentAction)
            {
                case BoundsAction.Translate:
                    m_TargetOverride.position = endPosition;
                    if (isFlat)
                    {
                        Vector3 objectTowardCamera = Vector3.ProjectOnPlane(Vector3.Normalize(CenterCamera.centerCamera.transform.position - m_TargetOverride.position), Vector3.up);
                        Quaternion newRot = Quaternion.FromToRotation(m_OriginalTowardCamera, objectTowardCamera);
                        m_TargetOverride.rotation = newRot * m_TargetOverride.rotation;
                        m_OriginalTowardCamera = objectTowardCamera;
                    }
                    onTranslateUpdate?.Invoke();
                    break;
                case BoundsAction.Rotate:
                    Vector3 currentDir = Vector3.ProjectOnPlane(endPosition - m_TargetOverride.position, m_CurrentAxis).normalized;
                    Quaternion result = Quaternion.FromToRotation(m_OriginalCenterToHandlerOnTargetAxis, currentDir) * m_OriginalRotation;
                    m_TargetOverride.rotation = result;
                    onRotateUpdate?.Invoke();
                    break;
                case BoundsAction.Scale:
                    /*Vector3 pointOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(fingerPosition);
                    Vector3 objectOnCamera = CenterCamera.centerCamera.WorldToScreenPoint(m_TargetOverride.position);
                    Vector3 centerToHandler = pointOnCamera - objectOnCamera;

                    float xWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.x) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y));
                    float yWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.y) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y));
                    
                    Vector2 newPositionDiff = new Vector2((centerToHandler - m_CenterToHandlerAtBeginning).x, (centerToHandler - m_CenterToHandlerAtBeginning).y);

                    float xMul = xWeight * newPositionDiff.x * m_CornerRescaleDirection.x / 1500;
                    float yMul = yWeight * newPositionDiff.y * m_CornerRescaleDirection.y / 1500;
                    float finalMulResult = (xMul + yMul) / (xWeight + yWeight);

                    float scaleSize = (finalMulResult * m_GlobalScaleAtInit.magnitude + m_OriginalLocalScale.x) / m_LocalScaleAtInit.x;*/
                    Vector3 centerToHandler = fingerPosition - m_TargetOverride.position;
                    
                    float xWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.x) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y) + Mathf.Abs(m_CenterToHandlerAtBeginning.z));
                    float yWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.y) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y) + Mathf.Abs(m_CenterToHandlerAtBeginning.z));
                    float zWeight = Mathf.Abs(m_CenterToHandlerAtBeginning.z) / (Mathf.Abs(m_CenterToHandlerAtBeginning.x) + Mathf.Abs(m_CenterToHandlerAtBeginning.y) + Mathf.Abs(m_CenterToHandlerAtBeginning.z));
                    float xMul = centerToHandler.x / m_CenterToHandlerAtBeginning.x;
                    xMul = xMul * xWeight;
                    float yMul = centerToHandler.y / m_CenterToHandlerAtBeginning.y;
                    yMul = yMul * yWeight;
                    float zMul = centerToHandler.z / m_CenterToHandlerAtBeginning.z;
                    zMul = zMul * zWeight;

                    float finalMulResult = (xMul + yMul + zMul) / (xWeight + yWeight + zWeight);
                    float scaleSize = finalMulResult * m_OriginalLocalScale.x / m_LocalScaleAtInit.x;

                    if (scaleSize > scaleMin && scaleSize < scaleMax)
                    {
                        //Vector3 tempScale = m_LocalScaleAtInit * scaleSize;
                        Vector3 tempScale = m_OriginalLocalScale * finalMulResult;
                        if (isFlat)
                        {
                            m_TargetOverride.localScale = new Vector3(tempScale.x, tempScale.y, m_TargetOverride.localScale.z);
                            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().UpdateFlatZScale(scaleSize);
                        }
                        else
                            m_TargetOverride.localScale = tempScale;
                    }
                    else if (scaleSize <= scaleMin)
                    {
                        Vector3 tempScale = scaleMin * m_LocalScaleAtInit;
                        if (isFlat)
                        {
                            m_TargetOverride.localScale = new Vector3(tempScale.x, tempScale.y, m_TargetOverride.localScale.z);
                            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().UpdateFlatZScale(scaleMin);
                        }
                        else
                            m_TargetOverride.localScale = tempScale;
                    }
                    else
                    {
                        Vector3 tempScale = scaleMax * m_LocalScaleAtInit;
                        if (isFlat)
                        {
                            m_TargetOverride.localScale = new Vector3(tempScale.x, tempScale.y, m_TargetOverride.localScale.z);
                            m_BoundingBoxRoot.GetComponent<BoundsScaleController>().UpdateFlatZScale(scaleMax);
                        }
                        else
                            m_TargetOverride.localScale = tempScale;
                    }
                    onScaleUpdate?.Invoke();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates all bounding box handlers interaction status. <br>
        /// 更新包围盒所有控制点的交互状态。
        /// </summary>
        /// <param name="status">Interaction status. <br>交互状态.</param>
        public void SetAllChildrenStatus(bool status)
        {
            onStatusChange?.Invoke(status);
        }

        /// <summary>
        /// Updates all bounding box handlers focus status. <br>
        /// 更新包围盒所有控制点的被选中状态。
        /// </summary>
        /// <param name="status">Interaction status. <br>被选中状态.</param>
        public void SetFocusStatus(bool status)
        {
            if (activationType != BoundingBoxActivation.ActivateOnStart)
            {
                m_IsFocusOn = status;
                if (!status)
                    m_TimerForFocus = Time.realtimeSinceStartup;
                else
                    m_BoundingBoxRoot.gameObject.SetActive(true);
            }
        }

        BoxCollider AddBoundsToAllChildren(Transform t, Vector3 sizeRatio)
        {
            BoxCollider boxCol;
            Vector3 parentScale = t.parent == null ? new Vector3(1, 1, 1) : t.parent.lossyScale;
            Vector3 pos = t.position;
            Vector3 localScl = t.localScale;
            Quaternion rot = t.rotation;
            t.position = new Vector3(0, 0, 0);
            t.rotation = new Quaternion(0, 0, 0, 1);
            Vector3 MultipleTimes = new Vector3(1.0f / parentScale.x, 1.0f / parentScale.y, 1.0f / parentScale.z);
            t.localScale = MultipleTimes;

            if (t.gameObject.GetComponent<BoxCollider>() == null)
                boxCol = t.gameObject.AddComponent<BoxCollider>();
            else
                boxCol = t.gameObject.GetComponent<BoxCollider>();

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (t.gameObject.GetComponent<Renderer>() != null)
            {
                Renderer thisRenderer = t.GetComponent<Renderer>();
                bounds.Encapsulate(thisRenderer.bounds);
                boxCol.center = bounds.center - t.position;
                boxCol.size = new Vector3(bounds.size.x * sizeRatio.x, bounds.size.y * sizeRatio.y, bounds.size.z * sizeRatio.z);
            }
            Transform[] allDescendants = t.gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform desc in allDescendants)
            {
                if (desc.GetComponent<Renderer>() == null)
                    continue;
                Renderer childRenderer = desc.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
                boxCol.center = bounds.center - t.position;
                boxCol.size = new Vector3(bounds.size.x * sizeRatio.x, bounds.size.y * sizeRatio.y, bounds.size.z * sizeRatio.z);
            }
            t.position = pos;
            t.rotation = rot;
            t.localScale = localScl;
            return boxCol;
        }
    }
}
