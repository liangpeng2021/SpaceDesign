using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for bounding box ui interaction receiver. <br>
    /// 包围盒接收端近场交互的类。
    /// </summary>
    public class BoundingBoxTouchableReceiverHelper : UiInteractionPointerHandler
    {
        /// <summary>
        /// BoxCollider that used for detect the interaction, use the BoxCollider on the current object if leave it blank. <br>
        /// 用于检测抓取的BoxCollider，如果将其预留为空则默认为当前物体上的BoxCollider。
        /// </summary>
        public BoxCollider colliderOverride;
        public Action onInteractionEnabled, onInteractionDisabled, onPinchDown, onPinchUp;
        //按钮collider检测时，collider中心（local坐标系下）
        Vector3 m_ColliderCenter = Vector3.zero;
        //用于检测交互位置是否在boxCollider范围内
        Vector3 m_Bounds;

        BoundsActionAxis m_TargetLocalAxis = BoundsActionAxis.None;

        BoundsAction m_TargetAction = BoundsAction.None;
        public BoundsAction targetAction
        {
            get { return m_TargetAction; }
        }

        Transform m_BoundingboxRoot;
        BoundingBox m_TargetObject;

        /// <summary>
        /// Target bounding box. <br>
        /// 目标操作包围盒。
        /// </summary>
        public BoundingBox targetObject
        {
            get { return m_TargetObject; }
        }

        /// <summary>
        /// Initializes bounding box ray interaction receiver. <br>
        /// 初始化包围盒接收端射线交互设置。
        /// </summary>
        /// <param name="root">The bounding box root <br>包围盒节点物体.</param>
        /// <param name="target">The bounding box controller <br>当前节点的父包围盒控制脚本.</param>
        /// <param name="action">Current interaction priority<br>对应交互优先级.</param>
        /// <param name="axis">Rotate interaction axis. <br>旋转交互对应旋转轴.</param>
        public void Init(Transform root, BoundingBox target, BoundsAction action, BoundsActionAxis axis)
        {
            m_BoundingboxRoot = root;
            m_TargetObject = target;
            m_TargetLocalAxis = axis;
            m_TargetAction = action;
            m_TargetObject.onStatusChange += SetInteractionStatus;
        }


        /// <summary>
        /// Initializes bounding box ray interaction receiver. <br>
        /// 初始化包围盒接收端射线交互设置。
        /// </summary>
        /// <param name="root">The bounding box root <br>包围盒节点物体.</param>
        /// <param name="target">The bounding box controller <br>当前节点的父包围盒控制脚本.</param>
        /// <param name="action">Current interaction priority<br>对应交互优先级.</param>
        public void Init(Transform root, BoundingBox target, BoundsAction action)
        {
            m_BoundingboxRoot = root;
            m_TargetObject = target;
            m_TargetAction = action;
            m_TargetObject.onStatusChange += SetInteractionStatus;
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
                else
                {
                    colliderOverride = gameObject.AddComponent<BoxCollider>();
                    Debug.LogError("No boxColliderDetected, add a boxcollider");
                }
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

        void SetInteractionStatus(bool status)
        {
            m_IsInInteraction = status;
        }

        #region UiInteractionPointerHandler Implementation
        /// <summary>
        /// Called when the interaction finger comes close into the checking area of object. <br>
        /// 当用户交互手指靠近物体进入检测范围时调用。
        /// </summary>
        public override void OnComeClose()
        {
            base.OnComeClose();
            onInteractionEnabled?.Invoke();
            m_TargetObject.SetAllChildrenStatus(true);
            m_TargetObject.SetFocusStatus(true);
        }

        /// <summary>
        /// Called when the interaction finger leaves the checking area of object. <br>
        /// 当用户交互手指远离物体退出检测范围时调用。
        /// </summary>
        public override void OnLeaveFar()
        {
            base.OnLeaveFar();
            onInteractionDisabled?.Invoke();
            m_TargetObject.SetAllChildrenStatus(false);
            m_TargetObject.SetFocusStatus(false);
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
                if(hits[i].transform == colliderOverride.transform)
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
        /// Checks whether the object is grabbable. <br>
        /// 获取当前物体是否为可抓取物体。
        /// </summary>
        public override bool CheckGrabbable()
        {
            return true;
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并进行捏取操作时调用。
        /// </summary>
        /// <param name="fingerPosition">Start pinch point. <br>近场交互捏取点.</param>
        public override void OnPinchDown(Vector3 fingerPosition)
        {
            base.OnPinchDown(fingerPosition);
            onPinchDown?.Invoke();
            m_TargetObject.StartUiAction(m_TargetAction, m_TargetLocalAxis, fingerPosition);
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="fingerPosition">pinch position. <br>近场交互捏取点.</param>
        public override void OnDragging(Vector3 fingerPosition)
        {
            base.OnDragging(fingerPosition);
            m_TargetObject.UpdateUiAction(fingerPosition);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            onPinchUp?.Invoke();
            base.OnPinchUp();
            m_TargetObject.EndAction(m_TargetAction);
        }
        #endregion
    }
}