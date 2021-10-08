using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Time2File
{
    //private static string BasePath = "/storage/emulated/0/DCIM/TestCamera/";
    //private static string BasePath = "/storage/emulated/0/DCIM/";

    public static string GetDataPath(string path, bool video)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string date = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        if (video)
        {
            path = path + date + ".mp4";
        }
        else
        {
            path = path + date + ".png";
        }

        return path;
    }
}
