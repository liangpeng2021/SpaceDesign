using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDesign
{
    /// <summary>
    /// The class for slate scrolling control. <br>
    /// 控制面板滚动交互的类。
    /// </summary>
    public class MySlateController : MonoBehaviour
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
            //if (Mathf.Abs(x) < 0.01f)
            //{
            //    if (gameObject.name.Equals("3"))
            //    {
            //        MusicMaxMag.Inst.OnPlay();
            //    }
            //}
            //else
            //{
            //    if (x > 0)
            //    {
            //        MusicMaxMag.Inst.OnRight();
            //    }
            //    else if (x < 0)
            //    {
            //        MusicMaxMag.Inst.OnLeft();
            //    }
            //}
        }
        public virtual void UpdatePinchPointer(Vector3 pointOnSlate)
        {
            endPoint = pointOnSlate;
        }
    }
}