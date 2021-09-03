using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for cursor visualizations in ray interaction. <br>
    /// 远端交互中控制光标显示的类。
    /// </summary>
    public class CursorVisualizationControl : MonoBehaviour
    {
        [SerializeField] Transform m_CursorPivot;
        private float m_ScaleTarget = 1;
        Transform m_MainCamera;
        float m_DegreeScale = 2f;
        
        /// <summary>
        /// All of the cursor visualizations in ray interaction. <br>
        /// 远端交互中所有光标可显示的状态。
        /// </summary>
        [SerializeField]
        [Tooltip("Determines the context state when this object is targeted.")]
        public CursorStatus[] allCursorStatus;

        /// <summary>
        /// Default status of cursor visualization. <br>
        /// 光标默认显示状态。
        /// </summary>
        public BoundsAction defaultAction = BoundsAction.None;

        private Dictionary<BoundsAction, CursorStatus> m_StatusDic = new Dictionary<BoundsAction, CursorStatus>();
        
        void Awake()
        {
            foreach(CursorStatus status in allCursorStatus)
            {
                if (!m_StatusDic.ContainsKey(status.statusAction))
                    m_StatusDic.Add(status.statusAction, status);
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                m_MainCamera = XRCameraManager.Instance.eventCamera.transform;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                m_MainCamera = XRCameraManager.Instance.stereoCamera.transform;
            }
        }

        void Update()
        {
            // Calculate the realtime cursor scale;
            float camToCursorDist = Vector3.Distance(m_MainCamera.position, transform.position);
            m_ScaleTarget = camToCursorDist * Mathf.Tan(0.5f * m_DegreeScale * Mathf.Deg2Rad) /  (2*transform.parent.lossyScale.x);

            // Assign the Cursor scale
            Vector3 targetScale = new Vector3(m_ScaleTarget, m_ScaleTarget, m_ScaleTarget);
            if(m_CursorPivot != null) 
                m_CursorPivot.localScale = Vector3.MoveTowards(m_CursorPivot.localScale, targetScale, 0.3f);
        }

        public void UpdateCursor(BoundsAction action)
        {
            if (action != defaultAction)
            {
                if (m_StatusDic.ContainsKey(action) || m_StatusDic.ContainsKey(defaultAction))
                {
                    ResetCursorVisual();
                    if (m_StatusDic.ContainsKey(action))
                        m_StatusDic[action].statusObject.SetActive(true);
                    else
                        m_StatusDic[defaultAction].statusObject.SetActive(true);
                }
            }
            else
            {
                if (m_StatusDic.ContainsKey(action))
                {
                    ResetCursorVisual();
                    if (m_StatusDic.ContainsKey(action))
                        m_StatusDic[action].statusObject.SetActive(true);
                }
            }
        }

        void ResetCursorVisual()
        {
            if (m_StatusDic.Count == 0)
                return;

            foreach(CursorStatus status in m_StatusDic.Values)
            {
                status.statusObject.SetActive(false);
            }
        }
    }
}