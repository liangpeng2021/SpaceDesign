/* Create by zh at 2021-10-19

    游戏控制脚本

    奇幻森林：          com.gabor.artowermotion
    忒依亚传说(moba):   com.baymax.omoba
    AR动物园:           com.xyani.findanimals

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SpaceDesign.DeskGame
{
    public class DeskGameManage : MonoBehaviour
    {
        static DeskGameManage inst;
        public static DeskGameManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<DeskGameManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //播放模型
        public Transform traModel;
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;

        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
        }

        //void Start()
        //{
        //    //btnCheckTranslate.onClick.AddListener(OnCheckTranslate);
        //    //btnQuit.onClick.AddListener(OnQuit);

        //    ////===========================================================================
        //    ////Icon点击触发
        //    //EventTrigger _trigger = traIcon.GetComponent<EventTrigger>();
        //    //if (_trigger == null)
        //    //    _trigger = traIcon.gameObject.AddComponent<EventTrigger>();

        //    //EventTrigger.Entry _entry = new EventTrigger.Entry
        //    //{
        //    //    eventID = EventTriggerType.PointerClick,
        //    //    callback = new EventTrigger.TriggerEvent(),
        //    //};
        //    //_entry.callback.AddListener(x =>
        //    //{
        //    //    if (curPlayerPosState == PlayerPosState.Middle)
        //    //        StartCoroutine(IEFarToMiddle());
        //    //});
        //    //_trigger.triggers.Add(_entry);
        //    ////===========================================================================
        //}

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Middle)
                StartCoroutine(IEFarToMiddle());
        }

        public void CallApp(string strPackName)
        {
            //print($"拉起应用:{strPackName}");
            XR.AppManager.StartApp(strPackName);
        }

        public TextMesh tt;

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            if (bUIChanging == true)
                return;

            Vector3 _v3 = traIcon.position;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"目标的距离:{_dis}");
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            if (_dis > 5f)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                curPlayerPosState = PlayerPosState.Far;
            }
            else if (_dis <= 5f)// && _dis > 1.5f)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                curPlayerPosState = PlayerPosState.Middle;
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
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fIconSpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }


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

                timelineHide.time = 0;
                timelineShow.Stop();
                timelineHide.Play();

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * Time.deltaTime);
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
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //Icon的移动速度
        public float fIconSpeed = 1;
        #endregion

        #region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;
        //Timeline：显示
        public PlayableDirector timelineShow;
        //Timeline：隐藏
        public PlayableDirector timelineHide;
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