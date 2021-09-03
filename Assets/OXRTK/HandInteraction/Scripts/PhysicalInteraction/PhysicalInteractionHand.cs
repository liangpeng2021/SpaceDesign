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

        // TODO:Double check
        // self implemented moving average algorithm

        //Quaternion m_AverageRotation = Quaternion.identity;
        Queue<Vector3> m_QueueOfTriangulation1Pos = new Queue<Vector3>();
        Queue<Vector3> m_QueueOfTriangulation2Pos = new Queue<Vector3>();
        Queue<Vector3> m_QueueOfTriangulation3Pos = new Queue<Vector3>();
        int m_WindowWidth = 60;
        //List<Quaternion> m_ListOfQuaternions = new List<Quaternion>();
        Transform m_GrabbedObjs;
        Transform m_GrabMoveRoot;
        Transform m_TriangulationP1;
        Transform m_TriangulationP2;
        Transform m_TriangulationP3;
        bool m_TriangulationReady = false;

        List<PhysicalInteractionFingerTip> m_FingerTips = new List<PhysicalInteractionFingerTip>();

        Coroutine m_InitRoutine = null;
        void Start()
        {
            if (m_HandController == null) m_HandController = GetComponent<HandController>();
            if (m_HandController == null) { Debug.LogError("HandController Reference Null! Finger Tip interaction Disable! - Hand:[" + name + "]"); return; }

            m_InitRoutine = StartCoroutine(Init());
            HandController.onActiveHandChanged += OnHandStyleChange;
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
                ToggleFingerTipsOnOff();
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
            set { ToggleFingerTipsOnOff(value); }
        }

        void ToggleFingerTipsOnOff(bool isEnableTp)
        {
            if (m_IsEnabled != isEnableTp)
            {
                m_IsEnabled = isEnableTp;
                ToggleFingerTipsOnOff();
                if (!m_IsEnabled && m_GrabbedObjs != null)
                    m_GrabbedObjs.GetComponent<PhysicalInteractionInteractable>()?.FTHReset();
            }
        }

        void ToggleFingerTipsOnOff()
        {
            for (int i = 0; i < m_FingerTips.Count; i++)
            {
                m_FingerTips[i].IsEnabled = m_IsEnabled;
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
            Vector3 tempPos1 = m_TriangulationP1.position;
            Vector3 tempPos2 = m_TriangulationP2.position;
            Vector3 tempPos3 = m_TriangulationP3.position;

            m_QueueOfTriangulation1Pos.Enqueue(tempPos1);
            m_QueueOfTriangulation2Pos.Enqueue(tempPos2);
            m_QueueOfTriangulation3Pos.Enqueue(tempPos3);

            if (m_QueueOfTriangulation1Pos.Count > m_WindowWidth)
            {
                m_QueueOfTriangulation1Pos.Dequeue();
                tempPos1 = Vector3.zero;
                for (int i = 0; i < m_QueueOfTriangulation1Pos.Count; i++)
                {
                    tempPos1 += m_QueueOfTriangulation1Pos.ToArray()[i];
                }
                tempPos1 /= m_WindowWidth;
            }
            if (m_QueueOfTriangulation2Pos.Count > m_WindowWidth)
            {
                m_QueueOfTriangulation2Pos.Dequeue();
                tempPos2 = Vector3.zero;
                for (int i = 0; i < m_QueueOfTriangulation2Pos.Count; i++)
                {
                    tempPos2 += m_QueueOfTriangulation2Pos.ToArray()[i];
                }
                tempPos2 /= m_WindowWidth;
            }
            if (m_QueueOfTriangulation3Pos.Count > m_WindowWidth)
            {
                m_QueueOfTriangulation3Pos.Dequeue();
                tempPos3 = Vector3.zero;
                for (int i = 0; i < m_QueueOfTriangulation3Pos.Count; i++)
                {
                    tempPos3 += m_QueueOfTriangulation3Pos.ToArray()[i];
                }
                tempPos3 /= m_WindowWidth;
            }

            m_GrabMoveRoot.position = (m_TriangulationP1.position + m_TriangulationP2.position + m_TriangulationP3.position) / 3f;
            Vector3 vec1 = m_TriangulationP2.position - m_TriangulationP1.position;
            Vector3 vec2 = m_TriangulationP3.position - m_TriangulationP1.position;
            Vector3 normal = Vector3.Cross(vec1, vec2).normalized;

            //m_GrabMoveRoot.rotation = Quaternion.LookRotation((m_TriangulationP1.position - m_GrabMoveRoot.position).normalized, normal);
            m_GrabMoveRoot.rotation = Quaternion.LookRotation((tempPos1 - (tempPos1+tempPos2+tempPos3)/3f).normalized, normal);
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
