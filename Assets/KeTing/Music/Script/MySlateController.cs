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
            x = endPoint.x - startPoint.x;
            //tt.text = x.ToString();

            if (Mathf.Abs(x) < 0.05f)
            {
                //点击专辑图，播放或者暂停
                //if (gameObject.name.Equals("3"))
                //{

                //    if (MusicManage.Inst.bPlaying == false)
                //        MusicManage.Inst.OnPlay();
                //    else
                //        MusicManage.Inst.OnPause();
                //}
            }
            else
            {
                if (x > 0.15f)
                {
                    MusicManage.Inst.OnRight();
                    if (x > 0.5f)
                    {
                        MusicManage.Inst.OnRight();
                    }
                }
                else if (x < -0.15f)
                {
                    MusicManage.Inst.OnLeft();
                    if (x < -0.5f)
                    {
                        MusicManage.Inst.OnLeft();
                    }
                }
            }
        }
        public virtual void UpdatePinchPointer(Vector3 pointOnSlate)
        {
            endPoint = pointOnSlate;
        }
    }
}