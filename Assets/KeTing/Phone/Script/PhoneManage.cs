/* Create by zh at 2021-09-22

    语音通话总控制脚本
    
 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpaceDesign
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
        //呼叫等待时间
        public float fCallingTime = 20;
        //Icon、UI等正在切换中
        public bool bUIChanging = false;
        //运动阈值
        private float fThreshold = 0.1f;
        //对象初始位置
        [SerializeField]
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================
        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnRingOff.onPinchDown.AddListener(OnRingOff);
            btnAnswer.onPinchDown.AddListener(OnAnswer);
            btnReCall.onPinchDown.AddListener(OnReCall);
            btnCallingOff.onPinchDown.AddListener(OnRingOff);
            btnTalkingOff.onPinchDown.AddListener(OnTalkingOff);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnRingOff.onPinchDown.RemoveAllListeners();
            btnAnswer.onPinchDown.RemoveAllListeners();
            btnReCall.onPinchDown.RemoveAllListeners();
            btnCallingOff.onPinchDown.RemoveAllListeners();
            btnTalkingOff.onPinchDown.RemoveAllListeners();
        }

        void Start()
        {
            fTalkTotalTime = (float)(videoPlayer.clip.length);
            v3OriPos = this.transform.position;
            tt.gameObject.SetActive(false);
        }

        void OnDestroy() { StopAllCoroutines(); }

        void Update()
        {
            if (bCalling)
            {
                fCallEventDis = LoadPrefab.IconDisData.PhoneMissAndReCall;

                fCallingTempTime += Time.deltaTime;
                if (fCallingTempTime > fCallingTime)
                {
                    fCallingTempTime = 0;
                    bCalling = false;
                    bMissing = true;
                    btnRingOff.onPinchDown.Invoke();
                    ////未接挂断跟
                    //StartCoroutine("_IERingOff", true);
                }
            }
            else if (bTalking)
            {
                fCallEventDis = LoadPrefab.IconDisData.PhoneTalking;

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
                if (fTalkTempTime >= fTalkTotalTime - 0.5f)
                {
                    fTalkTempTime = 0;
                    bTalking = false;
                    btnTalkingOff.onPinchDown.Invoke();
                }
            }
            else if (bRecalling || bMissing)
            {
                fCallEventDis = LoadPrefab.IconDisData.PhoneMissAndReCall;
            }
            else
            {
                fCallEventDis = LoadPrefab.IconDisData.PhoneCalling;
            }

        }

        //触发事件的距离
        float fCallEventDis = LoadPrefab.IconDisData.PhoneCalling;

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
            //tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;
            //print($"目标的距离:{_dis}--lastPPS:{lastPPS}--curPPS:{curPlayerPosState}");

            float _fFar = LoadPrefab.IconDisData.PhoneFar;

            if (_dis > _fFar)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else if (_dis <= _fFar && _dis > fCallEventDis)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            else if (_dis <= fCallEventDis)
            {
                curPlayerPosState = PlayerPosState.Close;
                if (lastPPS == PlayerPosState.Close)
                    return;
            }

            StopCoroutine("IERefreshPos");
            StartCoroutine("IERefreshPos", lastPPS);
        }

        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            //print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            if (lastPPS == PlayerPosState.Far)
            {
                if (curPlayerPosState == PlayerPosState.Middle)/// 远距离=>中距离
                    yield return IEFarToMiddle();/// 远距离=>中距离
                else if (curPlayerPosState == PlayerPosState.Close)/// 远距离=>近距离
                {
                    yield return IEFarToMiddle();
                    yield return IEMiddleToClose();
                }
            }
            else if (lastPPS == PlayerPosState.Middle)
            {
                if (curPlayerPosState == PlayerPosState.Close)/// 中距离=>近距离
                    yield return IEMiddleToClose();

                else if (curPlayerPosState == PlayerPosState.Far)/// 中距离=>远距离
                    yield return IEMiddleToFar();
            }
            else if (lastPPS == PlayerPosState.Close)
            {
                if (curPlayerPosState == PlayerPosState.Middle)/// 近距离=>中距离
                    yield return IECloseToMiddle(false);
                else if (curPlayerPosState == PlayerPosState.Far)/// 近距离=>远距离
                {
                    yield return IECloseToMiddle(false);
                    yield return IEMiddleToFar();
                }
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
            {
                v.Play(0, -1, 0f);
                v.Update(0);
                v.enabled = false;
            }
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

            if (bMissing == false && bTalking == false)
            {
                bCalling = true;

                traCallingUI.localScale = Vector3.one;
                yield return IECallingUI(true);
                //UI变化未结束
                bUIChanging = true;

            }
            else
            {
                traCallingUI.localScale = Vector3.zero;
            }
            fCallingTempTime = 0;

            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);
            float _fSp;
            while (true)
            {
                _fSp = fUISpeed * Time.deltaTime;
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, _fSp);
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, _v3, _fSp);
                float _fDis = Vector3.Distance(traTotalUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    //这里界面先放大再缩小，所以这里不用赋值位置最大值（否则会跳一下的感觉）
                    //traTotalUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            _v3 = Vector3.one;
            yield return new WaitForSeconds(0.1f);
            while (true)
            {
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traTotalUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traTotalUI.localScale = _v3;

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
            //UI开始变化
            bUIChanging = true;

            if (audioSource.isPlaying)
                audioSource.Stop();

            if (bRecalling)
            {
                //这里需要停止所有协程
                StopSomeCoroutine();
                ResetBoolState();
                if (audioSource.isPlaying)
                    audioSource.Stop();
                traReCallUI.localScale = Vector3.zero;
                traMissedUI.localScale = Vector3.one;
                bMissing = true;
                //OnRingOff();
            }
            else if (bMissing)
            {
                traReCallUI.localScale = Vector3.zero;
                traMissedUI.localScale = Vector3.one;
                bRecalling = false;
            }
            else if (bTalking)
            {
                OnTalkingOff();
            }
            bCalling = false;
            fCallingTempTime = 0;

            //近距离=>中距离
            animAnswer.enabled = false;

            Vector3 _v3Icon = LoadPrefab.IconSize;
            float _fSp;
            while (true)
            {
                _fSp = fUISpeed * Time.deltaTime;
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, _v3Icon, _fSp);
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, Vector3.zero, _fSp);
                float _fDis = Vector3.Distance(traTotalUI.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = _v3Icon;
                    traTotalUI.localScale = Vector3.zero;

                    if (audioSource.isPlaying)
                        audioSource.Stop();
                    break;
                }
                yield return 0;
            }

            traReCallUI.localScale = Vector3.zero;
            bRecalling = false;

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

                bCalling = bRecalling = bMissing = bTalking = false;
                fTalkTempTime = 0;
                //enabled = false;
                bUIChanging = false;
                animAnswer.enabled = false;
                animReCalling.enabled = false;
                fCallingTempTime = 0;
                //===========================================================================
            }
        }

        /// <summary>
        /// 呼叫界面动画
        /// </summary>
        IEnumerator IECallingUI(bool bShow)
        {
            bUIChanging = true;

            bCalling = bShow;
            if (bShow)
            {
                traMissedUI.localScale = Vector3.zero;
                traReCallUI.localScale = Vector3.zero;
            }
            animAnswer.enabled = bShow;
            if (bCalling)
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.loop = true;
                audioSource.clip = adClipCalling;
                audioSource.Play();
            }
            float _f = bShow ? 1 : 5;
            Vector3 _v3 = bShow ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.zero;
            while (true)
            {
                traCallingUI.localScale = Vector3.Lerp(traCallingUI.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traCallingUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    if (bShow == false)
                        traCallingUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            if (bShow)
            {
                yield return new WaitForSeconds(0.1f);
                _v3 = Vector3.one;
                while (true)
                {
                    traCallingUI.localScale = Vector3.Lerp(traCallingUI.localScale, _v3, fUISpeed * Time.deltaTime);
                    float _fDis = Vector3.Distance(traCallingUI.localScale, _v3);
                    if (_fDis < fThreshold)
                    {
                        traCallingUI.localScale = _v3;
                        break;
                    }
                    yield return 0;
                }
            }

            yield return 0;
            bUIChanging = false;

        }
        /// <summary>
        /// 未接界面动画
        /// </summary>
        IEnumerator IEMissedUI(bool bShow)
        {
            bUIChanging = true;

            bMissing = bShow;

            if (bShow)
                traReCallUI.localScale = Vector3.zero;

            Vector3 _v3 = bShow ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.zero;
            while (true)
            {
                traCallingUI.localScale = Vector3.Lerp(traCallingUI.localScale, Vector3.zero, 2 * fUISpeed * Time.deltaTime);
                traReCallUI.localScale = Vector3.Lerp(traReCallUI.localScale, Vector3.zero, 2 * fUISpeed * Time.deltaTime);
                traMissedUI.localScale = Vector3.Lerp(traMissedUI.localScale, _v3, fUISpeed * Time.deltaTime);

                float _fDis = Vector3.Distance(traMissedUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traMissedUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            if (bShow)
            {
                yield return new WaitForSeconds(0.1f);
                _v3 = Vector3.one;
                while (true)
                {
                    traMissedUI.localScale = Vector3.Lerp(traMissedUI.localScale, _v3, fUISpeed * Time.deltaTime);
                    float _fDis = Vector3.Distance(traMissedUI.localScale, _v3);
                    if (_fDis < fThreshold)
                    {
                        traMissedUI.localScale = _v3;
                        break;
                    }
                    yield return 0;
                }
            }

            yield return 0;
            bUIChanging = false;

        }
        /// <summary>
        /// 重新拨打等待动画
        /// </summary>
        IEnumerator IEReCallUI(bool bShow)
        {
            bUIChanging = true;

            bRecalling = bShow;

            if(bRecalling)
            {
                traMissedUI.localScale = Vector3.zero;
            }

            float _f = bShow ? 1 : 5;
            animReCalling.enabled = bShow;
            Vector3 _v3 = bShow ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.zero;
            while (true)
            {
                traMissedUI.localScale = Vector3.Lerp(traMissedUI.localScale, Vector3.zero, 2 * fUISpeed * Time.deltaTime);
                traReCallUI.localScale = Vector3.Lerp(traReCallUI.localScale, _v3, fUISpeed * Time.deltaTime);
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
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.loop = true;
                audioSource.clip = adClipReCalling;
                audioSource.Play();

                yield return new WaitForSeconds(0.1f);
                _v3 = Vector3.one;
                while (true)
                {
                    traReCallUI.localScale = Vector3.Lerp(traReCallUI.localScale, _v3, fUISpeed * Time.deltaTime);
                    float _fDis = Vector3.Distance(traReCallUI.localScale, _v3);
                    if (_fDis < fThreshold)
                    {
                        traReCallUI.localScale = _v3;
                        break;
                    }
                    yield return 0;
                }
                //等待的过程中可以挂断电话
                bUIChanging = false;

                //等待N秒
                yield return new WaitForSeconds(4);
                float _fff = 0f;
                while (_fff < 4)
                {
                    _fff += Time.deltaTime;
                    if (bRecalling == false)
                    {
                        print("中断了");

                        bUIChanging = false;
                        yield break;
                    }

                    yield return 0;
                }

                audioSource.Stop();
                yield return IEReCallUI(false);
                //UI变化未结束
                bUIChanging = true;
                yield return IETalkingUI(true);
                //UI变化未结束
                bUIChanging = true;
            }

            yield return 0;
            bUIChanging = false;

        }

        /// <summary>
        /// 视频通话中界面
        /// </summary>
        IEnumerator IETalkingUI(bool bShow)
        {
            bUIChanging = true;

            //fTalkTempTime音频时长赋值，必须在bTalking赋值之前，否则Update函数中，计时错误
            if (bShow)
            {
                videoPlayer.targetTexture.Release();
                textTalkTiming.text = "00:00";
                videoPlayer.time = 0;
                videoPlayer.Play();
                fTalkTempTime = 0;
            }
            bTalking = bShow;

            float _f = bShow ? 1 : 5;
            Vector3 _v3 = bShow ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.zero;
            while (true)
            {
                traCallingUI.localScale = Vector3.Lerp(traCallingUI.localScale, Vector3.zero, 2 * fUISpeed * Time.deltaTime);
                traReCallUI.localScale = Vector3.Lerp(traReCallUI.localScale, Vector3.zero, 2 * fUISpeed * Time.deltaTime);
                traTalkingUI.localScale = Vector3.Lerp(traTalkingUI.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traTalkingUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traTalkingUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            if (bShow)
            {
                _v3 = Vector3.one;
                yield return new WaitForSeconds(0.1f);
                while (true)
                {
                    traTalkingUI.localScale = Vector3.Lerp(traTalkingUI.localScale, _v3, fUISpeed * Time.deltaTime);
                    float _fDis = Vector3.Distance(traTalkingUI.localScale, _v3);
                    if (_fDis < fThreshold)
                    {
                        traTalkingUI.localScale = _v3;
                        break;
                    }
                    yield return 0;
                }
            }
            yield return 0;
            bUIChanging = false;
        }

        #region Icon变化，远距离
        [Header("===Icon变化，远距离")]
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        //Icon的对象
        public Transform traIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (bUIChanging)
                return;

            if (curPlayerPosState == PlayerPosState.Close)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
            }
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //音频播放
        public AudioSource audioSource;
        public AudioClip adClipCalling;
        public AudioClip adClipMissed_short;
        public AudioClip adClipReCalling;

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
        public ButtonRayReceiver btnRingOff;
        //接听
        public ButtonRayReceiver btnAnswer;
        //接听的外发光动画
        public Animator animAnswer;
        //回拨
        public ButtonRayReceiver btnReCall;
        //回拨挂断
        public ButtonRayReceiver btnCallingOff;
        //通话中的挂断
        public ButtonRayReceiver btnTalkingOff;
        //是否开始呼叫计时
        [SerializeField]
        private bool bCalling;
        //是否回拨中
        [SerializeField]
        private bool bRecalling;
        //是否未接来电中
        [SerializeField]
        private bool bMissing;
        //呼叫计时
        [SerializeField]
        private float fCallingTempTime;
        //通话中
        [SerializeField]
        private bool bTalking;
        //通话计时（时间到了自动停止用）
        private float fTalkTempTime;
        //通话总时长
        private float fTalkTotalTime;


        void StopSomeCoroutine()
        {
            if (bUIChanging)
                return;

            StopCoroutine("IECallingUI");
            StopCoroutine("IEMissedUI");
            StopCoroutine("IEReCallUI");
            StopCoroutine("IETalkingUI");
        }

        public void ResetBoolState()
        {
            bCalling = bRecalling = bMissing = bTalking = false;
        }

        public void OnRingOff()
        {
            if (bUIChanging)
                return;

            //这里需要停止所有协程
            StopSomeCoroutine();
            ResetBoolState();

            if (audioSource.isPlaying)
                audioSource.Stop();
            audioSource.clip = adClipMissed_short;
            audioSource.loop = false;
            audioSource.Play();
            StartCoroutine("IEMissedUI", true);
        }

        public void OnAnswer()
        {
            if (bUIChanging)
                return;

            //这里需要停止所有协程
            StopSomeCoroutine();
            ResetBoolState();
            if (audioSource.isPlaying)
                audioSource.Stop();

            StartCoroutine("IETalkingUI", true);
        }

        public void OnReCall()
        {
            if (bUIChanging)
                return;

            //这里需要停止所有协程
            StopSomeCoroutine();
            ResetBoolState();
            StartCoroutine("IEReCallUI", true);
        }

        public void OnTalkingOff()
        {
            //if (bUIChanging)
            //    return;

            videoPlayer.Stop();
            //这里需要停止所有协程
            StopSomeCoroutine();
            ResetBoolState();

            StartCoroutine("IECloseToMiddle", true);
        }
        #endregion
    }
}