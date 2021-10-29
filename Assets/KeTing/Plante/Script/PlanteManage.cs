﻿/* Create by zh at 2021-10-19

    植物控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SpaceDesign.Plante
{
    public class PlanteManage : MonoBehaviour
    {
        static PlanteManage inst;
        public static PlanteManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<PlanteManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        [SerializeField]
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================

        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchUp.AddListener(ClickIcon);
            btnQuit.onPinchUp.AddListener(Hide);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchUp.RemoveAllListeners();
            btnQuit.onPinchUp.RemoveAllListeners();
        }

        void Start()
        {
            v3OriPos = this.transform.position;
        }
        void OnDestroy()
        {
            StopAllCoroutines();
        }

        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.A))
        //    {
        //        Hide();
        //    }
        //}

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            //if (bUIChanging == true)
            //    return;

            Vector3 _v3 = v3OriPos;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"目标的距离:{_dis}");
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            if (_dis > 5f)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else //if (_dis <= 5f && _dis > 1.5f)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            //else if (_dis <= 1.5f)
            //{
            //    if (lastPPS == PlayerPosState.Close)
            //        return;
            //    curPlayerPosState = PlayerPosState.Close;
            //}

            StartCoroutine("IERefreshPos", lastPPS);
        }


        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

            if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 远距离=>中距离
                yield return IEFarToMiddle();
            }
            //else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Close)
            //{
            //    /// 中距离=>近距离
            //    yield return IEMiddleToClose();
            //}
            //else if (lastPPS == PlayerPosState.Close && curPlayerPosState == PlayerPosState.Middle)
            //{
            //    /// 近距离=>中距离
            //    yield return IECloseToMiddle(false);
            //}
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Far)
            {
                /// 中距离=>远距离
                yield return IEMiddleToFar();
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
                v.enabled = false;
            //Icon自身上下浮动开启
            animIconFar.enabled = false;
            traIcon.gameObject.SetActive(true);

            timelineShow.time = 0;
            timelineHide.Stop();
            timelineShow.Play();

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

            bUIShow = true;

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
            traIcon.gameObject.SetActive(true);

            if (bUIShow == true)
            {
                timelineHide.time = 0;
                timelineShow.Stop();
                timelineHide.Play();
            }

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

            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动关闭
            animIconFar.enabled = true;

            bUIShow = false;

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离（大于5米，静态）（小于5米，大于1.5米，动态）
        [Header("===Icon变化，原距离（大于5米，或者小于5米，大于1.5米）")]
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
        //Icon对象的AR手势Button按钮
        public ButtonRayReceiver btnIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //Icon的移动速度
        public float fIconSpeed = 1;

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Middle)
                StartCoroutine(IEFarToMiddle());
        }

        #endregion

        #region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;
        //Timeline：显示
        public PlayableDirector timelineShow;
        //Timeline：隐藏
        public PlayableDirector timelineHide;
        public ButtonRayReceiver btnQuit;
        private bool bUIShow = false;

        public void Hide()
        {
            StartCoroutine(IEMiddleToFar());
        }
        public void Show()
        {
            StartCoroutine(IEFarToMiddle());
        }
        #endregion

    }
}