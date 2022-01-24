using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace SpaceDesign
{
    public class VideoTVCtr : MonoBehaviour
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

        //LelinkSourceSDK.MEDIA_TYPE_AUDIO —-音频(101)
        //LelinkSourceSDK.MEDIA_TYPE_VIDEO —- 视频(102)
        //LelinkSourceSDK.MEDIA_TYPE_IMAGE —- 图片(103)
        const int videoType = 102;
        const int imgType = 103;


        void Start()
        {
            //OnInit();
        }


        string GetBlackImgPth()
        {
            return Path.Combine(Application.persistentDataPath, "b.jpg");
        }

        string GetVideoPth(bool b2D)
        {
            return Path.Combine(Application.persistentDataPath, (b2D ? "2.ts" : "3.ts"));
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

        #region 


        #endregion

        #region 
        #endregion

        #region 
        #endregion
    }
}