///* Create by zh at 2021-09-28

//    音乐播放界面（小），CD图片的弧形进度条的点控制脚本

// */

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace SpaceDesign.Music
//{
//    public class MySliderPoint : MonoBehaviour
//    {
//        public Image img;

//        void Update()
//        {
//            transform.localEulerAngles = new Vector3(0, 0, -180 * img.fillAmount);
//        }
//    }
//}