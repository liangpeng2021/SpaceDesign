///* Create by zh at 2021-09-17

//    音乐的Icon动画控制脚本（Icon界面：动画等）

// */

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpaceDesign.Music
//{
//    public class MusicIconMag : MonoBehaviour
//    {
//        public Transform traFarPos;
//        public Transform traMiddlePos;

//        public Transform traIcon;

//        //吸引态，上下移动动画
//        public Animator animIconFar;

//        //轻交互，半球动画+音符动画
//        public Animator[] animIconMiddle;

//        /// <summary>
//        /// 刷新位置消息
//        /// </summary>
//        public void RefreshPos(IconPlayerState ips)
//        {
//            switch (ips)
//            {
//                case IconPlayerState.Far:
//                    traIcon.SetParent(traFarPos);
//                    traIcon.localPosition = Vector3.zero;
//                    foreach (var v in animIconMiddle)
//                    {
//                        v.enabled = false;
//                    }
//                    animIconFar.enabled = true;
//                    traIcon.gameObject.SetActive(true);
//                    break;
//                case IconPlayerState.Middle:
//                    traIcon.SetParent(traMiddlePos);
//                    traIcon.localPosition = Vector3.zero;
//                    animIconFar.enabled = false;
//                    foreach (var v in animIconMiddle)
//                    {
//                        v.enabled = true;
//                    }
//                    traIcon.gameObject.SetActive(true);
//                    break;
//                case IconPlayerState.Close:

//                    traIcon.gameObject.SetActive(false);
//                    break;
//            }
//        }
//        //private void Update()
//        //{
//        //    if (Input.GetKeyDown(KeyCode.A))
//        //    {
//        //        //animIconFar.SetInteger("bState", 1);
//        //        animIconFar.SetTrigger("triFar");//SetInteger("bState", 0);
//        //    }
//        //    if (Input.GetKeyDown(KeyCode.B))
//        //    {
//        //        animIconFar.SetTrigger("triMiddle");//.SetInteger("bState", 2);
//        //    }
//        //}
//    }
//}