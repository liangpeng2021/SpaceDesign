using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDesign
{
    public class VideoTV3DCtr : MonoBehaviour
    {
        public void TVLog(string s)
        {
            Debug.Log("3D电视回调:" + s);
        }

        public void ARLog(string s)
        {
            Debug.Log("3D电视眼镜:" + s);
        }

        //3D视频的图片
        public Sprite spr3DVideo;

        //是否AR3D模式
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

        //当前播放视频的总时长（这里是秒，跟电视端的长度一致）
        int iTotalPlayTime = 120;


        //视频的状态（默认是Stop状态）
        public VideoState playState = VideoState.Stop;

        void OnEnable()
        {
            VideoUICtr.ChangeVideTypeEvent += ChangeVideType;
            btnPlay.onPinchDown.AddListener(OnPlay);
            btnPause.onPinchDown.AddListener(OnPause);

            sliderVideo.onInteractionStart.AddListener(SliderVideoPointerDown);
            sliderVideo.onInteractionEnd.AddListener(SliderVideoPointerUp);
        }

        void OnDisable()
        {
            VideoUICtr.ChangeVideTypeEvent -= ChangeVideType;
            btnPlay.onPinchDown.RemoveAllListeners();
            btnPause.onPinchDown.RemoveAllListeners();

            sliderVideo.onInteractionStart.RemoveAllListeners();
            sliderVideo.onInteractionEnd.RemoveAllListeners();
        }

        public void ChangeVideType(VideoType lastTyp, VideoType curTyp)
        {
            if (curTyp == VideoType.TV3D)
            {
                //===========================================================================
                //弹出等待加载UI，Play回调之后，隐藏加载UI
                VideoManage2.Inst.SetLoadingUI(true);
                //===========================================================================
                //要切换的目标状态，都延迟0.2秒（防止Unity视频和容积视频冲突崩溃）
                Invoke("InvokeGo", 0.2f);

                return;
            }
            else if (lastTyp == VideoType.TV3D)
            {
                ////上次的状态，即要停止的状态
                //OnStop();
                TurnStopState();
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
            bSlideDragging = false;
            //本次的状态，即要切换的目标状态
            VideoUICtr.Inst.SetVideoValue(spr3DVideo);
            //先设置总长度（函数中计算总时长），再设置当前播放进度
            SetTotalPlayTime();
            SetCurTimeAndSlider(0, false);
            bRun = true;
            VideoTVCtr.Inst.OnPush(false);
        }

        /// <summary>
        /// 停止后或暂停后播放
        /// </summary>
        void OnPlay()
        {
            if (bRun == false)
                return;

            VideoUICtr.Inst.ResetAutoHideUITime();

            //===========================================================================
            //中间弹出等待加载UI
            VideoManage2.Inst.SetLoadingUI(true);
            //===========================================================================

            //从暂停恢复的
            bool _bResume = (playState == VideoState.Pause);

            if (_bResume == true)
                VideoTVCtr.Inst.OnResume();
            else
                VideoTVCtr.Inst.OnPlay();
        }

        void TurnPlayState()
        {
            //if (playState != VideoState.Play)
            {
                VideoManage2.Inst.SetLoadingUI(false);

                playState = VideoState.Play;

                if (btnPlay.gameObject.activeSelf == true)
                    btnPlay.gameObject.SetActive(false);
                if (btnPause.gameObject.activeSelf == false)
                    btnPause.gameObject.SetActive(true);
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

            VideoTVCtr.Inst.OnPause();
        }

        public void CallbackPause()
        {
            TurnPauseState();
        }

        void TurnPauseState()
        {
            //if (playState != VideoState.Pause)
            {
                VideoManage2.Inst.SetLoadingUI(false);
                playState = VideoState.Pause;

                if (btnPlay.gameObject.activeSelf == false)
                    btnPlay.gameObject.SetActive(true);
                if (btnPause.gameObject.activeSelf == true)
                    btnPause.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void OnStop()
        {
            if (bRun == false)
                return;

            VideoUICtr.Inst.ResetAutoHideUITime();

            VideoTVCtr.Inst.OnClose();
        }
        public void CallbackStop()
        {
            TurnStopState();
        }

        void TurnStopState()
        {
            //if (playState != VideoState.Stop)
            {
                VideoManage2.Inst.SetLoadingUI(false);

                playState = VideoState.Stop;

                //停止的时候播放时间和进度条归零
                SetCurTimeAndSlider(0, false);

                if (btnPlay.gameObject.activeSelf == false)
                    btnPlay.gameObject.SetActive(true);
                if (btnPause.gameObject.activeSelf == true)
                    btnPause.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 进度条按下
        /// </summary>
        void SliderVideoPointerDown()
        {
            VideoUICtr.Inst.ResetAutoHideUITime();

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

            int _iTotal = (int)iTotalPlayTime;
            int _iCur = (int)(_iTotal * (sliderVideo.sliderValue));
            VideoTVCtr.Inst.OnSetSlider(_iTotal, _iCur);
            bSlideDragging = false;
        }

        public void CallbackSlider(int _duration, int _position, float _fRate)
        {
            if (bSlideDragging == true)
            {
                Debug.Log("拖拽中，电视的进度回调不解析");
                return;
            }

            if (iTotalPlayTime != _duration)
            {
                iTotalPlayTime = _duration;
                SetTotalPlayTime();
            }

            SetCurTimeAndSlider(_fRate, true);
        }

        void SetCurTimeAndSlider(float _fRate, bool _bTurnPlay)
        {
            sliderVideo.sliderValue = _fRate;

            //float fCurTime = (float)vdp3D.time;//fVal * fTotalPlayTime;
            float fCurTime = _fRate * iTotalPlayTime;

            if (fCurTime < 0)
                fCurTime = 0;

            int s = Mathf.FloorToInt(fCurTime % 60);
            int m = Mathf.FloorToInt(((fCurTime - s) / 60) % 60);
            int h = Mathf.FloorToInt((fCurTime - s) / 3600);
            textCurPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";

            if (_bTurnPlay)
                TurnPlayState();
        }

        /// <summary>
        /// 设置总播放时间
        /// </summary>
        void SetTotalPlayTime()
        {
            if (iTotalPlayTime < 0)
                iTotalPlayTime = 0;

            //Debug.Log("播放总时间：" + fTotalPlayTime);

            int s = Mathf.FloorToInt((float)iTotalPlayTime % 60);
            int m = Mathf.FloorToInt(((float)(iTotalPlayTime - s) / 60) % 60);
            int h = Mathf.FloorToInt((float)(iTotalPlayTime - s) / 3600);
            textTotalPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
        }

    }
}



///// <summary>
///// 设置当前时间和进度条【0-1】
///// </summary>
///// <param name="fVal">直接赋值进度条，不通过当前播放帧计算</param>
//public void SetCurTimeAndSlider(bool bSetSlider = true, float fVal = -1)
//{
//    //if (bSlideDragging == false)
//    //{
//    //    try
//    //    {
//    //        if (fVal < 0)
//    //        {
//    //            fVal = (float)vdp3D.time / fTotalPlayTime;
//    //        }
//    //    }
//    //    catch
//    //    {
//    //        Debug.Log("当前时间进度条错误：" + fVal.ToString());
//    //        fVal = 0;
//    //    }

//    //    if (fVal < 0)
//    //        fVal = 0;
//    //    else if (fVal > 1)
//    //        fVal = 1;

//    //    sliderVideo.sliderValue = fVal;
//    //}
//    if (bSetSlider)
//    {
//        try
//        {
//            if (fVal < 0)
//            {
//                fVal = (float)vdp3D.time / iTotalPlayTime;
//            }
//        }
//        catch
//        {
//            Debug.Log("当前时间进度条错误：" + fVal.ToString());
//            fVal = 0;
//        }

//        if (fVal < 0)
//            fVal = 0;
//        else if (fVal > 1)
//            fVal = 1;

//        sliderVideo.sliderValue = fVal;
//    }
//    else
//    {
//        //vdp3D.time = Mathf.FloorToInt(sliderVideo.sliderValue * fTotalPlayTime);
//        fVal = sliderVideo.sliderValue;
//    }

//    //float fCurTime = (float)vdp3D.time;//fVal * fTotalPlayTime;
//    float fCurTime = fVal * iTotalPlayTime;

//    if (fCurTime < 0)
//        fCurTime = 0;

//    int s = Mathf.FloorToInt(fCurTime % 60);
//    int m = Mathf.FloorToInt(((fCurTime - s) / 60) % 60);
//    int h = Mathf.FloorToInt((fCurTime - s) / 3600);
//    textCurPlayTime.text = $"{h.ToString("D2")}:{m.ToString("D2")}:{s.ToString("D2")}";
//}