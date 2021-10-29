﻿using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理冰箱的部分效果，/*create by 梁鹏 2021-10-18 */
/// </summary>
namespace SpaceDesign
{
    public class BingxiangManager : MonoBehaviour
    {
        public struct DoorState
        {
            public int ErrorCode;
            public int DoorId;
            public string Status;
        }
        
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        public Vector3 v3OriPos;
        
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            AddButtonRayEvent();
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            RemoveButtonRayEvent();
        }

        void Start()
        {
            v3OriPos = this.transform.position;
            bingxiangTimeline.StartPause();
            bingxiangTimeline.gameObject.SetActive(false);
        }

        public TextMesh tt;

        private void Update()
        {
            timeCount += Time.deltaTime;
            if (timeCount > 0.5f)
            {
                timeCount = 0;
#if UNITY_EDITOR
                if (Input.GetKey(KeyCode.O))
                    SetBingxiangAnimation("Open");
                if (Input.GetKey(KeyCode.P))
                    SetBingxiangAnimation("Closed");
                return;
#endif
                ClickGetDoorState();
            }
        }

        float timeCount = 0;

        string lastDoorState="";
        /// <summary>
        /// 查看冰箱是否打开
        /// </summary>
        void ClickGetDoorState()
        {
            //开启新协程
            IEnumerator enumerator = YoopInterfaceSupport.SendDataToCPE<DoorState>(/*wwwFrom, */YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.cpeipport] + "iot/sensor/door?id=1",
                //回调
                (sd) =>
                {
                    SetBingxiangAnimation(sd.Status);
                }
                );

            ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }
        /// <summary>
        /// 根据是否开门设置冰箱UI
        /// </summary>
        /// <param name="state"></param>
        void SetBingxiangAnimation(string state)
        {
            if (lastDoorState.Equals(state))
                return;
            //保存之前的状态
            lastDoorState = state;
            if (curPlayerPosState != PlayerPosState.Close)
                return;
            //发生变化的瞬间触发一次
            if (lastDoorState.Equals("Open"))
            {
                bingxiangTimeline.gameObject.SetActive(true);
                bingxiangTimeline.SetCurTimelineData("开冰箱");
            }
            else
            {
                bingxiangTimeline.gameObject.SetActive(true);
                bingxiangTimeline.SetCurTimelineData("提示到信息");
            }

            Debug.Log("MyLog::获取门禁状态:" + state);
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            //if (bUIChanging)
            //    return;
            Vector3 _v3 = v3OriPos;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            
            tt.text = _dis.ToString();
            
            PlayerPosState lastPPS = curPlayerPosState;

            if (_dis > 5f)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                curPlayerPosState = PlayerPosState.Far;
            }
            else if (_dis <= 5f && _dis > 1.5f)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                curPlayerPosState = PlayerPosState.Middle;
            }
            else if (_dis <= 1.5f)
            {
                //Debug.Log(lastPPS);
                if (lastPPS == PlayerPosState.Close)
                    return;
                curPlayerPosState = PlayerPosState.Close;
            }

            StartCoroutine("IERefreshPos", lastPPS);
        }
        
        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            //print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 远距离=>中距离
                yield return IEFarToMiddle();
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Close)
            {
                /// 中距离=>近距离
                yield return IEMiddleToClose();
            }
            else if (lastPPS == PlayerPosState.Close && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 近距离=>中距离
                yield return IECloseToMiddle();
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Far)
            {
                /// 中距离=>远距离
                yield return IEMiddleToFar();
            }
            else if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Close)
            {
                /// 一来就是近距离
                yield return IEMiddleToClose();
            }
            yield return 0;
        }

        /// <summary>
        /// 远距离=>中距离
        /// </summary>
        IEnumerator IEFarToMiddle()
        {
            //UI开始变化
            bUIChanging = true;

            //远距离=>中距离
            //Icon从静态变成动态
            //Icon的自旋转动画开启
            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动开启
            animIconFar.enabled = true;
            traIcon.gameObject.SetActive(true);

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 中距离=>远距离
        /// </summary>
        IEnumerator IEMiddleToFar()
        {
            //UI开始变化
            bUIChanging = true;

            //中距离=>远距离
            //Icon从动态变成静态
            //Icon的自旋转动画关闭
            foreach (var v in animIconMiddle)
                v.enabled = false;
            //Icon自身上下浮动关闭
            animIconFar.enabled = false;
            traIcon.gameObject.SetActive(true);

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 中距离=>近距离
        /// </summary>
        IEnumerator IEMiddleToClose()
        {
            //UI开始变化
            bUIChanging = true;

            //中距离=>近距离
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, 0.1f);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }
            bingxiangTimeline.gameObject.SetActive(true);
            bingxiangTimeline.SetCurTimelineData("提示到信息");

            //UI变化结束
            bUIChanging = false;
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            //UI开始变化
            bUIChanging = true;
            //近距离=>中距离

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, 0.1f);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }
            bingxiangTimeline.gameObject.SetActive(false);
            lastDoorState = "";
            //UI变化结束
            bUIChanging = false;
        }

#region Icon变化，远距离（大于5米，静态）（小于5米，大于1.5米，动态）
        [Header("===Icon变化，原距离（大于5米，或者小于5米，大于1.5米）")]
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        /// <summary>
        /// 控制Timeline
        /// </summary>
        public TimelineControl bingxiangTimeline;
        
        //Icon的移动速度
        public float fIconSpeed = 1;
        /// <summary>
        /// 触点按钮
        /// </summary>
        public ButtonRayReceiver chudianBtn;
        
        /// <summary>
        /// 点击Icon
        /// </summary>
        void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Close)
                StartCoroutine(IEMiddleToClose());
        }

        void AddButtonRayEvent()
        {
            chudianBtn.onPinchDown.AddListener(ClickIcon);
            closePayBtn.onPinchDown.AddListener(OnClosePay);
            directPayBtn.onPinchDown.AddListener(DirectPay);
            payBtn.onPinchDown.AddListener(ChoosePay);
            gochoose.onPinchDown.AddListener(GoChoose);
        }

        void RemoveButtonRayEvent()
        {
            chudianBtn.onPinchDown.RemoveAllListeners();
            closePayBtn.onPinchDown.RemoveAllListeners();
            directPayBtn.onPinchDown.RemoveAllListeners();
            payBtn.onPinchDown.RemoveAllListeners();
            gochoose.onPinchDown.RemoveAllListeners();
        }

#endregion

#region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;

        /// <summary>
        /// 购买关闭按钮
        /// </summary>
        public ButtonRayReceiver closePayBtn;
        /// <summary>
        /// 一键复购
        /// </summary>
        public ButtonRayReceiver directPayBtn;
        /// <summary>
        /// 选择后购买
        /// </summary>
        public ButtonRayReceiver payBtn;
        /// <summary>
        /// 进入选择界面
        /// </summary>
        public ButtonRayReceiver gochoose;

        void OnClosePay()
        {
            bingxiangTimeline.SetCurTimelineData("关闭购买");
        }

        void GoChoose()
        {
            bingxiangTimeline.SetCurTimelineData("点击可乐");
        }

        void DirectPay()
        {
            bingxiangTimeline.SetCurTimelineData("一键复购");
        }

        void ChoosePay()
        {
            bingxiangTimeline.SetCurTimelineData("点击购买");
        }
#endregion
    }
}