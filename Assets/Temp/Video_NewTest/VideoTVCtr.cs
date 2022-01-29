using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace SpaceDesign
{
    public class VideoTVCtr : MonoBehaviour
    {
        static VideoTVCtr inst;
        public static VideoTVCtr Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<VideoTVCtr>();
                return inst;
            }
        }


        public void TVLog(string s)
        {
            Debug.Log("视频Debug-电视:" + s);
        }

        public void ARLog(string s)
        {
            Debug.Log("视频Debug-AR:" + s);
        }

        static AndroidJavaObject _ajo;
        static AndroidJavaObject AJO
        {
            get
            {
                if (_ajo == null)
                    _ajo = new AndroidJavaObject("com.example.functionmodule.LeLinkFunctions");
                return _ajo;
            }
        }

        //LelinkSourceSDK.MEDIA_TYPE_AUDIO —-音频(101)
        //LelinkSourceSDK.MEDIA_TYPE_VIDEO —- 视频(102)
        //LelinkSourceSDK.MEDIA_TYPE_IMAGE —- 图片(103)
        const int videoType = 102;
        const int imgType = 103;


        void Start()
        {
            OnInit();
        }


        string GetBlackImgPth()
        {
            return Path.Combine(SpaceDesign.PathConfig.GetPth(), "b.jpg");
            //return Path.Combine(Application.persistentDataPath, "b.jpg");
        }

        string GetVideoPth(bool b2D)
        {
            return Path.Combine(SpaceDesign.PathConfig.GetPth(), (b2D ? "2.ts" : "3.ts"));
            //return Path.Combine(Application.persistentDataPath, (b2D ? "2.ts" : "3.ts"));
        }


        #region 初始化-搜索-连接
        //是否连接成功
        public bool bConnected = false;
        //搜索中
        bool bSearching = false;
        //连接中
        bool bConnecting = false;

        /// <summary>
        /// 设置Lebo的回调监听，并初始化SDK
        /// </summary>
        public void OnInit()
        {
            bSearching = false;
            bConnecting = false;
            bConnected = false;

            ARLog("开始初始化Init");

            try
            {
                AJO.Call("Init", this.transform.name);
            }
            catch (Exception exc)
            {
                ARLog("初始化报错：/n" + exc.ToString());
            }
        }
        /// <summary>
        /// 初始化回调
        /// </summary>
        public void CallbackLocal(string s)
        {
            ARLog("初始化回调:" + s);

            if (s.Contains("InitSuc"))
            {
                if (bSearching == false)
                {
                    bSearching = true;
                    Invoke("OnStartSearch", 1);
                }
            }
        }

        /// <summary>
        /// 开始搜索
        /// </summary>
        public void OnStartSearch()
        {
            ARLog("开始搜索");

            //return;
            try
            {
                AJO.Call("StartBrowse");
            }
            catch (Exception exc)
            {
                ARLog("开始搜索报错/n" + exc.ToString());
            }
        }
        /// <summary>
        /// 搜索结果回调
        /// </summary>
        public void CallbackSearch(string s)
        {
            ARLog("搜索回调:" + s);

            if (s.Contains("SearchSuc"))
            {
                if (bConnecting == false)
                {
                    bConnecting = true;
                    ARLog("Java内停止搜索，搜索成功回调AR，开始自动连接");
                    Invoke("OnStartConnect", 1);
                }
            }
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        public void OnStartConnect()
        {
            ARLog("开始连接");
            try
            {
                AJO.Call("Connect");
            }
            catch (Exception exc)
            {
                ARLog("开始连接报错/n" + exc.ToString());
            }
        }
        /// <summary>
        /// 停止连接
        /// </summary>
        public void OnStopConnect()
        {
            bConnecting = false;

            ARLog("开始停止连接");
            try
            {
                AJO.Call("DisConnect");
            }
            catch (Exception exc)
            {
                ARLog("停止连接报错/n" + exc.ToString());
            }
        }

        /// <summary>
        /// 连接回调
        /// </summary>
        public void CallbackConnect(string s)
        {
            ARLog("连接回调:" + s);

            if (s.Contains("ConnectSuc"))
            {
                ARLog("连接回调：连接成功");
                bConnected = true;
                bConnecting = false;
                bSearching = false;
            }
        }

        #endregion

        #region 播放控制：Push、Play、Pause、Stop、Jump
        /// <summary>
        /// 关闭（推送一个黑色的图片）
        /// </summary>
        public void OnClose()
        {
            ARLog("关闭（推送一个黑色的图片）");

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                bSetbackSlider = false;
                AJO.Call("Push", true, GetBlackImgPth(), imgType, true);
            }
            catch (Exception exc)
            {
                ARLog("关闭，（推送图片）报错/n" + exc.ToString());
            }
        }

        //bool b2DVideo = true;
        bool b2DVideo
        {
            get
            {
                return VideoUICtr.Inst.b2D;
            }
        }


        /// <summary>
        /// 推送
        /// </summary>
        /// <param name="b2D">播放的是2D视频</param>
        /// <param name="bCallbackAutoPlay">回调后自动播放</param>
        /// <param name="bCallbackSetSlider">回调后设置进度条（AR模式切TV模式）</param>
        public void OnPush(bool b2D)
        {
            //ARLog("开始推送视频是否2D：" + b2D + ",回调后是否自动播放:" + bPushBackAutoPlay + ",回调后是否自动设置跳转进度:" + bCallBackSetSlider);
            ARLog("开始推送视频是否2D：" + b2D);

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            //b2DVideo = b2D;

            try
            {
                bSetbackSlider = true;
                AJO.Call("Push", true, GetVideoPth(b2D), videoType, true);
            }
            catch (Exception exc)
            {
                ARLog("推送报错/n" + exc.ToString());
            }
        }


        /// <summary>
        /// 播放（TV端每次调用都是重新开始的）
        /// </summary>
        public void OnPlay()
        {
            ARLog("开始Play播放视频：" + b2DVideo);

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                bSetbackSlider = true;
                AJO.Call("PlayVideo", GetVideoPth(b2DVideo), videoType, true);
            }
            catch (Exception exc)
            {
                ARLog("播放报错/n" + exc.ToString());
            }
        }

        /// <summary>
        /// 播放回调
        /// </summary>
        public void CallbackPlay(string s)
        {
            ARLog("播放回调:" + s);
            if (b2DVideo)
            {
                VideoManage2.Inst.videoTV2DCtr.CallbackPlay();
            }
            else
            {
                VideoManage2.Inst.videoTV3DCtr.CallbackPlay();
            }
        }

        /// <summary>
        /// 暂停后，恢复播放（非重新开始）
        /// </summary>
        public void OnResume()
        {
            ARLog("开始恢复暂停");

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                bSetbackSlider = true;
                AJO.Call("ResumeVideo");
            }
            catch (Exception exc)
            {
                ARLog("播放报错/n" + exc.ToString());
            }
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void OnPause()
        {
            ARLog("开始暂停视频");

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                AJO.Call("PauseVideo");
            }
            catch (Exception exc)
            {
                ARLog("暂停报错/n" + exc.ToString());
            }
        }
        /// <summary>
        /// 暂停回调
        /// </summary>
        public void CallbackPause(string s)
        {
            ARLog("暂停回调（不改变AR端状态）:" + s);
            //if (b2DVideo)
            //{
            //    VideoManage2.Inst.videoTV2DCtr.CallbackPause();
            //}
            //else
            //{
            //    VideoManage2.Inst.videoTV3DCtr.CallbackPause();
            //}
        }
        ///// <summary>
        ///// 停止播放
        ///// </summary>
        //public void OnStopPlay()
        //{
        //    ARLog("开始停止视频");

        //    if (bConnected == false)
        //    {
        //        OnInit();
        //        return;
        //    }

        //    OnClose();

        //}
        /// <summary>
        /// 停止回调
        /// </summary>
        public void CallbackStop(string s)
        {
            ARLog("停止回调:" + s);

            //if (b2DVideo)
            //{
            //    VideoManage2.Inst.videoTV2DCtr.CallbackStop();
            //}
            //else
            //{
            //    VideoManage2.Inst.videoTV3DCtr.CallbackStop();
            //}
        }
        /// <summary>
        /// 播放完成回调
        /// </summary>
        public void CallbackCompletion(string s)
        {
            ARLog("播放完成回调:" + s);

            if (b2DVideo)
            {
                VideoManage2.Inst.videoTV2DCtr.CallbackStop();
            }
            else
            {
                VideoManage2.Inst.videoTV3DCtr.CallbackStop();
            }
        }

        /// <summary>
        /// 进度条
        /// </summary>
        public void OnSetSlider(int duration, int position)
        {
            ARLog("开始设置进度条");

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                AJO.Call("SetSlider", duration, position);
            }
            catch (Exception exc)
            {
                ARLog("设置进度条错误/n" + exc.ToString());
            }
        }

        //是否设置进度条
        public bool bSetbackSlider = true;
        /// <summary>
        /// 进度条修改回调
        /// </summary>
        public void CallbackSlider(string strTime)
        {
            if (VideoUICtr.Inst.bTV == false)
                return;

            if (bSetbackSlider == false)
                return;

            string[] ss = strTime.Split('-');
            int _duration = int.Parse(ss[0]);

            if ((_duration == 0))
                return;

            ////return;
            int _position = int.Parse(ss[1]);

            float _fRate = (float)_position / (float)_duration;
            ARLog($"进度条回调：duration:{_duration},position:{_position},_fRate:{_fRate}");

            if (b2DVideo)
            {
                VideoManage2.Inst.videoTV2DCtr.CallbackSlider(_duration, _position, _fRate);
            }
            else
            {
                VideoManage2.Inst.videoTV3DCtr.CallbackSlider(_duration, _position, _fRate);
            }
        }
        #endregion

    }
}