using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for touchable ui interaction receiver. <br>
    /// 用于近场可触碰UI交互接收端的类。
    /// </summary>
    public class UiInteractionPointerHandler : MonoBehaviour
    {
        [SerializeField]
        [Range(0.05f, 0.2f)]
        protected float m_MaxInteractionDistance = 0.15f;

        /// <summary>
        /// Max interaction distance(between object and index finger tip) for this object. <br>
        /// 当前物体（食指指尖和物体指尖）距离检测范围最大值。<br> 
        /// </summary>
        public float maxInteractionDistance
        {
            get { return m_MaxInteractionDistance; }
        }

        protected bool m_IsInInteraction = false;

        /// <summary>
        /// Gets object interaction status. <br>
        /// 获取物体交互状态。
        /// </summary>
        public bool isInInteraction
        {
            get { return m_IsInInteraction; }
        }

        //当前交互位置到collider距离
        protected float m_TouchableDistance = float.PositiveInfinity;

        /// <summary>
        /// Gets current distance between interaction finger tip and object on pressable direction. <br>
        /// 获取当前物体在按压方向上与手的距离。
        /// </summary>
        public float touchableDistance { get { return m_TouchableDistance; } }

        /// <summary>
        /// Called when the interaction finger comes close into the checking area of object. <br>
        /// 当用户交互手指靠近物体进入检测范围时调用。
        /// </summary>
        public virtual void OnComeClose()
        {
            m_IsInInteraction = true;
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnComeClose " + gameObject.name);
        }

        /// <summary>
        /// Called when the interaction finger leaves the checking area of object. <br>
        /// 当用户交互手指远离物体退出检测范围时调用。
        /// </summary>
        public virtual void OnLeaveFar()
        {
            m_IsInInteraction = false;
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnLeaveFar " + gameObject.name);
        }

        /// <summary>
        /// Continuously called when the interaction finger is in the checking area of object. <br>
        /// 当用户交互手指在物体检测范围时连续调用。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distance">Distance between interaction finger tip and object on pressable direction <br>当前物体在按压方向上与手的距离.</param>
        public virtual void OnTouchUpdate(Vector3 tipPos, float distance) 
        {

        }

        /// <summary>
        /// Calculates current distance between interaction finger tip and object. <br>
        /// 计算当前物体与手的距离。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        public virtual float DistanceToTouchable(Vector3 tipPoint)
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Don't use base.DistanceToTouchable or use this directly, please override it");
            return Vector3.Distance(transform.position, tipPoint);
        }

        /// <summary>
        /// Checks whether the object is touchble based on current finger tip position. <br>
        /// 获取当前物体是否可被手指触碰。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distanceOnPressableDirection">Distance on pressable direction. <br>指尖在按压方向上的距离.</param>
        /// <param name="normal">Object normal <br>物体法线.</param>
        /// <returns>Whether the object can be interacted based on the current finger position. <br>在当前手的位置上，物体是否可能被交互</returns>
        public virtual bool CheckTouchable(Vector3 tipPoint, out float distanceOnPressableDirection, out Vector3 normal) 
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Don't use base.CheckTouchable or use this directly, please override it");
            distanceOnPressableDirection = float.MaxValue;
            normal = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Checks whether the object is grabbable. <br>
        /// 获取当前物体是否为可抓取物体。
        /// </summary>
        public virtual bool CheckGrabbable()
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Don't use base.CheckGrabbable or use this directly, please override it");
            return false;
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并进行捏取操作时调用。
        /// </summary>
        /// <param name="fingerPosition">Start pinch point. <br>近场交互捏取点.</param>
        public virtual void OnPinchDown(Vector3 fingerPosition)
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnPinchDown " + gameObject.name);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public virtual void OnPinchUp()
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log("OnPinchUp " + gameObject.name);
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="fingerPosition">pinch position. <br>近场交互捏取点.</param>
        public virtual void OnDragging(Vector3 fingerPosition)
        {

        }
    }
}
