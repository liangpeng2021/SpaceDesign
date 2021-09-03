using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class to control the size of all of the corners and edges on the current bounding box. <br>
    /// 用于管理当前包围盒上所有边角大小的类。
    /// </summary>
    public class BoundsScaleController : MonoBehaviour
    {
        BoxCollider boundsCollider;
        List<UnScale> m_Corners = new List<UnScale>();
        List<UnScale> m_Edges = new List<UnScale>();

        float m_MinAngle = 4.5f;
        float m_MaxAngle = 7.0f;
        Vector3 m_OriginalBoundsWorldSize;
        bool m_IsRotation = false;

        private void Start()
        {
            m_OriginalBoundsWorldSize = GetWorldScale(boundsCollider.transform, boundsCollider.size);
        }

        void Update()
        {
            float maxsize = 0f;
            UnScale maxObj = null;
            if (m_Corners.Count <= 0)
                return;
            
            foreach(UnScale corner in m_Corners)
            {
                corner.UpdateSize();
                float cornerAngle = corner.GetFovAngle();
                if(maxsize < cornerAngle)
                {
                    maxObj = corner;
                    maxsize = cornerAngle;
                }
            }

            if (maxObj == null)
                return;


            if (!m_IsRotation)
            {
                if (maxsize > m_MaxAngle)
                {
                    float r = maxObj.GetRescaleRatio(m_MaxAngle / 2);
                    foreach (UnScale corner in m_Corners)
                    {
                        corner.SetScreenSize(r);
                    }
                    foreach (UnScale edge in m_Edges)
                    {
                        edge.SetScreenSize(r);
                    }
                }
                else if (maxsize < m_MinAngle)
                {
                    float r = maxObj.GetRescaleRatio(m_MinAngle / 2);
                    foreach (UnScale corner in m_Corners)
                    {
                        corner.SetScreenSize(r);
                    }
                    foreach (UnScale edge in m_Edges)
                    {
                        edge.SetScreenSize(r);
                    }
                }
            }

            float minHandlerEdgeLengthAddUp = m_Corners[0].GetLongestEdge() + m_Edges[0].GetLongestEdge();
            Vector3 boundsWorldSize = GetWorldScale(boundsCollider.transform, boundsCollider.size);
            float minBoundEdge;

            if (m_Corners.Count > 4 && m_Edges.Count > 4)
            {
                minBoundEdge = Mathf.Min(Mathf.Min(boundsWorldSize.x, boundsWorldSize.y), boundsWorldSize.z);
            } else
            {
                minBoundEdge = Mathf.Min(boundsWorldSize.x, boundsWorldSize.y);
            }

            if (minHandlerEdgeLengthAddUp > minBoundEdge)
            {
                float r = minHandlerEdgeLengthAddUp / minBoundEdge;
                foreach (UnScale edge in m_Edges)
                {
                    edge.SetScreenSize(1.0f / r);
                }
                foreach (UnScale corner in m_Corners)
                {
                    corner.SetScreenSize(1.0f / r);
                }
            }

            foreach(UnScale edge in m_Edges)
            {
                float currentWrapperScaleOverOriginal = boundsWorldSize.x / m_OriginalBoundsWorldSize.x;
                edge.UpdateModelScale(currentWrapperScaleOverOriginal);
            }

            foreach (UnScale corner in m_Corners)
            {
                float currentWrapperScaleOverOriginal = boundsWorldSize.x / m_OriginalBoundsWorldSize.x;
                corner.UpdateModelScale(currentWrapperScaleOverOriginal);
            }
        }

        /// <summary>
        /// Adds all the corners on the current bounding box into the list. <br>
        /// 将当前包围盒上所有角点加入列表。
        /// </summary>
        public void AddCorner(UnScale corner)
        {
            if(!m_Corners.Contains(corner))
                m_Corners.Add(corner);
        }

        /// <summary>
        /// Set the value of isRotation, which if is true disables size change of model due to fov changes
        /// 设置isRotation的值，如果isRotation为真，边角构件的大小不会随fov变化而变化
        /// </summary>
        public void SetIsRotation(bool value)
        {
            m_IsRotation = value;
        }

        /// <summary>
        /// Adds all the edges on the current bounding box into the list. <br>
        /// 将当前包围盒上所有边加入列表。
        /// </summary>
        public void AddEdge(UnScale edge)
        {
            if(!m_Edges.Contains(edge))
                m_Edges.Add(edge);
        }

        /// <summary>
        /// Get the box collider used for create bounding box. <br>
        /// 获取用于生成包围盒的碰撞盒。
        /// </summary>
        public void SetBoundCollider(BoxCollider collider)
        {
            boundsCollider = collider;
        }

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
        /// Update z local scale for flat object. <br>
        /// 更新平面物体的z轴大小。
        /// </summary>
        public void UpdateFlatZScale(float zMul)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, zMul);
        }

    }
}
