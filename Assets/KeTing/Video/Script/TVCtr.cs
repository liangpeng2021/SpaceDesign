/* Create by zh at 2021-10-17

   电视控制（乐播控制）

 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpaceDesign
{
    public class TVCtr : MonoBehaviour
    {
        public void TVLog(string s)
        {
            Debug.Log("电视Debug:" + s);
        }

        public void ARLog(string s)
        {
            Debug.Log("ARDebug:" + s);
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

        //是否播放中
        public bool bPlaying;
        //是否连接成功
        public bool bConnected = false;
        ////是否推送成功
        //public bool bPushed = true;

        //LelinkSourceSDK.MEDIA_TYPE_AUDIO —-音频(101)
        //LelinkSourceSDK.MEDIA_TYPE_VIDEO —- 视频(102)
        //LelinkSourceSDK.MEDIA_TYPE_IMAGE —- 图片(103)
        const int videoType = 102;
        const int imgType = 103;
        //bLocalRes - - 是否为本地文件
        //bool bLocalRes = true;

        //void Start()
        //{
        //    //#if UNITY_EDITOR
        //    //            url = @"E:\LenQiy\OPPO\";
        //    //#else
        //    //            url = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android", StringComparison.Ordinal));
        //    //            url = Path.Combine(url, "LenQiy");
        //    //#endif
        //    //if (Directory.Exists(url) == false)
        //    //    Directory.CreateDirectory(url);
        //    //TempLog("路径：" + url + "\n");

        //    //url = Application.persistentDataPath;
        //    //textLocal.text = url + "===" + Directory.Exists(url);
        //}

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
        //搜索中
        bool bSearching = false;
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

            //string[] strs = s.Split('-');
            //for (int i = 0; i < textAryToggleConnect.Length; i++)
            //{
            //    if (i < strs.Length)
            //    {
            //        textAryToggleConnect[i].text = strs[i];
            //    }
            //    else
            //    {
            //        textAryToggleConnect[i].text = null;
            //    }
            //}

            if (s.Contains("SearchSuc"))
            {
                if (bConnecting == false)
                {
                    bConnecting = true;
                    ARLog("Java内停止搜索，自动连接");
                    Invoke("OnStartConnect", 1);
                }
            }
        }

        //连接中
        bool bConnecting = false;
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

        string GetBlackImgPth()
        {
            return Path.Combine(Application.persistentDataPath, "b.jpg");
        }

        string GetVideoPth(bool b2D)
        {
            return Path.Combine(Application.persistentDataPath, (b2D ? "2.ts" : "3.ts"));
        }

        //推送的是关闭的黑色图
        public bool bPushBackAutoPlay = false;
        //推送回调，直接设置进度条（AR模式切TV模式）
        public bool bCallBackSetSlider = false;
        /// <summary>
        /// 关闭（推送一个黑色的图片）
        /// </summary>
        public void OnClose()
        {
            bPushBackAutoPlay = false;
            ARLog("关闭（推送一个黑色的图片）");

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                AJO.Call("Push", GetBlackImgPth(), imgType, true);
            }
            catch (Exception exc)
            {
                ARLog("关闭，（推送图片）报错/n" + exc.ToString());
            }
        }

        //自动播放状态：【不播放，-1】【播放2D，2】【播放3D，3】
        int iAutoPlay = -1;
        /// <summary>
        /// 推送
        /// </summary>
        /// <param name="b2D">播放的是2D视频</param>
        /// <param name="bCallbackAutoPlay">回调后自动播放</param>
        /// <param name="bCallbackSetSlider">回调后设置进度条（AR模式切TV模式）</param>
        public void OnPush(bool b2D, bool bCallbackAutoPlay, bool bCallbackSetSlider)
        {
            bSetSliderFirstPlay = false;

            bPushBackAutoPlay = bCallbackAutoPlay;
            bCallBackSetSlider = bCallbackSetSlider;
            ARLog("开始推送视频是否2D：" + b2D + ",回调后是否自动播放:" + bPushBackAutoPlay + ",回调后是否自动设置跳转进度:" + bCallBackSetSlider);

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            iAutoPlay = b2D ? 2 : 3;

            try
            {
                AJO.Call("Push", GetVideoPth(b2D), videoType, true);
            }
            catch (Exception exc)
            {
                ARLog("推送报错/n" + exc.ToString());
            }
        }

        /// <summary>
        /// 推送回调
        /// </summary>
        public void CallbackPush(string s)
        {
            ARLog("推送Push回调:" + s);

            //推送的不是黑色关闭图，才触发回调
            //播放的时候也有判断（切换TV和AR模式，先推送，再设置进度，所以第一遍的推送不自动开始）
            if (VideoManage.Inst.bTV)
            {
                if (s.Contains("VideoStart"))
                {
                    if (bPushBackAutoPlay == true)
                    {
                        bPushBackAutoPlay = false;
                        if (iAutoPlay == 2 || iAutoPlay == 3)
                        {
                            VideoManage.Inst.sliderVideo.sliderValue = 0;
                            VideoManage.Inst.PlayAR(false);
                        }
                    }
                    else
                    {
                        if (bCallBackSetSlider)
                        {
                            bCallBackSetSlider = false;
                            VideoManage.Inst.SliderVideoPointerUp(false);
                        }
                    }
                }
                else if (s.Contains("VideoFinish"))
                {
                    //VideoManage.Inst.OnPause();
                    //VideoManage.Inst.OnStop();
                }
                else if (s.Contains("NotInit"))
                {
                    ARLog("电视连接失败，重新初始化");
                    OnInit();
                }
            }
        }

        /// <summary>
        /// 播放（每次调用都是重新开始）
        /// </summary>
        public void OnPlay(bool b2D)
        {
            ARLog("开始Play播放视频2D：" + b2D);

            if (bConnected == false)
            {
                OnInit();
                return;
            }

            try
            {
                AJO.Call("PlayVideo", GetVideoPth(b2D), videoType, true);
            }
            catch (Exception exc)
            {
                ARLog("播放报错/n" + exc.ToString());
            }
        }
        public void CallbackPlay(string s)
        {
            ARLog("播放回调:" + s);
            VideoManage.Inst.PlayAR(false);
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
                AJO.Call("ResumeVideo");
            }
            catch (Exception exc)
            {
                ARLog("播放报错/n" + exc.ToString());
            }
        }
        /// <summary>
        /// 恢复播放回调
        /// </summary>
        public void CallbackResume(string s)
        {
            ARLog("恢复播放回调:" + s);
            VideoManage.Inst.PlayAR(true);
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
        /// 停止播放
        /// </summary>
        public void OnStopPlay()
        {
            ARLog("开始停止视频");

            OnClose();

            if (bConnected == false)
            {
                OnInit();
                return;
            }
        }

        /// <summary>
        /// 进度条
        /// </summary>
        public void OnSetSlider(int duration, int position)
        {
            bSetSliderFirstPlay = false;

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

        bool bSetSliderFirstPlay = false;
        /// <summary>
        /// 进度条修改回调
        /// </summary>
        public void CallbackSlider(string strTime)
        {
            ////return;
            string[] ss = strTime.Split('-');
            int duration = int.Parse(ss[0]);
            int position = int.Parse(ss[1]);

            ////float _fTime = float.Parse(strTime);
            float _fTime = (float)position / (float)duration;
            ARLog($"进度条回调：duration:{duration},position:{position},_fTime:{_fTime}");

            //textLocal.text = "进度条回调：" + _fTime.ToString();
            //textPush.text = "进度条回调：" + _fTime.ToString();
            ////textSearch.text = "进度条回调：" + _fTime.ToString();

            //这里回调之后，乐播其实还没推送完，所以，进度条的回调播放，还是要在CallBackPush里面的“播放成功”中
            //bPushBackAutoPlay = true;
            ////if (VideoManage.Inst.bTV)
            ////{
            ////    VideoManage.Inst.SetSliderByTV(_fTime);
            ////    ////VideoManage.Inst.sliderVideo.sliderValue = _fTime;
            ////    //VideoManage.Inst.SliderVideoPointerUp(false);
            ////    //VideoManage.Inst.SetTextCurPlayTime(true);
            ////    ////VideoManage.Inst.PlayAR();
            ////}
            ///
            if (bSetSliderFirstPlay == false)
            {
                bSetSliderFirstPlay = true;
                VideoManage.Inst.PlayAR(false);
            }

        }
    }
}