//using OXRTK.ARHandTracking;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.UI;

//namespace SpaceDesign
//{
//    public class VideoControl : MonoBehaviour
//    {
//        static VideoControl inst;
//        public static VideoControl Inst
//        {
//            get
//            {
//                if (inst == null)
//                    inst = FindObjectOfType<VideoControl>();
//                return inst;
//            }
//        }

//        /// <summary>
//        /// 视频播放事件，参数为Resume(是否从暂停恢复的播放)
//        /// </summary>
//        public static UnityAction<bool> VideoPlayEvent;
//        /// <summary>
//        /// 视频暂停事件
//        /// </summary>
//        public static UnityAction VideoPauseEvent;
//        /// <summary>
//        /// 视频停止事件
//        /// </summary>
//        public static UnityAction VideoStopEvent;
//        /// <summary>
//        /// 视频跳转事件，进度条【0-1】
//        /// </summary>
//        public static UnityAction<float> VideoJumpEvent;

//        //当前播放时长Text
//        public Text textCurPlayTime;
//        //总播放时长Text
//        public Text textTotalPlayTime;
//        //播放进度条
//        //sliderVideo.sliderValue【0-1】
//        public PinchSlider sliderVideo;
//        //进度条手动拖拽中
//        public bool bSlideDragging;
//        //按钮：播放
//        public ButtonRayReceiver btnPlay;
//        //按钮：暂停
//        public ButtonRayReceiver btnPause;

//        //当前播放视频的总时长（这里是秒）
//        public float fTotalPlayTime = 1f;
//        //当前播放视频的总帧数（这里是帧）
//        public float fTotalFrame = 1;
//        //当前播放视频的帧率
//        public float fFrameRate;
//        //当前播放到第几帧
//        public float fCurFrame = 0;

//        //视频的状态（默认是Stop状态）
//        public VideoState playState = VideoState.Stop;

//        void OnEnable()
//        {
//            btnPlay.onPinchDown.AddListener(OnPlay);
//            btnPause.onPinchDown.AddListener(OnPause);

//            sliderVideo.onInteractionStart.AddListener(SliderVideoPointerDown);
//            sliderVideo.onInteractionEnd.AddListener(SliderVideoPointerUp);
//        }
//        void OnDisable()
//        {
//            btnPlay.onPinchDown.RemoveAllListeners();
//            btnPause.onPinchDown.RemoveAllListeners();

//            sliderVideo.onInteractionStart.RemoveAllListeners();
//            sliderVideo.onInteractionEnd.RemoveAllListeners();
//        }


//        void Update()
//        {
//            //if (VideoUICtr.Inst.curVideoType != VideoType.AR2D)
//            //    return;

//            if (playState == VideoState.Play)
//            {
//                SetCurTimeAndSlider();
//            }
//        }

//        public void SetVideValue(float fframerate, float ftotalframe)
//        {
//            //先设置总长度（函数中计算总时长），再设置当前播放进度
//            SetTotalPlayTime(fframerate, ftotalframe);
//            fCurFrame = 0;
//            SetCurTimeAndSlider(0);
//        }

//        /// <summary>
//        /// 播放
//        /// </summary>
//        void OnPlay()
//        {
//            VideoUICtr.Inst.ResetAutoHideUITime();

//            //从暂停恢复的
//            bool _bResume = (playState == VideoState.Pause);
//            VideoPlayEvent?.Invoke(_bResume);

//            playState = VideoState.Play;
//            btnPlay.gameObject.SetActive(false);
//            btnPause.gameObject.SetActive(true);
//        }
//        /// <summary>
//        /// 暂停播放
//        /// </summary>
//        void OnPause()
//        {
//            VideoUICtr.Inst.ResetAutoHideUITime();

//            VideoPauseEvent?.Invoke();

//            playState = VideoState.Stop;
//            btnPause.gameObject.SetActive(false);
//            btnPlay.gameObject.SetActive(true);
//        }
//        /// <summary>
//        /// 停止播放
//        /// </summary>
//        public void OnStop()
//        {
//            VideoUICtr.Inst.ResetAutoHideUITime();

//            VideoStopEvent?.Invoke();

//            playState = VideoState.Stop;
//            btnPause.gameObject.SetActive(false);
//            btnPlay.gameObject.SetActive(true);
//            //停止的时候播放时间和进度条归零
//            SetCurTimeAndSlider(0);
//        }


//        /// <summary>
//        /// 进度条按下
//        /// </summary>
//        void SliderVideoPointerDown()
//        {
//            VideoUICtr.Inst.ResetAutoHideUITime();

//            bSlideDragging = true;
//            OnPause();
//        }

//        /// <summary>
//        /// 进度条抬起
//        /// </summary>
//        public void SliderVideoPointerUp()
//        {
//            VideoUICtr.Inst.ResetAutoHideUITime();

//            bSlideDragging = false;

//            VideoJumpEvent(sliderVideo.sliderValue);
//        }

//        ///// <param name="bSetSlider">是否设置进度条</param>
//        ///// <param name="bConvertCurFrame">通过当前播放帧，计算进度条</param>
//        //public void SetCurTimeAndSlider(bool bSetSlider, bool bConvertCurFrame, float fVal = 0)
//        /// <summary>
//        /// 设置当前时间和进度条【0-1】
//        /// </summary>
//        /// <param name="fVal">直接赋值进度条，不通过当前播放帧计算</param>
//        public void SetCurTimeAndSlider(float fVal = -1)
//        {
//            if (bSlideDragging == false)
//            {
//                try
//                {
//                    if (fVal < 0)
//                        fVal = fCurFrame / fTotalFrame;
//                }
//                catch
//                {
//                    Debug.Log("当前时间进度条错误：" + fVal.ToString());
//                    fVal = 0;
//                }

//                if (fVal < 0)
//                    fVal = 0;
//                else if (fVal > 1)
//                    fVal = 1;

//                sliderVideo.sliderValue = fVal;
//            }

//            float fCurTime = fVal * fTotalPlayTime;

//            if (fCurTime < 0)
//                fCurTime = 0;

//            int s = Mathf.FloorToInt(fCurTime % 60);
//            int m = Mathf.FloorToInt(((fCurTime - s) / 60) % 60);
//            int h = Mathf.FloorToInt((fCurTime - s) / 3600);
//            textCurPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
//        }

//        /// <summary>
//        /// 设置总播放时间
//        /// </summary>
//        void SetTotalPlayTime(float fframerate, float ftotalframe)
//        {
//            fTotalFrame = ftotalframe;
//            fFrameRate = fframerate;

//            try
//            {
//                fTotalPlayTime = (fTotalFrame / fFrameRate);
//            }
//            catch
//            {
//                Debug.Log("当前总时长错误：" + fTotalPlayTime.ToString());
//                fTotalPlayTime = 0;
//            }
//            if (fTotalPlayTime < 0)
//                fTotalPlayTime = 0;

//            Debug.Log("播放总时间：" + fTotalPlayTime);

//            int s = Mathf.FloorToInt(fTotalPlayTime % 60);
//            int m = Mathf.FloorToInt(((fTotalPlayTime - s) / 60) % 60);
//            int h = Mathf.FloorToInt((fTotalPlayTime - s) / 3600);
//            textTotalPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
//        }
//    }
//}