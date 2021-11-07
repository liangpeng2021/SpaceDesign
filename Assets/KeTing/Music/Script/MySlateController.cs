using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDesign.Music
{
    /// <summary>
    /// The class for slate scrolling control. <br>
    /// 控制面板滚动交互的类。
    /// </summary>
    public class MySlateController : MonoBehaviour
    {
        //public TextMesh tt;

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
            //edit by lp
            //x = endPoint.x - startPoint.x;
            ////tt.text = x.ToString();

            //if (Mathf.Abs(x) < 0.05f)
            //{
            //    //点击专辑图，播放或者暂停
            //    //if (gameObject.name.Equals("3"))
            //    //{

            //    //    if (MusicManage.Inst.bPlaying == false)
            //    //        MusicManage.Inst.OnPlay();
            //    //    else
            //    //        MusicManage.Inst.OnPause();
            //    //}
            //}
            //else
            //{
            //    if (x > 0.15f)
            //    {
            //        MusicManage.Inst.OnLeft();
            //        if (x > 0.5f)
            //        {
            //            MusicManage.Inst.OnLeft();
            //        }
            //    }
            //    else if (x < -0.15f)
            //    {
            //        MusicManage.Inst.OnRight();
            //        if (x < -0.5f)
            //        {
            //            MusicManage.Inst.OnRight();
            //        }
            //    }
            //}

            //计算阈值
            float dist = Vector3.Distance(endPoint, startPoint);
            //计算方向
            Vector3 dir = (endPoint - startPoint).normalized;

            //Y方向不考虑
            dir = new Vector3(dir.x, 0, dir.z);
            Vector3 tempright = new Vector3(transform.right.x, 0, transform.right.z);

            if (dist > 0.1f)
            {
                //判断是否和右轴在同一个方向
                float angle = Vector3.Angle(dir, tempright);

                if (angle < 30)
                {
                    MusicManage.Inst.OnLeft();
                    if (dist > 0.5f)
                        MusicManage.Inst.OnLeft();
                }
                else if (angle > 150)
                {
                    MusicManage.Inst.OnRight();
                    if (dist > 0.5f)
                        MusicManage.Inst.OnRight();
                }
            }
            //end
        }
        public virtual void UpdatePinchPointer(Vector3 pointOnSlate)
        {
            endPoint = pointOnSlate;
        }
    }
}