/* Create by zh at 2021-09-22

    用户对象管理类

 */

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
        near,
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

        private void Start()
        {
            print("房间原尺寸是50，实际场景按照5米算，房间缩放到0.15，相当于7.5米");
        }
        void Update()
        {
            fTime += Time.deltaTime;
            if (fTime >= refreshFrequency)
            {
                fTime = 0;
                refreshPlayerPosEvt?.Invoke(transform.position);
            }
        }
    }
}