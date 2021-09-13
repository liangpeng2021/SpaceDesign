using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

/// <summary>
///  <summary>/// create by liangpeng 2020-7-1 下载管理
/// </summary>
/// 
public class DownLoadManager : Singleton<DownLoadManager>
{
	/// <summary>
	/// 下载队列文件名
	/// </summary>
	List<DownloadUnit> _downLoadList = new List<DownloadUnit>();
	
	/// <summary>
	/// 存放下载文件的本地路径
	/// </summary>
	string localPath;
	/// <summary>
	/// 安卓类
	/// </summary>
	static AndroidJavaObject androidJavaObject;

    /// <summary>
    /// 存储空间
    /// </summary>
    public Text[] _StorageSpaceTexts;
    public Slider _StorageSpaceslider;

    /// <summary>
    /// 最大同时下载数
    /// </summary>
    int maxDownloadCount = 20;
    
	private void Start()
	{
		//设置最大连接数
		System.Net.ServicePointManager.DefaultConnectionLimit = maxDownloadCount;
#if UNITY_EDITOR
		localPath = "D:/LenQiy/LenQiyFile/";
#elif UNITY_ANDROID
			//localPath = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android", StringComparison.Ordinal))+"LenQiy/LenQiyFile/";
        localPath = Application.persistentDataPath+ "/LenQiy/LenQiyFile/";
#endif
        
#if UNITY_ANDROID
        try
        {
			//通过该API来实例化导入的arr中对应的类，参数为 包名+类名
			using (AndroidJavaClass jc = new AndroidJavaClass("com.lp.callanotherapp.CallAnother"))
			{
				androidJavaObject = jc.CallStatic<AndroidJavaObject>("getInstance");
			}
		}
		catch (Exception e)
		{
			Debug.Log("1:" + e.ToString());
		}

#endif
		CheckStorageSpace();
	}

	//读取json文件
	private static T ParseJson<T>(string filePath)
	{
		try
		{
			if (!File.Exists(filePath)) return default(T);
			var content = File.ReadAllText(filePath);
			return JsonConvert.DeserializeObject<T>(content);
		}
		catch (Exception ex)
		{
			Debug.Log("File Parse" + ex.Message);
			return default(T);
		}
	}
    
	/// <summary>
	/// 开始下载
	/// </summary>
	/// <param name="videoData"></param>
	public void StartDownLoad()
	{
        //链表中没有，添加
        DownloadUnit unit = new DownloadUnit("http://www.yoop.com.cn/upload/Oppo/model/" + "long.obj", localPath + "long.obj", false, false);
        
        //_downLoadList.Add(unit);
        Down_Single(unit);
    }

	void Down_Single(DownloadUnit unit)
	{
		Downloader.SingleDownload(unit, (currentSize, totalSize) =>
		{
			float percent = (currentSize / (float)totalSize);
			
			float mbValue = totalSize / (1024.0f * 1024.0f);
			
			if (currentSize == totalSize)
			{
				LoadDone(unit);
			}
		},
		(downUnit) =>
		{
			downUnit.isStop = true;
		});
	}

	public string GetLocalPath()
	{
		return localPath;
	}

	private void Update()
	{
		//if (_downLoadVideoData != null)
		//{
		//	lock (_downLoadVideoData)
		//	{
		//		Messenger.Broadcast(MessengerEventType.downloaddone.ToString(), _downLoadVideoData);
		//		UIManager.Instance.videoControl.ChangeVideoPlayURL(localPath+ _downLoadVideoData.downloadName);
		//		_downLoadVideoData = null;
		//		CheckStorageSpace();
		//	}
		//}
	}
    
	/// <summary>
	/// 下载完成
	/// </summary>
	void LoadDone(DownloadUnit unit)
	{
		//lock(unit)
		//{
		//	unit._videoData.downLoadState = DownLoadState.done;
		//	_downLoadVideoData = unit._videoData;
		//	if (_downLoadList.Contains(unit))
		//		_downLoadList.Remove(unit);
		//}

		////将本来准备下载，却没有下载的，非人工暂停的文件，继续下载
		//for (int i = 0; i < _downLoadList.Count; i++)
		//{
		//	if (_downLoadList[i]._videoData.downLoadState == DownLoadState.loading
		//		&& !_downLoadList[i]._threadStart)
		//	{
		//		_downLoadList[i].isStop = false;
		//		Down_Single(_downLoadList[i]);

		//		return;
		//	}
		//}
	}
	
	/// <summary>
	/// 删除下载中的文件
	/// </summary>
	/// <param name="_curFile"></param>
	public void DeleteDownLoading(/*VideoData videoData*/)
	{
		//for (int i = 0; i < _downLoadList.Count; i++)
		//{
		//	//链表中有,则删除下载中文件
		//	if (_downLoadList[i]._videoData.Equals(videoData))
		//	{
		//		Downloader.DeleteFile(_downLoadList[i]);
		//		_downLoadList.Remove(_downLoadList[i]);

  //              if (loadVideoIdList.ContainsKey(videoData.videoID))
  //              {
                    
  //                  loadVideoIdList.Remove(videoData.videoID);
  //              } 
		//		CheckStorageSpace();
		//		return;
		//	}
		//}
		//Debug.Log("未找到该下载中文件");
	}

	//若已经下载，直接删除
	public void DeleteDownLoaded(/*VideoData videoData*/)
	{
		//if (File.Exists(localPath + videoData.video_Name))
		//{
		//	File.Delete(localPath + videoData.video_Name);
		//	if (loadVideoIdList.ContainsKey(videoData.videoID))
		//		loadVideoIdList.Remove(videoData.videoID);
		//} 
		//CheckStorageSpace();
	}

	public long GetLoaclFileSize(string filename)
	{
		string path = localPath + filename;
		long length=0;
		if (File.Exists(path))
		{
			var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
			length= fs.Length;
			
			fs.Close();
			fs.Dispose();
		}

		return length;
	}
	/// <summary>
	/// 查看存储空间
	/// </summary>
	public void CheckStorageSpace()
	{
#if UNITY_ANDROID
		try
		{
			if (androidJavaObject != null)
			{
				long cursize = androidJavaObject.Call<long>("GetFreeDiskSpace");
				long totalsize = androidJavaObject.Call<long>("GetSDTotalSize");
                if (_StorageSpaceslider)
                    _StorageSpaceslider.value = 1 - cursize / (float)totalsize;

                for (int i = 0; i < _StorageSpaceTexts.Length; i++)
                {
                    _StorageSpaceTexts[i].text = (cursize / 1024.0f).ToString("f1") + "GB/" + (totalsize / 1024.0f).ToString("f1") + "GB";
                }
            }
		}
		catch (Exception e)
		{
			Debug.Log("2:" + e.ToString());
		}
#endif
	}

	private void OnApplicationQuit()
	{
        for (int i = 0; i < _downLoadList.Count; i++)
		{
			_downLoadList[i].isStop = true;
		}
	}
}
