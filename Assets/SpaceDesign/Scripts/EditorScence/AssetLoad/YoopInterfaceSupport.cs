using Museum;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceDesign
{
    /// <summary>
    /// Yoop后台接口数据类型
    /// </summary>
    [System.Serializable]
    public struct YoopInterface
    {
        public InterfaceName _interfaceName;
        public string _url;
    }

    /// <summary>
    /// 设置该脚本执行顺序最前面
    /// </summary>
    public class YoopInterfaceSupport : MonoBehaviour
    {
        [Header("Yoop后台接口地址")]
        public YoopInterface[] yoopInterfaces;

        /// <summary>
        /// 所有的接口地址
        /// </summary>
        public static Dictionary<InterfaceName, string> yoopInterfaceDic = new Dictionary<InterfaceName, string>();
        
        private void Awake()
        {
            for (int i = 0; i < yoopInterfaces.Length; i++)
            {
                if (!yoopInterfaceDic.ContainsKey(yoopInterfaces[i]._interfaceName))
                {
                    yoopInterfaceDic.Add(yoopInterfaces[i]._interfaceName, yoopInterfaces[i]._url);
                }
            }
        }

        /// <summary>
        /// 获取后台数据
        /// </summary>
        /// <returns></returns>
        public static IEnumerator GetHttpVideoData<T>(WWWForm wwwForm, InterfaceName interfaceName, Action<T> callback)
        {
            if (GameTools.NetWorkEnv == NetState.NoNet)
            {
                //LogManager.Instance.ShowTipObj("网络连接失败，请检查网络设置", 2f);
                yield break;
            }

            UnityWebRequest www = UnityWebRequest.Post(yoopInterfaceDic[interfaceName], wwwForm);

            www.timeout = 60;
            yield return www.SendWebRequest();
            T yyd;
            try
            {
                if (www.error != null)
                {
                    Debug.Log(interfaceName + "|www.error:" + www.error);
                    Debug.Log(yoopInterfaceDic[interfaceName]);

                    //LogManager.Instance.ShowTipObj("网络超时，请稍后再试", 2.5f);
                }

                if (www.isDone && string.IsNullOrEmpty(www.error))
                {

#if UNITY_EDITOR
                    //if (interfaceName == InterfaceName.deleteGuankan
                    //	|| interfaceName == InterfaceName.cancelShoucang
                    //	|| interfaceName == InterfaceName.cancelDianzan
                    //	|| interfaceName == InterfaceName.dianzanpost)
                    {
                        Debug.Log(interfaceName);
                        Debug.Log(www.downloadHandler.text);
                    }
                    //Debug.Log("-------------");
#endif
                    yyd = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);

                    if (yyd != null)
                        callback?.Invoke(yyd);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                //LogManager.Instance.ShowTipObj("网络超时，请稍后再试", 2f);
            }

            yield return 0;
        }

        // 上传文件
        public static IEnumerator UploadFile<T>(WWWForm wwwForm, InterfaceName interfaceName, Action<T> callback)
        {
            byte[] gifByte = File.ReadAllBytes("E:/Work/ffepgtest/gif/a.gif");
            //根据自己长传的文件修改格式
            wwwForm.AddBinaryData("file", gifByte, "myGif.mp4", "a/gif");

            using (UnityWebRequest www = UnityWebRequest.Post(yoopInterfaceDic[interfaceName], wwwForm))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    string text = www.downloadHandler.text;
                    Debug.Log("服务器返回值" + text);//正确打印服务器返回值
                }
            }
        }
    }
}
