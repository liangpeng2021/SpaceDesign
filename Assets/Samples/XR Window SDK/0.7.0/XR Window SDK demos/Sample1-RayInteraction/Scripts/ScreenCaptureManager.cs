using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using XR;

public class ScreenCaptureManager : MonoBehaviour
{
    #region Instance

    private const int CameraWidth = 1920;
    private const int CameraHeight = 1080;

    public static ScreenCaptureManager Instance { get; private set; }

    #endregion

    #region UnityMethods

    private void Awake()
    {
        Instance = this;

        RgbCamera.OpenRgbCamera();

        XR.ScreenCapture.OnScreenShotData += OnScreenShotData;
    }

    private void OnDestroy()
    {
        RgbCamera.CloseRgbCamera();

        XR.ScreenCapture.OnScreenShotData -= OnScreenShotData;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            RgbCamera.CloseRgbCamera();
        else
            RgbCamera.OpenRgbCamera();
    }

    #endregion

    public event Action<Texture> OnXRCaptureResult;

    public XRError TakeCameraShot(string path)
    {
        var options = XRScreenCaptureOptions.Default;
        options.flags = (ulong)(XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_MIXREALITY //显示现实背景(相机作为背景)
                                 | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_OVERVIEW //隐藏预览图
                                 | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_CURSOR); //隐藏光标、射线等
        options.width = (uint)RgbCamera.Width;
        options.height = (uint)RgbCamera.Height;

        var result = XR.ScreenCapture.ScreenShot(options, path);

        return result;
    }

    public XRError StartVideoCapture(string path)
    {
        var options = XRScreenCaptureOptions.Default;
        options.width = (uint)RgbCamera.Width;
        options.height = (uint)RgbCamera.Height;

        options.flags = (ulong)(XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_MIXREALITY //显示现实背景(相机作为背景)
                                 | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_STATUS_BAR //隐藏录屏状态条
                                 | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_OVERVIEW //隐藏预览图
                                 | XRScreenCaptureFlag.XR_SCREEN_CAPTURE_FLAG_HIDE_CURSOR); //隐藏光标、射线等

        const XRScreenCaptureMode mode = XRScreenCaptureMode.XR_SCREEN_CAPTURE_MODE_CAPTURE;
        XRScreenCaptureExtraOptions extraOptions = new XRScreenCaptureExtraOptions();
        XRScreenCaptureStreamOptions streamOptions = new XRScreenCaptureStreamOptions();
        streamOptions.audioSource = XRScreenCaptureAudioSource.XR_SCREEN_CAPTURE_AUDIO_SOURCE_MIC;

        var result = XR.ScreenCapture.StartCapture(options, mode, path, streamOptions, extraOptions);

        return result;
    }

    public XRError StopVideoCapture()
    {
        return XR.ScreenCapture.EndCapture();
    }

    private Texture2D previewTexture;
    private void OnScreenShotData(long identifier, BufferImage image)
    {
        if (previewTexture == null)
        {
            previewTexture = new Texture2D(RgbCamera.Width, RgbCamera.Height, TextureFormat.RGBA32, false);
        }
        else
        {
            previewTexture.Resize(RgbCamera.Width, RgbCamera.Height);
        }

        var buffer = previewTexture.GetRawTextureData<byte>();
        unsafe
        {
            Buffer.MemoryCopy(image.DataPointer.ToPointer(),
                buffer.GetUnsafePtr(),
                buffer.Length,
                image.Width * image.Height * 4);
        }
        previewTexture.Apply();

        OnXRCaptureResult?.Invoke(previewTexture);
    }
}
