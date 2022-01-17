/* Create by zh at 2021-09-24

    视频，电视，AR总控制脚本
    PS:控制进度条，AR和电视同时播放

    * 2D视频，VideoPlayer用Frame计算（换算秒数，要除以rate），最后一帧不播放
    
    * 3D声音，用秒数计算
    * 3D视频，MeshPlayerPRM用Frame计算（换算秒数，要除以rate），最后一帧不播放
    
    * TV电视，传的是秒数（Int）

 */

using OXRTK.ARHandTracking;
using prometheus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpaceDesign
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
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        [SerializeField]
        private Vector3 v3OriPos;
        //3D视频的名称（"Video3D.mp4"）
        [SerializeField]
        private string str3DVideoPth = "Video3D.mp4";
        //3D视频的加载是否成功
        public bool b3DOK;
        //开启了电视或AR模式（进入根据距离判断，退出只通过按钮控制）
        public bool bOpenVideo = false;
        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================

        //自动隐藏UI界面等待时间
        public float fAutoHideUITime = 30;
        //自动隐藏UI界面计时时间
        public float fAutoHideUITiming;
        void Awake()
        {
            fAutoHideUITiming = fAutoHideUITime;
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnVideoClose.onPinchDown.AddListener(OnVideoClose);
            btnVideoSize.onPinchDown.AddListener(OnVideoSize);
            btnVideoNotMove.onPinchDown.AddListener(OnVideoNotMove);
            btnVideoMove.onPinchDown.AddListener(OnVideoMove);

            btnWatchNow.onPinchDown.AddListener(OnWatchNow);
            btnVideo2D.onPinchDown.AddListener(() => { SetVideo(true, false); });
            btnVideo3D.onPinchDown.AddListener(() => { SetVideo(false, false); });

            btnARWindows.onPointerEnter.AddListener(() =>
            {
                if (bKarting == true || bSizeSmall == false)
                    SetExpand(true);
            });
            btnARWindows.onPinchDown.AddListener(() =>
            {
                if (bKarting == false && bSizeSmall == true)
                    OnVideoSize();
            });
            btnPlay.onPinchDown.AddListener(() =>
            {
                //if (bPause)
                if (iPlayState == 1)
                    OnUnPause();
                else
                    OnPlay();
            });
            btnPause.onPinchDown.AddListener(OnPause);
            btnAR.onPinchDown.AddListener(() => { SetTV(false, false, false); });
            btnTV.onPinchDown.AddListener(() => { SetTV(true, true, false); });
            btnQuit.onPinchDown.AddListener(OnVideoClose);
            sliderVideo.onInteractionStart.AddListener(SliderVideoPointerDown);
            sliderVideo.onInteractionEnd.AddListener(() => { SliderVideoPointerUp(true); });
            timelineExpandHide.SetActive(false);
            timelineExpandShow.SetActive(false);
            timelineReminderHide.SetActive(false);
            timelineReminderShow.SetActive(false);
            HideLoadingUI();
        }

        void OnDisable()
        {
            //OnPause();

            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnVideoClose.onPinchDown.RemoveAllListeners();
            btnVideoSize.onPinchDown.RemoveAllListeners();
            btnVideoNotMove.onPinchDown.RemoveAllListeners();
            btnVideoMove.onPinchDown.RemoveAllListeners();

            btnWatchNow.onPinchDown.RemoveAllListeners();
            btnARWindows.onPointerEnter.RemoveAllListeners();
            btnARWindows.onPinchDown.RemoveAllListeners();
            btnVideo2D.onPinchDown.RemoveAllListeners();
            btnVideo3D.onPinchDown.RemoveAllListeners();
            btnPlay.onPinchDown.RemoveAllListeners();
            btnPause.onPinchDown.RemoveAllListeners();
            btnAR.onPinchDown.RemoveAllListeners();
            btnTV.onPinchDown.RemoveAllListeners();
            btnQuit.onPinchDown.RemoveAllListeners();
            sliderVideo.onInteractionStart.RemoveAllListeners();
            sliderVideo.onInteractionEnd.RemoveAllListeners();
        }
        private void OnDestroy()
        {
            OnStop();
        }
        void Start()
        {
            HideLoadingUI();

            tvCtr.OnInit();
            //fTotalTime2D = (float)vdp2D.length;
            //视频播放不播放最后一帧
            fTotalFrame2D = (float)(vdp2D.frameCount - 1);
            obj3DStage.SetActive(false);
            //关闭视频上的碰撞盒，防止触碰显示宽展界面
            colliderBtnARWindows.enabled = false;
            objWindowsBtnParent.SetActive(false);
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
            //视频未打开，后面不运行
            if (bOpenVideo == false)
                return;

            //3D模式下，3D还没有准备成功，后面不运行
            //if (b2D == false && b3DOK == false)
            //    return;

            //隐藏的判断：AR模式 + 扩展模式 + （卡丁车模式、或非小框模式）
            if ((bTV == false) && (bExpand == true) && ((bSizeSmall == false) || (bKarting == true)))
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
                //if (bPlaying)
                if (iPlayState == 0)
                {
                    //print("未拖拽，播放中");
                    SetSliderVideo(-1);
                    SetTextCurPlayTime(false);
                }

                ////电视的播放完成在回调
                ////if (bTV == false)
                //{
                //    //if (sliderVideo.sliderValue >= 0.997f)
                //    if (sliderVideo.sliderValue >= 1f)
                //    {
                //        OnStop();
                //    }
                //}
            }
            else
            {
                //print("拖拽中");
                SetTextCurPlayTime(true);
            }
        }

        Vector3 v3AutoSmallLastPos;
        int iAutoSmallTimes;
        bool bInitAutoSmall;
        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            if (bInitAutoSmall == false)
            {
                bInitAutoSmall = true;
                v3AutoSmallLastPos = pos;
            }
            else
            {
                //AR状态下，移动跟随窗口状态，一秒内，距离大于1米，自动到小窗模式
                if ((bTV == false) && (videoAutoRotate.bMove == true))
                {
                    if (bSizeSmall == false)
                    {
                        iAutoSmallTimes += 1;
                        //从PlayerManage每0.5秒刷新一次距离，够2次（1秒）判断一次距离
                        if (iAutoSmallTimes > 2)
                        {
                            iAutoSmallTimes = 0;
                            if (Vector3.Distance(v3AutoSmallLastPos, pos) > 1f)
                            {
                                //1秒走路超过1米，自动缩小
                                OnVideoSize();
                                v3AutoSmallLastPos = pos;
                            }
                            else
                            {
                                iAutoSmallTimes = 0;
                                v3AutoSmallLastPos = pos;
                            }
                        }
                    }
                    else
                    {
                        iAutoSmallTimes = 0;
                        v3AutoSmallLastPos = pos;
                    }
                }
                else
                {
                    iAutoSmallTimes = 0;
                    v3AutoSmallLastPos = pos;
                }

            }

            //if (bUIChanging == true)
            //    return;

            Vector3 _v3 = traIcon.position;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"目标的距离:{_dis}");

            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            float _fFar = LoadPrefab.IconDisData.VideoFar;
            float _fMid = LoadPrefab.IconDisData.VideoMiddle;

            if (_dis > _fFar)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else if (_dis <= _fFar && _dis > _fMid)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            else if (_dis <= _fMid)
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
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (bOpenVideo)
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
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (bOpenVideo)
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
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (bOpenVideo)
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
        IEnumerator IECloseToMiddle(bool bSetTV)
        {
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (bOpenVideo)
                yield break;

            //UI开始变化
            bUIChanging = true;

            objWindowsBtnParent.SetActive(false);

            if (bReminder == true)
                SetReminder(false);
            else if (bExpand == true)
                SetExpand(false);

            Vector3 _v3Icon = LoadPrefab.IconSize;
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, _v3Icon, fUISpeed * 2f * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, _v3Icon);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = _v3Icon;
                    break;
                }
                yield return 0;
            }

            if (bSetTV)
            {
                yield return new WaitForSeconds(0.5f);
                bTV = bTVTemp;

                if (bTV)
                    tvCtr.OnClose();
                else
                {
                    SetTV(true, false, false);
                }
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
        //碰撞盒，2D和3D视频的父节点
        public BoxCollider colliderBtnARWindows;
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

        //窗口旁边按钮的父节点
        public GameObject objWindowsBtnParent;
        //按钮：退出视频
        public ButtonRayReceiver btnVideoClose;
        //按钮：放大或缩小视频
        public ButtonRayReceiver btnVideoSize;
        //按钮：固定位置
        public ButtonRayReceiver btnVideoNotMove;
        //按钮：跟随移动
        public ButtonRayReceiver btnVideoMove;


        //按钮：TV转AR模式
        public ButtonRayReceiver btnAR;
        //按钮：AR转TV模式
        public ButtonRayReceiver btnTV;
        //按钮：关闭电视
        public ButtonRayReceiver btnQuit;

        public Transform traTVParent;

        //跟随父节点（自动旋转）
        public VideoAutoRotate videoAutoRotate;
        //跟随普通界面（大界面）
        public Transform traFollowNormalParent;
        //跟随小界面
        public Transform traFollowSmallParent;

        //视频的类型（2-2D；3-3D）
        public bool b2D = true;
        //播放类型（是否电视TV端播放）
        public bool bTV = true;

        //3D视频的2D画面控制器
        public TVCtr tvCtr;
        //3D视频播放控制脚本（显隐必须在Pause之后）
        public MeshPlayerPRM vdp3D;
        //3D视频Stop过
        public bool b3DStop = true;
        //3D视频的MeshRender
        public MeshRenderer mrVdp3D;
        //3D视频的舞台模型和特效
        public GameObject obj3DStage;
        //3D特效的人物动画
        public Animator animator3DChar;
        //3D特效
        public ParticleSystem particle3D;
        //3D视频是否开启过
        private bool b3DMeshOpen = false;
        //3D视频播放的音频
        public AudioSource ads3D;
        //3D视频的图片
        public Sprite spr3DVideo;
        //当前播放对象的总时长（这里是秒）
        public float fTotalPlayTime = 0.1f;
        //2D总长（这里是帧数，不是秒数，秒数乘以帧率）VideoPlayer的总帧数还要减1，因为最后一帧不播放
        public float fTotalFrame2D;
        //3D总长（这里是帧数，不是秒数，秒数乘以帧率）容积视频的总帧数还要减1，因为最后一帧不播放
        float fTotalFrame3D = 2613;
        //3D的音乐总长，（这里是秒数，跟容积视频的长度是不同的）
        float fTotalTime3DMusic = 115.271f;

        //2D视频播放
        public VideoPlayer vdp2D;
        //2D视频的图片
        public Sprite spr2DVideo;
        /// <summary>
        /// 播放的状态【0：Play】【1：Pause】【2：Stop】
        /// </summary>
        [SerializeField]
        private int iPlayState = 2;
        //预览界面
        public bool bReminder;
        //大界面
        public bool bExpand;

        //小尺寸
        public bool bSizeSmall = false;
        //卡丁车模式（绑定在卡丁车界面）
        public bool bKarting = false;
        //加载中等待的UI对象
        public GameObject objLoadingUI;
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
            if (bExpand == true)
            {
                //打开视频上的碰撞盒
                colliderBtnARWindows.enabled = true;
            }

            bReminder = false;
            timelineReminderShow.SetActive(false);
            timelineReminderHide.SetActive(false);

            timelineExpandShow.SetActive(bOpen);
            timelineExpandHide.SetActive(!bOpen);

            objWindowsBtnParent.SetActive(bOpen);
        }
        /// <summary>
        /// 设置是否显示2D的播放框
        /// </summary>
        void SetVideoWindow(bool bShow)
        {
            vdp2D.SetDirectAudioMute(0, !bShow);
            ads3D.mute = !bShow;
            btnARWindows.transform.localScale = bShow ? new Vector3(0.8f, 0.8f, 0.8f) : Vector3.zero;
        }
        bool bTVTemp;
        /// <summary>
        /// 关闭视频
        /// </summary>
        void OnVideoClose()
        {
            bOpenVideo = false;

            OnStop();
            //视频框直接隐藏先（缩放为0）
            btnARWindows.transform.localScale = Vector3.zero;
            //先把TV模式设置成true，否则IECloseToMiddle不进行
            bTVTemp = bTV;
            bTV = true;
            StopCoroutine("IECloseToMiddle");
            StartCoroutine("IECloseToMiddle", true);
            //bOpenVideo = false;
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

                btnPause.onPinchDown.Invoke();
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
                btnPlay.onPinchDown.Invoke();
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
        /// 视频，固定按钮响应
        /// </summary>
        void OnVideoNotMove()
        {
            btnVideoNotMove.gameObject.SetActive(false);
            btnVideoMove.gameObject.SetActive(true);
            videoAutoRotate.bMove = true;
        }

        /// <summary>
        /// 视频，跟随按钮响应
        /// </summary>
        void OnVideoMove()
        {
            btnVideoMove.gameObject.SetActive(false);
            btnVideoNotMove.gameObject.SetActive(true);
            videoAutoRotate.bMove = false;
        }

        /// <summary>
        /// 预览界面，立刻观看按钮响应
        /// </summary>
        void OnWatchNow()
        {
            bOpenVideo = true;

            b3DOK = false;

            SetReminder(false);

            WatchNowOpen3D();

            Invoke("_InvokeWatchNow", 1.3f);
        }

        void WatchNowOpen3D()
        {
            //先显示Render
            mrVdp3D.enabled = false;
            //===========================================================================
            if (b3DStop)
            {
                //print("==============mesh为空");
                vdp3D.OpenSourceAsync(str3DVideoPth, true, 0, () =>
                {
                    b3DStop = false;
                    Debug.Log("MyLog:WatchNow_3DVideo_Open_OK");
                    if (vdp3D.isPlaying)
                        vdp3D.Pause();
                    b3DOK = true;
                });
                //vdp3D.OpenSource(str3DVideoPth);
            }
            else
                //===========================================================================
                vdp3D.JumpFrame(0, true);

            if (ads3D.isPlaying == true)
                ads3D.Pause();
        }

        void _InvokeWatchNow()
        {
            if (vdp3D.isPlaying)
                vdp3D.Pause();
            vdp2D.targetTexture.Release();
            SetExpand(true);
            //先设置是否TV
            bTV = false;
            SetTV(true, true, true);
            //再设置是否2D视频
            b2D = true;//SetVideo函数中判断b2D是否与传进的数值相同，下面传的是false，所以这里先true一下，让函数可以运行
            SetVideo(false, true);
        }

        /// <summary>
        /// 设置显示模式（TV/AR）
        /// </summary>
        void SetTV(bool btv, bool bPush, bool bFromBtnWatchNow)
        {
            bSlideDragging = false;
            ResetAutoHideUITime();

            if (bTV == btv)
                return;

            bTV = btv;
            if (bTV == false)
            {
                SetExpand(false);

                //非TV模式，隐藏加载中UI提示
                HideLoadingUI();
            }
            else
            {
                //TV模式，先显示加载中UI提示
                objLoadingUI.SetActive(true);

                //print("TV模式，先显示加载中UI提示");
            }

            btnQuit.gameObject.SetActive(bTV);
            obj3DStage.SetActive(!bTV);

            if (bFromBtnWatchNow)
            {
                bFromBtnWatchNow = false;
            }
            else
            {
                if (bTV == true && bPush == true)
                {
                    bSlideDragging = true;

                    OnPause();
                    tvCtr.OnPush(b2D, false, true);
                }
                else
                {
                    tvCtr.OnClose();
                }
            }
            //===========================================================================
            //Debug.LogError("AR和视频切换，播到一半的时候切换有问题");
            //TV->AR：瞬间切换，TV黑屏（推送一个黑色图片）
            //AR->TV:先推送（从零开始播放），AR端会同步恢复0的位置，切换前记录进度？切换后，播放后，再跳转？
            //===========================================================================

            //if (bTV == false)
            //    tvCtr.OnClose();
            SetVideoWindow(!bTV);

            traVideoExpand.SetParent(bTV ? traTVParent : traFollowNormalParent);
            traVideoExpand.SetAsFirstSibling();
            traVideoExpand.localEulerAngles = traVideoExpand.localPosition = Vector3.zero;
            traVideoExpand.localScale = Vector3.one;

            //bool _bShowVideoCtrBtn = false;
            //if (bSizeSmall == true)
            //    _bShowVideoCtrBtn = false;
            //else
            //    _bShowVideoCtrBtn = !bTV;

            //objWindowsBtnParent.SetActive(_bShowVideoCtrBtn);
            ////btnVideoClose.gameObject.SetActive(_bShowVideoCtrBtn);
            ////btnVideoSize.gameObject.SetActive(_bShowVideoCtrBtn);

            btnAR.gameObject.SetActive(bTV);
            btnTV.gameObject.SetActive(!bTV);

            if (bTV == false)
                btnAR.GetComponentInChildren<TextMesh>().text = b2D ? "AR模式" : "3D全息模式";

            videoAutoRotate.enabled = !bTV;
        }

        void SetVideo(bool b2d, bool bFromBtnWatchNow)
        {
            bSlideDragging = false;
            ResetAutoHideUITime();

            if (b2D == b2d)
                return;

            if (bTV)
            {
                //TV模式，先显示加载中UI提示
                objLoadingUI.SetActive(true);
            }

            //这里主要为了停止AR的控制，不用给TV停止（即不用多发送一次黑屏图片）
            //如果从立即观看按钮进来，也不停止
            if (bFromBtnWatchNow == false)
                OnStop(false);

            b2D = b2d;
            if (b2D == false && bFromBtnWatchNow == false)
                b3DOK = false;

            //暂停完，等半秒再打开新的，防止容积视频崩溃
            Invoke("_DelaySetVideo", 0.1f);
        }

        void _DelaySetVideo()
        {
            traVideoChoose.SetParent(b2D ? btnVideo2D.transform : btnVideo3D.transform);
            traVideoChoose.localPosition = Vector3.zero;

            imgDetailPic.sprite = b2D ? spr2DVideo : spr3DVideo;
            vdp2D.gameObject.SetActive(b2D);

            //2D视频情况下，隐藏3D视频，隐藏前前确保他是暂停的（停止的）
            if (b2D == true)
            {
                //===========================================================================
                if (vdp3D.isPlaying == true)
                    vdp3D.Stop();
                //vdp3D.Pause();
                //===========================================================================
            }
            vdp3D.gameObject.SetActive(!b2D);
            //这里的3D视频模型保持隐藏，播放之后再显示
            mrVdp3D.enabled = false;
            obj3DStage.SetActive(!b2D);

            ads3D.gameObject.SetActive(!b2D);

            SetTotalPlayTime();

            SetSliderVideo(0);
            SetTextCurPlayTime(true);

            btnAR.gameObject.SetActive(bTV);
            btnTV.gameObject.SetActive(!bTV);
            if (bTV)
            {
                tvCtr.OnPush(b2D, true, false);
                btnAR.GetComponentInChildren<TextMesh>().text = b2D ? "AR模式" : "3D全息模式";
                //btnAR.gameObject.SetActive(true);
#if UNITY_EDITOR
                //OnPlay();
#endif
            }
            else
            {
                OnPlay();
            }
        }

        //恢复播放（取消暂停）【判断TV，然后播放AR】
        public void OnUnPause()
        {
            if (bTV)
            {
                tvCtr.OnResume();
            }
            else
            {
                PlayAR(true);
            }
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
            else
            {
                PlayAR(false);
            }
        }

        /// <summary>
        /// 仅播放AR内容
        /// </summary>
        public void PlayAR(bool bResume)
        {
            ResetAutoHideUITime();

            btnPlay.gameObject.SetActive(false);
            btnPause.gameObject.SetActive(true);

            if (b2D)
            {
                if (vdp2D.isPlaying == false)
                    vdp2D.Play();

                iPlayState = 0;

                //2D模式播放后，隐藏加载中UI提示
                HideLoadingUI();
            }
            else
            {
                //3D没播并且（非暂停恢复，或者停止状态）
                if (vdp3D.isPlaying == false && ((bResume == false) || iPlayState == 2))
                {
                    animator3DChar.gameObject.SetActive(true);
                    animator3DChar.Play("Take 001");

                    StopCoroutine("IEOpen3DVideo");
                    StartCoroutine("IEOpen3DVideo");
                    //Open3DVideo();
                    Invoke("_DelayPlay", 1.3f);
                    //这里的播放状态等延迟播放之后再赋值!!!
                    //mrVdp3D.enabled = true;
                    //iPlayState = 0;
                    //b3DOK = true;
                }
                else
                {
                    if (vdp3D.isPlaying == false)
                    {
                        print("vdp3D.PlayOrPause");
                        if (animator3DChar.gameObject.activeSelf == true)
                            animator3DChar.gameObject.SetActive(false);

                        vdp3D.PlayOrPause();
                    }
                    mrVdp3D.enabled = true;
                    iPlayState = 0;
                    b3DOK = true;
                    if (ads3D.isPlaying == false)
                        ads3D.Play();

                    //3D模式-不用加载容积，隐藏加载中UI提示
                    HideLoadingUI();
                }
            }
        }
        IEnumerator IEOpen3DVideo()
        {
            yield return new WaitForSeconds(1);
            particle3D.gameObject.SetActive(true);
            particle3D.Play();
            yield return new WaitForSeconds(0.2f);
            animator3DChar.gameObject.SetActive(false);
            particle3D.gameObject.SetActive(false);
        }
        void _DelayPlay()
        {
            if (bTV)
            {
                Debug.Log("切换至TV模式，不延迟播放了");
                return;
            }

            //先显示Render
            mrVdp3D.enabled = true;
            //===========================================================================
            //if (iPlayState == 2 || vdp3D.meshComponent.mesh == null)
            if (b3DStop)
            {
                //print("==============mesh为空");
                vdp3D.OpenSourceAsync(str3DVideoPth, true, 0, () =>
                {
                    b3DStop = false;
                    Debug.Log("MyLog:3DVideo_Open_OK");
                    vdp3D.Play();

                    //3D模式-加载容积，隐藏加载中UI提示
                    HideLoadingUI();
                });
                //vdp3D.OpenSource(str3DVideoPth);
            }
            else
            {
                //===========================================================================
                vdp3D.JumpFrame(0, true);
                //3D模式-容积跳转，隐藏加载中UI提示
                HideLoadingUI();
            }

            if (ads3D.isPlaying == false)
                ads3D.Play();
            iPlayState = 0;
            b3DOK = true;
        }
        /// <summary>
        /// TV模式下，播放后，隐藏加载提示UI（2D，3D根据各自的播放流程判断隐藏）
        /// </summary>
        void HideLoadingUI()
        {
            //print("隐藏了");

            if (objLoadingUI.activeSelf == true)
                objLoadingUI.SetActive(false);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void OnPause()
        {
            ResetAutoHideUITime();

            iPlayState = 1;
            //Debug.Log("暂停");

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
        public void OnStop(bool bSetTV = true)
        {
            ResetAutoHideUITime();
            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);

            iPlayState = 2;
            if (bSetTV)
            {
                if (bTV)
                {
                    tvCtr.OnStopPlay();
                }
            }
            if (b2D)
            {
                vdp2D.frame = 0;
                vdp2D.Stop();
            }
            else
            {
                //这里暂停不住，所以播完之后循环播放
                ads3D.time = 0;
                ads3D.Stop();

                print("vdp3D.Stop");

                //===========================================================================
                //vdp3D.Pause();
                vdp3D.Stop();
                b3DStop = true;
                //===========================================================================
                //这里如果用Pause，重新开启的时候，模型还是显示的？
                if (vdp3D.meshComponent.mesh != null)
                    DestroyImmediate(vdp3D.meshComponent.mesh);
                mrVdp3D.enabled = false;
                vdp3D.frameIndex = 0;
                animator3DChar.gameObject.SetActive(true);
            }

            SetSliderVideo(0);
            SetTextCurPlayTime(true);
        }

        /// <summary>
        /// 设置进度条进度【0-1】
        /// </summary>
        public void SetSliderVideo(float f)
        {
            //3D模式下，3D还没有准备成功，后面不运行
            if (b2D == false && b3DOK == false)
                return;

            try
            {
                if (f < -0.5f)
                {
                    if (b2D)
                        f = (float)vdp2D.frame / fTotalFrame2D;
                    else
                        f = (float)vdp3D.frameIndex / fTotalFrame3D;
                }
            }
            catch
            {
                Debug.Log("当前时间进度条错误：" + f.ToString());
                f = 0;
            }

            if (f < 0)
                f = 0;
            else if (f > 1)
                f = 1;

            sliderVideo.sliderValue = f;
            //print("设置进度条了");
        }

        /// <summary>
        /// 当前播放时间
        /// </summary>
        public void SetTextCurPlayTime(bool bBySlider /*bool bSetSlider*/)
        {
            //3D模式下，3D还没有准备成功，后面不运行
            if (b2D == false && b3DOK == false)
                return;

            float fCurTime = 0;
            try
            {
                if (bBySlider)
                    fCurTime = sliderVideo.sliderValue * fTotalPlayTime;
                else
                {
                    if (b2D)
                        fCurTime = ((float)vdp2D.frame / vdp2D.frameRate);
                    else
                        fCurTime = ((float)vdp3D.frameIndex / vdp3D.frameRate);
                }
            }
            catch
            {
                Debug.Log("当前时间错误：" + fCurTime.ToString());
                fCurTime = 0;
            }

            if (fCurTime < 0)
                fCurTime = 0;

            int s = Mathf.FloorToInt(fCurTime % 60);
            int m = Mathf.FloorToInt(((fCurTime - s) / 60) % 60);
            int h = Mathf.FloorToInt((fCurTime - s) / 3600);
            textCurPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
        }

        /// <summary>
        /// 设置总播放时间
        /// </summary>
        /// <param name="fTotalTime"></param>
        void SetTotalPlayTime()
        {
            //3D模式下，3D还没有准备成功，后面不运行
            if (b2D == false && b3DOK == false)
                return;

            try
            {
                //这里从帧数换算成秒数
                if (b2D)
                    fTotalPlayTime = (fTotalFrame2D / vdp2D.frameRate);
                else
                    fTotalPlayTime = (fTotalFrame3D / vdp3D.frameRate);
            }
            catch
            {
                Debug.Log("当前总时长错误：" + fTotalPlayTime.ToString());
                fTotalPlayTime = 0;
            }
            if (fTotalPlayTime < 0)
                fTotalPlayTime = 0;

            //SetDbg("播放总时间：" + fTotalPlayTime);

            //print($"fTotalPlayTime :{fTotalPlayTime},fTotalFrame2D:{fTotalFrame2D},vdp2D.frameRate:{vdp2D.frameRate},fTotalFrame3D :{fTotalFrame3D },vdp3D.frameRate:{vdp3D.frameRate}");
            int s = Mathf.FloorToInt(fTotalPlayTime % 60);
            int m = Mathf.FloorToInt(((fTotalPlayTime - s) / 60) % 60);
            int h = Mathf.FloorToInt((fTotalPlayTime - s) / 3600);
            textTotalPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
        }

        /// <summary>
        /// 进度条按下
        /// </summary>
        void SliderVideoPointerDown()
        {
            //3D模式下，3D还没有准备成功，后面不运行
            if (b2D == false && b3DOK == false)
                return;

            ResetAutoHideUITime();
            bSlideDragging = true;
            OnPause();
        }

        /// <summary>
        /// 进度条抬起
        /// </summary>
        public void SliderVideoPointerUp(bool bSetTV)
        {
            //3D模式下，3D还没有准备成功，后面不运行
            if (b2D == false && b3DOK == false)
                return;

            ResetAutoHideUITime();
            bSlideDragging = false;
            if (b2D)
            {
                //跳帧，2D视频，按照帧数控制
                vdp2D.frame = Mathf.FloorToInt(sliderVideo.sliderValue * fTotalFrame2D);
            }
            else
            {
                //声音用秒数计算（计算方法和视频的不同）
                ads3D.time = sliderVideo.sliderValue * fTotalTime3DMusic;

                //跳帧，3D视频，按照帧数控制
                //JumpFrame调用后会直接运行,所以如果是TV的3D模式，后面需要再暂停一下
                Debug.Log("vdp3D.JumpFrame");
                bool _bAutoPlay = true;

                //TV的3D模式，拖拽进度条后，这里先暂停了
                if (bTV)
                    _bAutoPlay = false;

                vdp3D.JumpFrame(Mathf.FloorToInt(sliderVideo.sliderValue * fTotalFrame3D), _bAutoPlay);
            }

            if (bTV)
            {
                if (bSetTV)
                {
                    //电视传的，总时长和当前位置，都是秒数（Int）
                    int _iCurTime = Mathf.FloorToInt(b2D ? (vdp2D.frame / vdp2D.frameRate) : (vdp3D.frameIndex / vdp3D.frameRate));
                    tvCtr.OnSetSlider((int)(fTotalPlayTime), _iCurTime);
                }
                //电视的拖拽播放，在乐播回调之后调用 CallbackSlider()
            }
            else
            {
                //AR的拖拽播放，用恢复暂停
                PlayAR(true);
            }
            print("跳转进度条了");

        }
        #endregion

    }
}
//if (Input.GetKeyDown(KeyCode.Alpha0))
//{
//    vdp3D.autoPlay = false;
//    vdp3D.OpenSource(str3DVideoPth);
//}
//if (Input.GetKeyDown(KeyCode.Alpha1))
//{
//    vdp3D.autoPlay = true;
//    vdp3D.OpenSource(str3DVideoPth);

//}
//if (Input.GetKeyDown(KeyCode.Alpha2))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.JumpFrame(0, false);
//}
//if (Input.GetKeyDown(KeyCode.Alpha3))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.JumpFrame(0, true);
//}

//if (Input.GetKeyDown(KeyCode.Alpha4))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.JumpFrame(1000, false);
//}
//if (Input.GetKeyDown(KeyCode.Alpha5))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.JumpFrame(1000, true);
//}
//if (Input.GetKeyDown(KeyCode.Alpha6))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.Pause();
//}
//if (Input.GetKeyDown(KeyCode.Alpha7))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.Stop();
//}
//if (Input.GetKeyDown(KeyCode.Alpha8))
//{
//    vdp3D.OpenSource(str3DVideoPth);
//    vdp3D.JumpFrame(0, true);
//    vdp3D.Pause();
//}
//if (Input.GetKeyDown(KeyCode.A))
//{
//    vdp3D.Play();
//}
//if (Input.GetKeyDown(KeyCode.B))
//{
//    vdp3D.Pause();
//}
//if (Input.GetKeyDown(KeyCode.C))
//{
//    vdp3D.Stop();
//}
//if (Input.GetKeyDown(KeyCode.D))
//{
//    vdp3D.frameIndex = 0;
//}