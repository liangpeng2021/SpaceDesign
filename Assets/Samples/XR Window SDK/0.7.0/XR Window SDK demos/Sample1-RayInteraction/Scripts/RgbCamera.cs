using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

public static class RgbCamera
{
    private static CameraDevice rgbCamera;

    private static bool camerOpen = false;

    public static readonly int Width = 1920;
    public static readonly int Height = 1080;

    public static bool OpenRgbCamera()
    {
        if (camerOpen)
        {
            Debug.Log("RGBCamera is already open");
        }

        if (Application.platform != RuntimePlatform.Android)
            return false;

        CameraDevice[] devices = CameraDevice.GetCameraList();

        if (devices == null)
        {
            Debug.Log("No camera devices");
            camerOpen = false;
            return false;
        }

        XRCameraConfig deviceConfig = new XRCameraConfig();

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].SupportedConfigs != null)
            {
                foreach (var config in devices[i].SupportedConfigs)
                {
                    if (config.width == Width && config.height == Height)
                    {
                        deviceConfig = config;
                        rgbCamera = devices[i];
                        break;
                    }
                }
            }
        }

        if (rgbCamera == null)
        {
            Debug.Log("Can not find rgb camera");
            camerOpen = false;
            return false;
        }

        XRError innerError = rgbCamera.Open(deviceConfig);

        if (innerError == XRError.XR_ERROR_SUCCESS)
        {
            camerOpen = true;
            return true;
        }
        else
        {
            camerOpen = false;
            Debug.Log("Open rgbcamera failed. Check if casting or capturing!");
            return false;
        }
    }

    public static void CloseRgbCamera()
    {
        if (rgbCamera != null)
        {
            if (camerOpen)
            {
                rgbCamera.Close();
                camerOpen = false;
            }

            rgbCamera = null;
        }
    }
}
