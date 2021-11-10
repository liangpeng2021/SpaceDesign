using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
/// <summary>
/// 路径配置，/*create by 梁鹏 2021-11-10 */
/// </summary>
namespace SpaceDesign
{
    public class PathConfig
    {
        /// <summary>
        /// 获取配置文件的路径
        /// </summary>
        public static string GetPth()
        {
            string path;
#if UNITY_ANDROID && !UNITY_EDITOR
                path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android", StringComparison.Ordinal));
#else
            //path = Application.streamingAssetsPath.Substring(0, Application.streamingAssetsPath.IndexOf("opporoom", StringComparison.Ordinal));
            path = Application.streamingAssetsPath.Substring(0, Application.streamingAssetsPath.IndexOf("OPPO", StringComparison.Ordinal));
#endif
            path = Path.Combine(path, "LenQiy", "Scences");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}