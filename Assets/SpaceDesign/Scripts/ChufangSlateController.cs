using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDesign
{
    /// <summary>
    /// The class for slate scrolling control. <br>
    /// 控制面板滚动交互的类。
    /// </summary>
    public class ChufangSlateController : MonoBehaviour
    {
        protected Vector3 startPoint;
        
        protected Vector3 endPoint;

        public virtual void UpdatePinchPointerStart(Vector3 pointOnSlate)
        {
            startPoint = pointOnSlate;
        }
        public virtual void UpdatePinchPointerEnd()
        {
            //计算阈值
            float dist = Vector3.Distance(endPoint, startPoint);
            //计算方向
            Vector3 dir = (endPoint - startPoint).normalized;
            
            //Y方向不考虑
            dir = new Vector3(dir.x,0, dir.z);
            Vector3 tempright = new Vector3(transform.right.x,0, transform.right.z);
            
            if (dist > 0.1f)
            {
                //判断是否和右轴在同一个方向
                float angle = Vector3.Angle(dir, tempright);

                if (angle < 30)
                {
                    ChufangManager.Inst?.ChangeLiuChengLastAnimation(false);
                }
                else if (angle > 150)
                {
                    ChufangManager.Inst?.ChangeLiuChengLastAnimation(true);
                }
            }
        }
        public virtual void UpdatePinchPointer(Vector3 pointOnSlate)
        {
            endPoint = pointOnSlate;
        }
    }
}