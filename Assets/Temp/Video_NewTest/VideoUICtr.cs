using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SpaceDesign
{
    public enum VideoType
    {
        None = 0,
        TV3D = 10,
        TV2D = 11,
        AR3D = 20,
        AR2D = 21,
    }
    public enum VideoUIType
    {
        None = 0,
        Reminder = 1,//预览界面
        ExpandHide = 2,//扩展-隐藏界面
        ExpandBig = 3,//扩展-大界面
        ExpandSmall = 4,//扩展-小界面
    }

    public class VideoUICtr : MonoBehaviour
    {
        static VideoUICtr inst;
        public static VideoUICtr Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<VideoUICtr>();
                return inst;
            }
        }

        //切换视频的模式<上一次的状态，要改变的状态>（AR2D、AR3D、TV2D、TV3D）
        public static UnityAction<VideoType, VideoType> ChangeVideTypeEvent;
        //之前视频的状态
        public VideoType lastVideoType = VideoType.None;
        //当前视频的状态
        public VideoType curVideoType = VideoType.None;
        //当前视频的UI状态
        public VideoUIType curVideoUIType = VideoUIType.None;

        //立即观看
        public ButtonRayReceiver btnWatchNow;

        //2D视频的对象按钮
        public ButtonRayReceiver btnVideo2D;
        //3D视频的对象按钮
        public ButtonRayReceiver btnVideo3D;

        //按钮：TV转AR模式
        public ButtonRayReceiver btnAR;
        //按钮：AR转TV模式
        public ButtonRayReceiver btnTV;
        //按钮：关闭电视
        public ButtonRayReceiver btnQuit;

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

        //2D和3D视频的父节点的按钮（碰撞显示UI，最小化点击恢复大小）
        public ButtonRayReceiver btnARWindows;
        //碰撞盒，2D和3D视频的父节点
        public BoxCollider colliderBtnARWindows;
        ////视频选择对象
        public Transform traVideoChoose;
        //视频的详情图片
        public Image imgDetailPic;

        public Transform traTVParent;

        //跟随父节点（自动旋转）
        public VideoAutoRotate videoAutoRotate;
        //跟随普通界面（大界面）
        public Transform traFollowNormalParent;
        //跟随小界面
        public Transform traFollowSmallParent;

        //开启了电视或AR模式（进入根据距离判断，退出只通过按钮控制）
        public bool bOpenVideo = false;

        public bool bTV = true;
        public bool b2D = true;

        //自动隐藏UI界面等待时间
        public float fAutoHideUITime = 30;
        //自动隐藏UI界面计时时间
        public float fAutoHideUITiming;


        Vector3 v3AutoSmallLastPos;
        int iAutoSmallTimes;
        bool bInitAutoSmall;

        void Awake()
        {
            fAutoHideUITiming = fAutoHideUITime;
        }

        void OnEnable()
        {
            btnWatchNow.onPinchDown.AddListener(OnWatchNow);
            btnVideo2D.onPinchDown.AddListener(() => { Set2DVideo(true); });
            btnVideo3D.onPinchDown.AddListener(() => { Set2DVideo(false); });
            btnTV.onPinchDown.AddListener(() => { SetTVVideo(true); });
            btnAR.onPinchDown.AddListener(() => { SetTVVideo(false); });
            btnQuit.onPinchDown.AddListener(OnVideoClose);
            btnVideoClose.onPinchDown.AddListener(OnVideoClose);
            btnVideoSize.onPinchDown.AddListener(OnVideoSize);
            btnVideoNotMove.onPinchDown.AddListener(OnVideoNotMove);
            btnVideoMove.onPinchDown.AddListener(OnVideoMove);
            btnARWindows.onPointerEnter.AddListener(() =>
            {
                if (curVideoUIType == VideoUIType.ExpandHide)
                    SetExpandUI(false, true);
            });
            btnARWindows.onPinchDown.AddListener(() =>
            {
                if (curVideoUIType == VideoUIType.ExpandSmall)
                    OnVideoSize();
            });

        }
        void OnDisable()
        {
            btnWatchNow.onPinchDown.RemoveAllListeners();
            btnVideo2D.onPinchDown.RemoveAllListeners();
            btnVideo3D.onPinchDown.RemoveAllListeners();
            btnAR.onPinchDown.RemoveAllListeners();
            btnTV.onPinchDown.RemoveAllListeners();
            btnQuit.onPinchDown.RemoveAllListeners();
            btnVideoClose.onPinchDown.RemoveAllListeners();
            btnVideoSize.onPinchDown.RemoveAllListeners();
            btnVideoNotMove.onPinchDown.RemoveAllListeners();
            btnVideoMove.onPinchDown.RemoveAllListeners();
            btnARWindows.onPointerEnter.RemoveAllListeners();
            btnARWindows.onPinchDown.RemoveAllListeners();
        }
        void Start()
        {
            //关闭视频上的碰撞盒，防止触碰显示宽展界面
            colliderBtnARWindows.enabled = false;
        }
        void Update()
        {
            //隐藏的判断：AR模式 + 扩展模式 + （卡丁车模式、或非小框模式）
            //if ((bTV == false) && (bExpand == true) && ((bSizeSmall == false) || (bKarting == true)))
            if (curVideoType == VideoType.AR2D || curVideoType == VideoType.AR3D)
            {
                if (curVideoUIType == VideoUIType.ExpandBig)
                {
                    if (fAutoHideUITiming >= 0)
                    {
                        fAutoHideUITiming -= Time.deltaTime;
                        if (fAutoHideUITiming <= 0)
                        {
                            ResetAutoHideUITime();
                            SetExpandUI(false, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据位移自动缩小播放窗口
        /// </summary>
        /// <param name="pos"></param>
        public void AutoSmall(Vector3 pos)
        {
            if (bInitAutoSmall == false)
            {
                bInitAutoSmall = true;
                v3AutoSmallLastPos = pos;
            }
            else
            {
                //AR状态下，移动跟随窗口状态，一秒内，距离大于1米，自动到小窗模式
                //if ((bTV == false) && (videoAutoRotate.bMove == true))
                if ((curVideoType == VideoType.AR2D || curVideoType == VideoType.AR3D) && (videoAutoRotate.bMove == true))
                {
                    if (curVideoUIType != VideoUIType.ExpandSmall)
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
        }

        void SetVideoType(bool b2d, bool btv)
        {
            lastVideoType = curVideoType;
            if (b2d)
            {
                if (btv)
                    curVideoType = VideoType.TV2D;
                else
                    curVideoType = VideoType.AR2D;
            }
            else
            {
                if (btv)
                    curVideoType = VideoType.TV3D;
                else
                    curVideoType = VideoType.AR3D;
            }
            if (curVideoType != lastVideoType)
                if (curVideoType != VideoType.None)
                    ChangeVideType();
        }

        /// <summary>
        /// 设置2D或3D视频
        /// </summary>
        void Set2DVideo(bool b2d)
        {
            lastVideoType = curVideoType;
            b2D = b2d;
            if (b2D)
            {
                if (bTV)
                    curVideoType = VideoType.TV2D;
                else
                    curVideoType = VideoType.AR2D;
            }
            else
            {
                if (bTV)
                    curVideoType = VideoType.TV3D;
                else
                    curVideoType = VideoType.AR3D;
            }

            if (curVideoType != lastVideoType)
                if (curVideoType != VideoType.None)
                    ChangeVideType();
        }
        /// <summary>
        /// 设置TV或AR视频模式
        /// </summary>
        void SetTVVideo(bool btv)
        {
            lastVideoType = curVideoType;
            bTV = btv;
            if (bTV)
            {
                if (b2D)
                    curVideoType = VideoType.TV2D;
                else
                    curVideoType = VideoType.TV3D;
            }
            else
            {
                if (b2D)
                    curVideoType = VideoType.AR2D;
                else
                    curVideoType = VideoType.AR3D;
            }

            if (curVideoType != lastVideoType)
                if (curVideoType != VideoType.None)
                    ChangeVideType();
        }

        /// <summary>
        /// 切换视频类型
        /// </summary>
        public void ChangeVideType()
        {
            ResetAutoHideUITime();

            btnAR.gameObject.SetActive(bTV);
            btnTV.gameObject.SetActive(!bTV);

            traVideoChoose.SetParent(b2D ? btnVideo2D.transform : btnVideo3D.transform);
            traVideoChoose.localPosition = Vector3.zero;

            btnARWindows.transform.localScale = bTV ? Vector3.zero : new Vector3(0.8f, 0.8f, 0.8f);

            if (bTV == true)
            {
                btnAR.GetComponentInChildren<TextMesh>().text = b2D ? "AR模式" : "3D全息模式";
                objWindowsBtnParent.SetActive(false);
            }


            ChangeVideTypeEvent(lastVideoType, curVideoType);
        }
        public void SetVideoValue(Sprite sp)
        {
            imgDetailPic.sprite = sp;

        }

        /// <summary>
        /// 预览界面，立刻观看按钮响应
        /// </summary>
        void OnWatchNow()
        {
            bOpenVideo = true;

            SetReminderUI(false, false);
            Invoke("_InvokeWatchNow", 1.3f);
        }

        void _InvokeWatchNow()
        {
            SetExpandUI(false, true);
            SetVideoType(true, true);
        }
        /// <summary>
        /// 设置预览界面
        /// </summary>
        public void SetReminderUI(bool bAllDisable, bool bOpen)
        {
            if (bAllDisable)
            {
                timelineReminderShow.SetActive(false);
                timelineReminderHide.SetActive(false);
            }
            else
            {
                timelineReminderShow.SetActive(bOpen);
                timelineReminderHide.SetActive(!bOpen);
                if (bOpen)
                    curVideoUIType = VideoUIType.Reminder;
                else
                    curVideoUIType = VideoUIType.None;
            }
        }
        /// <summary>
        /// 设置扩展界面
        /// </summary>
        public void SetExpandUI(bool bAllDisable, bool bOpen)
        {
            if (bAllDisable)
            {
                timelineExpandShow.SetActive(false);
                timelineExpandHide.SetActive(false);
                objWindowsBtnParent.SetActive(false);
            }
            else
            {
                timelineExpandShow.SetActive(bOpen);
                timelineExpandHide.SetActive(!bOpen);

                //打开的时候只有放大态的扩展界面（即缩小态不显示扩展界面）
                if (bOpen)
                {
                    //AR模式下才显示
                    if (bTV == false)
                        objWindowsBtnParent.SetActive(true);

                    //打开视频上的碰撞盒
                    colliderBtnARWindows.enabled = true;
                    curVideoUIType = VideoUIType.ExpandBig;
                }
                else
                {

                    objWindowsBtnParent.SetActive(false);
                    curVideoUIType = VideoUIType.ExpandHide;
                }
            }
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
            ////加一个临时bool变量，普通状态缩小的时候，直接先赋值为小状态（防止射线碰撞，出现两边详情界面）
            //bool bTempSizeSmall = bSizeSmall;

            //if (bTempSizeSmall == false)
            //{
            //    //普通状态缩小的时候，直接先赋值为小状态（防止射线碰撞，出现两边详情界面）
            //    bSizeSmall = true;

            //    btnPause.onPinchDown.Invoke();
            //    //Normal -> Small  先隐藏扩展界面，再缩小
            //    if (bExpand == true)
            //    {
            //        SetExpand(false);
            //        yield return new WaitForSeconds(0.5f);
            //    }

            //    traVideoExpand.SetParent(traFollowSmallParent);
            //}
            //else
            //{
            //    btnPlay.onPinchDown.Invoke();
            //    traVideoExpand.SetParent(traFollowNormalParent);
            //}

            //while (true)
            //{
            //    traVideoExpand.localScale = Vector3.Lerp(traVideoExpand.localScale, Vector3.one, fUISpeed * 2 * Time.deltaTime);
            //    traVideoExpand.localPosition = Vector3.Lerp(traVideoExpand.localPosition, Vector3.zero, fUISpeed * Time.deltaTime);
            //    traVideoExpand.localRotation = Quaternion.Lerp(traVideoExpand.localRotation, Quaternion.identity, fUISpeed * 2 * Time.deltaTime);
            //    float _fDis = Vector3.Distance(traVideoExpand.localPosition, Vector3.zero);
            //    if (_fDis < fThreshold)
            //    {
            //        traVideoExpand.localScale = Vector3.one;
            //        traVideoExpand.localPosition = Vector3.zero;
            //        traVideoExpand.localRotation = Quaternion.identity;
            //        break;
            //    }
            //    yield return 0;
            //}

            //if (bTempSizeSmall == true)
            //{
            //    //Small  -> Normal 先恢复扩展界面，再放大
            //    if (timelineExpandHide.gameObject.activeSelf == true)
            //    {
            //        SetExpand(true);
            //        yield return new WaitForSeconds(1.5f);
            //    }
            //}

            //bSizeSmall = !bTempSizeSmall;

            //traVideoExpand.localScale = Vector3.one;
            //traVideoExpand.localPosition = Vector3.zero;
            //traVideoExpand.localRotation = Quaternion.identity;

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
        /// 关闭视频
        /// </summary>
        void OnVideoClose()
        {
            bOpenVideo = false;

            switch (curVideoType)
            {
                case VideoType.AR2D: VideoManage2.Inst.videoAR2DCtr.OnStop(); break;
                case VideoType.AR3D: VideoManage2.Inst.videoAR3DCtr.OnStop(); break;
                case VideoType.TV2D: VideoManage2.Inst.videoTV2DCtr.OnStop(); break;
                case VideoType.TV3D: VideoManage2.Inst.videoTV3DCtr.OnStop(); break;
            }


            //视频框直接隐藏先（缩放为0）
            btnARWindows.transform.localScale = Vector3.zero;
            VideoManage2.Inst.StopCoroutine("IECloseToMiddle");
            VideoManage2.Inst.StartCoroutine("IECloseToMiddle");
            SetExpandUI(false, false);
        }
        /// <summary>
        /// 重置隐藏UI计时
        /// </summary>
        public void ResetAutoHideUITime()
        {
            fAutoHideUITiming = fAutoHideUITime;
        }

    }
}