/* Create by zh at 2021-09-22

    用户对象管理类

 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDesign
{
    /// <summary>
    /// 目标物和人物的距离状态
    /// </summary>
    public enum PlayerPosState
    {
        Far,
        Middle,
        Close,
    }


    /// <summary>
    /// 用户位置刷新事件
    /// </summary>
    public delegate void RefreshPlayerPosEvent(Vector3 p);

    public class PlayerManage : MonoBehaviour
    {
        static PlayerManage inst;
        public static PlayerManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<PlayerManage>();
                return inst;
            }
        }

        /// <summary>
        /// 用户位置刷新事件
        /// </summary>
        public static RefreshPlayerPosEvent refreshPlayerPosEvt;

        /// <summary>
        /// 位置刷新频率
        /// </summary>
        [SerializeField]
        private float refreshFrequency = 0.5f;
        //计时
        private float fTime;

        //private void Start()
        //{
        //    print("房间原尺寸是50，实际场景按照5米算，房间缩放到0.15，相当于7.5米");
        //}
        void Update()
        {
            if (refreshPlayerPosEvt == null)
                return;

            fTime += Time.deltaTime;
            if (fTime >= refreshFrequency)
            {
                fTime = 0;
                if (refreshPlayerPosEvt != null)
                    refreshPlayerPosEvt.Invoke(transform.position);
            }
        }

        public static void InitPlayerPosEvt()
        {
            if (refreshPlayerPosEvt == null)
                return;

            Delegate[] _dels = refreshPlayerPosEvt.GetInvocationList();
            int _iLen = _dels.Length;
            for (int i = 0; i < _iLen; i++)
            {
                refreshPlayerPosEvt -= _dels[i] as RefreshPlayerPosEvent;
            }
        }
    }
}