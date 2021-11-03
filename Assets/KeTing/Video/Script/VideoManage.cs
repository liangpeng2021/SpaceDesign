/* Create by zh at 2021-09-24

    视频，电视，AR总控制脚本
    PS:控制进度条，AR和电视同时播放
 */

using OXRTK.ARHandTracking;
using prometheus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpaceDesign.Video
{
    public enum VideoAnimType
    {
        Center,//【唯一】最中间居中的
        AllRight,//【唯一】全部（包括不显示的）：最右边的
        AlLeft,//【唯一】全部（包括不显示的）：最左边的
        ShowRight,//【唯一】显示：最右边的
        ShowLeft,//【唯一】显示：最左边的
        Other,//【不唯一】
    }

    public class VideoManage : MonoBehaviour
    {
        static VideoManage inst;
        public static VideoManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<VideoManage>();
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
        //对象初始位置
        [SerializeField]
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================

        //自动隐藏UI界面等待时间
        public float fAutoHideUITime = 30;
        //自动隐藏UI界面计时时间
        private float fAutoHideUITiming;
        void Awake()
        {
            fAutoHideUITiming = fAutoHideUITime;
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchUp.AddListener(ClickIcon);
            btnVideoClose.onPinchUp.AddListener(OnVideoClose);
            btnVideoSize.onPinchUp.AddListener(OnVideoSize);
            btnWatchNow.onPinchUp.AddListener(OnWatchNow);
            btnVideo2D.onPinchUp.AddListener(OnVideo2D);
            btnVideo3D.onPinchUp.AddListener(OnVideo3D);
            btnARWindows.onPointerEnter.AddListener(() =>
            {
                if (bSizeSmall == false)
                    SetExpand(true);
            });
            btnARWindows.onPinchUp.AddListener(() =>
            {
                if (bSizeSmall == true)
                    OnVideoSize();
            });
            btnPlay.onPinchUp.AddListener(() =>
            {
                if (bPause)
                    OnUnPause();
                else
                    OnPlay();
            });
            btnPause.onPinchUp.AddListener(OnPause);
            btnAR.onPinchUp.AddListener(OnAR);
            btnTV.onPinchUp.AddListener(OnTV);
            btnQuit.onPinchUp.AddListener(OnVideoClose);
            //sliderVideo.onInteractionStart.AddListener(SliderVideoPointerDown);
            //sliderVideo.onInteractionEnd.AddListener(SliderVideoPointerUp);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchUp.RemoveAllListeners();
            btnVideoClose.onPinchUp.RemoveAllListeners();
            btnVideoSize.onPinchUp.RemoveAllListeners();
            btnWatchNow.onPinchUp.RemoveAllListeners();
            btnARWindows.onPointerEnter.RemoveAllListeners();
            btnARWindows.onPinchUp.RemoveAllListeners();
            btnVideo2D.onPinchUp.RemoveAllListeners();
            btnVideo3D.onPinchUp.RemoveAllListeners();
            btnPlay.onPinchUp.RemoveAllListeners();
            btnPause.onPinchUp.RemoveAllListeners();
            btnAR.onPinchUp.RemoveAllListeners();
            btnTV.onPinchUp.RemoveAllListeners();
            btnQuit.onPinchUp.RemoveAllListeners();
            //sliderVideo.onInteractionStart.RemoveAllListeners();
            //sliderVideo.onInteractionEnd.RemoveAllListeners();
        }

        void Start()
        {
            tvCtr.OnInit();
            traVideoExpand.gameObject.SetActive(false);
        }

        /// <summary>
        /// 重置隐藏UI计时
        /// </summary>
        void ResetAutoHideUITime()
        {
            fAutoHideUITiming = fAutoHideUITime;
        }

        void Update()
        {
            if ((bTV == false) && (bExpand == true) && (bSizeSmall == false))
            {
                if (fAutoHideUITiming >= 0)
                {
                    fAutoHideUITiming -= Time.deltaTime;
                    if (fAutoHideUITiming <= 0)
                    {
                        ResetAutoHideUITime();
                        SetExpand(false);
                    }
                }
            }

            if (bSlideDragging == false)
            {
                if (vdp2D.isPlaying || vdp3D.isPlaying || tvCtr.bPlaying)
                {
                    //print("未拖拽，播放中");

                    SetTextCurPlayTime(true);

                    if (sliderVideo.sliderValue >= 0.997f)
                    {
                        print("播放完成，结束:" + sliderVideo.sliderValue);
                        //OnPause();
                        OnPause();
                        if (b2D)
                        {
                            b2D = false;
                            OnVideo2D();
                        }
                        else
                        {
                            b2D = true;
                            OnVideo3D();
                        }
                    }
                }
            }
            else
            {
                //print("拖拽中");
                SetTextCurPlayTime(false);
            }
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            //if (bUIChanging == true)
            //    return;

            Vector3 _v3 = traIcon.position;
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
            else if (_dis <= 5f && _dis > 3f)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            else if (_dis <= 3f)
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

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

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
                    yield return IECloseToMiddle();
                else if (curPlayerPosState == PlayerPosState.Far)/// 近距离=>远距离
                {
                    yield return IECloseToMiddle();
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
            if (bTV == false)
                yield break;

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
            if (bTV == false)
                yield break;

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
            if (bTV == false)
                yield break;

            //UI开始变化
            bUIChanging = true;

            //中距离=>近距离

            if (bExpand == false)
                SetReminder(true);

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }
            //UI变化结束
            bUIChanging = false;

            yield return 0;
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            if (bTV == false)
                yield break;

            //UI开始变化
            bUIChanging = true;

            //如果是AR模式，不关闭扩展界面
            if (bReminder == true)
                SetReminder(false);
            if (bTV)
            {
                if (bReminder == true)
                    SetExpand(false);
            }

            ////近距离=>中距离
            //animAnswer.enabled = false;

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fUISpeed * 2f * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;

            yield return 0;
        }

        #region Icon变化，远距离
        [Header("===Icon变化，远距离")]
        //Icon的对象
        public Transform traIcon;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Close)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
            }
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;

        //立即观看
        public ButtonRayReceiver btnWatchNow;

        //预览显示Timeline
        public GameObject timelineReminderShow;
        //预览隐藏Timeline
        public GameObject timelineReminderHide;

        //大界面显示Timeline
        public GameObject timelineExpandShow;
        //大界面隐藏Timeline
        public GameObject timelineExpandHide;

        //控制界面（播放、暂停、进度条、片源选择等父节点）
        public Transform traVideoExpand;
        //2D视频的对象按钮
        public ButtonRayReceiver btnVideo2D;
        //3D视频的对象按钮
        public ButtonRayReceiver btnVideo3D;
        //2D和3D视频的父节点的按钮（碰撞显示UI，最小化点击恢复大小）
        public ButtonRayReceiver btnARWindows;
        ////视频选择对象
        public Transform traVideoChoose;
        //视频的详情图片
        public Image imgDetailPic;

        //当前播放时长Text
        public Text textCurPlayTime;
        //总播放时长Text
        public Text textTotalPlayTime;
        //播放进度条
        public PinchSlider sliderVideo;
        //进度条手动拖拽中
        public bool bSlideDragging;
        //按钮：播放
        public ButtonRayReceiver btnPlay;
        //按钮：暂停
        public ButtonRayReceiver btnPause;

        //按钮：退出视频
        public ButtonRayReceiver btnVideoClose;
        //按钮：放大或缩小视频
        public ButtonRayReceiver btnVideoSize;

        //按钮：TV转AR模式
        public ButtonRayReceiver btnAR;
        //按钮：AR转TV模式
        public ButtonRayReceiver btnTV;
        //按钮：关闭电视
        public ButtonRayReceiver btnQuit;

        public Transform traTVParent;

        //跟随父节点
        public GameObject objFollow;
        //跟随普通界面（大界面）
        public Transform traFollowNormalParent;
        //跟随小界面
        public Transform traFollowSmallParent;

        //视频的类型（2-2D；3-3D）
        public bool b2D = true;
        //播放类型（是否电视TV端播放）
        public bool bTV = true;

        //AR状态下，video显示对象父节点
        public Transform traARVideoWindows;

        //3D视频的2D画面控制器
        public TVCtr tvCtr;
        //3D视频播放控制脚本
        public MeshPlayerPRM vdp3D;
        //3D视频播放的音频
        public AudioSource ads3D;
        //3D视频的图片
        public Sprite spr3DVideo;
        //3D视频的帧速率（每秒多少帧）
        public float f3DVideoRate = 15;
        //拖拽进度条等，改变了Frame
        public bool bChange3DFrame;
        //当前播放对象的总时长
        public float fTotalPlayTime = 0.1f;

        //2D视频播放
        public VideoPlayer vdp2D;
        //2D视频的图片
        public Sprite spr2DVideo;
        //播放中
        public bool bPlaying;
        //预览界面
        public bool bReminder;
        //大界面
        public bool bExpand;

        //小尺寸
        public bool bSizeSmall = false;
        /// <summary>
        /// 设置预览UI
        /// </summary>
        void SetReminder(bool bOpen)
        {
            bReminder = bOpen;
            bExpand = false;
            timelineExpandShow.SetActive(false);
            timelineExpandHide.SetActive(false);
            timelineReminderShow.SetActive(bOpen);
            timelineReminderHide.SetActive(!bOpen);
        }
        /// <summary>
        /// 设置大的扩展界面
        /// </summary>
        public void SetExpand(bool bOpen)
        {
            bExpand = bOpen;
            //开始默认隐藏该对象，所以这里需要打开
            if (traVideoExpand.gameObject.activeSelf == false)
                traVideoExpand.gameObject.SetActive(true);

            bReminder = false;
            timelineReminderShow.SetActive(false);
            timelineReminderHide.SetActive(false);

            timelineExpandShow.SetActive(bOpen);
            timelineExpandHide.SetActive(!bOpen);
        }
        /// <summary>
        /// 设置是否显示2D的播放框
        /// </summary>
        void SetVideoWindow(bool bShow)
        {
            vdp2D.SetDirectAudioMute(0, !bShow);
            ads3D.mute = !bShow;
            traARVideoWindows.localScale = bShow ? new Vector3(0.8f, 0.8f, 0.8f) : Vector3.zero;
        }
        /// <summary>
        /// 关闭视频
        /// </summary>
        void OnVideoClose()
        {
            OnStop();

            if (bTV)
                tvCtr.OnClose();
            else
                OnTV();

            traVideoExpand.gameObject.SetActive(false);

            if (bReminder)
                SetReminder(false);
            if (bExpand)
                SetExpand(false);
        }

        /// <summary>
        /// 设置尺寸
        /// </summary>
        void OnVideoSize()
        {
            ResetAutoHideUITime();

            StopCoroutine("IEVideoSize");
            StartCoroutine("IEVideoSize");

        }
        IEnumerator IEVideoSize()
        {
            //加一个临时bool变量，普通状态缩小的时候，直接先赋值为小状态（防止射线碰撞，出现两边详情界面）
            bool bTempSizeSmall = bSizeSmall;
            if (bTempSizeSmall == false)
            {
                //普通状态缩小的时候，直接先赋值为小状态（防止射线碰撞，出现两边详情界面）
                bSizeSmall = true;

                btnPause.onPinchUp.Invoke();
                //Normal -> Small  先隐藏扩展界面，再缩小
                if (bExpand == true)
                {
                    SetExpand(false);
                    yield return new WaitForSeconds(0.5f);
                }

                traVideoExpand.SetParent(traFollowSmallParent);
            }
            else
            {
                btnPlay.onPinchUp.Invoke();
                traVideoExpand.SetParent(traFollowNormalParent);
            }

            while (true)
            {
                traVideoExpand.localScale = Vector3.Lerp(traVideoExpand.localScale, Vector3.one, fUISpeed * 2 * Time.deltaTime);
                traVideoExpand.localPosition = Vector3.Lerp(traVideoExpand.localPosition, Vector3.zero, fUISpeed * Time.deltaTime);
                traVideoExpand.localRotation = Quaternion.Lerp(traVideoExpand.localRotation, Quaternion.identity, fUISpeed * 2 * Time.deltaTime);
                float _fDis = Vector3.Distance(traVideoExpand.localPosition, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traVideoExpand.localScale = Vector3.one;
                    traVideoExpand.localPosition = Vector3.zero;
                    traVideoExpand.localRotation = Quaternion.identity;
                    break;
                }
                yield return 0;
            }

            if (bTempSizeSmall == true)
            {
                //Small  -> Normal 先恢复扩展界面，再放大
                if (timelineExpandHide.gameObject.activeSelf == true)
                {
                    SetExpand(true);
                    yield return new WaitForSeconds(1.5f);
                }
            }

            bSizeSmall = !bTempSizeSmall;

            traVideoExpand.localScale = Vector3.one;
            traVideoExpand.localPosition = Vector3.zero;
            traVideoExpand.localRotation = Quaternion.identity;

            yield return 0;
        }

        /// <summary>
        /// 预览界面，立刻观看按钮响应
        /// </summary>
        void OnWatchNow()
        {
            SetReminder(false);
            Invoke("_InvokeShowExpand", 1.5f);
        }
        void _InvokeShowExpand()
        {
            SetExpand(true);

            //先设置是否TV
            bTV = false;
            OnTV();
            //再设置是否2D视频
            b2D = false;
            OnVideo2D();
        }

        /// <summary>
        /// TV模式下，点击AR按钮响应
        /// </summary>
        void OnAR()
        {
            //先开启目标对象，再移动子节点
            objFollow.SetActive(true);

            btnQuit.gameObject.SetActive(false);

            //===========================================================================

            Debug.LogError("AR和视频切换，播到一半的时候切换有问题");

            //TV->AR：瞬间切换，TV黑屏（推送一个黑色图片）
            //AR->TV:先推送（从零开始播放），AR端会同步恢复0的位置，切换前记录进度？切换后，播放后，再跳转？

            tvCtr.OnClose();

            //===========================================================================

            SetVideoWindow(true);

            bTV = false;
            traFollowNormalParent.gameObject.SetActive(true);

            traVideoExpand.gameObject.SetActive(true);
            traVideoExpand.SetParent(traFollowNormalParent);
            traVideoExpand.SetAsFirstSibling();
            traVideoExpand.localEulerAngles = traVideoExpand.localPosition = Vector3.zero;
            traVideoExpand.localScale = Vector3.one;

            btnVideoClose.gameObject.SetActive(true);
            btnVideoSize.gameObject.SetActive(true);

            btnAR.gameObject.SetActive(false);
            btnTV.gameObject.SetActive(true);
        }

        /// <summary>
        /// AR模式下，点击TV按钮响应
        /// </summary>
        void OnTV()
        {
            SetVideoWindow(false);
            btnQuit.gameObject.SetActive(true);

            bTV = true;

            //AR切换TV，直接重播
            if (b2D)
            {
                b2D = false;
                OnVideo2D();
            }
            else
            {
                b2D = true;
                OnVideo3D();
            }

            traVideoExpand.gameObject.SetActive(true);
            traVideoExpand.SetParent(traTVParent);
            traVideoExpand.SetAsFirstSibling();
            traVideoExpand.localEulerAngles = traVideoExpand.localPosition = Vector3.zero;
            traVideoExpand.localScale = Vector3.one;

            btnVideoClose.gameObject.SetActive(true);
            btnVideoSize.gameObject.SetActive(false);

            btnTV.gameObject.SetActive(false);
            btnAR.GetComponentInChildren<TextMesh>().text = b2D ? "AR模式" : "3D全息模式";
            btnAR.gameObject.SetActive(true);
            objFollow.SetActive(false);
        }

        /// <summary>
        /// 2D视频按钮响应
        /// </summary>
        void OnVideo2D()
        {
            if (b2D == true)
                return;

            ResetAutoHideUITime();

            traVideoChoose.SetParent(btnVideo2D.transform);
            traVideoChoose.localPosition = Vector3.zero;

            //先暂停3D的，再清除3D状态
            //OnStop();
            b2D = true;

            imgDetailPic.sprite = spr2DVideo;
            vdp3D.gameObject.SetActive(false);
            vdp2D.gameObject.SetActive(true);
            sliderVideo.sliderValue = 0;
            SetTextCurPlayTime(true);
            fTotalPlayTime = (float)(vdp2D.length);
            SetTextTotalPlayTime();

            if (bTV)
            {
                tvCtr.OnPush(true);
                btnAR.GetComponentInChildren<TextMesh>().text = b2D ? "AR模式" : "3D全息模式";
                btnAR.gameObject.SetActive(true);
            }
            else
            {
                OnPlay();
            }
#if UNITY_EDITOR
            OnPlay();
#endif
        }

        /// <summary>
        /// 3D视频按钮响应
        /// </summary>
        void OnVideo3D()
        {
            if (b2D == false)
                return;

            ResetAutoHideUITime();

            traVideoChoose.SetParent(btnVideo3D.transform);
            traVideoChoose.localPosition = Vector3.zero;
            //先暂停2D的，再清除2D状态
            //OnStop();
            b2D = false;

            imgDetailPic.sprite = spr3DVideo;
            vdp2D.gameObject.SetActive(false);
            vdp3D.gameObject.SetActive(true);
            //先打开资源
            vdp3D.OpenSource(vdp3D.sourceUrl, 0, false);
            //再设置总长度
            fTotalPlayTime = (vdp3D.sourceFrameCount - 1);

            sliderVideo.sliderValue = 0;
            SetTextTotalPlayTime();
            SetTextCurPlayTime(true);

            if (bTV)
            {
                tvCtr.OnPush(false);
                btnAR.GetComponentInChildren<TextMesh>().text = b2D ? "AR模式" : "3D全息模式";
                btnAR.gameObject.SetActive(true);
            }
            else
            {
                OnPlay();
            }
#if UNITY_EDITOR
            OnPlay();
#endif
        }

        //恢复播放（取消暂停）【判断TV，然后播放AR】
        public void OnUnPause()
        {
            if (bTV)
            {
                tvCtr.OnResume();
            }
            PlayAR();
        }

        /// <summary>
        /// TV：播放（重新开始播放）【判断TV，然后播放AR】
        /// </summary>
        public void OnPlay()
        {
            if (bTV)
            {
                tvCtr.OnPlay(b2D);
            }
            PlayAR();
        }

        /// <summary>
        /// 仅播放AR内容
        /// </summary>
        public void PlayAR()
        {
            ResetAutoHideUITime();

            bPlaying = true;
            btnPlay.gameObject.SetActive(false);
            btnPause.gameObject.SetActive(true);

            if (b2D)
            {
                vdp2D.Play();
            }
            else
            {
                float _fCur = (sliderVideo.sliderValue * fTotalPlayTime);
                //ads3D.time = sliderVideo.value / f3DVideoRate;
                ads3D.time = _fCur;//_fCur / f3DVideoRate;
                ads3D.Play();
                //print($"ads3D.time:{ads3D.time}-----_fCur:{_fCur}");

                if (bChange3DFrame)
                {
                    bChange3DFrame = false;
                    //int _iCurFrame = (int)sliderVideo.value;
                    int _iCurFrame = (int)_fCur;
                    vdp3D.PreparePreviewFrame(_iCurFrame);
                    vdp3D.OpenSource(vdp3D.sourceUrl, _iCurFrame, true);
                }
                else
                {
                    vdp3D.Play();
                }
            }
            bPause = false;
        }

        //是否暂停过
        public bool bPause = false;
        /// <summary>
        /// 暂停
        /// </summary>
        public void OnPause()
        {
            ResetAutoHideUITime();

            bPause = true;

            Debug.Log("暂停");

            bPlaying = false;
            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);

            if (bTV)
            {
                tvCtr.OnPause();
            }
            if (b2D)
            {
                vdp2D.Pause();
            }
            else
            {
                ads3D.Pause();
                vdp3D.Pause();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void OnStop()
        {
            ResetAutoHideUITime();
            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);

            bPlaying = false;
            bPause = false;
            if (bTV)
            {
                tvCtr.OnStopPlay();
            }
            if (b2D)
            {
                vdp2D.Stop();
            }
            else
            {
                ads3D.Stop();
                vdp3D.Stop();
            }
        }

        //设置播放进度
        public void SetSliderValue(bool bSetTV)
        {
            ResetAutoHideUITime();
            float _fCur = (sliderVideo.sliderValue * fTotalPlayTime);
            if (bTV)
            {
                //tvCtr.OnSetSlider((int)(sliderVideo.maxValue), (int)(sliderVideo.value));
                if (bSetTV)
                    tvCtr.OnSetSlider((int)(fTotalPlayTime), (int)_fCur);
            }
            if (b2D)
            {
                //vdp2D.time = sliderVideo.value;
                vdp2D.time = _fCur;
            }
            else
            {
                bChange3DFrame = true;
                //print("设置进度:" + sliderVideo.value);
            }

            OnUnPause();
        }

        /// <summary>
        /// 当前播放时间
        /// </summary>
        void SetTextCurPlayTime(bool bSetSlider)
        {
            float fCurTime = 0;
            if (b2D)
                fCurTime = (float)(vdp2D.time);//2D用.time，获取的是当前播放秒数
            else
                fCurTime = vdp3D.videoPlayer.frame;//3D用.frame获取的是当前帧数，值大于1

            //print($"vp3D.frame:{vp3D.frame};vp3D.time:{vp3D.time}");
            //print($"fCurTime:{fCurTime};bSetSlider:{bSetSlider}");

            //3D视频需要除以帧速率
            if (b2D == false)
            {
                f3DVideoRate = (f3DVideoRate <= 0) ? 1 : f3DVideoRate;
                fCurTime /= f3DVideoRate;
                //print("计算帧率");
            }

            if (bSetSlider)
            {
                //sliderVideo.SetValueWithoutNotify(fCurTime);
                sliderVideo.sliderValue = fCurTime / fTotalPlayTime;
            }

            float s = (fCurTime % 60);
            float m = (((fCurTime - s) / 60) % 60);
            float h = ((fCurTime - s) / 3600);
            textCurPlayTime.text = $"{((int)h).ToString("D2")}:{((int)m).ToString("D2")}:{((int)s).ToString("D2")}";
        }

        /// <summary>
        /// 设置总播放时间
        /// </summary>
        /// <param name="fTotalTime"></param>
        void SetTextTotalPlayTime()
        {
            //3D视频需要除以帧速率
            if (b2D == false)
            {
                f3DVideoRate = (f3DVideoRate <= 0) ? 1 : f3DVideoRate;
                fTotalPlayTime /= f3DVideoRate;
            }

            float s = (fTotalPlayTime % 60);
            float m = (((fTotalPlayTime - s) / 60) % 60);
            float h = ((fTotalPlayTime - s) / 3600);
            textTotalPlayTime.text = $"{((int)h).ToString("D2")}:{((int)m).ToString("D2")}:{((int)s).ToString("D2")}";
        }

        /// <summary>
        /// 进度条按下
        /// </summary>
        void SliderVideoPointerDown()
        {
            bSlideDragging = true;
            OnPause();
        }

        /// <summary>
        /// 进度条抬起
        /// </summary>
        void SliderVideoPointerUp()
        {
            bSlideDragging = false;
            SetSliderValue(true);
        }
        #endregion

    }
}