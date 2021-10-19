/* Create by zh at 2021-09-22

    语音通话总控制脚本

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpaceDesign.Phone
{
    public class PhoneManage : MonoBehaviour
    {
        static PhoneManage inst;
        public static PhoneManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<PhoneManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //播放模型
        public Transform traModel;
        //呼叫等待时间
        public float fCallingTime = 20;
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

        void Start()
        {
            fTalkTotalTime = (float)(videoPlayer.clip.length);

            btnRingOff.onClick.AddListener(OnRingOff);
            btnAnswer.onClick.AddListener(OnAnswer);
            btnReCall.onClick.AddListener(OnReCall);
            btnCallingOff.onClick.AddListener(OnRingOff);
            btnTalkingOff.onClick.AddListener(OnTalkingOff);
        }

        void Update()
        {
            if (bCalling)
            {
                fCallingTempTime += Time.deltaTime;
                if (fCallingTempTime > fCallingTime)
                {
                    fCallingTempTime = 0;
                    bCalling = false;
                    btnRingOff.onClick.Invoke();
                }
            }
            else if (bTalking)
            {
                int second = 0;
                int min = 0;
                second = (int)(fTalkTempTime % 60);
                if (fTalkTempTime > 59)
                {
                    min = (int)(fTalkTempTime / 60);
                    if (min > 59)
                    {
                        min = min % 60;
                    }
                }
                textTalkTiming.text = $"{min.ToString("d2")}:{second.ToString("d2")}";

                fTalkTempTime += Time.deltaTime;
                if (fTalkTempTime > fTalkTotalTime)
                {
                    fTalkTempTime = 0;
                    bTalking = false;
                    btnTalkingOff.onClick.Invoke();
                }
            }
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            if (bUIChanging == true)
                return;

            Vector3 _v3 = traModel.position;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"目标的距离:{_dis}");

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
            print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

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
                yield return IECloseToMiddle(false);
            }
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
            animAnswer.enabled = true;

            bCalling = true;

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fIconSpeed * 2f * Time.deltaTime);
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traTotalUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    traTotalUI.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }
            //UI变化结束
            bUIChanging = false;
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle(bool bTalkOver)
        {
            if (bTalkOver == false)
            {
                //呼叫中、或者视频通话中，距离变远也不隐藏
                if (bCalling == true || bTalking == true)
                    yield break;
            }

            //UI开始变化
            bUIChanging = true;

            //近距离=>中距离
            animAnswer.enabled = false;

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * 2f * Time.deltaTime);
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traTotalUI.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    traTotalUI.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }


            //UI变化结束
            bUIChanging = false;

            if (bTalkOver)
            {
                //===========================================================================
                //通话结束，初始化内容，不再进入，除非下次重置

                traCallingUI.localScale = Vector3.one;
                traMissedUI.localScale = Vector3.zero;
                traReCallUI.localScale = Vector3.zero;
                traTalkingUI.localScale = Vector3.zero;

                bCalling = bTalking = false;
                fTalkTempTime = 0;
                enabled = false;
                bUIChanging = false;
                animAnswer.enabled = false;
                animReCalling.enabled = false;
                //StopAllCoroutines();
                //===========================================================================
            }
        }

        /// <summary>
        /// 呼叫界面动画
        /// </summary>
        IEnumerator IECallingUI(bool bShow)
        {
            animAnswer.enabled = bShow;
            float _f = bShow ? 1 : 5;
            Vector3 _v3 = bShow ? Vector3.one : Vector3.zero;
            while (true)
            {
                traCallingUI.localScale = Vector3.Lerp(traCallingUI.localScale, _v3, fUISpeed * _f * Time.deltaTime);
                float _fDis = Vector3.Distance(traCallingUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traCallingUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            yield return 0;
        }
        /// <summary>
        /// 未接界面动画
        /// </summary>
        IEnumerator IEMissedUI(bool bShow)
        {
            float _f = bShow ? 1 : 5;
            Vector3 _v3 = bShow ? Vector3.one : Vector3.zero;
            while (true)
            {
                traMissedUI.localScale = Vector3.Lerp(traMissedUI.localScale, _v3, fUISpeed * _f * Time.deltaTime);
                float _fDis = Vector3.Distance(traMissedUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traMissedUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            yield return 0;
        }
        /// <summary>
        /// 重新拨打等待动画
        /// </summary>
        IEnumerator IEReCallUI(bool bShow)
        {
            float _f = bShow ? 1 : 5;
            animReCalling.enabled = bShow;
            Vector3 _v3 = bShow ? Vector3.one : Vector3.zero;
            while (true)
            {
                traReCallUI.localScale = Vector3.Lerp(traReCallUI.localScale, _v3, fUISpeed * _f * Time.deltaTime);
                float _fDis = Vector3.Distance(traReCallUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traReCallUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            if (bShow)
            {
                //等待N秒
                yield return new WaitForSeconds(4);

                //===========================================================================
                //等待过程中播放呼叫语音
                //===========================================================================

                //等待4秒后，隐藏

                _v3 = Vector3.zero;
                _f = 5;
                while (true)
                {
                    traReCallUI.localScale = Vector3.Lerp(traReCallUI.localScale, _v3, fUISpeed * _f * Time.deltaTime);
                    float _fDis = Vector3.Distance(traReCallUI.localScale, _v3);
                    if (_fDis < fThreshold)
                    {
                        traReCallUI.localScale = _v3;
                        break;
                    }
                    yield return 0;
                }
                yield return IETalkingUI(true);
            }
        }

        /// <summary>
        /// 视频通话中界面
        /// </summary>
        IEnumerator IETalkingUI(bool bShow)
        {
            //fTalkTempTime音频时长赋值，必须在bTalking赋值之前，否则Update函数中，计时错误
            if (bShow)
            {
                textTalkTiming.text = "00:00";
                videoPlayer.time = 0;
                videoPlayer.Play();
                fTalkTempTime = 0;
            }
            float _f = bShow ? 1 : 5;
            Vector3 _v3 = bShow ? Vector3.one : Vector3.zero;
            bTalking = bShow;
            while (true)
            {
                traTalkingUI.localScale = Vector3.Lerp(traTalkingUI.localScale, _v3, fUISpeed * _f * Time.deltaTime);
                float _fDis = Vector3.Distance(traTalkingUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traTalkingUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            yield return 0;

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
        //小UI（不包含更多音乐按钮，因为动画变化不同）
        public Transform traTotalUI;
        //呼叫中UI
        public Transform traCallingUI;
        //未接来电UI
        public Transform traMissedUI;
        //重新拨打UI
        public Transform traReCallUI;
        //拨打中动画
        public Animator animReCalling;
        //通话中UI
        public Transform traTalkingUI;
        //通话中的计时
        public Text textTalkTiming;
        //通话视频
        public VideoPlayer videoPlayer;
        //挂断
        public Button btnRingOff;
        //接听
        public Button btnAnswer;
        //接听的外发光动画
        public Animator animAnswer;
        //回拨
        public Button btnReCall;
        //回拨挂断
        public Button btnCallingOff;
        //通话中的挂断
        public Button btnTalkingOff;
        //是否开始呼叫计时
        private bool bCalling;
        //呼叫计时
        private float fCallingTempTime;
        //通话中
        private bool bTalking;
        //通话计时（时间到了自动停止用）
        private float fTalkTempTime;
        //通话总时长
        private float fTalkTotalTime;

        void OnRingOff()
        {
            bCalling = false;
            StopAllCoroutines();
            //StopCoroutine("_IERingOff");
            StartCoroutine("_IERingOff");
        }
        IEnumerator _IERingOff()
        {
            yield return IECallingUI(false);
            yield return IEReCallUI(false);
            yield return IEMissedUI(true);
        }

        void OnAnswer()
        {
            bCalling = false;
            StopAllCoroutines();
            //StopCoroutine("_IEAnswer");
            StartCoroutine("_IEAnswer");
        }
        IEnumerator _IEAnswer()
        {
            yield return IECallingUI(false);
            yield return IETalkingUI(true);
        }

        void OnReCall()
        {
            StopAllCoroutines();
            //StopCoroutine("_IEReCall");
            StartCoroutine("_IEReCall");
        }
        IEnumerator _IEReCall()
        {
            yield return IEMissedUI(false);
            yield return IEReCallUI(true);
        }

        void OnTalkingOff()
        {
            videoPlayer.Stop();
            StopAllCoroutines();
            //StopCoroutine("IECloseToMiddle");
            StartCoroutine("IECloseToMiddle", true);
        }
        #endregion

    }
}