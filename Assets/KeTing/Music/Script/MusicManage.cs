/* Create by zh at 2021-09-17

    音乐总控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDesign
{
    public enum MusicPlayState
    {
        AllLoop,
        OneLoop,
        Order,
    }

    public class MusicManage : MonoBehaviour
    {
        static MusicManage inst;
        public static MusicManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<MusicManage>();
                return inst;
            }
        }

        [Header("(7首歌曲，最右边从0开始，从右往左，依次增大)")]
        public EachMusicAttr[] aryEachMusicAttr;
        //重交互到轻交互，等待的时长
        public float fAutoTurnUITime = 60f;

        [Header("=====音乐通用变量")]
        //UI的变化速度
        public float fUISpeed = 5;
        //音乐播放源
        public AudioSource audioSource;
        //当前音乐播放状态
        public MusicPlayState curMusicPlayState = MusicPlayState.Order;
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //是否正在播放中
        public bool bPlaying = false;
        //当前播放的时间进度（秒）
        public float fCurPlayTime = 0;
        //当前播放的音乐的序号
        public int iCurMusicNum = 1;
        //中距离和近距离，同一个图片动画，轻交互的位置
        public Transform traTempImageMinPos;
        //中距离和近距离，同一个图片动画，重交互的位置
        public Transform traTempImageMaxPos;
        //中距离和近距离，同一个图片动画，的图片
        public Image imgTempImage;
        //七张音乐图的，中间图片对象
        public Transform traImgCenter;
        //抬起在中新图片的左右（0-左短，1-左长，2-右短，3-右长）
        public int iUpCenterPos = -1;
        //中心图片进入或离开（0-进入，1-离开）
        public int iImgCenterEnterOrExit = -1;
        public float fRotAngle = -4.5f;
        public float fRotSpeed2 = 0.1f;

        //按下中心图片的放大
        private Vector3 v3ImgCenterEnter = new Vector3(1.2f, 1.2f, 1.2f);
        //Icon、UI等正在切换中
        private bool bUIChanging = false;
        //运动阈值
        private float fThreshold = 0.1f;
        //对象初始位置
        private Vector3 v3OriPos;
        //Oppo手机系统的最大音量
        private const float OPPOSystemMaxVolum = 16f;
        //声明旋转目标角度
        private Quaternion targetRotation;
        //定义每次旋转的角度
        private float RotateAngle = 51.5f;
        private Vector3 v3ScaleCenter = new Vector3(480f, 480f, 1.5f);
        private Vector3 v3ScaleNormal = new Vector3(320f, 320f, 1f);
        //临时计算是否暂停过用
        private bool _bTempPause = false;

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

            btnEffect.onPinchDown.AddListener(ClickFarToMiddle);
            btnIcon.onPinchDown.AddListener(ClickFarToMiddle);
            btnMoreMusicMin.onPinchDown.AddListener(OnMoreMusicMin);
            btnLeftMin.onPinchDown.AddListener(OnLeft);
            btnLeftMax.onPinchDown.AddListener(OnLeft);
            btnRightMin.onPinchDown.AddListener(OnRight);
            btnRightMax.onPinchDown.AddListener(OnRight);
            btnPlayMin.onPinchDown.AddListener(() => { OnPlay(); });
            btnPlayMax.onPinchDown.AddListener(() => { OnPlay(); });
            btnPauseMin.onPinchDown.AddListener(OnPause);
            btnPauseMax.onPinchDown.AddListener(OnPause);

            btnStateAllloop.onPinchDown.AddListener(OnState);
            btnStateOneloop.onPinchDown.AddListener(OnState);
            btnStateOrder.onPinchDown.AddListener(OnState);

            btnVolumeMax.onPointerEnter.AddListener(EnterBtnVolum);
            btnVolumeMax.onPointerExit.AddListener(ExitBtnVolum);
            btnVolumeMax.onPinchDown.AddListener(OnVolumeMute);
            btnVolumeMuteMax.onPointerEnter.AddListener(EnterBtnVolum);
            btnVolumeMuteMax.onPointerExit.AddListener(ExitBtnVolum);
            btnVolumeMuteMax.onPinchDown.AddListener(OnVolumeOpen);

            btnExitSliderVolum.onPointerExit.AddListener(ExitObjVolum);
            btnSliderVolumMax.onPointerEnter.AddListener(EnterObjVolum);
            //因为要发送给CPE：不要实时变化，抬起才触发一次
            pinchSliderVolumMax.onValueChanged.AddListener(OnSliderVolumeChange);
            pinchSliderVolumMax.onInteractionEnd.AddListener(OnSliderVolume);
#if UNITY_ANDROID && !UNITY_EDITOR
            //系统音量触发
            XR.AudioManager.OnAudioVolume += OnSystemVolumeChange;
#endif
            pinchSliderMusicMax.onInteractionStart.AddListener(SliderMusicMaxPointerDown);
            pinchSliderMusicMax.onInteractionEnd.AddListener(SliderMusicMaxPointerUp);

        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnEffect.onPinchDown.RemoveAllListeners();
            btnIcon.onPinchDown.RemoveAllListeners();
            btnMoreMusicMin.onPinchDown.RemoveAllListeners();
            btnLeftMin.onPinchDown.RemoveAllListeners();
            btnLeftMax.onPinchDown.RemoveAllListeners();
            btnRightMin.onPinchDown.RemoveAllListeners();
            btnRightMax.onPinchDown.RemoveAllListeners();
            btnPlayMin.onPinchDown.RemoveAllListeners();
            btnPlayMax.onPinchDown.RemoveAllListeners();
            btnPauseMin.onPinchDown.RemoveAllListeners();
            btnPauseMax.onPinchDown.RemoveAllListeners();

            btnStateAllloop.onPinchDown.RemoveAllListeners();
            btnStateOneloop.onPinchDown.RemoveAllListeners();
            btnStateOrder.onPinchDown.RemoveAllListeners();

            btnVolumeMax.onPointerEnter.RemoveAllListeners();
            btnVolumeMax.onPointerExit.RemoveAllListeners();
            btnVolumeMax.onPinchDown.RemoveAllListeners();
            btnVolumeMuteMax.onPointerEnter.RemoveAllListeners();
            btnVolumeMuteMax.onPointerExit.RemoveAllListeners();
            btnVolumeMuteMax.onPinchDown.RemoveAllListeners();
            btnSliderVolumMax.onPointerEnter.RemoveAllListeners(); ;
            btnExitSliderVolum.onPointerExit.RemoveAllListeners();
            pinchSliderVolumMax.onValueChanged.RemoveAllListeners();
            pinchSliderVolumMax.onInteractionEnd.RemoveAllListeners();
#if UNITY_ANDROID && !UNITY_EDITOR
            //系统音量触发
            XR.AudioManager.OnAudioVolume -= OnSystemVolumeChange;
#endif
            pinchSliderMusicMax.onInteractionStart.RemoveAllListeners();
            pinchSliderMusicMax.onInteractionEnd.RemoveAllListeners();
        }

        void OnDestroy()
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            StopAllCoroutines();
        }

        void Start()
        {
            v3OriPos = this.transform.position;

            Canvas[] _canvasAry = transform.GetComponentsInChildren<Canvas>();
            foreach (Canvas v in _canvasAry)
            {
                v.worldCamera = XR.XRCameraManager.Instance.eventCamera;
            }

            //播放的循环，不用该值控制
            audioSource.loop = false;

            ////开始的时候刷新一下数据
            _SetCurMusicNum(1);
            _InitEachMusicAnim();

            traImgCenter.localScale = Vector3.zero;

            targetRotation = traMaxUIImage.localRotation;
            int _iVolum = 1;
#if UNITY_ANDROID && !UNITY_EDITOR
            //系统音量：【0-16】整型
            XR.AudioManager.GetStreamVolume(XR.XRAudioStream.XR_AUDIO_STREAM_MUSIC, ref _iVolum);
#endif
            //XR.AudioManager.SetStreamVolume(XR.XRAudioStream.XR_AUDIO_STREAM_MUSIC,);
            //Debug.Log("MyLog: 声音" + _iVolum);

            OnSliderVolumeChange((float)_iVolum / OPPOSystemMaxVolum);
            OnSliderVolume();
        }

        void Update()
        {
            if (bUIChanging)
                return;

            if (bPlaying)
            {
                imgSliderMin.fillAmount = pinchSliderMusicMax.sliderValue;//slidMusicMax.value;

                //播放中，中距离，小UI，倒计时，自动变音符特效对象
                if (bMinTiming)
                {
                    fMinToEffectTemp += Time.deltaTime;
                    if (fMinToEffectTemp > fAutoTurnUITime)
                    {
                        StopCoroutine("IEMiddleToFar");
                        StartCoroutine("IEMiddleToFar");
                        fMinToEffectTemp = 0;
                    }
                }

            }
            #region 重交互
            if (bMaxTiming)
            {
                fMaxToMinTemp += Time.deltaTime;
                if (fMaxToMinTemp > fAutoTurnUITime)
                {
                    StopCoroutine("IEMaxToMin");
                    StartCoroutine("IEMaxToMin");
                    fMaxToMinTemp = 0;
                }
            }

            if (bGoAnim)
            {
                traMaxUIImage.localRotation = Quaternion.Slerp(traMaxUIImage.localRotation, targetRotation, Time.deltaTime * fUISpeed);

                if (Quaternion.Angle(targetRotation, traMaxUIImage.localRotation) < fThreshold)
                {
                    traMaxUIImage.localRotation = targetRotation;
                    bGoAnim = false;

                    //移动完之后，赋值中间位置的对象
                    Transform _tra = aryEachMusicAttr[iCurMusicNum - 1].transform;
                    _tra.SetParent(traImgCenter);
                    _tra.localScale = v3ScaleCenter;
                    _tra.localPosition = Vector3.zero;
                    traImgCenter.GetComponent<BoxCollider>().enabled = true;
                }
            }

            if (bSlideDragging == false)
            {
                if (audioSource.isPlaying)
                {
                    _SetCurPlayTime(true);
                }
                else
                {
                    //播完了，判断状态，操作下一步
                    if (audioSource.time >= fTotalPlayTime)
                    {
                        //print($"播完了，切换:{curMusicPlayState}");

                        AutoNext();
                    }
                }
            }
            else
            {
                _SetCurPlayTime(false);
            }
            #endregion

            if (iImgCenterEnterOrExit == 0)
            {
                RestartTimeMaxToMin();

                float _f = Vector3.Distance(traImgCenter.localScale, v3ImgCenterEnter);
                if (_f > 0.01f)
                {
                    traImgCenter.localScale = Vector3.Lerp(traImgCenter.localScale, v3ImgCenterEnter, fUISpeed * Time.deltaTime);
                }
                else
                {
                    traImgCenter.localScale = v3ImgCenterEnter;
                }

            }
            else if (iImgCenterEnterOrExit == 1)
            {
                RestartTimeMaxToMin();

                float _f = Vector3.Distance(traImgCenter.localScale, Vector3.one);
                if (_f > 0.01f)
                {
                    traImgCenter.localScale = Vector3.Lerp(traImgCenter.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                }
                else
                {
                    iImgCenterEnterOrExit = -1;
                    traImgCenter.localScale = Vector3.one;
                }
            }
            else if (iImgCenterEnterOrExit == 2)
            {
                RestartTimeMaxToMin();

                float _f = Vector3.Distance(traImgCenter.localScale, Vector3.zero);
                if (_f > 0.01f)
                {
                    traImgCenter.localScale = Vector3.Lerp(traImgCenter.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                }
                else
                {
                    iImgCenterEnterOrExit = -1;
                    traImgCenter.localScale = Vector3.zero;
                }
            }
        }

        void AutoNext()
        {
            switch (curMusicPlayState)
            {
                case MusicPlayState.AllLoop:
                    OnRight();
                    break;
                case MusicPlayState.OneLoop:
                    pinchSliderMusicMax.sliderValue = 0;
                    SliderMusicMaxPointerUp();
                    OnPlay(iCurMusicNum);
                    break;
                case MusicPlayState.Order:
                    if (iCurMusicNum < iTotalMusicNum)
                        OnRight();
                    else
                    {
                        pinchSliderMusicMax.sliderValue = 0;
                        SliderMusicMaxPointerUp();
                        OnPause();
                    }
                    break;
            }
        }

        /// <summary>
        /// 中间图片进入或离开
        /// </summary>
        public void CenterImgEnterOrExit(bool b)
        {
            iImgCenterEnterOrExit = b ? 0 : 1;
        }

        /// <summary>
        /// 音乐进度条按下
        /// </summary>
        void SliderMusicMaxPointerDown()
        {
            bSlideDragging = true;
        }

        /// <summary>
        /// 音乐进度条抬起
        /// </summary>
        void SliderMusicMaxPointerUp()
        {
            bSlideDragging = false;
            //print($"pinchSliderMusicMax.sliderValue:{pinchSliderMusicMax.sliderValue },fTotalPlayTime:{fTotalPlayTime}");
            imgSliderMin.fillAmount = pinchSliderMusicMax.sliderValue;//slidMusicMax.value;
            SetAudioTime(pinchSliderMusicMax.sliderValue * fTotalPlayTime);
            _SetCurPlayTime(false);
        }

        /// <summary>
        /// 点击Button，触发从远到近的动画
        /// </summary>
        void ClickFarToMiddle()
        {
            if (bUIChanging)
                return;

            if (curPlayerPosState == PlayerPosState.Middle)
            {
                if (bMinTiming)
                    return;

                StopCoroutine("IEFarToMiddle");
                StartCoroutine("IEFarToMiddle");
            }
        }

        /// <summary>
        /// 更多音乐响应
        /// </summary>
        void OnMoreMusicMin()
        {
            if (bUIChanging)
                return;

            StopCoroutine("IEMinToMax");
            StartCoroutine("IEMinToMax");
            btnSliderVolumMax.gameObject.SetActive(false);
        }

        /// <summary>
        /// 播放状态按钮
        /// </summary>
        void OnState()
        {
            RestartTimeMaxToMin();
            curMusicPlayState = ((int)(curMusicPlayState)) == 2 ? 0 : curMusicPlayState + 1;
            btnStateAllloop.gameObject.SetActive(curMusicPlayState == MusicPlayState.AllLoop);
            btnStateOneloop.gameObject.SetActive(curMusicPlayState == MusicPlayState.OneLoop);
            btnStateOrder.gameObject.SetActive(curMusicPlayState == MusicPlayState.Order);
        }

        /// <summary>
        /// 播放
        /// </summary>
        void OnPlay(int index = -1)
        {
            //播放某首音乐、没有暂停过、播放到头了、播放时间为0
            if ((index != -1))
            {
                iCurMusicNum = index;
                SetAudioPlay(iCurMusicNum);
            }
            else if ((_bTempPause == false))
            {
                SetAudioPlay(iCurMusicNum);
            }
            else if ((audioSource.time >= fTotalPlayTime) || (audioSource.time <= 0))
            {
                SetAudioTime(0);
                SetAudioPlay(iCurMusicNum);
            }
            else
            {
                SetAudioUnPause();
            }

            bPlaying = true;
            btnPauseMax.gameObject.SetActive(bPlaying);
            btnPlayMax.gameObject.SetActive(!bPlaying);
            btnPauseMin.gameObject.SetActive(bPlaying);
            btnPlayMin.gameObject.SetActive(!bPlaying);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        void OnPause()
        {
            _bTempPause = true;
            bPlaying = false;
            btnPauseMax.gameObject.SetActive(bPlaying);
            btnPlayMax.gameObject.SetActive(!bPlaying);
            btnPauseMin.gameObject.SetActive(bPlaying);
            btnPlayMin.gameObject.SetActive(!bPlaying);
            SetAudioPause();
        }

        /// <summary>
        /// 向左切换按钮
        /// </summary>
        public void OnLeft()
        {
            imgSliderMin.fillAmount = 0;
            _SetCurMusicNum(iCurMusicNum - 1);
            bClockWise = true;
            _InitEachMusicAnim();
        }
        /// <summary>
        /// 向右切换按钮
        /// </summary>
        public void OnRight()
        {
            imgSliderMin.fillAmount = 0;
            _SetCurMusicNum(iCurMusicNum + 1);
            bClockWise = false;
            _InitEachMusicAnim();
        }

        /// <summary>
        /// 切换完后，初始化音乐（动画、音频等）
        /// </summary>
        void _InitEachMusicAnim()
        {
            if (audioSource.isPlaying)
            {
                _SetCurPlayTime(true);
                SetAudioStop();
            }

            int _iLen = aryEachMusicAttr.Length;

            Vector3 _v3 = targetRotation.eulerAngles;

            if (bClockWise)
                _v3.y -= 51.5f;
            else
                _v3.y += 51.5f;
            targetRotation.eulerAngles = _v3;


            if (bMinTiming)
            {
                //移动完之后，赋值中间位置的对象
                Transform _tra = aryEachMusicAttr[iCurMusicNum - 1].transform;
                _tra.SetParent(traImgCenter);
                _tra.localScale = v3ScaleCenter;
                _tra.localPosition = Vector3.zero;
                _tra.localEulerAngles = Vector3.zero;
                iImgCenterEnterOrExit = -1;
                traImgCenter.localScale = Vector3.zero;
                traImgCenter.GetComponent<BoxCollider>().enabled = false;
            }


            EachMusicAttr _ema = aryEachMusicAttr[iCurMusicNum - 1];
            SetAudioTime(0);

            _SetCurPlayTime(true);
            textCurMusicNameMax.text = $"{_ema.strName} - {_ema.strAuthor}";
            textCurMusicNameMin.text = _ema.strName;
            textCurMusicAuthorMin.text = _ema.strAuthor;
            imgCurMusicMin.sprite = _ema.sprite;
            imgTempImage.sprite = _ema.sprite;
            audioSource.clip = _ema.audioClip;
            fTotalPlayTime = audioSource.clip.length;
            textTotalPlayTime.text = ((int)(fTotalPlayTime / 60)).ToString("D2") + ":" + ((int)(fTotalPlayTime % 60)).ToString("D2");
            if (bPlaying)
            {
                SetAudioPlay(iCurMusicNum);
            }

            bGoAnim = true;
        }

        //设置当前播放的时间
        void _SetCurPlayTime(bool bSetSlider)
        {
            fCurPlayTime = audioSource.time;
            if (bSetSlider)
            {
                pinchSliderMusicMax.sliderValue = (fCurPlayTime / fTotalPlayTime);
            }
            textCurPlayTime.text = ((int)(fCurPlayTime / 60)).ToString("D2") + ":" + ((int)(fCurPlayTime % 60)).ToString("D2");
        }

        /// <summary>
        /// 设置并显示当前的音乐序号
        /// </summary>
        void _SetCurMusicNum(int iNum)
        {
            traImgCenter.GetComponent<BoxCollider>().enabled = false;
            Transform _tra = aryEachMusicAttr[iCurMusicNum - 1].transform;
            _tra.SetParent(traMaxUIImage.GetChild(iCurMusicNum - 1));
            _tra.localScale = Vector3.one;
            _tra.localPosition = Vector3.zero;
            _tra.localEulerAngles = Vector3.zero;

            RestartTimeMaxToMin();

            iCurMusicNum = iNum;
            if (iNum <= 0)
                iCurMusicNum = iTotalMusicNum;
            else if (iNum > iTotalMusicNum)
                iCurMusicNum = 1;

            textCurMusicNum.text = iCurMusicNum.ToString();
        }
        /// <summary>
        /// 刷新位置消息
        /// </summary>
        void RefreshPos(Vector3 pos)
        {
            if (bUIChanging == true)
                return;

            Vector3 _v3 = v3OriPos;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"音乐的距离:{_dis}");
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;


            float _fFar = LoadPrefab.IconDisData.MusicFar;

            if (_dis > _fFar)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
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

            //UI开始变化
            bUIChanging = true;

            if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
            {
                yield return IEFarToMiddle();
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Far)
            {
                yield return IEMiddleToFar();
            }

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }

        /// <summary>
        /// 远距离=>中距离
        /// </summary>
        IEnumerator IEFarToMiddle()
        {
            //UI开始变化
            bUIChanging = true;

            bMaxTiming = false;
            bMinTiming = true;

            //远距离=>中距离
            //Icon先左移动，然后UI从小变大出现

            if (btnEffect.gameObject.activeSelf)
            {
                btnEffect.gameObject.SetActive(false);
            }

            //1、变Icon
            //Icon的自旋转动画先开启
            RunIconMiddle();

            //Icon自身上下浮动关闭
            animIconFar.enabled = false;
            traIcon.gameObject.SetActive(true);
            traIcon.SetParent(traIconMiddlePos);
            while (true)
            {
                //Icon要复原到1
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * Time.deltaTime * 2);

                traIcon.localPosition = Vector3.Lerp(traIcon.localPosition, Vector3.zero, fIconSpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localPosition, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    traIcon.localPosition = Vector3.zero;
                    break;
                }
                yield return 0;
            }
            //2、变MinUI
            yield return new WaitForSeconds(0.1f);
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);

            while (true)
            {
                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, _v3, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMinUI.localScale, _v3);

                if (_fDis < fThreshold)
                {
                    traMinUI.localScale = _v3;
                    _traBtnMoreMusic.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            _v3 = Vector3.one;
            while (true)
            {
                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, _v3, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMinUI.localScale, _v3);

                if (_fDis < fThreshold)
                {
                    traMinUI.localScale = _v3;
                    _traBtnMoreMusic.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //UI开始变化
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
            if (bMaxTiming)
                yield return IEMaxToMin();

            bMaxTiming = false;
            bMinTiming = false;

            //1、变MinUI
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            while (true)
            {
                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.zero);

                if (_fDis < fThreshold)
                {
                    traMinUI.localScale = Vector3.zero;
                    _traBtnMoreMusic.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }
            yield return new WaitForSeconds(0.1f);

            //2、变Icon
            //Icon的自旋转动画先开启
            RunIconMiddle();
            traIcon.gameObject.SetActive(true);
            traIcon.SetParent(traIconFarPos);
            Vector3 _v3Icon = LoadPrefab.IconSize;
            while (true)
            {
                //播放中，显示音符特效，并Icon缩小为0
                //traIcon.localScale = Vector3.Lerp(traIcon.localScale, (bPlaying ? Vector3.zero : Vector3.one), fIconSpeed * Time.deltaTime * 2);
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, _v3Icon, fIconSpeed * Time.deltaTime * 2);

                traIcon.localPosition = Vector3.Lerp(traIcon.localPosition, Vector3.zero, fIconSpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localPosition, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    //traIcon.localScale = bPlaying ? Vector3.zero : Vector3.one;
                    traIcon.localScale = _v3Icon;
                    traIcon.localPosition = Vector3.zero;
                    //Icon自身上下浮动开启
                    animIconFar.enabled = true;
                    break;
                }
                yield return 0;
            }

            //3、音符
            //如果在播放中，显示音符特效
            if (bPlaying)
            {
                btnEffect.gameObject.SetActive(true);
                //print("音符特效");
            }
            //UI开始变化
            bUIChanging = false;
        }

        /// <summary>
        /// 轻交互=>重交互
        /// </summary>
        IEnumerator IEMinToMax()
        {
            //UI开始变化
            bUIChanging = true;

            bMaxTiming = true;
            bMinTiming = false;

            //1、MinUI缩小，Icon缩小，Min和Max通用的图片放大并且位移

            //Icon的自旋转动画开启
            RunIconMiddle();

            objMinPic.SetActive(false);

            //中距离和近距离的同一个音乐的图片
            Transform _traImgTempImage = imgTempImage.transform.parent.transform;
            _traImgTempImage.GetComponent<Image>().enabled = false;
            _traImgTempImage.gameObject.SetActive(true);
            _traImgTempImage.SetParent(traTempImageMaxPos);
            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);
            //更多音乐按钮对象
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            while (true)
            {
                _traImgTempImage.localScale = Vector3.Lerp(_traImgTempImage.localScale, _v3, fUISpeed * Time.deltaTime);
                _traImgTempImage.localPosition = Vector3.Lerp(_traImgTempImage.localPosition, Vector3.zero, fUISpeed * Time.deltaTime);
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fIconSpeed * Time.deltaTime);
                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, Vector3.zero, fUISpeed * Time.deltaTime);


                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    _traImgTempImage.localScale = _v3;
                    _traImgTempImage.localPosition = Vector3.zero;
                    traIcon.localScale = Vector3.zero;
                    traMinUI.localScale = Vector3.zero;
                    _traBtnMoreMusic.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

            ////2、MaxUI的控制按钮放大，图片旋转出现

            traMaxUIImage.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            float _fOri = traMaxUIImage.localEulerAngles.y;

            Quaternion _quaOri = traMaxUIImage.localRotation;
            Quaternion _quaTar = _quaOri;
            Vector3 _v3Tar = _quaTar.eulerAngles;
            _v3Tar.y -= 51.5f;
            //_v3Tar.y -= 103f;
            _quaTar.eulerAngles = _v3Tar;
            traMaxUIImage.localRotation = _quaTar;
            float _fTar = _fOri - 328.5f;
            //float _fTar = _fOri - 257f;
            float _fAbs = Mathf.Abs(fRotAngle) + 0.1f;

            foreach (var v in aryEachMusicAttr)
            {
                v.transform.localScale = Vector3.zero;
            }

            //[0-6]
            int _iCurRotNum = (iCurMusicNum - 1);
            Transform _traTemp = aryEachMusicAttr[_iCurRotNum].transform;
            float _y1 = traMaxUIImage.localEulerAngles.y;
            float _y2 = _y1 - 51.5f;
            if (_y2 <= 0)
                _y2 = _y2 + 360f;

            while (true)
            {

                traMaxUIImage.Rotate(traMaxUIImage.up, fRotAngle, Space.Self);
                _y1 = traMaxUIImage.localEulerAngles.y;
                if ((_y1 > (_fOri - _fAbs)) && (_y1 < (_fOri + _fAbs)))
                {

                    break;
                }

                //print(_y1 + ":::" + _y2);

                _traTemp = aryEachMusicAttr[_iCurRotNum].transform;
                _traTemp.localScale = Vector3.Lerp(_traTemp.localScale, Vector3.one, fRotSpeed2);

                if ((_y1 > (_y2 - _fAbs)) && (_y1 < (_y2 + _fAbs)))
                {

                    _y2 = (_y1 - 51.5f);
                    if (_y2 <= 0)
                        _y2 = _y2 + 360f;

                    _traTemp.localScale = Vector3.one;
                    _iCurRotNum -= 1;
                    if (_iCurRotNum == iCurMusicNum)
                        _iCurRotNum -= 1;
                    if (_iCurRotNum < 0)
                        _iCurRotNum = 6;
                }
                yield return 0;
            }

            foreach (var v in aryEachMusicAttr)
            {
                v.transform.localScale = Vector3.one;
            }

            aryEachMusicAttr[iCurMusicNum - 1].transform.localScale = v3ScaleCenter;

            while (true)
            {
                _traImgTempImage.localScale = Vector3.Lerp(_traImgTempImage.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUICtr.localScale, _v3);

                if (_fDis < fThreshold)
                {
                    _traImgTempImage.localScale = Vector3.one;
                    //traMaxUICtr.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            //不用复原原点
            //traMaxUIImage.localRotation = _quaOri;

            Vector3 _v32 = new Vector3(1.5f, 1.5f, 1.5f);

            while (true)
            {
                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                //traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, _v32, fUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUIText.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUIText.localScale, _v3);

                if (_fDis < fThreshold)
                {
                    traMaxUICtr.localScale = Vector3.one;
                    //traMaxUIImage.localScale = _v32;
                    traMaxUIText.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //_v32 = new Vector3(1.5f, 1.5f, 1.5f);
            //while (true)
            //{
            //    //traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, _v32, fUISpeed * Time.deltaTime);
            //    traMaxUIText.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.one, fUISpeed * Time.deltaTime);
            //    float _fDis = Vector3.Distance(traMaxUIText.localScale, Vector3.one);

            //    if (_fDis < fThreshold)
            //    {
            //        //traMaxUIImage.localScale = _v32;
            //        traMaxUIText.localScale = Vector3.one;
            //        break;
            //    }
            //    yield return 0;
            //}

            //traImgCenter.gameObject.SetActive(true);

            //objMinPic.SetActive(true);
            traMaxUIImage.gameObject.SetActive(true);
            _traImgTempImage.gameObject.SetActive(false);
            //_traImgTempImage.GetComponent<Image>().enabled = false;
            //_traImgTempImage.SetParent(traTempImageMinPos);
            //_traImgTempImage.localScale = Vector3.one;
            //_traImgTempImage.localPosition = Vector3.zero;

            traImgCenter.GetComponent<BoxCollider>().enabled = true;
            traImgCenter.localScale = Vector3.one;
            //traImgCenter.gameObject.SetActive(true);

            //UI变化结束
            bUIChanging = false;

            RestartTimeMaxToMin();
        }

        /// <summary>
        /// 重交互=>轻交互
        /// </summary>
        IEnumerator IEMaxToMin()
        {
            //UI开始变化
            bUIChanging = true;

            bMaxTiming = false;
            bMinTiming = true;

            //中心图片隐藏
            iImgCenterEnterOrExit = 2;
            traImgCenter.GetComponent<BoxCollider>().enabled = false;
            traImgCenter.localScale = Vector3.zero;

            //中距离和近距离的同一个音乐的图片，这里不要动画
            Transform _traImgTempImage = imgTempImage.transform.parent.transform;
            _traImgTempImage.gameObject.SetActive(true);
            _traImgTempImage.SetParent(traTempImageMinPos);


            //1、MaxUI的控制按钮缩小，图片坠落隐藏

            while (true)
            {
                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUIText.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUICtr.localScale, Vector3.zero);

                if (_fDis < fThreshold)
                {
                    traMaxUICtr.localScale = Vector3.zero;
                    traMaxUIText.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

            //2、MinUI放大，Icon放大，Min和Max通用的图片缩小并且位移
            //Icon的自旋转动画开启
            RunIconMiddle();
            //更多音乐按钮对象
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            Vector3 _v3 = Vector3.one;//new Vector3(1.2f, 1.2f, 1.2f);
            while (true)
            {

                _traImgTempImage.localScale = Vector3.Lerp(_traImgTempImage.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                _traImgTempImage.localPosition = Vector3.Lerp(_traImgTempImage.localPosition, Vector3.zero, 1.5f * fUISpeed * Time.deltaTime);

                traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, Vector3.zero, fUISpeed * Time.deltaTime);


                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * Time.deltaTime);

                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, _v3, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, _v3, fUISpeed * Time.deltaTime);


                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    _traImgTempImage.GetComponent<Image>().enabled = true;

                    objMinPic.SetActive(true);
                    _traImgTempImage.gameObject.SetActive(false);


                    _traImgTempImage.localScale = Vector3.one;
                    _traImgTempImage.localPosition = Vector3.zero;

                    traMaxUIImage.localScale = Vector3.zero;

                    traIcon.localScale = Vector3.one;

                    traMinUI.localScale = _v3;
                    _traBtnMoreMusic.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;
        }


        #region 吸引态，Icon变化，远距离
        [Header("=====吸引态，Icon变化，远距离")]

        //远距离Icon的位置
        public Transform traIconFarPos;
        //中距离Icon的位置
        public Transform traIconMiddlePos;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        //Icon的对象
        public Transform traIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //Icon的移动速度
        public float fIconSpeed = 1;

        void RunIconMiddle()
        {
            foreach (var v in animIconMiddle)
                v.SetTrigger("triRun");
        }
        #endregion

        #region 轻交互，小UI，中距离
        [Header("=====轻交互，小UI，中距离")]
        //小UI（不包含更多音乐按钮，因为动画变化不同）
        public Transform traMinUI;
        //小UI的图片总对象
        public GameObject objMinPic;
        //更多音乐按钮（不在traMinUI的子节点下，因为动画变化不同）
        public ButtonRayReceiver btnMoreMusicMin;
        //左切换按钮
        public ButtonRayReceiver btnLeftMin;
        //右切换按钮
        public ButtonRayReceiver btnRightMin;
        //播放按钮
        public ButtonRayReceiver btnPlayMin;
        //暂停按钮
        public ButtonRayReceiver btnPauseMin;
        //[Header("音乐播放的进度条,这个地方用图片的FileAmount属性控制")]
        //音乐播放的进度条,这个地方用图片的FileAmount属性控制
        public Image imgSliderMin;
        //进度条的圆点
        //public Transform traSliderMinPoint;
        //当前音乐的名称（小UI）
        public Text textCurMusicNameMin;
        //当前音乐的名称（小UI）
        public Text textCurMusicAuthorMin;
        //当前音乐的图片（小UI）
        public Image imgCurMusicMin;

        //轻交互，开始计时，N长时间后无操作，自动变为特效音符
        private bool bMinTiming;
        //轻交互到特效音符，计时用
        private float fMinToEffectTemp = 0f;
        #endregion

        #region 重交互，大UI，近距离
        [Header("=====重交互，大UI，近距离")]
        //大UI的多个图片
        public Transform traMaxUIImage;
        //大UI的控制按钮等
        public Transform traMaxUICtr;
        //大UI的歌曲和作者信息
        public Transform traMaxUIText;

        //重交互，开始计时，N长时间后无操作，自动变为轻交互
        private bool bMaxTiming;
        //重交互到轻交互，计时用
        private float fMaxToMinTemp = 0f;

        //渐隐渐显速度
        public float fFadeSpeed = 0.05f;
        //移动速度
        public float fPosSpeed = 0.04f;
        //旋转速度
        public float fRotSpeed = 0.08f;
        //是否顺时针
        public bool bClockWise = true;

        //播放状态按钮，全部循环
        public ButtonRayReceiver btnStateAllloop;
        //播放状态按钮，单曲循环
        public ButtonRayReceiver btnStateOneloop;
        //播放状态按钮，顺序
        public ButtonRayReceiver btnStateOrder;
        //左切换按钮
        public ButtonRayReceiver btnLeftMax;
        //右切换按钮
        public ButtonRayReceiver btnRightMax;
        //播放按钮
        public ButtonRayReceiver btnPlayMax;
        //暂停按钮
        public ButtonRayReceiver btnPauseMax;
        //音量按钮
        public ButtonRayReceiver btnVolumeMax;
        //音量静音按钮
        public ButtonRayReceiver btnVolumeMuteMax;
        //音量滑动条背景的按钮（碰触，显示音量设置条UI）
        public ButtonRayReceiver btnSliderVolumMax;
        //离开音量滑动调界面按钮
        public ButtonRayReceiver btnExitSliderVolum;
        //音乐播放的进度条
        //public Slider slidMusicMax;
        //音乐播放的进度条
        public PinchSlider pinchSliderMusicMax;
        //音量控制条
        //public Slider slidVolumeMax;
        //音量控制条
        public PinchSlider pinchSliderVolumMax;
        //进度条手动拖拽中
        public bool bSlideDragging;

        //显示当前播放时间
        public Text textCurPlayTime;
        //当前播放音乐的总时长
        public float fTotalPlayTime = 0.1f;
        //显示当前播放音乐的总时长
        public Text textTotalPlayTime;

        //总的音乐个数
        public int iTotalMusicNum = 7;
        //显示当前音乐的序号
        public Text textCurMusicNum;
        //当前音乐的名称（大UI）
        public Text textCurMusicNameMax;
        //是否运行动画
        public bool bGoAnim = false;

        //是否还碰触着音量按钮
        public bool bTouchBtnVolum = false;
        //是否还碰触着音量控制对象
        public bool bTouchObjVolum = false;

        //声音大小
        public float fVolume = 1;
        //声音是否静音
        public bool bMute = false;

        public void OnVolumeOpen()
        {
            if (fVolume <= 0)
                fVolume = 1;
            pinchSliderVolumMax.sliderValue = fVolume;
            JudgeVolume();
            //OnSliderVolume();

            //if (audioSource.volume > 0.01f)
            //{
            //    //audioSource.volume = 0;
            //    SetAudioVolume(0);
            //    pinchSliderVolumMax.sliderValue = 0;
            //    //slidVolumeMax.SetValueWithoutNotify(0);
            //}
            //else
            //{
            //    //audioSource.volume = 1;
            //    SetAudioVolume(1);
            //    pinchSliderVolumMax.sliderValue = 1;
            //    //slidVolumeMax.SetValueWithoutNotify(1);
            //}
        }
        public void OnVolumeMute()
        {
            pinchSliderVolumMax.sliderValue = 0;

            JudgeVolume();
            //OnSliderVolume();

        }
        void OnSystemVolumeChange(XR.XRAudioStream typ, int iValue)
        {
            //Debug.Log(typ + ":" + iValue);
            //系统音量：【0-16】整型
            if (typ == XR.XRAudioStream.XR_AUDIO_STREAM_MUSIC)
            {
                float _f = (float)iValue / OPPOSystemMaxVolum;
                pinchSliderVolumMax.sliderValue = _f;
                OnSliderVolumeChange(_f);
            }
        }
        public void OnSliderVolumeChange(float fValue)
        {
            RestartTimeMaxToMin();
            //if (fVolume == fValue)
            //    return;

            //fVolume = fValue;
            audioSource.volume = fValue;

            bool _bMute = audioSource.volume <= 0.01f;
            btnVolumeMuteMax.gameObject.SetActive(_bMute);
            btnVolumeMax.gameObject.SetActive(!_bMute);

            //这里滑动条数值改变，实时触发，所以不发送消息给IoT

        }
        public void OnSliderVolume()
        {
            fVolume = pinchSliderVolumMax.sliderValue;
            JudgeVolume();
            //SetAudioVolume(pinchSliderVolumMax.sliderValue);
            //audioSource.volume = f;
        }
        void JudgeVolume()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            int _i = Mathf.FloorToInt(pinchSliderVolumMax.sliderValue * OPPOSystemMaxVolum);
            if (_i < 0)
                _i = 0;
            else if (_i > 16)
                _i = 16;
            XR.AudioManager.SetStreamVolume(XR.XRAudioStream.XR_AUDIO_STREAM_MUSIC, _i);
#endif

            SetAudioVolume(pinchSliderVolumMax.sliderValue);
        }

        public void EnterBtnVolum()
        {
            //print("进入音量按钮对象");
            bTouchObjVolum = false;
            bTouchBtnVolum = true;
            btnSliderVolumMax.gameObject.SetActive(true);
        }
        public void ExitBtnVolum()
        {
            //print("离开音量按钮对象");
            bTouchBtnVolum = false;
            HideVolum();
        }
        public void EnterObjVolum()
        {
            //print("进入音量对象");
            bTouchObjVolum = true;
            bTouchBtnVolum = false;
            btnSliderVolumMax.gameObject.SetActive(true);
        }
        public void ExitObjVolum()
        {
            //print("离开音量对象");
            bTouchObjVolum = false;
            HideVolum();
        }
        void HideVolum()
        {
            //延迟0.1秒隐藏，射线在音量图标上，移动到音量控制条上的时候不能隐藏
            CancelInvoke("_InvokeHideVolum");
            Invoke("_InvokeHideVolum", 0.1f);
        }
        void _InvokeHideVolum()
        {
            if (bTouchBtnVolum == true || bTouchObjVolum == true)
                return;
            btnSliderVolumMax.gameObject.SetActive(false);
        }
        #endregion

        #region 音符特效
        //播放过程中的特效（音响周围的特效）
        public ButtonRayReceiver btnEffect;
        #endregion

        /// <summary>
        /// 重置大UI的计时（大UI自动变小UI）
        /// </summary>
        void RestartTimeMaxToMin()
        {
            fMaxToMinTemp = 0;
            fMinToEffectTemp = 0;
        }

        #region 调用CPE接口
        /// <summary>
        /// 音乐数据
        /// </summary>
        public struct MusicData
        {
            public int ErrorCode;
            public int MusicId;
        }
        /// <summary>
        /// 音量数据
        /// </summary>
        public struct VomlumeData
        {
            public int ErrorCode;
            public int Volume;
        }

        void ClickMusic(string str)
        {
            //开启新协程
            IEnumerator enumerator = YoopInterfaceSupport.SendDataToCPE<MusicData>(YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.cpeipport] + "iot/music/" + str,
                //回调
                (sd) =>
                {
                    Debug.Log("MyLog::音乐" + str + ":" + sd.MusicId);
                }
                );

            ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }

        void ClickMusicVolume(string str)
        {
            //开启新协程
            IEnumerator enumerator = YoopInterfaceSupport.SendDataToCPE<VomlumeData>(YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.cpeipport] + "iot/music/setting?action=setVolume&value=" + str,
                //回调
                (sd) =>
                {
                    Debug.Log("MyLog::音响" + str + ":" + sd.Volume);
                }
                );

            ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }

        public void SetAudioPlay(int index = -1)
        {
            RestartTimeMaxToMin();
            if (index != -1)
            {
                audioSource.Play();
                //===========================================================================
                //CPE发送：带序号播放
                ClickMusic("play?id=" + index.ToString());
                //===========================================================================
            }
        }
        public void SetAudioUnPause()
        {
            RestartTimeMaxToMin();
            audioSource.UnPause();
            //===========================================================================
            //CPE发送：恢复暂停
            ClickMusic("resume");
            //===========================================================================
        }
        public void SetAudioPause()
        {
            RestartTimeMaxToMin();
            audioSource.Pause();
            //===========================================================================
            //CPE发送：暂停
            ClickMusic("pause");
            //===========================================================================
        }
        public void SetAudioStop()
        {
            RestartTimeMaxToMin();
            audioSource.Stop();
            //===========================================================================
            //CPE发送：停止
            ClickMusic("stop");
            //===========================================================================
        }
        public void SetAudioVolume(float fValue)
        {
            RestartTimeMaxToMin();
            audioSource.volume = fValue;

            bool _bMute = audioSource.volume <= 0.01f;
            btnVolumeMuteMax.gameObject.SetActive(_bMute);
            btnVolumeMax.gameObject.SetActive(!_bMute);

            //音量CPE赋值：数值乘以100
            fValue *= 100;
            //===========================================================================
            //CPE发送：音量
            ClickMusicVolume(fValue.ToString("f0"));
            //===========================================================================
        }
        public void SetAudioTime(float fTime)
        {
            RestartTimeMaxToMin();
            if (fTime >= fTotalPlayTime)
            {
                AutoNext();
                return;
            }
            bool _bPlaying = audioSource.isPlaying;
            //暂停情况下先播放，audioSource.time才能被赋值（暂停情况下，拖拽进度条，设置当前播放时间用）
            if (_bPlaying == false)
                audioSource.Play();
            audioSource.time = fTime;

            if (_bPlaying == false)
                audioSource.Pause();

            //进度CPE赋值：数值即为当前秒数
            //===========================================================================
            //CPE发送：进度
            ClickMusic("setting?action=setPace&value=" + fTime.ToString());
            //===========================================================================
        }

        #endregion
    }
}