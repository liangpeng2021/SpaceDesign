/* Create by zh at 2021-09-17

    音乐总控制脚本

 */

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
        //播放模型
        public Transform traMusicModel;
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
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.05f;

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
            //播放的循环，不用该值控制
            audioSource.loop = false;

            btnLeftMin.onClick.AddListener(OnLeft);
            btnLeftMax.onClick.AddListener(OnLeft);
            btnRightMin.onClick.AddListener(OnRight);
            btnRightMax.onClick.AddListener(OnRight);
            btnPlayMin.onClick.AddListener(OnPlay);
            btnPlayMax.onClick.AddListener(OnPlay);
            btnPauseMin.onClick.AddListener(OnPause);
            btnPauseMax.onClick.AddListener(OnPause);

            //btnStateMax.onClick.AddListener(OnState);
            btnStateAllloop.onClick.AddListener(OnState);
            btnStateOneloop.onClick.AddListener(OnState);
            btnStateOrder.onClick.AddListener(OnState);

            btnMoreMusicMin.onClick.AddListener(() => { StartCoroutine(IEMinToMax()); });

            btnVolumeMax.onClick.AddListener(OnBtnVolume);
            slidVolumeMax.onValueChanged.AddListener(OnSliderVolume);

            //=====================MaxUI里面拖拽进度条===================================
            EventTrigger _trigger = slidMusicMax.GetComponent<EventTrigger>();
            if (_trigger == null)
                _trigger = slidMusicMax.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry _entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
                callback = new EventTrigger.TriggerEvent(),
            };
            _entry.callback.AddListener(x => { bSlideDragging = true; });
            _trigger.triggers.Add(_entry);

            _entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
                callback = new EventTrigger.TriggerEvent(),
            };
            _entry.callback.AddListener(x => { bSlideDragging = false; audioSource.time = slidMusicMax.value * fTotalPlayTime; });
            _trigger.triggers.Add(_entry);
            //===========================================================================

            //开始的时候刷新一下数据
            _SetCurMusicNum(1);
            OnLeft();
            OnRight();

            //赋值Canvas的worldCamera
            Canvas[] _canvasAry = transform.GetComponentsInChildren<Canvas>();
            foreach (Canvas v in _canvasAry)
            {
                v.worldCamera = XR.XRCameraManager.Instance.eventCamera;
            }
        }

        void Update()
        {
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
                imgSliderMin.fillAmount = slidMusicMax.value;
                //traSliderMinPoint.localEulerAngles = new Vector3(0, 0, -180 * imgSliderMin.fillAmount);

                //播放中，中距离，小UI，倒计时，自动变音符特效对象
                if (bMinTiming)
                {
                    fMinToEffectTemp += Time.deltaTime;
                    if (fMinToEffectTemp > fAutoTurnUITime)
                    {
                        StartCoroutine(IEMiddleToFar());
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
                    StartCoroutine(IEMaxToMin());
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
                                audioSource.Stop();
                                audioSource.time = 0;
                                audioSource.Play();
                                break;
                            case MusicPlayState.Order:
                                if (iCurMusicNum < iTotalMusicNum)
                                    OnRight();
                                else
                                {
                                    audioSource.time = 0;
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
        }

        /// <summary>
        /// 播放状态按钮
        /// </summary>
        public void OnState()
        {
            RestartTimeMaxToMin();
            curMusicPlayState = ((int)(curMusicPlayState)) == 2 ? 0 : curMusicPlayState + 1;
            //string _str = null;
            btnStateAllloop.gameObject.SetActive(curMusicPlayState == MusicPlayState.AllLoop);
            btnStateOneloop.gameObject.SetActive(curMusicPlayState == MusicPlayState.OneLoop);
            btnStateOrder.gameObject.SetActive(curMusicPlayState == MusicPlayState.Order);
            //switch (curMusicPlayState)
            //{
            //    case MusicPlayState.AllLoop:
            //        //_str = "全部循环"; 
            //        btnStateOneloop.gameObject.SetActive(true);
            //        break;
            //    case MusicPlayState.OneLoop:
            //        //_str = "单首循环";
            //        btnStateOneloop.gameObject.SetActive(false);
            //        btnStateOrder.gameObject.SetActive(true);
            //        break;
            //    case MusicPlayState.Order:
            //        //_str = "顺序播放";
            //        btnStateOrder.gameObject.SetActive(false);
            //        btnStateAllloop.gameObject.SetActive(true);
            //        break;
            //}
            //btnStateMax.GetComponentInChildren<Text>().text = _str;
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void OnPlay()
        {
            RestartTimeMaxToMin();
            bPlaying = true;
            btnPauseMax.gameObject.SetActive(bPlaying);
            btnPlayMax.gameObject.SetActive(!bPlaying);
            btnPauseMin.gameObject.SetActive(bPlaying);
            btnPlayMin.gameObject.SetActive(!bPlaying);
            if (audioSource.time >= fTotalPlayTime)
                audioSource.time = 0;
            audioSource.Play();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void OnPause()
        {
            RestartTimeMaxToMin();
            bPlaying = false;
            btnPauseMax.gameObject.SetActive(bPlaying);
            btnPlayMax.gameObject.SetActive(!bPlaying);
            btnPauseMin.gameObject.SetActive(bPlaying);
            btnPlayMin.gameObject.SetActive(!bPlaying);
            audioSource.Pause();
        }

        /// <summary>
        /// 向左切换按钮
        /// </summary>
        public void OnLeft()
        {
            RestartTimeMaxToMin();
            _SetCurMusicNum(iCurMusicNum - 1);
            bClockWise = true;
            _InitEachMusicAnim();
        }
        /// <summary>
        /// 向右切换按钮
        /// </summary>
        public void OnRight()
        {
            RestartTimeMaxToMin();
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
                audioSource.time = 0;
                _SetCurPlayTime(true);
                audioSource.Stop();
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
                    //bool _bAutoPlay = audioSource.isPlaying;
                    //if (_bAutoPlay)
                    //    audioSource.Stop();
                    audioSource.time = 0;
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
                        audioSource.Play();
                }
            }

            bGoAnim = true;
        }

        //设置当前播放的时间
        void _SetCurPlayTime(bool bSetSlider)
        {
            fCurPlayTime = audioSource.time;
            if (bSetSlider)
                slidMusicMax.SetValueWithoutNotify(fCurPlayTime / fTotalPlayTime);
            //print($"{((int)(_f / 60))}----{((int)(_f % 60))}");
            textCurPlayTime.text = ((int)(fCurPlayTime / 60)).ToString("D2") + ":" + ((int)(fCurPlayTime % 60)).ToString("D2");
        }


        /// <summary>
        /// 设置并显示当前的音乐序号
        /// </summary>
        void _SetCurMusicNum(int iNum)
        {
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

            Vector3 _v3 = traMusicModel.position;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"音乐的距离:{_dis}");

            PlayerPosState lastPPS = curPlayerPosState;

            if (_dis >= 5f)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                curPlayerPosState = PlayerPosState.Far;
            }
            else //if (_dis < 5f && _dis > 1.5f)
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

            //UI开始变化
            bUIChanging = true;

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

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

            if (objEffect.activeSelf)
            {
                objEffect.SetActive(false);
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
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * 5f * Time.deltaTime);

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
            while (true)
            {
                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, Vector3.one, fUISpeed * 1.1f * Time.deltaTime);
                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.one);

                if (_fDis < fThreshold)
                {
                    traMinUI.localScale = Vector3.one;
                    _traBtnMoreMusic.localScale = Vector3.one;
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
            bMaxTiming = false;
            bMinTiming = false;

            //中距离=>远距离

            //1、变MinUI
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            while (true)
            {
                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, Vector3.zero, fUISpeed * 1.1f * Time.deltaTime);
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
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, (bPlaying ? Vector3.zero : Vector3.one), fIconSpeed * 1.5f * Time.deltaTime);

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
                objEffect.SetActive(true);
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
            //更多音乐按钮对象
            Transform _traBtnMoreMusic = btnMoreMusicMin.transform;
            while (true)
            {
                _traImgTempImage.localScale = Vector3.Lerp(_traImgTempImage.localScale, Vector3.one, fIconSpeed * 1.5f * Time.deltaTime);
                _traImgTempImage.localPosition = Vector3.Lerp(_traImgTempImage.localPosition, Vector3.zero, fIconSpeed * 2.5f * Time.deltaTime);


                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fIconSpeed * 1.5f * Time.deltaTime);

                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, Vector3.zero, fUISpeed * 1.1f * Time.deltaTime);


                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    _traImgTempImage.localScale = Vector3.one;
                    _traImgTempImage.localPosition = Vector3.zero;

                    traIcon.localScale = Vector3.zero;

                    traMinUI.localScale = Vector3.zero;
                    _traBtnMoreMusic.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }
            //yield return new WaitForSeconds(0.1f);

            //2、MaxUI的控制按钮放大，图片旋转出现

            while (true)
            {

                traMaxUICtr.localScale = Vector3.Lerp(traMaxUICtr.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUICtr.localScale, Vector3.one);

                if (_fDis < fThreshold)
                {
                    traMaxUICtr.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }
            //yield return new WaitForSeconds(0.1f);

            while (true)
            {
                //traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, Vector3.one, fMinUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUIText.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUIText.localScale, Vector3.one);

                if (_fDis < fThreshold)
                {
                    //traMaxUIImage.localScale = Vector3.one;
                    traMaxUIText.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

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

            traMaxUIImage.gameObject.SetActive(false);

            //中距离和近距离的同一个音乐的图片，这里不要动画
            //Transform _traImgTempImage = imgTempImage.transform.parent.transform;
            //_traImgTempImage.gameObject.SetActive(true);
            //_traImgTempImage.SetParent(traTempImageMinPos);

            //1、MaxUI的控制按钮缩小，图片坠落隐藏

            while (true)
            {
                //traMaxUIImage.localScale = Vector3.Lerp(traMaxUIImage.localScale, Vector3.zero, fMinUISpeed * Time.deltaTime);
                traMaxUIText.localScale = Vector3.Lerp(traMaxUIText.localScale, Vector3.zero, fUISpeed * 2 * Time.deltaTime);
                float _fDis = Vector3.Distance(traMaxUIText.localScale, Vector3.zero);

                if (_fDis < fThreshold)
                {
                    //traMaxUIImage.localScale = Vector3.zero;
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
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * 1.5f * Time.deltaTime);

                traMinUI.localScale = Vector3.Lerp(traMinUI.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                _traBtnMoreMusic.localScale = Vector3.Lerp(_traBtnMoreMusic.localScale, Vector3.one, fUISpeed * 1.1f * Time.deltaTime);


                float _fDis = Vector3.Distance(traMinUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {

                    traIcon.localScale = Vector3.one;

                    traMinUI.localScale = Vector3.one;
                    _traBtnMoreMusic.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }
            //yield return new WaitForSeconds(0.1f);

            //_traImgTempImage.gameObject.SetActive(false);
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
        public Button btnMoreMusicMin;
        //左切换按钮
        public Button btnLeftMin;
        //右切换按钮
        public Button btnRightMin;
        //播放按钮
        public Button btnPlayMin;
        //暂停按钮
        public Button btnPauseMin;
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
        public Button btnStateAllloop;
        //播放状态按钮，单曲循环
        public Button btnStateOneloop;
        //播放状态按钮，顺序
        public Button btnStateOrder;
        //左切换按钮
        public Button btnLeftMax;
        //右切换按钮
        public Button btnRightMax;
        //播放按钮
        public Button btnPlayMax;
        //暂停按钮
        public Button btnPauseMax;
        //音量按钮
        public Button btnVolumeMax;
        //音乐播放的进度条
        public Slider slidMusicMax;
        //音量控制条
        public Slider slidVolumeMax;
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
        //音量对象
        public GameObject objVolum;

        /// <summary>
        /// 重置大UI的计时（大UI自动变小UI）
        /// </summary>
        void RestartTimeMaxToMin()
        {
            fMaxToMinTemp = 0;
            fMinToEffectTemp = 0;
        }

        void OnBtnVolume()
        {
            RestartTimeMaxToMin();

            if (audioSource.volume > 0.01f)
            {
                audioSource.volume = 0;
                slidVolumeMax.SetValueWithoutNotify(0);
            }
            else
            {
                audioSource.volume = 1;
                slidVolumeMax.SetValueWithoutNotify(1);
            }
        }
        void OnSliderVolume(float f)
        {
            RestartTimeMaxToMin();
            audioSource.volume = f;
        }

        public void EnterBtnVolum()
        {
            RestartTimeMaxToMin();
            bTouchBtnVolum = true;
            objVolum.SetActive(true);
        }
        public void ExitBtnVolum()
        {
            bTouchBtnVolum = false;
            HideVolum();
        }
        public void EnterObjVolum()
        {
            RestartTimeMaxToMin();
            bTouchObjVolum = true;
            objVolum.SetActive(true);
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
            objVolum.SetActive(false);
        }
        #endregion

        #region 音符特效
        public GameObject objEffect;
        public void EffectToMinUI()
        {
            StartCoroutine(IEFarToMiddle());
        }

        #endregion
    }
}