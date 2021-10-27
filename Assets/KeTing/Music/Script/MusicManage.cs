﻿/* Create by zh at 2021-09-17

    音乐总控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceDesign.Music
{
    public enum MusicAnimType
    {
        Center,//【唯一】最中间居中的
        AllRight,//【唯一】全部（包括不显示的）：最右边的
        AlLeft,//【唯一】全部（包括不显示的）：最左边的
        ShowRight,//【唯一】显示：最右边的
        ShowLeft,//【唯一】显示：最左边的
        Other,//【不唯一】
    }
    public enum MusicPlayState
    {
        AllLoop,
        OneLoop,
        Order,
        //Random
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

        [Header("(7个图片内容，最右边从0开始，从右往左，依次增大)")]
        public EachMusicAnim[] aryEachMusicAnim;

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

        //Icon、UI等正在切换中
        private bool bUIChanging = false;
        //运动阈值
        private float fThreshold = 0.1f;

        //对象初始位置
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================

        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;

            btnEffect.onPinchUp.AddListener(ClickFarToMiddle);
            btnIcon.onPinchUp.AddListener(ClickFarToMiddle);
            btnMoreMusicMin.onPinchUp.AddListener(OnMoreMusicMin);
            btnLeftMin.onPinchUp.AddListener(OnLeft);
            btnLeftMax.onPinchUp.AddListener(OnLeft);
            btnRightMin.onPinchUp.AddListener(OnRight);
            btnRightMax.onPinchUp.AddListener(OnRight);
            btnPlayMin.onPinchUp.AddListener(() => { OnPlay(); });
            btnPlayMax.onPinchUp.AddListener(() => { OnPlay(); });
            btnPauseMin.onPinchUp.AddListener(OnPause);
            btnPauseMax.onPinchUp.AddListener(OnPause);

            btnStateAllloop.onPinchUp.AddListener(OnState);
            btnStateOneloop.onPinchUp.AddListener(OnState);
            btnStateOrder.onPinchUp.AddListener(OnState);

            btnVolumeMax.onPointerEnter.AddListener(EnterBtnVolum);
            btnVolumeMax.onPointerExit.AddListener(ExitBtnVolum);
            btnVolumeMax.onPinchUp.AddListener(OnBtnVolume);
            btnExitSliderVolum.onPointerExit.AddListener(ExitObjVolum);
            btnSliderVolumMax.onPointerEnter.AddListener(EnterObjVolum);
            //因为要发送给CPE：不要实时变化，抬起才触发一次
            //pinchSliderVolumMax.onValueChanged.AddListener(OnSliderVolume);
            pinchSliderVolumMax.onInteractionEnd.AddListener(OnSliderVolume);

            pinchSliderMusicMax.onInteractionStart.AddListener(SliderMusicMaxPointerDown);
            pinchSliderMusicMax.onInteractionEnd.AddListener(SliderMusicMaxPointerUp);

        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnEffect.onPinchUp.RemoveAllListeners();
            btnIcon.onPinchUp.RemoveAllListeners();
            btnMoreMusicMin.onPinchUp.RemoveAllListeners();
            btnLeftMin.onPinchUp.RemoveAllListeners();
            btnLeftMax.onPinchUp.RemoveAllListeners();
            btnRightMin.onPinchUp.RemoveAllListeners();
            btnRightMax.onPinchUp.RemoveAllListeners();
            btnPlayMin.onPinchUp.RemoveAllListeners();
            btnPlayMax.onPinchUp.RemoveAllListeners();
            btnPauseMin.onPinchUp.RemoveAllListeners();
            btnPauseMax.onPinchUp.RemoveAllListeners();

            btnStateAllloop.onPinchUp.RemoveAllListeners();
            btnStateOneloop.onPinchUp.RemoveAllListeners();
            btnStateOrder.onPinchUp.RemoveAllListeners();

            btnVolumeMax.onPointerEnter.RemoveAllListeners();
            btnVolumeMax.onPointerExit.RemoveAllListeners();
            btnVolumeMax.onPinchUp.RemoveAllListeners();
            btnSliderVolumMax.onPointerEnter.RemoveAllListeners(); ;
            btnExitSliderVolum.onPointerExit.RemoveAllListeners();
            pinchSliderVolumMax.onValueChanged.RemoveAllListeners();
            pinchSliderMusicMax.onInteractionStart.RemoveAllListeners();
            pinchSliderMusicMax.onInteractionEnd.RemoveAllListeners();
        }

        void OnDestroy()
        {
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

            //开始的时候刷新一下数据
            _SetCurMusicNum(1);
            OnLeft();
            OnRight();

        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Z))
            //{
            //    OnLeft();
            //}
            //if (Input.GetKeyDown(KeyCode.X))
            //{
            //    OnRight();
            //}
            //if (Input.GetKeyDown(KeyCode.C))
            //{
            //    OnPlay();
            //}
            //if (Input.GetKeyDown(KeyCode.V))
            //{
            //    OnPause();
            //}

            //if (Input.GetMouseButtonDown(0))
            //{
            //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //    if (Physics.Raycast(ray, out RaycastHit hit, 50)) //如果碰撞检测到物体
            //    {
            //        if (hit.transform.gameObject.Equals(objEffect))
            //            EffectToMinUI();
            //    }
            //}

            if (bPlaying)
            {
                imgSliderMin.fillAmount = pinchSliderMusicMax.sliderValue;//slidMusicMax.value;
                //traSliderMinPoint.localEulerAngles = new Vector3(0, 0, -180 * imgSliderMin.fillAmount);

                //播放中，中距离，小UI，倒计时，自动变音符特效对象
                if (bMinTiming)
                {
                    fMinToEffectTemp += Time.deltaTime;
                    if (fMinToEffectTemp > fAutoTurnUITime)
                    {
                        StopAllCoroutines();
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
                    StopAllCoroutines();
                    StartCoroutine("IEMaxToMin");
                    fMaxToMinTemp = 0;
                }
            }

            if (bGoAnim)
            {
                bGoAnim = false;
                foreach (var v in aryEachMusicAnim)
                {
                    //有一个未完成，这里都要继续
                    if (v.RefreshMotion() == false)
                    {
                        bGoAnim = true;
                    }
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
                        //print($"播完了，切换:{musicPlayState}");

                        switch (curMusicPlayState)
                        {
                            case MusicPlayState.AllLoop:
                                OnRight();
                                break;
                            case MusicPlayState.OneLoop:
                                //audioSource.Stop();
                                //audioSource.time = 0;
                                //audioSource.Play();
                                OnPlay(iCurMusicNum);
                                break;
                            case MusicPlayState.Order:
                                if (iCurMusicNum < iTotalMusicNum)
                                    OnRight();
                                else
                                {
                                    SetAudioTime(0);
                                    //audioSource.time = 0;
                                    OnPause();
                                }
                                break;
                        }
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
                float _f = Vector3.Distance(traImgCenter.localScale, v3ImgCenterEnter);
                if (_f > 0.01f)
                {
                    traImgCenter.localScale = Vector3.Lerp(traImgCenter.localScale, v3ImgCenterEnter, 0.1f);
                }
                else
                {
                    traImgCenter.localScale = v3ImgCenterEnter;
                }

            }
            else if (iImgCenterEnterOrExit == 1)
            {
                float _f = Vector3.Distance(traImgCenter.localScale, Vector3.one);
                if (_f > 0.01f)
                {
                    traImgCenter.localScale = Vector3.Lerp(traImgCenter.localScale, Vector3.one, 0.1f);
                }
                else
                {
                    iImgCenterEnterOrExit = -1;
                    traImgCenter.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// 七张音乐图的，中间图片对象
        /// </summary>
        public Transform traImgCenter;

        //抬起在中新图片的左右（0-左短，1-左长，2-右短，3-右长）
        public int iUpCenterPos = -1;
        //中心图片进入或离开（0-进入，1-离开）
        public int iImgCenterEnterOrExit = -1;

        /// <summary>
        /// 按下中心图片的放大
        /// </summary>
        private Vector3 v3ImgCenterEnter = new Vector3(1.2f, 1.2f, 1.2f);

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
        public void SliderMusicMaxPointerDown()
        {
            bSlideDragging = true;
        }

        /// <summary>
        /// 音乐进度条抬起
        /// </summary>
        public void SliderMusicMaxPointerUp()
        {
            bSlideDragging = false;
            SetAudioTime(pinchSliderMusicMax.sliderValue * fTotalPlayTime);
            //audioSource.time = (pinchSliderMusicMax.sliderValue * fTotalPlayTime);
        }

        /// <summary>
        /// 点击Button，触发从远到近的动画
        /// </summary>
        public void ClickFarToMiddle()
        {
            if (curPlayerPosState == PlayerPosState.Middle)
            {
                print(11111);
                StopAllCoroutines();
                StartCoroutine("IEFarToMiddle");
            }
        }

        /// <summary>
        /// 更多音乐响应
        /// </summary>
        public void OnMoreMusicMin()
        {
            StopAllCoroutines();
            StartCoroutine("IEMinToMax");
        }

        /// <summary>
        /// 播放状态按钮
        /// </summary>
        public void OnState()
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
        public void OnPlay(int index = -1)
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

        //临时计算是否暂停过用
        bool _bTempPause = false;
        /// <summary>
        /// 暂停
        /// </summary>
        public void OnPause()
        {
            _bTempPause = true;
            bPlaying = false;
            btnPauseMax.gameObject.SetActive(bPlaying);
            btnPlayMax.gameObject.SetActive(!bPlaying);
            btnPauseMin.gameObject.SetActive(bPlaying);
            btnPlayMin.gameObject.SetActive(!bPlaying);
            //audioSource.Pause();
            SetAudioPause();
        }

        /// <summary>
        /// 向左切换按钮
        /// </summary>
        public void OnLeft()
        {
            _SetCurMusicNum(iCurMusicNum - 1);
            bClockWise = true;
            _InitEachMusicAnim();
        }
        /// <summary>
        /// 向右切换按钮
        /// </summary>
        public void OnRight()
        {
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
                //audioSource.time = 0;
                SetAudioTime(0);
                _SetCurPlayTime(true);
                //audioSource.Stop();
                SetAudioStop();
            }

            int _iLen = aryEachMusicAnim.Length;

            if (bClockWise)
            {
                for (int i = 0; i < _iLen; i++)
                {
                    EachMusicAnim emp = null;
                    if (i < _iLen - 1)
                        emp = aryEachMusicAnim[i + 1];
                    else if (i == (_iLen - 1))
                        emp = aryEachMusicAnim[0];
                    else
                        break;
                    aryEachMusicAnim[i].Init(emp);
                }
            }
            else
            {
                for (int i = _iLen - 1; i >= 0; i--)
                {
                    EachMusicAnim emp = null;
                    if (i > 0)
                        emp = aryEachMusicAnim[i - 1];
                    else if (i == 0)
                        emp = aryEachMusicAnim[_iLen - 1];
                    else
                        break;
                    aryEachMusicAnim[i].Init(emp);
                }
            }

            foreach (var v in aryEachMusicAnim)
            {
                v.SetValue();
                //最中间的音乐播放
                if (v.musicPicType == MusicAnimType.Center)
                {
                    ////bool _bAutoPlay = audioSource.isPlaying;
                    ////if (_bAutoPlay)
                    ////    audioSource.Stop();

                    //audioSource.time = 0;
                    SetAudioTime(0);

                    _SetCurPlayTime(true);
                    textCurMusicNameMax.text = $"{v.emaCurChild.strName} - {v.emaCurChild.strAuthor}";
                    textCurMusicNameMin.text = v.emaCurChild.strName;
                    textCurMusicAuthorMin.text = v.emaCurChild.strAuthor;
                    imgCurMusicMin.sprite = v.emaCurChild.sprite;
                    imgTempImage.sprite = v.emaCurChild.sprite;
                    audioSource.clip = v.emaCurChild.audioClip;
                    fTotalPlayTime = audioSource.clip.length;
                    textTotalPlayTime.text = ((int)(fTotalPlayTime / 60)).ToString("D2") + ":" + ((int)(fTotalPlayTime % 60)).ToString("D2");
                    if (bPlaying)
                    {
                        //audioSource.Play();
                        SetAudioPlay(iCurMusicNum);
                    }
                }
            }

            bGoAnim = true;
        }

        //设置当前播放的时间
        void _SetCurPlayTime(bool bSetSlider)
        {
            fCurPlayTime = audioSource.time;
            if (bSetSlider)
            {
                //slidMusicMax.SetValueWithoutNotify(fCurPlayTime / fTotalPlayTime);
                pinchSliderMusicMax.sliderValue = (fCurPlayTime / fTotalPlayTime);
            }
            //print($"{((int)(_f / 60))}----{((int)(_f % 60))}");
            textCurPlayTime.text = ((int)(fCurPlayTime / 60)).ToString("D2") + ":" + ((int)(fCurPlayTime % 60)).ToString("D2");
        }


        /// <summary>
        /// 设置并显示当前的音乐序号
        /// </summary>
        void _SetCurMusicNum(int iNum)
        {
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
        public void RefreshPos(Vector3 pos)
        {
            if (bUIChanging == true)
                return;

            Vector3 _v3 = v3OriPos;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"音乐的距离:{_dis}");
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            if (_dis >= 5f)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else //if (_dis < 5f && _dis > 1.5f)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            //else if (_dis <= 1.5f)
            //{
            //    curPlayerPosState = PlayerPosState.Close;
            //    if (lastPPS == PlayerPosState.Close)
            //        return;
            //}

            StopAllCoroutines();
            StartCoroutine("IERefreshPos", lastPPS);
        }


        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

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


        }
        /// <summary>
        /// 中距离=>远距离
        /// </summary>
        IEnumerator IEMiddleToFar()
        {
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
            while (true)
            {
                //播放中，显示音符特效，并Icon缩小为0
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, (bPlaying ? Vector3.zero : Vector3.one), fIconSpeed * Time.deltaTime * 2);

                traIcon.localPosition = Vector3.Lerp(traIcon.localPosition, Vector3.zero, fIconSpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localPosition, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = bPlaying ? Vector3.zero : Vector3.one;
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

            //2、MaxUI的控制按钮放大，图片旋转出现

            while (true)
            {
                _traImgTempImage.localScale = Vector3.Lerp(_traImgTempImage.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUICtr.localScale, _v3);

                if (_fDis < fThreshold)
                {
                    _traImgTempImage.localScale = Vector3.one;
                    traMaxUICtr.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            traImgCenter.gameObject.SetActive(false);
            Vector3 _v32 = new Vector3(1.7f, 1.7f, 1.7f);
            while (true)
            {
                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, _v32, fUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUIText.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUIText.localScale, _v3);

                if (_fDis < fThreshold)
                {
                    traMaxUICtr.localScale = Vector3.one;
                    traMaxUIImage.localScale = _v32;
                    traMaxUIText.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            _v32 = new Vector3(1.5f, 1.5f, 1.5f);
            while (true)
            {
                traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, _v32, fUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUIText.localScale, Vector3.one);

                if (_fDis < fThreshold)
                {
                    traMaxUIImage.localScale = _v32;
                    traMaxUIText.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

            traImgCenter.gameObject.SetActive(true);

            objMinPic.SetActive(true);
            traMaxUIImage.gameObject.SetActive(true);
            _traImgTempImage.gameObject.SetActive(false);
            _traImgTempImage.GetComponent<Image>().enabled = true;
            _traImgTempImage.SetParent(traTempImageMinPos);
            _traImgTempImage.localScale = Vector3.one;
            _traImgTempImage.localPosition = Vector3.zero;

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

            //traMaxUIImage.gameObject.SetActive(false);

            //中距离和近距离的同一个音乐的图片，这里不要动画
            //Transform _traImgTempImage = imgTempImage.transform.parent.transform;
            //_traImgTempImage.gameObject.SetActive(true);
            //_traImgTempImage.SetParent(traTempImageMinPos);

            //1、MaxUI的控制按钮缩小，图片坠落隐藏

            while (true)
            {
                traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUIText.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUIText.localScale, Vector3.zero);

                if (_fDis < fThreshold)
                {
                    traMaxUIImage.localScale = Vector3.zero;
                    traMaxUIText.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

            while (true)
            {
                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUICtr.localScale, Vector3.zero);

                if (_fDis < fThreshold)
                {
                    traMaxUICtr.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

            //2、MinUI放大，Icon放大，Min和Max通用的图片缩小并且位移
            //Icon的自旋转动画开启
            RunIconMiddle();
            //更多音乐按钮对象
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * Time.deltaTime);

                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, _v3, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, _v3, fUISpeed * Time.deltaTime);


                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {

                    traIcon.localScale = Vector3.one;

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
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, _v3, fUISpeed * Time.deltaTime * 1.1f);

                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {

                    traMinUI.localScale = _v3;
                    _traBtnMoreMusic.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;
        }


        #region 吸引态，Icon变化，原距离（大于5米）
        [Header("=====吸引态，Icon变化，原距离（大于5米）")]

        //远距离Icon的位置
        public Transform traIconFarPos;
        //中距离Icon的位置
        public Transform traIconMiddlePos;
        //Icon的对象
        public Transform traIcon;
        //Icon对象的AR手势Button按钮
        public ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
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

        #region 轻交互，小UI，中距离（小于5米，大于1.5米）
        [Header("=====轻交互，小UI，中距离（小于5米，大于1.5米）")]
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

        #region 重交互，大UI，近距离（小于1.5米）

        [Header("=====重交互，大UI，近距离（小于1.5米）")]
        //重交互，开始计时，N长时间后无操作，自动变为轻交互
        private bool bMaxTiming;
        //重交互到轻交互，计时用
        private float fMaxToMinTemp = 0f;

        //大UI的多个图片
        public Transform traMaxUIImage;
        //大UI的控制按钮等
        public Transform traMaxUICtr;
        //大UI的歌曲和作者信息
        public Transform traMaxUIText;

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
        private bool bGoAnim = false;

        //是否还碰触着音量按钮
        public bool bTouchBtnVolum = false;
        //是否还碰触着音量控制对象
        public bool bTouchObjVolum = false;

        public void OnBtnVolume()
        {
            if (audioSource.volume > 0.01f)
            {
                //audioSource.volume = 0;
                SetAudioVolume(0);
                pinchSliderVolumMax.sliderValue = 0;
                //slidVolumeMax.SetValueWithoutNotify(0);
            }
            else
            {
                //audioSource.volume = 1;
                SetAudioVolume(1);
                pinchSliderVolumMax.sliderValue = 1;
                //slidVolumeMax.SetValueWithoutNotify(1);
            }
        }
        public void OnSliderVolume()
        {
            SetAudioVolume(pinchSliderVolumMax.sliderValue);
            //audioSource.volume = f;
        }

        public void EnterBtnVolum()
        {
            bTouchBtnVolum = true;
            btnSliderVolumMax.gameObject.SetActive(true);
        }
        public void ExitBtnVolum()
        {
            bTouchBtnVolum = false;
            HideVolum();
        }
        public void EnterObjVolum()
        {
            bTouchObjVolum = true;
            btnSliderVolumMax.gameObject.SetActive(true);
        }
        public void ExitObjVolum()
        {
            bTouchObjVolum = false;
            HideVolum();
        }
        void HideVolum()
        {
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
        public void SetAudioPlay(int index = -1)
        {
            RestartTimeMaxToMin();
            if (index != -1)
            {
                audioSource.Play();
                //===========================================================================
                //CPE发送：带序号播放
                //===========================================================================
            }
        }
        public void SetAudioUnPause()
        {
            RestartTimeMaxToMin();
            audioSource.UnPause();
            //===========================================================================
            //CPE发送：恢复暂停
            //===========================================================================
        }
        public void SetAudioPause()
        {
            RestartTimeMaxToMin();
            audioSource.Pause();
            //===========================================================================
            //CPE发送：暂停
            //===========================================================================
        }
        public void SetAudioStop()
        {
            RestartTimeMaxToMin();
            audioSource.Stop();
            //===========================================================================
            //CPE发送：停止
            //===========================================================================
        }
        public void SetAudioVolume(float fValue)
        {
            RestartTimeMaxToMin();
            audioSource.volume = fValue;

            //音量CPE赋值：数值乘以100
            fValue *= 100;
            //===========================================================================
            //CPE发送：音量
            //===========================================================================
        }
        public void SetAudioTime(float fTime)
        {
            RestartTimeMaxToMin();
            audioSource.time = fTime;

            //进度CPE赋值：数值即为当前秒数
            //===========================================================================
            //CPE发送：进度
            //===========================================================================
        }

        #endregion
    }
}