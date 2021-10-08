using System;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking 
{
    /// <summary>
    /// The class for pinch slider. <br>
    /// 非UGUI滑条的类。
    /// </summary>
    public class PinchSlider : MonoBehaviour
    {
        [SerializeField]
        private InteractionAvailability m_InteractionAvailablity;
        /// <summary>
        /// Slider interaction type. <br>
        /// 滑条交互方式。
        /// </summary>
        public InteractionAvailability interactionAvailability 
        { 
            get { return m_InteractionAvailablity; } 
            set 
            { 
                m_InteractionAvailablity = value; 
            } 
        }

        [SerializeField]
        Transform m_HandlerRoot;
        /// <summary>
        /// Slider handler root. <br>
        /// 滑块根节点。
        /// </summary>
        public Transform handlerRoot 
        {
            get { return m_HandlerRoot; }
            set { m_HandlerRoot = value; }
        }
        
        [SerializeField]
        Transform m_HandlerVisualization;
        /// <summary>
        /// Slider handler visualization object. <br>
        /// 滑块可视化物体。
        /// </summary>
        public Transform handlerVisualization
        {
            get { return m_HandlerVisualization; }
            set { m_HandlerVisualization = value; }
        }

        [SerializeField]
        Transform m_TrackRoot;
        /// <summary>
        /// Slider back track root. <br>
        /// 滑条轨迹根节点。
        /// </summary>
        public Transform trackRoot
        {
            get { return m_TrackRoot; }
            set { m_TrackRoot = value; }
        }

        [SerializeField]
        PinchSliderAxis m_SliderAxis;
        /// <summary>
        /// Slider axis. <br>
        /// 滑条移动轴。
        /// </summary>
        public PinchSliderAxis sliderAxis 
        {
            get { return m_SliderAxis; }
            set 
            { 
                m_SliderAxis = value;
                InitializeSliderAxis(m_SliderAxis);
            }
        }

        [Range(0, 1)]
        [SerializeField]
        float m_SliderValue = 0.5f;
        /// <summary>
        /// Slider value. <br>
        /// 滑条数值。
        /// </summary>
        public float sliderValue
        {
            get { return m_SliderValue; }
            set 
            {
                float oldValue = m_SliderValue;
                m_SliderValue = value;
                UpdateHandlerPos();
                onValueChanged?.Invoke(m_SliderValue);
            }
        }

        /// <summary>
        /// Slider handler root localPosition on slider axis when slider value equals to 0. <br>
        /// 滑条数值为0时滑块根节点在滑条移动方向上的localPosition数值。
        /// </summary>
        public float startLocalValue;

        /// <summary>
        /// Slider handler root localPosition on slider axis when slider value equals to 0. <br>
        /// 滑条数值为1时滑块根节点在滑条移动方向上的localPosition数值。
        /// </summary>
        public float endLocalValue;

        /// <summary>
        /// Event is called when the slider value is changed. <br>
        /// 当滑条数值改变时触发的事件。
        /// </summary>
        [Serializable]
        public class PinchSliderEvent : UnityEvent<float> { }

        [SerializeField]
        PinchSliderEvent m_OnValueChanged = new PinchSliderEvent();
        /// <summary>
        /// UnityEvent when slider value changed. <br>
        /// 滑条数值改变时的事件更新。
        /// </summary>
        public PinchSliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        [SerializeField]
        UnityEvent m_OnHighlightStart = new UnityEvent();
        /// <summary>
        /// UnityEvent when slider is start highlighted. <br>
        /// 滑条开始高亮时的事件更新。
        /// </summary>
        public UnityEvent onHighlightStart { get { return m_OnHighlightStart; } set { m_OnHighlightStart = value; } }
        
        [SerializeField]
        UnityEvent m_OnHighlightEnd = new UnityEvent();
        /// <summary>
        /// UnityEvent when slider is end highlighted. <br>
        /// 滑条结束高亮时的事件更新。
        /// </summary>
        public UnityEvent onHighlightEnd { get { return m_OnHighlightEnd; } set { m_OnHighlightEnd = value; } }

       [SerializeField]
        UnityEvent m_OnInteractionStart = new UnityEvent();
        /// <summary>
        /// UnityEvent when slider is start interacted. <br>
        /// 滑条开始交互时的事件更新。
        /// </summary>
        public UnityEvent onInteractionStart { get { return m_OnInteractionStart; } set { m_OnInteractionStart = value; } }
        
        [SerializeField]
        UnityEvent m_OnInteractionEnd = new UnityEvent();
        /// <summary>
        /// UnityEvent when slider is end interacted. <br>
        /// 滑条结束交互时的事件更新。
        /// </summary>
        public UnityEvent onInteractionEnd { get { return m_OnInteractionEnd; } set { m_OnInteractionEnd = value; } }

        void InitializeSliderAxis(PinchSliderAxis axis)
        {
            InitHandler(axis);
            InitTrack(axis);
        }

        void InitHandler(PinchSliderAxis axis)
        {
            if (m_HandlerVisualization == null)
                return;

            switch (axis)
            {
                case PinchSliderAxis.X:
                    m_HandlerVisualization.localEulerAngles = new Vector3(0, 0, 0);
                    break;
                case PinchSliderAxis.Y:
                    m_HandlerVisualization.localEulerAngles = new Vector3(0, 0, 90);
                    break;
                case PinchSliderAxis.Z:
                    m_HandlerVisualization.localEulerAngles = new Vector3(0, 90, 0);
                    break;
            }
        }

        void InitTrack(PinchSliderAxis axis)
        {
            if (m_TrackRoot == null)
                return;

            switch (axis)
            {
                case PinchSliderAxis.X:
                    m_TrackRoot.localEulerAngles = new Vector3(0, 0, 0);
                    break;
                case PinchSliderAxis.Y:
                    m_TrackRoot.localEulerAngles = new Vector3(0, 0, 90);
                    break;
                case PinchSliderAxis.Z:
                    m_TrackRoot.localEulerAngles = new Vector3(0, 90, 0);
                    break;
            }
        }

        void UpdateHandlerPos()
        {
            Vector3 handlerMovementDir = default;
            switch (m_SliderAxis)
            {
                case PinchSliderAxis.X:
                    handlerMovementDir = transform.right;
                    break;
                case PinchSliderAxis.Y:
                    handlerMovementDir = transform.up;
                    break;
                case PinchSliderAxis.Z:
                    handlerMovementDir = transform.forward;
                    break;
            }
            Vector3 newHandlerPos = handlerMovementDir * (startLocalValue + (endLocalValue - startLocalValue) * m_SliderValue);
            m_HandlerRoot.localPosition = newHandlerPos;
        }

        void OnValidate()
        {
            sliderAxis = m_SliderAxis;
            sliderValue = m_SliderValue;

            if (m_HandlerVisualization != null)
            {
                if (interactionAvailability != m_InteractionAvailablity)
                {
                    PinchSliderRayReceiverHelper rayHelper;
                    PinchSliderTouchableReceiverHelper touchableHelper;

                    switch (m_InteractionAvailablity)
                    {
                        case InteractionAvailability.AllowRayInteraction:
                            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>() != null)
                            {
                                rayHelper = m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>();
                            }
                            else
                            {
                                rayHelper = m_HandlerVisualization.gameObject.AddComponent<PinchSliderRayReceiverHelper>();
                            }
                            rayHelper.Init(this);

                            if(m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>() != null)
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.delayCall += () =>
                                {
                                    DestroyImmediate(m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>());
                                };
#endif
                            }
                            break;
                        case InteractionAvailability.AllowUIInteraction:
                            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>() != null)
                            {
                                touchableHelper = m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>();
                            }
                            else
                            {
                                touchableHelper = m_HandlerVisualization.gameObject.AddComponent<PinchSliderTouchableReceiverHelper>();
                            }
                            touchableHelper.Init(this);

                            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>() != null)
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.delayCall += () =>
                                {
                                    DestroyImmediate(m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>());
                                };
#endif
                            }
                            break;
                        case InteractionAvailability.AllowBoth:
                            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>() != null)
                            {
                                rayHelper = m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>();
                            }
                            else
                            {
                                rayHelper = m_HandlerVisualization.gameObject.AddComponent<PinchSliderRayReceiverHelper>();
                            }
                            rayHelper.Init(this);

                            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>() != null)
                            {
                                touchableHelper = m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>();
                            }
                            else
                            {
                                touchableHelper = m_HandlerVisualization.gameObject.AddComponent<PinchSliderTouchableReceiverHelper>();
                            }
                            touchableHelper.Init(this);
                            break;
                    }
                }
            }
        }

        private void Start()
        {
            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>() != null)
                m_HandlerVisualization.gameObject.GetComponent<PinchSliderRayReceiverHelper>().Init(this);
            if (m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>() != null)
                m_HandlerVisualization.gameObject.GetComponent<PinchSliderTouchableReceiverHelper>().Init(this);
        }

        /// <summary>
        /// Update handler root localPosition. <br>
        /// 更新滑块根节点位置。
        /// </summary>
        /// <param name="endPosition">Target position of handler root. <br>滑块根节点目标位置.</param>
        public void UpdateHandlerPosition(Vector3 endPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(endPosition);
            float axisValue = 0;
            switch (m_SliderAxis)
            {
                case PinchSliderAxis.X:
                    axisValue = localPosition.x;
                    break;
                case PinchSliderAxis.Y:
                    axisValue = localPosition.y;
                    break;
                case PinchSliderAxis.Z:
                    axisValue = localPosition.z;
                    break;
            }
            float v = Mathf.Clamp(axisValue, startLocalValue, endLocalValue);
            sliderValue = (v - startLocalValue) / (endLocalValue - startLocalValue);
        }
    }
}