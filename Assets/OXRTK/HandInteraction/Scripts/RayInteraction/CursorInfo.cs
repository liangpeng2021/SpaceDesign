using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The struct for different cursor visualization in different status. <br>
    /// 控制光标在不同状态下进行不同显示的结构。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct CursorStatus
    {
        /// <summary>
        /// Target action. <br>
        /// 目标操作类型。
        /// </summary>
        public BoundsAction statusAction;

        /// <summary>
        /// The name of current status. <br>
        /// 当前状态名。
        /// </summary>
        public string statusName;

        /// <summary>
        /// The visualization object of current status. <br>
        /// 当前状态对应显示物体。
        /// </summary>
        public GameObject statusObject;
        
    }

    /// <summary>
    /// The class for cursor control in ray interaction. <br>
    /// 远端交互中控制光标的类。
    /// </summary>
    public class CursorInfo : MonoBehaviour
    {
        /// <summary>
        /// The component used for cursor visualization. <br>
        /// 控制当前光标显示的部件。
        /// </summary>
        public CursorVisualizationControl cursorVisual;

        Vector3 m_OriginalLocalScale;
        RayInteractionPointer m_parentPointer;

        void Awake()
        {
            m_OriginalLocalScale = cursorVisual.transform.localScale;
        }

        void Start()
        {
            UpdateCursorStatus();
        }

        /// <summary>
        /// Initializes cursor visualization. <br>
        /// 初始化光标可视化。
        /// </summary>
        public void Init(RayInteractionPointer pPointer)
        {
            m_parentPointer = pPointer;
            m_parentPointer.onActionEvent.AddListener(OnEvent);
        }

        void OnEvent(PointerEventType eventType, GameObject obj = null)
        {
            switch (eventType)
            {
                case PointerEventType.OnPinchDown:
                    cursorVisual.transform.localScale = new Vector3(m_OriginalLocalScale.x * 0.4f, m_OriginalLocalScale.y * 0.4f, m_OriginalLocalScale.z);
                    break;

                case PointerEventType.OnPinchUp:
                    cursorVisual.transform.localScale = m_OriginalLocalScale;
                    break;

                case PointerEventType.OnPointerStart:
                    if(obj != null && obj.GetComponent<BoundingBoxRayReceiverHelper>() != null)
                    {
                        UpdateCursorStatus(obj.GetComponent<BoundingBoxRayReceiverHelper>().targetAction);
                    }
                    break;
                case PointerEventType.OnPointerEnd:
                    if (obj != null && obj.GetComponent<BoundingBoxRayReceiverHelper>() != null)
                    {
                        UpdateCursorStatus();
                    }
                    break;
            }
        }

        void UpdateCursorStatus(BoundsAction action = BoundsAction.None)
        {
            if(cursorVisual != null)
                cursorVisual.UpdateCursor(action);
        }

        /// <summary>
        /// Updates cursor position and rotation. <br>
        /// 更新光标位置和旋转信息。
        /// </summary>
        public void UpdateTransform(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        /// <summary>
        /// Reset cursor status. <br>
        /// 重置光标状态。
        /// </summary>
        public void Reset()
        {
            UpdateCursorStatus();
        }
    }
}