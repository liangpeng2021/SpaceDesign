using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for bounding box ray interaction receiver. <br>
    /// 包围盒接收端远端交互的类。
    /// </summary>
    public class BoundingBoxRayReceiverHelper : RayPointerHandler
    {
        BoundsActionAxis m_TargetLocalAxis = BoundsActionAxis.None;
        
        BoundsAction m_TargetAction = BoundsAction.None;

        /// <summary>
        /// Target action of this handler. <br>
        /// 当前控件目标操作。
        /// </summary>
        public BoundsAction targetAction
        {
            get { return m_TargetAction; }
        }

        bool m_IsDragging = false;
        Vector3 m_DragStartPos;
        Vector3 m_DragDir;

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

        public Action onPointerEnter, onPointerExit, onPinchDown, onPinchUp;
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

        private void Start()
        {
            m_IsLockCursor = true;
        }

        void Update()
        {
            if (m_IsDragging)
            {
                DragAction();
            }
        }

        /// <summary>
        /// Called when the laser points to the object. <br>
        /// 当射线打中物体时调用。
        /// </summary>
        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            onPointerEnter?.Invoke();
            m_TargetObject.SetFocusStatus(true);
        }

        /// <summary>
        /// Called when the laser exit the object. <br>
        /// 当射线离开物体时调用。
        /// </summary>
        public override void OnPointerExit()
        {
            base.OnPointerExit();
            onPointerExit?.Invoke();
            m_TargetObject.SetFocusStatus(false);
        }

        /// <summary>
        /// Called when the user pinches down on the object. <br>
        /// 当射线打中物体并按下时调用。
        /// </summary>
        /// <param name="startPoint">Start point of ray in far interaction. <br>远端射线起点位置.</param>
        /// <param name="direction">Direction of the ray in far interaction. <br>远端射线方向.</param>
        /// <param name="targetPoint">End position of the ray in far interaction. <br>远端射线终点打到的位置.</param>
        public override void OnPinchDown(Vector3 startPoint, Vector3 direction, Vector3 targetPoint)
        {
            base.OnPinchDown(startPoint, direction, targetPoint);
            onPinchDown?.Invoke();
            m_TargetObject.SetAllChildrenStatus(true);
            m_TargetObject.StartRayAction(m_TargetAction, m_TargetLocalAxis, startPoint, direction, targetPoint);
        }

        /// <summary>
        /// Called when the user pinches up on the object. <br>
        /// 当射线松开时调用。
        /// </summary>
        public override void OnPinchUp()
        {
            base.OnPinchUp();
            onPinchUp?.Invoke();
            m_TargetObject.SetAllChildrenStatus(false);
            m_TargetObject.EndAction(m_TargetAction);
            m_IsDragging = false;
        }

        /// <summary>
        /// Called when the user drags the object. <br>
        /// 当用户拖拽物体时调用。
        /// </summary>
        /// <param name="startPosition">The start position of laser. <br>射线起点.</param>
        /// <param name="direction">The direction of laser. <br>射线方向.</param>
        public override void OnDragging(Vector3 startPosition, Vector3 direction)
        {
            m_IsDragging = true;
            m_DragStartPos = startPosition;
            m_DragDir = direction;
        }

        void DragAction()
        {
            base.OnDragging(m_DragStartPos, m_DragDir);
            m_TargetObject.UpdateRayAction(m_DragStartPos, m_DragDir);
        }

        void SetInteractionStatus(bool status)
        {
            m_IsInInteraction = status;
        }
    }
}
