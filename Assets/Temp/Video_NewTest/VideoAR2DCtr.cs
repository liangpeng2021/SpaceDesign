﻿using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SpaceDesign
{
    public class VideoAR2DCtr : MonoBehaviour
    {
        //2D视频播放
        public VideoPlayer vdp2D;
        //2D视频的图片
        public Sprite spr2DVideo;

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
        public float fTotalFrame = 1;
        //当前播放视频的帧率
        public float fFrameRate;
        //当前播放到第几帧
        //public long lCurFrame = 0;

        //视频的状态（默认是Stop状态）
        public VideoState playState = VideoState.Stop;

        void Awake()
        {
            vdp2D.gameObject.SetActive(false);
        }
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
        void OnDestroy()
        {
            ReleaseVideo();
        }
        void Update()
        {
            if (VideoUICtr.Inst.curVideoType != VideoType.AR2D)
                return;

            if ((playState == VideoState.Play))
            {
                SetCurTimeAndSlider();
            }
            else if(bSlideDragging)
                SetCurTimeAndSlider();

        }

        public void ChangeVideType(VideoType lastTyp, VideoType curTyp)
        {
            if (curTyp == VideoType.AR2D)
            {
                vdp2D.gameObject.SetActive(true);

                //要切换的目标状态，都延迟0.2秒（防止Unity视频和容积视频冲突崩溃）
                Invoke("InvokeGo", 0.2f);

                return;
            }
            else if (lastTyp == VideoType.AR2D)
            {
                //上次的状态，即要停止的状态
                OnStop();
                //先运行函数，最后再设置状态
                bRun = false;

                ReleaseVideo();
                vdp2D.gameObject.SetActive(false);

                return;
            }
            else
            {
                bRun = false;
                vdp2D.gameObject.SetActive(false);

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
            VideoUICtr.Inst.SetVideoValue(spr2DVideo);
            //unity视频播放不播放最后一帧
            fTotalFrame = (vdp2D.frameCount - 1);
            fFrameRate = vdp2D.frameRate;
            //先设置总长度（函数中计算总时长），再设置当前播放进度
            SetTotalPlayTime();
            //lCurFrame = 0;
            SetCurTimeAndSlider(true, 0);
            //VideoControl.Inst.SetVideValue(vdp2D.frameRate, fTotalFrame);
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

            btnPlay.gameObject.SetActive(false);
            btnPause.gameObject.SetActive(true);

            if (vdp2D.isPlaying == false)
            {
                //if (vdp2D.isPaused == true)
                //    vdp2D.Play();
                //else
                //    vdp2D.Play();
                //这里停止后和暂停后播放都是Play
                vdp2D.Play();
            }
            playState = VideoState.Play;
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

            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);

            if (vdp2D.isPlaying)
                vdp2D.Pause();
            playState = VideoState.Pause;
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

            btnPause.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);
            //停止的时候播放时间和进度条归零
            SetCurTimeAndSlider(true, 0);

            vdp2D.Stop();
            playState = VideoState.Stop;
        }


        /// <summary>
        /// 进度条按下
        /// </summary>
        void SliderVideoPointerDown()
        {
            VideoUICtr.Inst.ResetAutoHideUITime();

            OnPause();
            bSlideDragging = true;
        }

        /// <summary>
        /// 进度条抬起
        /// </summary>
        public void SliderVideoPointerUp()
        {
            if (bRun == false)
                return;

            VideoUICtr.Inst.ResetAutoHideUITime();


            //跳帧，2D视频，按照帧数控制
            //lCurFrame = Mathf.FloorToInt(sliderVideo.sliderValue * fTotalFrame);
            //vdp2D.frame = lCurFrame;
            vdp2D.time = Mathf.FloorToInt(sliderVideo.sliderValue * fTotalPlayTime);
            Debug.Log($"抬起的进度：{sliderVideo.sliderValue}==={vdp2D.frame}");

            OnPlay();
            //SetCurTimeAndSlider();
            //VideoJumpEvent(sliderVideo.sliderValue);
            bSlideDragging = false;
        }

        /// <summary>
        /// 设置当前时间和进度条【0-1】
        /// </summary>
        /// <param name="fVal">直接赋值进度条，不通过当前播放帧计算</param>
        public void SetCurTimeAndSlider(bool bSetSlider = true, float fVal = -1)
        {
            if (bSlideDragging == false)
            {
                try
                {
                    if (fVal < 0)
                    {
                        //lCurFrame = (int)vdp2D.frame;
                        //fVal = (float)lCurFrame / fTotalFrame;
                        fVal = (float)vdp2D.time / fTotalPlayTime;
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

            Debug.Log("播放进度：" + fVal);

            float fCurTime = (float)vdp2D.time;//fVal * fTotalPlayTime;

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

        void ReleaseVideo()
        {
            vdp2D.targetTexture.Release();
            vdp2D.targetTexture.MarkRestoreExpected();
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

//    if (vdp2D.isPlaying == false)
//    {
//        //if (vdp2D.isPaused == true)
//        //    vdp2D.Play();
//        //else
//        //    vdp2D.Play();
//        //这里停止后和暂停后播放都是Play
//        vdp2D.Play();
//    }
//}
///// <summary>
///// 暂停播放
///// </summary>
//void OnPause()
//{
//    if (bRun == false)
//        return;

//    if (vdp2D.isPlaying)
//        vdp2D.Pause();
//}
///// <summary>
///// 停止播放
///// </summary>
//void OnStop()
//{
//    if (bRun == false)
//        return;

//    vdp2D.Stop();
//}
///// <summary>
///// 跳转进度
///// </summary>
///// <param name="f">进度【0-1】</param>
//void OnJump(float f)
//{
//    if (bRun == false)
//        return;

//    //跳帧，2D视频，按照帧数控制
//    vdp2D.frame = Mathf.FloorToInt(f * fTotalFrame);
//    OnPlay(true);
//}
