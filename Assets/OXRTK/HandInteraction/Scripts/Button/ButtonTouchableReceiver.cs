using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for pressable button ui interaction receiver. <br>
    /// 可按压按键接收端近场交互的类。<br>
    /// Pressable direction is position direction on Z axis.<br>
    /// 按压方向需要为z轴正向。
    /// </summary>
    public class ButtonTouchableReceiver : UiInteractionPointerHandler
    {
        /// <summary>
        /// When the pressable button is not at the center of Box collider local Z axis, set it true. <br>
        /// 当按压最深位置不在按键碰撞盒z轴中心时置为true，否则为false。
        /// </summary>
        public bool distanceThresholdOverride = false;

        /// <summary>
        /// When distanceThresholdOverride is true, this parameter is used for setting the max pressable distance based on object's local scale. If the distanceThresholdOverride is flase, use the center of box collider. <br>
        /// 物体自身坐标系下，在本地z轴上最大按压距离（在物体坐标系的尺度下），当distanceThresholdOverride为true使用该值进行设置，否则使用碰撞盒中心进行计算。
        /// If distanceThresholdOverride is false, localPressableDistanceThreshold value doesn't affect the result. <br>
        /// 如果 distanceThresholdOverride 为false，localPressableDistanceThreshold 数值不用于计算。
        /// </summary>
        [Min(0)]
        public float localPressableDistanceThreshold = 1f;

        /// <summary>
        /// The handler to press on the button. <br>
        /// 按键在按压时被按下的部分。
        /// </summary>
        public Transform pressableHandler;

        /// <summary>
        /// The box collider used for detecting the press action of button, the default box collider is the one on object, need to cover both the button bottom and button pressable handler. <br>
        /// 用于检测按压按钮的包围盒，默认为transform上的BoxCollider，需要将按键底部和按压按键都框住。
        /// </summary>
        public BoxCollider colliderOverride;

        /// <summary>
        /// Called when the button is pressed to the bottom. <br>
        /// 当按键被按到底部时触发。
        /// </summary>
        public UnityEvent onClick;

        /// <summary>
        /// Called when the button start to be pressed down. <br>
        /// 当按键开始被按下时触发。
        /// </summary>
        public UnityEvent onPressDown;

        /// <summary>
        /// Called when the finger leave the button. <br>
        /// 当手指离开按键或按下弹起时触发。
        /// </summary>
        public UnityEvent onPressUp;

        /// <summary>
        /// Called when the button start able to interact with. <br>
        /// 当按键开始可被交互时触发。
        /// </summary>
        public UnityEvent onInteractionEnabled;

        /// <summary>
        /// Called when the button start unable to interact with. <br>
        /// 当按键开始不再可被交互时触发。
        /// </summary>
        public UnityEvent onInteractionDisabled;

        //被按压物体的起始localPosition
        private Vector3 m_HandlerStartPosition;
        //在按压方向上的最大按压距离（> 0）
        private float m_DistanceThreshold;
        //按钮collider检测时，collider中心（local坐标系下）
        private Vector3 m_ColliderCenter = Vector3.zero;
        //用于检测交互位置是否在boxCollider x轴和y轴交互范围内
        private Vector2 m_Bounds;
        //前一次按压方向上距离，用于判断按压方向
        private float m_PreviousDistance = float.NegativeInfinity;
        //用于判断计算是否进行按压计算
        private bool m_UpdatePress = false;
        //当按键按到底部时，参数为true，当交互位置仍在collider内但运动方向为local z负向时(抬起时)按键被锁，按压不弹起，只有当手脱离collider或超过交互距离或切换交互物体时按键才会重新弹起
        private bool m_IsLockedDir = false;
        //交互位置是否接触按钮
        private bool m_IsInTouch = false;
        //在一次按压操作中第一次按到底部时置为并保持true
        private bool m_IsClicked = false;
        //按压按键常态颜色。
        Color m_NormalColor = Color.white;

        bool m_StartTouching = false;
        float m_PrevDis = float.PositiveInfinity;

        void Start()
        {
            InitTouchableCollider();
            if (pressableHandler != null)
            {
                m_HandlerStartPosition = pressableHandler.localPosition;
            }
        }

        //初始化交互信息
        void InitTouchableCollider()
        {
            if(colliderOverride == null)
            {
                if (gameObject.GetComponent<BoxCollider>() != null)
                    colliderOverride = gameObject.GetComponent<BoxCollider>();
                else
                {
                    colliderOverride = gameObject.AddComponent<BoxCollider>();
                    colliderOverride.size = new Vector3(1, 1, 2);
                    colliderOverride.center = new Vector3(0, 0, 0.5f);
                    Debug.LogError("No boxColliderDetected, use the default setting");
                }
            }

            Vector2 adjustedSize = new Vector2(Mathf.Abs(colliderOverride.size.x), Mathf.Abs(colliderOverride.size.y));

            InitBounds(adjustedSize);
            InitDistanceThreshold();
            InitLocalCenter(colliderOverride.center);
        }

        //初始化button 交互范围
        void InitBounds(Vector2 bounds)
        {
            m_Bounds = bounds;
        }

        //初始化button collider中心
        void InitLocalCenter(Vector3 center)
        {
            m_ColliderCenter = center;
        }

        //初始化button 最大按下距离
        void InitDistanceThreshold()
        {
            if (distanceThresholdOverride)
            {
                Vector3 localThresholdVector = new Vector3(0, 0, localPressableDistanceThreshold);
                m_DistanceThreshold = GetWorldScale(transform, localThresholdVector).z;
            }
            else
            {
                m_DistanceThreshold = GetWorldScale(transform, colliderOverride.size).z / 2;
            }
        }

        //计算localScale 在x, y, z上的worldScale
        Vector3 GetWorldScale(Transform t, Vector3 localScale)
        {
            Transform temp = t;
            Vector3 tempScale = localScale;

            while(temp != null)
            {
                tempScale.x *= temp.localScale.x;
                tempScale.y *= temp.localScale.y;
                tempScale.z *= temp.localScale.z;

                temp = temp.parent;
            }
            return tempScale;
        }

        //计算worldScale在t local坐标系下 x, y, z的localScale
        Vector3 GetLocalScale(Transform t, Vector3 worldScale)
        {
            Transform temp = t.parent;
            Vector3 tempScale = worldScale;

            while (temp != null)
            {
                tempScale.x /= temp.localScale.x;
                tempScale.y /= temp.localScale.y;
                tempScale.z /= temp.localScale.z;

                temp = temp.parent;
            }
            return tempScale;
        }
        
        /// <summary>
        /// Calculates current distance between interaction finger tip and object on pressable direction. <br>
        /// 计算当前物体在按压方向上与手的距离。
        /// </summary>
        /// <param name="tipPoint">Finger tip position <br>
        /// 交互指尖位置.</param>
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

            Vector3 localPoint = transform.InverseTransformPoint(tipPoint) - m_ColliderCenter;

            if (localPoint.x < -m_Bounds.x / 2 ||
                localPoint.x > m_Bounds.x / 2 ||
                localPoint.y < -m_Bounds.y / 2 ||
                localPoint.y > m_Bounds.y / 2)
            {
                distanceOnPressableDirection = float.PositiveInfinity;
                if (m_StartTouching)
                {
                    m_StartTouching = false;
                    m_PrevDis = distanceOnPressableDirection;
                }
                return false;
            }

            if (m_TouchableDistance > maxInteractionDistance)
            {
                distanceOnPressableDirection = float.PositiveInfinity;
               
                return false;
            }

            Vector3 offset = Vector3.zero;
            if (distanceThresholdOverride)
                offset = new Vector3(0, 0, colliderOverride.size.z / 2 - localPressableDistanceThreshold);
            localPoint += offset;

            Vector3 localPointInWorldScale = GetWorldScale(transform, localPoint);
            distanceOnPressableDirection = -localPointInWorldScale.z;

            if (!m_StartTouching)
            {
                if (Mathf.Abs(distanceOnPressableDirection) < Mathf.Abs(m_DistanceThreshold))
                {
                    m_StartTouching = true;
                    m_PrevDis = distanceOnPressableDirection;
                }
            }
            else
            {
                if (Mathf.Abs(distanceOnPressableDirection) > Mathf.Abs(m_DistanceThreshold))
                {
                    m_PrevDis = distanceOnPressableDirection;
                    m_StartTouching = false;
                }
            } 
            return true;
        }

        /// <summary>
        /// Called when the interaction finger starts touching the object. <br>
        /// 当用户交互手指碰到物体时调用。
        /// </summary>
        public void OnTouchEnter()
        {
            Debug.Log("MyLog::m_IsInTouch:"+ m_IsInTouch);
            if (!m_IsInTouch)
            {
                m_IsInTouch = true;
                onPressDown?.Invoke();
                m_IsClicked = false;
            }
        }

        /// <summary>
        /// Called when the interaction finger ends touching the object. <br>
        /// 当用户交互手指离开物体时调用。
        /// </summary>
        public void OnTouchExit()
        {
            if (m_IsInTouch)
            {
                if (!m_UpdatePress)
                {
                    m_UpdatePress = true;
                }
                m_IsLockedDir = false;

                pressableHandler.localPosition = m_HandlerStartPosition;

                m_IsInTouch = false;
                onPressUp?.Invoke();
            }
        }

        /// <summary>
        /// Called when the interaction finger comes close into the checking area of object. <br>
        /// 当用户交互手指靠近物体进入检测范围时调用。
        /// </summary>
        public override void OnComeClose()
        {
            base.OnComeClose();
            onInteractionEnabled?.Invoke();
            if (!m_UpdatePress)
            {
                m_UpdatePress = true;
            }
            m_IsLockedDir = false;
        }

        /// <summary>
        /// Called when the interaction finger leaves the checking area of object. <br>
        /// 当用户交互手指远离物体退出检测范围时调用。
        /// </summary>
        public override void OnLeaveFar()
        {
            base.OnLeaveFar();
            if (m_UpdatePress)
            {
                m_UpdatePress = false;
                OnTouchExit();
                onInteractionDisabled?.Invoke();
                m_PreviousDistance = float.PositiveInfinity;
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
            if (!m_UpdatePress)
                return;

            if (pressableHandler != null)
            {
                if (distance < m_DistanceThreshold && distance >= 0 && !m_IsLockedDir)
                {
                    if (m_PrevDis <= 0)
                        return;

                    OnTouchEnter();
                    Vector3 newLocalosition =
                        m_HandlerStartPosition + GetLocalScale(pressableHandler, new Vector3(0, 0, (m_DistanceThreshold - distance)));

                    pressableHandler.localPosition = newLocalosition;
                } 
                // 按下到负向
                else if (distance < 0 ) 
                {
                    if (m_PrevDis <= 0)
                        return;

                    OnTouchEnter();
                    if (!m_IsClicked)
                    {
                        m_IsClicked = true;
                        onClick?.Invoke();
                    }
                    Vector3 newLocalosition =
                        m_HandlerStartPosition + GetLocalScale(pressableHandler, new Vector3(0, 0, m_DistanceThreshold));
                    pressableHandler.localPosition = newLocalosition;
                    if (!m_IsLockedDir)
                    {
                        m_IsLockedDir = true;
                    }
                }

                if(distance >= m_DistanceThreshold)
                {
                    OnTouchExit();
                }
            }
            if (m_IsLockedDir)
                m_PreviousDistance = Mathf.Min(m_PreviousDistance, distance);
            else
                m_PreviousDistance = distance;
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
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并进行捏取操作时调用。
        /// </summary>
        public override void OnPinchDown(Vector3 fingerPosition)
        {
            base.OnPinchDown(fingerPosition);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            base.OnPinchUp();
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        public override void OnDragging(Vector3 fingerPosition)
        {
            base.OnDragging(fingerPosition);
        }
    }
}
