using LitJson;
using Museum;
//using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        public static YoopInterfaceSupport Instance;
        
        /// <summary>
        /// 所有的接口地址
        /// </summary>
        public Dictionary<InterfaceName, string> yoopInterfaceDic = new Dictionary<InterfaceName, string>();

        private void Awake()
        {
            for (int i = 0; i < yoopInterfaces.Length; i++)
            {
                if (!yoopInterfaceDic.ContainsKey(yoopInterfaces[i]._interfaceName))
                {
                    yoopInterfaceDic.Add(yoopInterfaces[i]._interfaceName, yoopInterfaces[i]._url);
                }
            }

            Instance = this;
        }

        /// <summary>
        /// 获取后台数据
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetHttpData<T>(WWWForm wwwForm, InterfaceName interfaceName, Action<T> callback)
        {
            if (GameTools.NetWorkEnv == NetState.NoNet)
            {
                EditorControl.Instance.ShowTipTime("网络连接失败", 2f);
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
                    Debug.Log("MyLog::"+interfaceName + "|www.error:" + www.error);
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
                    //yyd = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
                    //Debug.Log("MyLog::www.downloadHandler.text:"+www.downloadHandler.text);
                    yyd = JsonMapper.ToObject<T>(www.downloadHandler.text);
                    if (yyd != null)
                        callback?.Invoke(yyd);
                }
            }
            catch (Exception e)
            {
                Debug.Log("MyLog::" + e);
                //LogManager.Instance.ShowTipObj("网络超时，请稍后再试", 2f);
            }

            yield return 0;
        }

        /// <summary>
        /// 下载文件加载到内存
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="callback"></param>
        /// <param name="errorcallback"></param>
        /// <returns></returns>
        public static IEnumerator GetScenceFileFromURL(string fileUrl, Action<ScenceData> callback, Action<String> errorcallback)
        {
            if (GameTools.NetWorkEnv == NetState.NoNet)
            {
                //LogManager.Instance.ShowTipObj("网络连接失败，请检查网络设置", 2f);
                yield break;
            }

            UnityWebRequest request = UnityWebRequest.Get(fileUrl);
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                errorcallback?.Invoke(request.error);
            }
            else
            {
                callback?.Invoke(MyDeSerialFromUrl(request.downloadHandler.data));
            }

            yield return null;
        }

        /// <summary>
        /// 反序列化（读取path路径下的文件），将数据从文件读取出来
        /// </summary>
        static ScenceData MyDeSerialFromUrl(byte[] bytes)
        {
            ScenceData gameObjectDatas = null;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                gameObjectDatas = bf.Deserialize(ms) as ScenceData;
            }//关闭Stream

            return gameObjectDatas;
        }

        // 上传文件
        public IEnumerator UploadFile<T>(WWWForm wwwForm, InterfaceName interfaceName, Action<T> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(yoopInterfaceDic[interfaceName], wwwForm))
            {
                yield return www.SendWebRequest();
                T yyd;
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    //yyd = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
                    yyd = JsonMapper.ToObject<T>(www.downloadHandler.text);
                    if (yyd != null)
                        callback?.Invoke(yyd);
                }
            }
        }

        public static IEnumerator SendDataToCPE<T>(string url, Action<T> callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                T yyd;
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("MyLog::request.error:" + request.error);
                }
                else
                {
                    yyd = JsonMapper.ToObject<T>(request.downloadHandler.text);
                    if (yyd != null)
                        callback?.Invoke(yyd);
                }

                yield return null;
            }
        }
    }
}
