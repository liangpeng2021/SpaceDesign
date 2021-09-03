using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for slate ui interaction receiver. <br>
    /// 面板接收端近场UI交互的类。
    /// </summary>
    public class SlateTouchableReceiver : UiInteractionPointerHandler
    {
        /// <summary>
        /// The BoxCollider that used for detecting the interaction, use the BoxCollider on the current object if leave it blank. <br>
        /// 用于检测抓取的BoxCollider，如果将其预留为空则默认为当前物体上的BoxCollider。
        /// </summary>
        public BoxCollider colliderOverride;

        /// <summary>
        /// Called when the finger start touching the slate. <br>
        /// 当手指碰到面板时触发。
        /// </summary>
        public UnityEvent onTouchDown;
        
        /// <summary>
        /// Called when the finger leave the slate. <br>
        /// 当手指离开面板时触发。
        /// </summary>
        public UnityEvent onTouchUp;

        private SlateController m_SlateController;
        private Vector3 m_ColliderCenter;
        private Vector2 m_Bounds;
        private bool m_UpdatePress;
        private bool m_IsInTouch;
        private bool m_IsActive = true;

        void Start()
        {
            InitTouchableCollider();
            if (gameObject.GetComponent<SlateController>() != null)
                m_SlateController = gameObject.GetComponent<SlateController>();
            else
                m_IsActive = false;
        }

        /// <summary>
        /// Initializes slate ui interaction receiver detection. <br>
        /// 初始化面板接收端近场UI交互检测。
        /// </summary>
        public void InitTouchableCollider()
        {
            if (colliderOverride == null)
            {
                if (gameObject.GetComponent<BoxCollider>() != null)
                    colliderOverride = gameObject.GetComponent<BoxCollider>();
                else
                {
                    colliderOverride = gameObject.AddComponent<BoxCollider>();
                    Debug.LogError("No boxColliderDetected, use the default setting");
                }
            }

            Vector2 adjustedSize = new Vector2(Mathf.Abs(colliderOverride.size.x), Mathf.Abs(colliderOverride.size.y));

            InitBounds(adjustedSize);
            InitLocalCenter(colliderOverride.center);
        }

        //初始化交互范围
        void InitBounds(Vector2 bounds)
        {
            m_Bounds = bounds;
        }

        //初始化button collider中心
        void InitLocalCenter(Vector3 center)
        {
            m_ColliderCenter = center;
        }

        void UpdateSlateDisplay(Vector3 tip)
        {
            if (m_IsInTouch)
            {
                m_SlateController.UpdatePointerUVCoord(tip, true);
            }
            else
            {
                OnTouchEnter(tip);
            }
        }

        //计算localScale 在x, y, z上的worldScale
        Vector3 GetWorldScale(Transform t, Vector3 localScale)
        {
            Transform temp = t;
            Vector3 tempScale = localScale;

            while (temp != null)
            {
                tempScale.x *= temp.localScale.x;
                tempScale.y *= temp.localScale.y;
                tempScale.z *= temp.localScale.z;

                temp = temp.parent;
            }
            return tempScale;
        }

        /// <summary>
        /// Checks whether the object is touchble based on current finger tip position. <br>
        /// 获取当前物体是否可被手指触碰。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distanceToSlate">Distance on pressable direction. <br>指尖在按压方向上到面板的距离.</param>
        /// <param name="normal">Object normal <br>物体法线.</param>
        /// <returns>Whether the object can be interacted based on the current finger position. <br>在当前手的位置上，物体是否可能被交互</returns>
        public override bool CheckTouchable(Vector3 tipPoint, out float distanceToSlate, out Vector3 normal)
        {
            normal = -transform.forward;

            Vector3 localPoint = transform.InverseTransformPoint(tipPoint) - m_ColliderCenter;

            if (localPoint.x < -m_Bounds.x / 2 ||
                localPoint.x > m_Bounds.x / 2 ||
                localPoint.y < -m_Bounds.y / 2 ||
                localPoint.y > m_Bounds.y / 2)
            {
                distanceToSlate = float.PositiveInfinity;
                return false;
            }

            if (m_TouchableDistance > maxInteractionDistance)
            {
                distanceToSlate = float.PositiveInfinity;
                return false;
            }

            localPoint = GetWorldScale(transform, localPoint);
            distanceToSlate = Mathf.Abs(-localPoint.z);
            return true;
        }

        /// <summary>
        /// Calculates current distance between interaction finger tip and object on pressable direction. <br>
        /// 计算当前物体在按压方向上与手的距离。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        public override float DistanceToTouchable(Vector3 tipPoint)
        {
            Vector3 bottomCenterPoint = transform.TransformPoint((colliderOverride.center + new Vector3(0, 0, colliderOverride.size.z / 2)));
            Ray ray = new Ray(tipPoint, Vector3.Normalize(bottomCenterPoint - tipPoint));


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

            if (Vector3.Dot((bottomCenterPoint - tipPoint), colliderOverride.transform.forward) < GetWorldScale(colliderOverride.transform, new Vector3(0, 0, colliderOverride.size.z)).z)
            {
                m_TouchableDistance = 0;
            }
            return m_TouchableDistance;
        }

        /// <summary>
        /// Called when the interaction finger comes close into the checking area of slate. <br>
        /// 当用户交互手指靠近面板进入检测范围时调用。
        /// </summary>
        public override void OnComeClose()
        {
            
            if (!m_IsActive)
                return;
            base.OnComeClose();
            if (!m_UpdatePress)
            {
                m_UpdatePress = true;
            }
        }

        /// <summary>
        /// Called when the interaction finger leaves the checking area of slate. <br>
        /// 当用户交互手指远离面板退出检测范围时调用。
        /// </summary>
        public override void OnLeaveFar()
        {
            if (!m_IsActive)
                return;
            base.OnLeaveFar();
                       
            if (m_UpdatePress)
            {
                m_UpdatePress = false;
                OnTouchExit();
            }
        }

        /// <summary>
        /// Called when the interaction finger starts touching the slate. <br>
        /// 当用户交互手指碰到面板时调用。
        /// </summary>
        public void OnTouchEnter(Vector3 interactionPosition)
        {
            if (!m_IsActive)
                return;
            if (!m_IsInTouch)
            {
                m_IsInTouch = true;
                onTouchDown?.Invoke();

                m_SlateController.UpdatePointerUVStartCood(interactionPosition);
                if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Slate On Touch Enter");
            }
        }

        /// <summary>
        /// Called when the interaction finger ends touching the slate. <br>
        /// 当用户交互手指离开面板时调用。
        /// </summary>
        public void OnTouchExit()
        {
            if (!m_IsActive)
                return;
            if (m_IsInTouch)
            {
                if (!m_UpdatePress)
                {
                    m_UpdatePress = true;
                }

                m_IsInTouch = false;
                onTouchUp?.Invoke();
                if (HandTrackingPlugin.debugLevel > 0) Debug.Log("Slate On Touch Exit");
            }
        }

        /// <summary>
        /// Continuously called when the interaction finger is in the checking area of object. <br>
        /// 当用户交互手指在物体检测范围时连续调用。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>交互指尖位置.</param>
        /// <param name="distance">Distance between interaction finger tip and object on pressable direction <br>当前物体在按压方向上与手的距离.</param>
        public override void OnTouchUpdate(Vector3 tipPoint, float distance)
        {
            base.OnTouchUpdate(tipPoint, distance);

            if (!m_IsActive)
                return;
            if (!m_UpdatePress)
                return;

            Vector3 localPoint = transform.InverseTransformPoint(tipPoint) - m_ColliderCenter;
            if (-localPoint.z <= 0) 
            {
                UpdateSlateDisplay(tipPoint);
            }
            else
            {
                OnTouchExit();
            }

        }

        /// <summary>
        /// Checks whether the object is grabbable. <br>
        /// 获取当前物体是否为可抓取物体。
        /// </summary>
        public override bool CheckGrabbable()
        {
            return false;
        }

        /// <summary>
        /// Called when the user pinches down on the slate. <br>
        /// 当用户手在面板上并进行捏取操作时调用。
        /// </summary>
        /// <param name="fingerPosition">Start pinch point. <br>近场交互捏取点.</param>
        public override void OnPinchDown(Vector3 fingerPosition)
        {
            base.OnPinchDown(fingerPosition);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当捏取松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            base.OnPinchUp();
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="fingerPosition">pinch position. <br>近场交互捏取点.</param>
        public override void OnDragging(Vector3 fingerPosition)
        {
            base.OnDragging(fingerPosition);
        }
    }
}
