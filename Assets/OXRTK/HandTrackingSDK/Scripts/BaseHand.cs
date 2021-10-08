using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OXRTK.ARHandTracking;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The base class for different hand visualizations.<br>
    /// 不同显示类型的手的基类。
    /// </summary>
    public class BaseHand : MonoBehaviour
    {
        /// <summary>
        /// The enum of hand's visualization types.<br>
        /// 手显示类型的枚举。<br>
        /// </summary>
        public enum HandVisualizationType
        {
            /// Skeleton visualization<br>
            /// 骨骼显示
            Skeleton = 0,
            /// 3D model visualization<br>
            /// 三维模型显示
            Model = 1
        }

        /// <summary>
        /// The visualization type of current hand, skeleton or 3D model.<br>
        /// 当前手的显示类型，骨骼或者三维模型。
        /// </summary>
        public HandVisualizationType handVisualizationType;

        /// <summary>
        /// The type of current hand, left hand or right hand.<br>
        /// 当前手的类型，左手或者右手。
        /// </summary>
        [HideInInspector] public HandTrackingPlugin.HandType handType;

        /// <summary>
        /// Determines how collider will be added to hand.<br>
        /// 决定如何将碰撞体添加到手上。
        /// </summary>
        /// <remarks>
        /// Supports none, indexFingerTip, first3FingerTip(thumb, index, middle), first4FingerTip(thumb, index, middle, ring), allFingerTip, allJoint.<br>
        /// 支持 不添加，食指指尖，前三根手指指尖（拇指，食指，中指），前四根手指指尖（拇指，食指，中指，无名指），所有手指指尖，所有手部节点。
        /// </remarks>
        [Tooltip("First 3 Finger(Thumb, Index, Middle)\nFirst 4 Finger(Thumb, Index, Middle, Ring)")]
        [SerializeField] protected HandColliderHandle.JointNeedCollider m_JointCollider = HandColliderHandle.JointNeedCollider.allFingerTip;

        /// <summary>
        /// The type of collider.<br>
        /// 碰撞体的类型。
        /// </summary>
        /// <remarks>
        /// Supports colliderOnly, colliderAndRigidbody, triggerOnly, triggerAndRigidbody.<br>
        /// 支持 仅有碰撞体，碰撞体和刚体，只有扳机，扳机和刚体。
        /// </remarks>
        [SerializeField] protected HandColliderHandle.ColliderType m_ColliderType = HandColliderHandle.ColliderType.colliderAndRigidbody;

        /// <summary>
        /// The controller that hand is connected to.<br>
        /// 手连接到的控制器。
        /// </summary>
        public HandController connectedController;

        /// <summary>
        /// The rendering gameobject of hand.<br>
        /// 手的渲染gameobject。
        /// </summary>
        public GameObject handGameObject;

        /// <summary>
        /// The array of renderers used for hand rendering.<br>
        /// 用于手部渲染的Renderer。
        /// </summary>
        [HideInInspector] public List<Renderer> handRenderers = new List<Renderer>();
        
        /// <summary>
        /// Scale factor for collider due to hand mode size difference.<br>
        /// 根据手模型的建模比例相应调整collider的大小比例参数。
        /// </summary>
        public float colliderScaleFactor = 1f;

        /// <summary>
        /// The joints of hand.<br>
        /// 手的节点。
        /// </summary>
        /**          
         *  <br>                  
         *  
         *              The order of all joints.
         *              所有节点的顺序。
         *              
         *              *17 --- *18 --- *19 --- *20                     Thumb           拇指
         *            /
         *           /         
         *          *0 ------- *13 ---- *14 ---- *15 ---- *16           Index finger    食指
         *           \\\          
         *            \\ ----- *9 ----- *10 ----- *11 ----- *12         Middle finger   中指
         *             \\       
         *              \ ---- *5 ---- *6 ---- *7 ---- *8               Ring finger     无名指
         *               \ 
         *                *1 -- *2 -- *3 -- *4                          Pinky finger    小指
         *                         
         */
        public Transform[] joints;

        /// <summary>
        /// All data output by algorithm.<br>
        /// 所有由算法输出的数据。
        /// </summary>
        protected HandTrackingPlugin.HandInfo m_HandInfo;

        /// <summary>
        /// Initializes hand based on its visualization type.<br>
        /// 基于显示类型对手进行初始化。
        /// </summary>
        protected virtual void Init()
        {
            if (handGameObject != null)
            {
                handGameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Rendering gameobject of hand is not set!");
            }
            
            // SwitchHandDisplay(connectedController.hidHandMode);
        }

        /// <summary>
        /// Updates hand based on its visualization type.<br>
        /// 基于显示类型对手进行更新。
        /// </summary>
        protected virtual void UpdateHand()
        {            
            UpdateHandData();
            UpdateHandTransform();
            UpdateHandRendering();
        }

        protected virtual void UpdateHandData()
        {
            switch (handType)
            {
                default:
                case HandTrackingPlugin.HandType.RightHand:
                    m_HandInfo = HandTrackingPlugin.instance.rightHandInfo;
                    break;
                case HandTrackingPlugin.HandType.LeftHand:
                    m_HandInfo = HandTrackingPlugin.instance.leftHandInfo;
                    break;
            }
        }

        protected virtual void UpdateHandTransform() { }

        protected virtual void UpdateHandRendering()
        {
            if (m_HandInfo.handDetected)
            {
                if (!handGameObject.activeSelf)
                {
                    handGameObject.SetActive(true);
                }
            }
            else
            {
                if (handGameObject.activeSelf)
                {
                    handGameObject.SetActive(false);
                }
            }
        }

        void Start()
        {
            handType = connectedController.handType;
            Init();

            if (HandTrackingPlugin.instance != null)
            {
                HandTrackingPlugin.instance.onHandDataUpdated += UpdateHand;
            }
        }

        /// <summary>
        /// Change to show or hide the hand model. if the input boolean is true, then it uses the hidden hand mode; Otherwise, it displays the hand model.<br>
        /// 控制是否渲染手模的开关，当isOn为真，则打开隐藏手模式；反之则打开手模。
        /// </summary>
        public void SwitchHandDisplay(bool isOn)
        {
            foreach (Renderer r in handRenderers)
            {
                r.enabled = !isOn;
            }
        }
    }
}

