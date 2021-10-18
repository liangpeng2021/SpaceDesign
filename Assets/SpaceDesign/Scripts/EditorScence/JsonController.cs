using LitJson;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
/// <summary>
/// json文件操作，/*create by 梁鹏 2021-8-17 */
/// </summary>
public class JsonController
{
    //读取json文件
    public static T ParseJsonNewtonsoft<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return default(T);
            var content = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(content);
        }
        catch (Exception ex)
        {
            Debug.Log("File Parse:" + ex.Message);
            return default(T);
        }
    }

    //保存json文件
    public static void SaveJsonNewtonsoft<T>(string filePath,T value)
    {
        try
        {
            FileStream fs = null;
            string direName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(direName)) Directory.CreateDirectory(direName);
            fs = new FileStream(filePath, FileMode.Create);

            string str = JsonConvert.SerializeObject(value);
            byte[] bts = System.Text.Encoding.UTF8.GetBytes(str);
            fs.Write(bts, 0, bts.Length);
            if (fs != null)
            {
                fs.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("File Save:" + ex.Message);
        }
    }

    //读取json文件
    public static T ParseJsonLitJson<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return default(T);
            var content = File.ReadAllText(filePath);
            return JsonMapper.ToObject<T>(content);
        }
        catch (Exception ex)
        {
            Debug.Log("File Parse:" + ex.Message);
            return default(T);
        }
    }

    //保存json文件
    public static void SaveJsonLitJson<T>(string filePath, T value)
    {
        try
        {
            FileStream fs = null;
            string direName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(direName)) Directory.CreateDirectory(direName);
            fs = new FileStream(filePath, FileMode.Create);

            string str = JsonMapper.ToJson(value);
            byte[] bts = System.Text.Encoding.UTF8.GetBytes(str);
            fs.Write(bts, 0, bts.Length);
            if (fs != null)
            {
                fs.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("File Save:" + ex.Message);
        }
    }
}
