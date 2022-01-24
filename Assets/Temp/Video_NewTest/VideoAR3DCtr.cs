using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpaceDesign
{
    public class VideoAR3DCtr : MonoBehaviour
    {
        //3D视频的名称（"Video3D.mp4"）
        [SerializeField]
        private string str3DVideoPth = "Video3D.mp4";
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
        //是否AR2D模式
        bool bRun = false;

        //当前播放时长Text
        public Text textCurPlayTime;
        //总播放时长Text
        public Text textTotalPlayTime;
        //播放进度条
        //sliderVideo.sliderValue【0-1】
        public PinchSlider sliderVideo;
        //进度条手动拖拽中
        public bool bSlideDragging;
        //按钮：播放
        public ButtonRayReceiver btnPlay;
        //按钮：暂停
        public ButtonRayReceiver btnPause;

        //当前播放视频的总时长（这里是秒）
        public float fTotalPlayTime = 1f;
        //当前播放视频的总帧数（这里是帧）
        //3D总长（这里是帧数，不是秒数，秒数乘以帧率）容积视频的总帧数还要减1，因为最后一帧不播放
        float fTotalFrame = 2613;
        //3D的音乐总长，（这里是秒数，跟容积视频的长度是不同的）
        float fTotalTime3DMusic = 115.271f;

        //当前播放视频的帧率
        public float fFrameRate;
        //当前播放到第几帧
        public float fCurFrame = 0;

        //视频的状态（默认是Stop状态）
        public VideoState playState = VideoState.Stop;


        void OnEnable()
        {
            //VideoControl.VideoPlayEvent += OnPlay;
            //VideoControl.VideoPauseEvent += OnPause;
            //VideoControl.VideoStopEvent += OnStop;
            //VideoControl.VideoJumpEvent += OnJump;
            VideoUICtr.ChangeVideTypeEvent += ChangeVideType;
            btnPlay.onPinchDown.AddListener(OnPlay);
            btnPause.onPinchDown.AddListener(OnPause);

            sliderVideo.onInteractionStart.AddListener(SliderVideoPointerDown);
            sliderVideo.onInteractionEnd.AddListener(SliderVideoPointerUp);
        }

        void OnDisable()
        {
            //VideoControl.VideoPlayEvent -= OnPlay;
            //VideoControl.VideoPauseEvent -= OnPause;
            //VideoControl.VideoStopEvent -= OnStop;
            //VideoControl.VideoJumpEvent -= OnJump;
            VideoUICtr.ChangeVideTypeEvent -= ChangeVideType;
            btnPlay.onPinchDown.RemoveAllListeners();
            btnPause.onPinchDown.RemoveAllListeners();

            sliderVideo.onInteractionStart.RemoveAllListeners();
            sliderVideo.onInteractionEnd.RemoveAllListeners();
        }
        void Start()
        {
            obj3DStage.SetActive(false);
        }
        void Update()
        {
            if (VideoUICtr.Inst.curVideoType != VideoType.AR3D)
                return;

            if (playState == VideoState.Play)
            {
                fCurFrame = vdp3D.frameIndex;
                SetCurTimeAndSlider();
            }
        }
        public void ChangeVideType(VideoType lastTyp, VideoType curTyp)
        {
            if (curTyp == VideoType.AR3D)
            {
                //要切换的目标状态，都延迟0.2秒（防止Unity视频和容积视频冲突崩溃）
                Invoke("InvokeGo", 0.2f);

                return;
            }
            else if (lastTyp == VideoType.AR3D)
            {
                //上次的状态，即要停止的状态
                OnStop();
                //先运行函数，最后再设置状态
                bRun = false;

                return;
            }
            else
            {
                bRun = false;

                return;
            }
        }

        /// <summary>
        /// 要切换的目标状态，都延迟0.2秒（防止Unity视频和容积视频冲突崩溃）
        /// </summary>
        void InvokeGo()
        {
            //本次的状态，即要切换的目标状态
            bRun = true;
            VideoUICtr.Inst.SetVideoValue(spr3DVideo);
            //unity视频播放不播放最后一帧
            fFrameRate = vdp3D.frameRate;
            //先设置总长度（函数中计算总时长），再设置当前播放进度
            SetTotalPlayTime();
            fCurFrame = 0;
            SetCurTimeAndSlider(0);
            //VideoControl.Inst.SetVideValue(vdp3D.frameRate, fTotalFrame3D);
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



        /// <summary>
        /// 停止后或暂停后播放
        /// </summary>
        void OnPlay()
        {
            if (bRun == false)
                return;

            VideoUICtr.Inst.ResetAutoHideUITime();

            //从暂停恢复的
            bool _bResume = (playState == VideoState.Pause);
            //VideoPlayEvent?.Invoke(_bResume);

            if (_bResume)
            {


                playState = VideoState.Play;
                btnPlay.gameObject.SetActive(false);
                btnPause.gameObject.SetActive(true);

            }
            else
            {
                animator3DChar.gameObject.SetActive(true);
                animator3DChar.Play("Take 001");

                StopCoroutine("IEOpen3DVideo");
                StartCoroutine("IEOpen3DVideo");
                //Open3DVideo();
                Invoke("_DelayPlay", 1.3f);


                ///放到加载完毕之后
                //===========================================================================
                playState = VideoState.Play;
                btnPlay.gameObject.SetActive(false);
                btnPause.gameObject.SetActive(true);
                //===========================================================================

            }
        }
        /// <summary>
        /// 暂停播放
        /// </summary>
        void OnPause()
        {
            if (bRun == false)
                return;

            VideoUICtr.Inst.ResetAutoHideUITime();

            //VideoPauseEvent?.Invoke();

            playState = VideoState.Stop;
            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);

            if (ads3D.isPlaying)
                ads3D.Pause();
            if (vdp3D.isPlaying)
                vdp3D.Pause();
        }
        /// <summary>
        /// 停止播放
        /// </summary>
        public void OnStop()
        {
            if (bRun == false)
                return;

            VideoUICtr.Inst.ResetAutoHideUITime();

            //VideoStopEvent?.Invoke();

            playState = VideoState.Stop;
            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);
            //停止的时候播放时间和进度条归零
            SetCurTimeAndSlider(0);


            b3DStop = true;

            //这里暂停不住，所以播完之后循环播放
            ads3D.time = 0;
            ads3D.Stop();

            //vdp3D.Pause();
            vdp3D.Stop();
            b3DStop = true;
            //===========================================================================
            //这里如果用Pause，重新开启的时候，模型还是显示的？
            mrVdp3D.enabled = false;
            if (vdp3D.meshComponent.mesh != null)
                DestroyImmediate(vdp3D.meshComponent.mesh);
            //vdp3D.frameIndex = 0;
            animator3DChar.Play(0, -1, -0f);
            animator3DChar.Update(0);
            animator3DChar.gameObject.SetActive(true);
        }


        /// <summary>
        /// 进度条按下
        /// </summary>
        void SliderVideoPointerDown()
        {
            VideoUICtr.Inst.ResetAutoHideUITime();

            bSlideDragging = true;
            OnPause();
        }

        /// <summary>
        /// 进度条抬起
        /// </summary>
        public void SliderVideoPointerUp()
        {
            VideoUICtr.Inst.ResetAutoHideUITime();

            bSlideDragging = false;

            if (bRun == false)
                return;

            //跳帧，3D视频，按照帧数控制
            Debug.Log("vdp3D.JumpFrame");

            //声音用秒数计算（计算方法和视频的不同）
            float _f = sliderVideo.sliderValue;
            ads3D.time = _f * fTotalTime3DMusic;
            ads3D.Play();
            vdp3D.JumpFrame(Mathf.FloorToInt(_f * fTotalFrame), true);
            //VideoJumpEvent(sliderVideo.sliderValue);
        }

        /// <summary>
        /// 设置当前时间和进度条【0-1】
        /// </summary>
        /// <param name="fVal">直接赋值进度条，不通过当前播放帧计算</param>
        public void SetCurTimeAndSlider(float fVal = -1)
        {
            if (bSlideDragging == false)
            {
                try
                {
                    if (fVal < 0)
                    {
                        fCurFrame = vdp3D.frameIndex;
                        fVal = fCurFrame / fTotalFrame;
                    }
                }
                catch
                {
                    Debug.Log("当前时间进度条错误：" + fVal.ToString());
                    fVal = 0;
                }

                if (fVal < 0)
                    fVal = 0;
                else if (fVal > 1)
                    fVal = 1;

                sliderVideo.sliderValue = fVal;
            }

            float fCurTime = fVal * fTotalPlayTime;

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
        void SetTotalPlayTime()
        {
            try
            {
                fTotalPlayTime = (fTotalFrame / fFrameRate);
            }
            catch
            {
                Debug.Log("当前总时长错误：" + fTotalPlayTime.ToString());
                fTotalPlayTime = 0;
            }
            if (fTotalPlayTime < 0)
                fTotalPlayTime = 0;

            Debug.Log("播放总时间：" + fTotalPlayTime);

            int s = Mathf.FloorToInt(fTotalPlayTime % 60);
            int m = Mathf.FloorToInt(((fTotalPlayTime - s) / 60) % 60);
            int h = Mathf.FloorToInt((fTotalPlayTime - s) / 3600);
            textTotalPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
        }
    }
}

///// <summary>
///// 停止后或暂停后播放
///// </summary>
//void OnPlay(bool bResume)
//{
//    if (bRun == false)
//        return;

//    if (bResume)
//    {

//    }
//    else
//    {
//        animator3DChar.gameObject.SetActive(true);
//        animator3DChar.Play("Take 001");

//        StopCoroutine("IEOpen3DVideo");
//        StartCoroutine("IEOpen3DVideo");
//        //Open3DVideo();
//        Invoke("_DelayPlay", 1.3f);
//    }
//}
///// <summary>
///// 暂停播放
///// </summary>
//void OnPause()
//{
//    if (bRun == false)
//        return;

//    if (ads3D.isPlaying)
//        ads3D.Pause();
//    if (vdp3D.isPlaying)
//        vdp3D.Pause();
//}
///// <summary>
///// 停止播放
///// </summary>
//void OnStop()
//{
//    if (bRun == false)
//        return;

//    b3DStop = true;

//    //这里暂停不住，所以播完之后循环播放
//    ads3D.time = 0;
//    ads3D.Stop();

//    //vdp3D.Pause();
//    vdp3D.Stop();
//    b3DStop = true;
//    //===========================================================================
//    //这里如果用Pause，重新开启的时候，模型还是显示的？
//    mrVdp3D.enabled = false;
//    if (vdp3D.meshComponent.mesh != null)
//        DestroyImmediate(vdp3D.meshComponent.mesh);
//    //vdp3D.frameIndex = 0;
//    animator3DChar.Play(0, -1, -0f);
//    animator3DChar.Update(0);
//    animator3DChar.gameObject.SetActive(true);
//}

///// <summary>
///// 跳转进度
///// </summary>
///// <param name="f">进度【0-1】</param>
//void OnJump(float f)
//{
//    if (bRun == false)
//        return;

//    //跳帧，3D视频，按照帧数控制
//    Debug.Log("vdp3D.JumpFrame");

//    //声音用秒数计算（计算方法和视频的不同）
//    ads3D.time = f * fTotalTime3DMusic;
//    ads3D.Play();
//    vdp3D.JumpFrame(Mathf.FloorToInt(f * fTotalFrame3D), true);
//}
