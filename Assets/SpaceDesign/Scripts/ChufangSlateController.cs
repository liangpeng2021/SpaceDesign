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
        protected float x;
        protected float y;
        protected Vector3 endPoint;
        
        public virtual void UpdatePinchPointerStart(Vector3 pointOnSlate)
        {
            startPoint = pointOnSlate;
        }
        public virtual void UpdatePinchPointerEnd()
        {
            x = endPoint.x - startPoint.x;
            //Debug.Log("MyLog::endPoint.x:"+ endPoint.x);
            //Debug.Log("MyLog::startPoint.x:" + startPoint.x);
            if (x > 0)
            {
                ChufangManager.Inst.ChangeLiuChengLastAnimation(false);
            }
            else if (x < 0)
            {
                ChufangManager.Inst.ChangeLiuChengLastAnimation(true);
            }
        }
        public virtual void UpdatePinchPointer(Vector3 pointOnSlate)
        {
            endPoint = pointOnSlate;
        }
    }
}