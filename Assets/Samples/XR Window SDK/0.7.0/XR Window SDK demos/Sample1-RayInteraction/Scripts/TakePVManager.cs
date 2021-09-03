/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace XR.Samples
{
    public class TakePVManager
    {
        private const string BasePath = "/storage/emulated/0/DCIM/TestCamera/";
        private const int CameraWidth = 1920;
        private const int CameraHeight = 1080;

        private static TakePVManager instance;
        public static TakePVManager GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new TakePVManager();
                return instance;
            }
        }

        private TakePVManager()
        {
            XR.ScreenCapture.OnScreenShotData += ScreenCapture_OnShotData;
            XR.ScreenCapture.OnScreenCaptureResult += ScreenCapture_OnCptResult;
            XR.ScreenCapture.OnScreenCaptureModeChange += ScreenCapture_OnModeChange;
        }

        private void ScreenCapture_OnModeChange(XRScreenCaptureMode mode, bool enable)
        {
        }

        private void ScreenCapture_OnCptResult(XRScreenCaptureEventType type, long identifier, XRError error)
        {
        }

        private void ScreenCapture_OnShotData(long identifier, BufferImage image)
        {
        }

        public string TakePhoto()
        {
            XRError err = TakePhotoInner();
            string str = "拍照成功-路径:DCIM/TestCamera";
            if (err != 0) str = "拍照失败！！！";
            return str;
        }

        public string StartVideo()
        {
            XRError err = StartVideoInner();
            string str = "录像开启成功！！！";
            if (err != 0) str = "录像开启失败！！！";
            return str;
        }

        public string StopVideo()
        {
            XRError err = StopVideoInner();
            string str = "结束录像-路径:DCIM/TestCamera";
            if (err != 0) str = "录像保存失败！！！";
            return str;
        }

        private XRError TakePhotoInner()
        {
            var options = XRScreenCaptureOptions.Default;
            options.flags = (ulong)(XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_MIXREALITY //显示现实背景(相机作为背景)
                                     | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_OVERVIEW //隐藏预览图
                                     | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_CURSOR); //隐藏光标、射线等
            options.width = CameraWidth;
            options.height = CameraHeight;
            var ret = XR.ScreenCapture.ScreenShot(options, GetDataPath(TakePVType.TakePhoto));

            return ret;
        }

        private XRError StartVideoInner()
        {
            var options = XRScreenCaptureOptions.Default;
            options.width = CameraWidth;
            options.height = CameraHeight;
            options.flags = (ulong)(XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_MIXREALITY //显示现实背景(相机作为背景)
                                     | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_STATUS_BAR //隐藏录屏状态条
                                     | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_OVERVIEW //隐藏预览图
                                     | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_CURSOR); //隐藏光标、射线等
            const XRScreenCaptureMode mode = XRScreenCaptureMode.XR_SCREEN_CAPTURE_MODE_CAPTURE;
            XRScreenCaptureExtraOptions extraOptions = new XRScreenCaptureExtraOptions();
            XRScreenCaptureStreamOptions streamOptions = new XRScreenCaptureStreamOptions();
            streamOptions.audioSource = XRScreenCaptureAudioSource.XR_SCREEN_CAPTURE_AUDIO_SOURCE_MIC;
            var ret = XR.ScreenCapture.StartCapture(options, mode, GetDataPath(TakePVType.Video), streamOptions, extraOptions);
            return ret;
        }

        private XRError StopVideoInner()
        {
            return XR.ScreenCapture.EndCapture();
        }

        private string GetDataPath(TakePVType type)
        {
            string path = "";
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            string date = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            switch (type)
            {
                case TakePVType.TakePhoto:
                    path = BasePath + date + ".png";
                    break;

                case TakePVType.Video:
                    path = BasePath + date + ".mp4";
                    break;
            }

            return path;
        }

        enum TakePVType
        {
            None = 0,
            TakePhoto,
            Video
        }
    }
}