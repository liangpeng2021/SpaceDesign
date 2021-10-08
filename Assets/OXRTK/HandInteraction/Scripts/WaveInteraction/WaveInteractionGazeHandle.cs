using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Module to handle hand wave interaction while camera gaze.<br>在相机凝视同时的挥动交互组件.</br>
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class WaveInteractionGazeHandle : WaveInteraction
    {
        [Space(5)]
        /// <summary>
        /// Event: When camera gaze over collider.<br>事件:当相机视线打到collider.</br>
        /// </summary>
        public UnityEvent g_OnGazeEnter;

        [Space(5)]
        /// <summary>
        /// Event: When camera gaze left collider.<br>事件:当相机视线离开collider.</br>
        /// </summary>
        public UnityEvent g_OnGazeExit;

        [Space(5)]
        /// <summary>
        /// Event: When camera gaze move in collider.<br>事件:当相机视线在collider内移动.</br>
        /// </summary>
        public UnityEventVector3 g_OnGaze;

        [Space(20)]

        [Tooltip("For drag only:\n  Threshold of when virtual press trigger.\n  Distance between hand and camera.\n仅对拖拽生效:\n  虚拟按压触发阈值,手到相机的距离.")]
        [Range(0.2f, 0.5f)]
        [SerializeField] float m_VirtualPressThreshold = 0.35f;

        [Space(5)]
        /// <summary>
        /// Event: Distance between camera and hand for virtual press event, relative distance to m_VirtualPressThreshold, normalized value 0-1.<br>事件:为空间按压提供的手到相机的相对距离,相对于m_VirtualPressThreshold统一化的距离0-1.</br>
        /// </summary>
        public UnityEventFloat g_OnHandToCamDistance;

        [Space(5)]
        /// <summary>
        /// Event: When hand virtual press forward in space.<br>事件:手在空间中向前按压.</br>
        /// </summary>
        public UnityEvent g_OnHandVirtualPress;

        [Space(5)]
        /// <summary>
        /// Event: When hand virtual stop press forward in space.<br>事件:手在空间中停止向前按压.</br>
        /// </summary>
        public UnityEvent g_OnHandVirtualRelease;


        bool m_GazeOn = false;
        bool m_HandInInteractionArea = false;

        protected override void Start()
        {
            base.Start();

            m_BondTopRight = new Vector3(m_Collider.size.x / 2f, m_Collider.size.y / 2f, 0);
            m_BondBotLeft = -m_BondTopRight;

            CheckAndInitWaveGaze();
            TrySetCameraReference();
        }

        void CheckAndInitWaveGaze()
        {
            if (WaveInteractionGazeCaster.instance == null || !WaveInteractionGazeCaster.instance.gameObject.activeInHierarchy)
            {
                XR.XRCameraManager.Instance.stereoCamera.gameObject.AddComponent<WaveInteractionGazeCaster>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!g_IsPause)
            {
                if (m_GazeOn)
                {
                    CheckHandActive();
                    if (m_ActiveHand != null)
                    {
                        CheckHandArea();
                        if (m_HandInInteractionArea)
                        {
                            CheckHandVirtualPress();
                            WaveMainLogic();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// For WaveGaze.cs use only.<br>仅供WaveGaze.cs使用.</br>
        /// </summary>
        public void OnGazeEnter()
        {
            m_GazeOn = true;
            g_OnGazeEnter?.Invoke();
            if (HandTrackingPlugin.debugLevel > 0) Debug.LogWarning("OnGazeEnter");
        }

        /// <summary>
        /// For WaveGaze.cs use only.<br>仅供WaveGaze.cs使用.</br>
        /// </summary>
        public void OnGazeExit()
        {
            m_GazeOn = false;
            HandExitView(false);
            g_OnGazeExit?.Invoke();
            if (HandTrackingPlugin.debugLevel > 0) Debug.LogWarning("OnGazeExit");
        }

        /// <summary>
        /// For WaveGaze.cs use only.<br>仅供WaveGaze.cs使用.</br>
        /// </summary>
        public void OnGaze(Vector3 point)
        {
            g_OnGaze?.Invoke(transform.InverseTransformPoint(point));
        }

        void CheckHandActive()
        {
            if (m_ActiveHand != null)
            {
                if (!m_ActiveHand.handGameObject.activeInHierarchy)
                {
                    HandExitView();
                }
            }
            else
            {
                TryGetActiveHand();
            }
        }

        void TryGetActiveHand()
        {
            if (HandTrackingPlugin.instance.leftHandController.activeHand != null && HandTrackingPlugin.instance.leftHandController.activeHand.handGameObject.activeInHierarchy)
            {
                HandEnterView(HandTrackingPlugin.instance.leftHandController.activeHand);
                return;
            }
            if (HandTrackingPlugin.instance.rightHandController.activeHand != null && HandTrackingPlugin.instance.rightHandController.activeHand.handGameObject.activeInHierarchy)
            {
                HandEnterView(HandTrackingPlugin.instance.rightHandController.activeHand);
                return;
            }
        }

        void HandEnterView(BaseHand hand)
        {
            m_ActiveHand = hand;
        }

        void HandExitView(bool getNewHand = true)
        {
            m_ActiveHand = null;
            HandExitInteractionArea();

            if (getNewHand)
                TryGetActiveHand();
        }

        bool m_IsPressing = false;
        int m_PressFrameCount = 0;
        void CheckHandVirtualPress()
        {
            if (m_MainCamera == null)
            {
                TrySetCameraReference();
                return;
            }
            Vector3 midNodePos = m_MainCamera.transform.InverseTransformPoint(m_ActiveHand.joints[9].transform.position);

            g_OnHandToCamDistance ?.Invoke (midNodePos.z/ m_VirtualPressThreshold);

            if (midNodePos.z > m_VirtualPressThreshold)
            {
                if (m_IsPressing)
                {
                    m_PressFrameCount = 0;
                }
                else
                {
                    m_PressFrameCount++;
                    if (m_PressFrameCount > 3) //press
                        VirtualPress();
                }
            }
            else
            {
                if (m_IsPressing)
                {
                    m_PressFrameCount--;
                    if (m_PressFrameCount < -3) //press
                        VirtualRelease();
                }
                else
                {
                    m_PressFrameCount = 0;
                }
            }
        }
        void VirtualPress()
        {
            m_IsPressing = true;
            m_PressFrameCount = 0;
            g_OnHandVirtualPress?.Invoke();
        }
        void VirtualRelease()
        {
            m_IsPressing = false;
            m_PressFrameCount = 0;
            g_OnHandVirtualRelease?.Invoke();
        }

        Vector3 m_BondTopRight = new Vector3(0.5f, 0.5f, 0);
        Vector3 m_BondBotLeft = new Vector3(-0.5f, -0.5f, 0);
        Camera m_MainCamera;
        void TrySetCameraReference()
        {
            if (m_MainCamera == null) m_MainCamera = XR.XRCameraManager.Instance.stereoCamera.Cam;
#if UNITY_EDITOR
            m_MainCamera = Camera.allCameras[0];
#endif
        }
        void CheckHandArea()
        {
            if (m_MainCamera == null)
            {
                TrySetCameraReference();
                return;
            }
            CheckHandInInteractiveArea();
        }
        void CheckHandInInteractiveArea()
        {//Check if hand in interactive zone.
            Vector3 cameraPos = transform.InverseTransformPoint(m_MainCamera.transform.position);
            Vector3 topNodePos = transform.InverseTransformPoint(m_ActiveHand.joints[12].transform.position);
            Vector3 midNodePos = transform.InverseTransformPoint(m_ActiveHand.joints[9].transform.position);
            Vector3 leftNodePos = transform.InverseTransformPoint(m_ActiveHand.joints[20].transform.position);
            Vector3 rightNodePos = transform.InverseTransformPoint(m_ActiveHand.joints[3].transform.position);

            Vector3 normal_Cam_TopNode = (topNodePos - cameraPos).normalized;
            Vector3 normal_Cam_MidNode = (midNodePos - cameraPos).normalized;
            Vector3 normal_Cam_LeftNode = (leftNodePos - cameraPos).normalized;
            Vector3 normal_Cam_RightNode = (rightNodePos - cameraPos).normalized;

            Vector3 normal_Cam_TopRight = (m_BondTopRight - cameraPos).normalized;
            Vector3 normal_Cam_BotLeft = (m_BondBotLeft - cameraPos).normalized;

            if (
                (normal_Cam_LeftNode.x > normal_Cam_BotLeft.x && normal_Cam_LeftNode.x < normal_Cam_TopRight.x
            && normal_Cam_LeftNode.y > normal_Cam_BotLeft.y && normal_Cam_LeftNode.y < normal_Cam_TopRight.y)
            || (normal_Cam_RightNode.x > normal_Cam_BotLeft.x && normal_Cam_RightNode.x < normal_Cam_TopRight.x
            && normal_Cam_RightNode.y > normal_Cam_BotLeft.y && normal_Cam_RightNode.y < normal_Cam_TopRight.y)
            || (normal_Cam_TopNode.x > normal_Cam_BotLeft.x && normal_Cam_TopNode.x < normal_Cam_TopRight.x
            && normal_Cam_TopNode.y > normal_Cam_BotLeft.y && normal_Cam_TopNode.y < normal_Cam_TopRight.y)
            || (normal_Cam_MidNode.x > normal_Cam_BotLeft.x && normal_Cam_MidNode.x < normal_Cam_TopRight.x
            && normal_Cam_MidNode.y > normal_Cam_BotLeft.y && normal_Cam_MidNode.y < normal_Cam_TopRight.y)
            )
            {
                if (!m_HandInInteractionArea)
                    HandEnterInteractionArea();
            }
            else
            {
                if (m_HandInInteractionArea)
                    HandExitInteractionArea();
            }
        }
       
        void HandEnterInteractionArea()
        {
            m_HandInInteractionArea = true;
            g_OnHandEnter?.Invoke((int)m_ActiveHand.handType);
            InitHandPos();
        }

        void HandExitInteractionArea()
        {
            m_HandInInteractionArea = false;
            g_OnHandExit?.Invoke();
            ResetWave();
        }


        #region Wave Logic

        protected override Vector2 GetHandPos()
        {
            Vector3 pos = m_MainCamera.WorldToViewportPoint(m_ActiveHand.joints[12].transform.position);

            Vector3 canvasTopRight = m_MainCamera.WorldToViewportPoint(transform.TransformPoint(m_BondTopRight));
            Vector3 canvasBotLeft = m_MainCamera.WorldToViewportPoint(transform.TransformPoint(m_BondBotLeft));
            m_InteractiveAreaSize = canvasTopRight - canvasBotLeft;
            Vector2 canvasCenter = (canvasTopRight + canvasBotLeft) / 2f;

            //Position relative to Canvas center in camera view coordinate, local center of these pos is 0,0
            pos.x -= canvasCenter.x;
            pos.y -= canvasCenter.y;

            return pos;
        }

        #endregion Wave Logic
    }
}