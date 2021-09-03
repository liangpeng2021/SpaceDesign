using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class control the corner and edge size on target boundingbox. <br>
    /// 用于控制目标包围盒上所有边角大小的类。
    /// </summary>
    public class UnScale : MonoBehaviour
    {
        BoxCollider m_Collider;
        Transform m_MainCamera;


        GameObject m_Model;
        // original scale of the model wrapped by the boxCol
        Vector3 m_OriginalModelScale; 
        float m_MinWrapperScaleRatio = 1.0f;
        float m_MaxModelScaleRatio = 1.0f;
        float m_Dia;
        float m_DistanceToCamera;

        /// <summary>
        /// Initialized the current bounding box handler. <br>
        /// 初始化当前包围盒交互物体大小控制。
        /// </summary>
        public void Init()
        {
            m_Collider = gameObject.GetComponent<BoxCollider>();
            m_MainCamera = XRCameraManager.Instance.stereoCamera.transform;
            if (transform.childCount > 0)
            {
                m_Model = transform.GetChild(0).gameObject;
                m_OriginalModelScale = transform.GetChild(0).localScale;
            }
            else
            {
                m_Model = null;
                m_OriginalModelScale = Vector3.zero;
            }
        }

        /// <summary>
        /// Update the scale of model
        /// 更新边角构建的模型尺寸
        /// </summary>
        public void UpdateModelScale(float currentWrapperScaleOverOriginal)
        {
            if (m_MaxModelScaleRatio > 1.0f && m_MinWrapperScaleRatio < 1.0f)
            {
                float k = (m_MaxModelScaleRatio - 1.0f) / (m_MinWrapperScaleRatio - 1.0f);
                float b = (m_MinWrapperScaleRatio - m_MaxModelScaleRatio) / (m_MinWrapperScaleRatio - 1.0f);
                float modelScaleRatio = k * currentWrapperScaleOverOriginal + b;
                modelScaleRatio = (modelScaleRatio > 1.0f) ? modelScaleRatio : 1.0f;
                if (m_Model != null)
                {
                    m_Model.transform.localScale = m_OriginalModelScale * modelScaleRatio;
                }
            }
        }
        


        /// <summary>
        /// Set the maximum ratio of current model scale over original, above which the handler model can't be scaled up more.
        /// 设置模型关于初始尺寸放大倍数的上限
        /// </summary>

        public void SetMaxModelScaleRatio(float r)
        {
            m_MaxModelScaleRatio = r;
        }

        /// <summary>
        /// Set the minimum ratio of current wrapper scale over original, below which the wrapper can't be shrinked more.
        /// 设置控制器关于初始尺寸缩小倍数的下限
        /// </summary>
        public void SetMinWrapperScaleRatio(float r)
        {
            m_MinWrapperScaleRatio = r;
        }

        /// <summary>
        /// Updates object diagonal. <br>
        /// 更新物体对角线距离。
        /// </summary>
        public void UpdateSize()
        {
            m_DistanceToCamera = Vector3.Distance(m_MainCamera.position, transform.position);
            m_Dia = GetDiagonal();
        }


        /// <summary>
        /// Gets object diagonal. <br>
        /// 获取物体包围盒对角线距离。
        /// </summary>
        public float GetDiagonal()
        {
            Vector3 leftLowerForward = transform.TransformPoint(m_Collider.center + new Vector3(-m_Collider.size.x, -m_Collider.size.y, -m_Collider.size.z) * 0.5f);
            Vector3 center = transform.TransformPoint(m_Collider.center);

            float dia = Vector3.Distance(leftLowerForward, center);
            return dia;
            
        }

        /// <summary>
        /// Get the longest edge of the object
        /// 获取物体最长的边
        /// </summary>
        public float GetLongestEdge()
        {
            Vector3 worldScale = GetWorldScale(m_Collider.transform, m_Collider.size);
            return Mathf.Max(Mathf.Max(worldScale.x, worldScale.y), worldScale.z);
        }

        /// <summary>
        /// Gets object display angle on screen. <br>
        /// 获取物体显示角度。
        /// </summary>
        public float GetFovAngle()
        {
            Vector3 leftLowerForward = transform.TransformPoint(m_Collider.center + new Vector3(-m_Collider.size.x, -m_Collider.size.y, -m_Collider.size.z) * 0.5f);
            Vector3 rightUpperBackward = transform.TransformPoint(m_Collider.center + new Vector3(+m_Collider.size.x, +m_Collider.size.y, +m_Collider.size.z) * 0.5f);
            Vector3 cameraToLeftLowerForward = leftLowerForward - m_MainCamera.position;
            Vector3 cameraToRightUpperBackward = rightUpperBackward - m_MainCamera.position;
            float angle = Vector3.Angle(cameraToLeftLowerForward, cameraToRightUpperBackward);
            return angle;
        }

        /// <summary>
        /// Sets object size based on ratio. <br>
        /// 根据缩放比例设置物体大小。
        /// </summary>
        public void SetScreenSize(float ratio)
        {
            transform.localScale *= ratio;
        }

        /// <summary>
        /// Gets object rescale ratio based on the object display angle. <br>
        /// 根据物体显示角度获取缩放比例。
        /// </summary>
        public float GetRescaleRatio(float degree)
        {
            float diaTemp = m_DistanceToCamera * Mathf.Tan(degree * Mathf.Deg2Rad);

            float ratio = diaTemp / m_Dia;

            return ratio;
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
    }
}