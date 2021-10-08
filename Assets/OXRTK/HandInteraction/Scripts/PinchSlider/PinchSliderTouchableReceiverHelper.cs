using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for pinch slider UI intetraction. <br>
    /// 非UGUI滑条接收UI交互的类。
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class PinchSliderTouchableReceiverHelper : UiInteractionPointerHandler
    {
        /// <summary>
        /// BoxCollider that used for detect the interaction, use the BoxCollider on the current object if leave it blank. <br>
        /// 用于检测抓取的BoxCollider，如果将其预留为空则默认为当前物体上的BoxCollider。
        /// </summary>
        public BoxCollider colliderOverride;
        //按钮collider检测时，collider中心（local坐标系下）
        Vector3 m_ColliderCenter = Vector3.zero;
        //用于检测交互位置是否在boxCollider范围内
        Vector3 m_Bounds;

        PinchSlider m_PinchSliderRoot;

        /// <summary>
        /// Initialize pinch slider UI handler. <br>
        /// 初始化滑条UI交互接收端。
        /// </summary>
        /// <param name="pinchSlider">Target pinch slider. <br>目标滑条.</param>
        public void Init(PinchSlider pinchSlider)
        {
            m_PinchSliderRoot = pinchSlider;
        }

        void Start()
        {
            InitTouchableCollider();
        }

        //初始化交互信息
        void InitTouchableCollider()
        {
            if (colliderOverride == null)
            {
                if (gameObject.GetComponent<BoxCollider>() != null)
                    colliderOverride = gameObject.GetComponent<BoxCollider>();
            }

            Vector3 adjustedSize = new Vector3(Mathf.Abs(colliderOverride.size.x), Mathf.Abs(colliderOverride.size.y), Mathf.Abs(colliderOverride.size.z));

            InitBounds(adjustedSize);
            InitLocalCenter(colliderOverride.center);
        }

        //初始化交互范围
        void InitBounds(Vector3 bounds)
        {
            m_Bounds = bounds;
        }

        //初始化collider中心
        void InitLocalCenter(Vector3 center)
        {
            m_ColliderCenter = center;
        }

        /// <summary>
        /// Checks whether the object is grabbable. <br>
        /// 获取当前物体是否为可抓取物体。
        /// </summary>
        public override bool CheckGrabbable()
        {
            return true;
        }

        /// <summary>
        /// Checks whether the object is touchble based on current finger tip position. <br>
        /// 获取当前物体是否可被手指触碰。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distanceOnPressableDirection">Distance on pressable direction. <br>指尖在按压方向上的距离.</param>
        /// <param name="normal">Object normal <br>物体法线.</param>
        /// <returns>Whether the object can be interacted based on the current finger position. <br>在当前手的位置上，物体是否可能被交互</returns>
        public override bool CheckTouchable(Vector3 tipPoint, out float distanceOnPressableDirection, out Vector3 normal)
        {
            normal = -transform.forward;
            distanceOnPressableDirection = float.PositiveInfinity;

            if (m_TouchableDistance < maxInteractionDistance)
            {
                distanceOnPressableDirection = m_TouchableDistance;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates current distance between interaction finger tip and object on pressable direction. <br>
        /// 计算当前物体在按压方向上与手的距离。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        public override float DistanceToTouchable(Vector3 tipPoint)
        {
            Vector3 centerPoint = transform.TransformPoint(colliderOverride.center);
            Ray ray = new Ray(tipPoint, Vector3.Normalize(centerPoint - tipPoint));

            RaycastHit[] hits;
            hits = Physics.RaycastAll(ray, float.PositiveInfinity, ~Physics.IgnoreRaycastLayer);

            m_TouchableDistance = float.PositiveInfinity;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == colliderOverride.transform)
                {
                    m_TouchableDistance = hits[i].distance;
                    break;
                }
            }

            Vector3 localPoint = transform.InverseTransformPoint(tipPoint) - m_ColliderCenter;

            if (localPoint.x > -m_Bounds.x / 2 &&
                localPoint.x < m_Bounds.x / 2 &&
                localPoint.y > -m_Bounds.y / 2 &&
                localPoint.y < m_Bounds.y / 2 &&
                localPoint.z > -m_Bounds.z / 2 &&
                localPoint.z < m_Bounds.z / 2)
            {
                m_TouchableDistance = 0;
            }

            return m_TouchableDistance;
        }

        /// <summary>
        /// Called when the interaction finger comes close into the checking area of object. <br>
        /// 当用户交互手指靠近物体进入检测范围时调用。
        /// </summary>
        public override void OnComeClose()
        {
            base.OnComeClose();
            m_PinchSliderRoot.onHighlightStart?.Invoke();
        }

        /// <summary>
        /// Called when the interaction finger leaves the checking area of object. <br>
        /// 当用户交互手指远离物体退出检测范围时调用。
        /// </summary>
        public override void OnLeaveFar()
        {
            base.OnLeaveFar();
            m_PinchSliderRoot.onHighlightEnd?.Invoke();
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并进行捏取操作时调用。
        /// </summary>
        /// <param name="fingerPosition">Start pinch point. <br>近场交互捏取点.</param>
        public override void OnPinchDown(Vector3 fingerPosition)
        {
            base.OnPinchDown(fingerPosition);
            m_PinchSliderRoot.onInteractionStart?.Invoke();
            m_PinchSliderRoot.UpdateHandlerPosition(fingerPosition);
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="fingerPosition">pinch position. <br>近场交互捏取点.</param>
        public override void OnDragging(Vector3 fingerPosition)
        {
            base.OnDragging(fingerPosition);
            m_PinchSliderRoot.UpdateHandlerPosition(fingerPosition);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            base.OnPinchUp();
            m_PinchSliderRoot.onInteractionEnd?.Invoke();
        }

        /// <summary>
        /// Continuously called when the interaction finger is in the checking area of object. <br>
        /// 当用户交互手指在物体检测范围时连续调用。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distance">Distance between interaction finger tip and object on pressable direction <br>当前物体在按压方向上与手的距离.</param>
        public override void OnTouchUpdate(Vector3 tipPos, float distance)
        {
            base.OnTouchUpdate(tipPos, distance);
        }

    }
}
