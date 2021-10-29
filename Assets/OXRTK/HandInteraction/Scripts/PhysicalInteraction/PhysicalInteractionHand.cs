using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Extended of the hand sdk input, add finger tip to hand and enable finger tip to interact with object through collider use unity physics. <br>扩展手势交互，添加指尖物理碰撞交互。</br>
    /// </summary>
    public class PhysicalInteractionHand : MonoBehaviour
    {
        enum WorkWithMode
        {
            Both, SkeletonHand, ModelHand
        }

        HandController m_HandController;

        [SerializeField] bool m_IsEnabled = false;
        [Tooltip("切换场景时是否自动禁用手势物理交互。")]
        [SerializeField] bool m_DisableWhenSwitchScene = true;

        Transform m_GrabbedObjs;
        Transform m_GrabMoveRoot;
        Transform m_TriangulationP1;
        Transform m_TriangulationP2;
        Transform m_TriangulationP3;
        bool m_TriangulationReady = false;
        bool m_IsPause = false;

        List<PhysicalInteractionFingerTip> m_FingerTips = new List<PhysicalInteractionFingerTip>();

        Coroutine m_InitRoutine = null;
        void Start()
        {
            if (m_HandController == null) m_HandController = GetComponent<HandController>();
            if (m_HandController == null) { Debug.LogError("HandController Reference Null! Finger Tip interaction Disable! - Hand:[" + name + "]"); return; }

            m_InitRoutine = StartCoroutine(Init());
            HandController.onActiveHandChanged += OnHandStyleChange;

            if (PointerManager.instance != null)
            {
                PointerManager.instance.onHandMenuChanged += UpdateHandMenuStatus;
            }
        }
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (m_DisableWhenSwitchScene) isEnabled = false;
        }
        void OnHandStyleChange(HandController controller)
        {
            if (controller == m_HandController)
            {
                if (m_InitRoutine != null) StopCoroutine(m_InitRoutine);
                Reset();
                m_InitRoutine = StartCoroutine(Init());
            }
        }

        IEnumerator Init()
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("FingerTipControl Start - Hand:[" + name + "]");

            yield return new WaitUntil(() => m_HandController.activeHand != null);

            yield return new WaitUntil(() => m_HandController.activeHand.joints.Length == 21);
            yield return StartCoroutine(InitFingerTip(FingerTipJointID.Thumb));
            yield return StartCoroutine(InitFingerTip(FingerTipJointID.Index));
            yield return StartCoroutine(InitFingerTip(FingerTipJointID.Middle));
            yield return StartCoroutine(InitFingerTip(FingerTipJointID.Ring));
            yield return StartCoroutine(GetHandMoveRoot());

            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Add finger tips done! - Hand:[" + name + "]");

            if (!m_IsEnabled)
            {
                EnableFingerTips(m_IsEnabled);
            }
            m_InitRoutine = null;
        }

        IEnumerator InitFingerTip(FingerTipJointID fingerTipID)
        {
            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)fingerTipID] != null);
            PhysicalInteractionFingerTip fingerRefTp = m_HandController.activeHand.joints[(int)fingerTipID].gameObject.AddComponent<PhysicalInteractionFingerTip>();
            fingerRefTp.Init(this, fingerTipID, m_HandController.activeHand.colliderScaleFactor);
            m_FingerTips.Add(fingerRefTp);
        }

        IEnumerator GetHandMoveRoot()
        {
            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.Wrist] != null);
            m_TriangulationP1 = m_HandController.activeHand.joints[(int)HandJointID.Wrist].transform;

            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.IndexNode1] != null);
            m_TriangulationP2 = m_HandController.activeHand.joints[(int)HandJointID.IndexNode1].transform;

            yield return new WaitUntil(() => m_HandController.activeHand.joints[(int)HandJointID.MiddleNode1] != null);
            m_TriangulationP3 = m_HandController.activeHand.joints[(int)HandJointID.MiddleNode1].transform;

            if (m_GrabMoveRoot == null) m_GrabMoveRoot = new GameObject(transform.name + "-MoveRoot").transform;

            m_GrabMoveRoot.SetParent(transform);
            m_TriangulationReady = true;

            CalculateGrabMoveRootPosRot();
        }

        /// <summary>
        /// Enable disable finger tip physical interaction collider interaction. <br>开启或禁用指尖物理碰撞交互.</br>
        /// </summary>
        public bool isEnabled
        {
            get { return m_IsEnabled; }
            set { EnableSystem(value); }
        }

        /// <summary>
        /// Temporary disable interaction. <br>暂时禁止交互.</br>
        /// </summary>
        public bool isPause
        {
            get { return m_IsPause; }
            set { m_IsPause = value; PauseModule(value); }

        }

        void EnableSystem(bool isEnableTp)
        {
            if (m_IsEnabled != isEnableTp)
            {
                m_IsEnabled = isEnableTp;
                EnableFingerTips(m_IsEnabled && !isPause);
                if (!m_IsEnabled && m_GrabbedObjs != null) m_GrabbedObjs.GetComponent<PhysicalInteractionInteractable>()?.FTHReset();
            }
        }

        void PauseModule(bool value)
        {
            if (m_IsEnabled)
            {
                EnableFingerTips(!value);
                if (value && m_GrabbedObjs != null) m_GrabbedObjs.GetComponent<PhysicalInteractionInteractable>()?.FTHReset();
            }
        }

        void EnableFingerTips(bool value)
        {
            for (int i = 0; i < m_FingerTips.Count; i++)
            {
                m_FingerTips[i].IsEnabled = value;
            }
        }

        void Reset()
        {
            for (int i = 0; i < m_FingerTips.Count; i++)
            {
                Destroy(m_FingerTips[i]);
            }
            m_FingerTips.Clear();
            m_TriangulationReady = false;
            if (m_GrabbedObjs != null)
                m_GrabbedObjs.GetComponent<PhysicalInteractionInteractable>()?.FTHReset();
        }

        /// <summary>
        /// For PhysicalInteractionGrabbable to use only. <br>仅供PhysicalInteractionGrabbable使用.</br>
        /// </summary>
        public bool TryGrabMe(Transform obj)
        {
            if (m_GrabbedObjs != null)
                return false;
            else
                CalculateGrabMoveRootPosRot();
            m_GrabbedObjs = obj;
            return true;
        }

        /// <summary>
        /// For PhysicalInteractionGrabbable to use only. <br>仅供PhysicalInteractionGrabbable使用.</br>
        /// </summary>
        public void ReleaseMe(Transform obj)
        {
            if (m_GrabbedObjs == obj)
                m_GrabbedObjs = null;
            else//error case
                Debug.LogError("Receive releaseMe from not grabbed obj [" + obj.name + "] - Hand:[" + name + "]");
        }

        /// <summary>
        /// For sync postion and rotation to hand move root. <br>用来同步手移动根节点的位移和旋转.</br>
        /// </summary>
        /// <param name="obj">Object to sync. <br>需要同步的物体.</br></param>
        public void SyncMove(Transform obj)
        {
            if (m_TriangulationReady && m_GrabMoveRoot != null)
            {
                CalculateGrabMoveRootPosRot();
                obj.position = m_GrabMoveRoot.position;
                obj.rotation = m_GrabMoveRoot.rotation;
            }
        }

        void CalculateGrabMoveRootPosRot()
        {
            m_GrabMoveRoot.position = (m_TriangulationP1.position + m_TriangulationP2.position + m_TriangulationP3.position) / 3f;
            Vector3 vec1 = m_TriangulationP2.position - m_TriangulationP1.position;
            Vector3 vec2 = m_TriangulationP3.position - m_TriangulationP1.position;
            Vector3 normal = Vector3.Cross(vec1, vec2).normalized;

            m_GrabMoveRoot.rotation = Quaternion.LookRotation((m_TriangulationP1.position - m_GrabMoveRoot.position).normalized, normal);
        }

        void UpdateHandMenuStatus(bool status)
        {
            if (isPause != status)
            {
                isPause = status;
            }
        }

        public bool IsFist()
        {   //0-13 vs 15-16  0-9 vs 11-12  0-5 vs 7-8
            //m_HandController.activeHand.joints.Length
            int fistFingerCount = 0;
            if (IsThisFingerFist(0,13,15,16))
                fistFingerCount++;

            if (IsThisFingerFist(0, 9, 11, 12))
                fistFingerCount++;

            if (IsThisFingerFist(0, 5, 7, 8))
                fistFingerCount++;

            return fistFingerCount >= 2;
        }

        bool IsThisFingerFist(int rootStart, int rootTip, int topStart, int topTip)
        {
            return 
                Vector3.Angle(
                    (m_HandController.activeHand.joints[rootTip].position - m_HandController.activeHand.joints[rootStart].position).normalized,
                    (m_HandController.activeHand.joints[topTip].position - m_HandController.activeHand.joints[topStart].position).normalized
                    )
                > 120f;
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            m_HandController = GetComponent<HandController>();
            if (m_HandController == null)
            {
                Debug.LogError("HandController missing. Can not add.");
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(this);
                return;
            }
#endif
        }
    }
}
