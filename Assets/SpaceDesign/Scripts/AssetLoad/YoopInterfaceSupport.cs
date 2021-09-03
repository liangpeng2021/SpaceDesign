using Museum;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
	[Header(" Yoop后台接口地址")]
	public YoopInterface[] yoopInterfaces;

	/// <summary>
	/// 所有的接口地址
	/// </summary>
	public static Dictionary<InterfaceName, string> yoopInterfaceDic=new Dictionary<InterfaceName, string>();
    
	/// <summary>
	/// 已获取的图片数据
	/// </summary>
	static Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();

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
	public static IEnumerator GetHttpVideoData<T>(WWWForm wwwForm, InterfaceName interfaceName,Action<T> callback)
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

	/// <summary>
	/// 下载图片加载到内存
	/// </summary>
	/// <param name="imageUrl"></param>
	/// <param name="width"></param>
	/// <param name="high"></param>
	/// <param name="callback"></param>
	/// <returns></returns>
	public static IEnumerator GetImageFromURL(string imageUrl, int width, int high, System.Action<Sprite> callback, System.Action<String> errorcallback)
	{
		if (GameTools.NetWorkEnv == NetState.NoNet)
		{
			//LogManager.Instance.ShowTipObj("网络连接失败，请检查网络设置", 2f);
			yield break;
		}

		//若已经下载过，直接用之前的
		if (spriteDic.ContainsKey(imageUrl))
		{
			callback?.Invoke(spriteDic[imageUrl]);

			yield break;
		}

		UnityWebRequest wr = new UnityWebRequest(imageUrl);
		DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
		wr.downloadHandler = texDl;
		wr.SendWebRequest();

		while (!wr.isDone)
		{
			yield return null;
		}

		if (wr.isNetworkError || wr.isHttpError)
		{
            errorcallback?.Invoke(wr.error);
            Debug.Log(imageUrl);
        }

        Sprite _sprite = null;

        if (wr.isDone && !wr.isNetworkError && !wr.isHttpError)
		{
			try
			{
				Texture2D tex = new Texture2D(width, high);
				tex = texDl.texture;
				_sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
				if (!spriteDic.ContainsKey(imageUrl))
				    spriteDic.Add(imageUrl, _sprite);
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
				//LogManager.Instance.AddLogMessage(e.ToString());
			}
		}
        callback?.Invoke(_sprite);

        yield return null;
	}
}
