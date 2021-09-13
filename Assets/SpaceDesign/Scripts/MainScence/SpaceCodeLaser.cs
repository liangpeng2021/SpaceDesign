using OXRTK.ARHandTracking;
using UnityEngine;

namespace chenh
{
    /// <summary>
    /// 用来在每一帧投射射线
    /// 并发送射线进入和退出物体事件
    /// </summary>
    public class SpaceCodeLaser : MonoBehaviour
    {
        /// <summary>
        /// 碰到的物体
        /// </summary>
        [HideInInspector]
        public Transform hitTrans = null;
        /// <summary>
        /// 碰到的距离
        /// </summary>
        public float hitdistance;
        /// <summary>
        /// 喷到的点
        /// </summary>
        public Vector3 hitPoint;
        /// <summary>
        /// 射线检测layer
        /// </summary>
        public LayerMask layer;
        
        /// <summary>
        /// 节点拉近的速度
        /// </summary>
        public float nodespeed;
        /// <summary>
        /// 忽略z值抖动
        /// </summary>
        public float ignorez;
        
        RaycastHit hit;

		public RayInteractionPointer rayInteractionPointer;
		public LineRenderer line;
		// Update is called once per frame
		void Update()
        {
            if (rayInteractionPointer && rayInteractionPointer.m_IsHandDetected)
            {
                if (rayInteractionPointer.m_PhysicalHitResult)
                {
                    transform.parent.position = rayInteractionPointer.m_StartPosition;
                    transform.parent.forward = rayInteractionPointer.m_Direction;
                    hit = rayInteractionPointer.m_HitInfo;
                    if (hitTrans != hit.collider.transform)
                    {
                        //UIEventManager.Instance.ExcuteEventHandler(UIEventManager.EventType.OnLaserExit, hitTrans, this);
                        //UIEventManager.Instance.ExcuteEventHandler(UIEventManager.EventType.OnLaserEnter, hit.collider.transform, this);
                    }

                    hitTrans = hit.collider.transform;
                    //设置射线模型的长度
                    hitdistance = hit.distance;
                    hitPoint = hit.point;
                    transform.localScale = new Vector3(1, 1, hitdistance);
                    //circle.forward = hit.normal;
                    //line.SetPosition(0, rayInteractionPointer.m_StartPosition);
                    //line.SetPosition(1, hitPoint);
                }
                else
                {
                    //line.SetPosition(0, rayInteractionPointer.m_StartPosition);
                    //line.SetPosition(1, rayInteractionPointer.m_StartPosition + rayInteractionPointer.m_Direction * 10f);
                    //if (hitTrans != null)
                    //    UIEventManager.Instance.ExcuteEventHandler(UIEventManager.EventType.OnLaserExit, hitTrans, this);
                    hitTrans = null;
                    transform.localScale = new Vector3(1, 1, 10);
                }
            }
        }
    }
}
