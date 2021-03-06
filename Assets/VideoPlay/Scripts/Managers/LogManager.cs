
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using XR;
//create by liangpeng，2020-7-4
//管理日志的输出
//end
public class LogManager : Singleton<LogManager>
{
	#region 输出日志

	public bool IsTest = true;

	[SerializeField]
	GameObject[] testObjs;
    
    public class LogData
	{
		public string str;
		public LogType logType;
        public int num = 1;
    }

	List<LogData> strList = new List<LogData>();
	int startCount = 0;
    /// <summary>
    /// 是否自动显示最新消息
    /// </summary>
    bool isAutoShowNew;
    /// <summary>
    /// 不显示最新消息，防止覆盖
    /// </summary>
    public Button dontShowNewBtn;
    /// <summary>
    /// 自动显示最新消息
    /// </summary>
    public Button showNewBtn;
    /// <summary>
    /// 菜单，点击复位所有按钮
    /// </summary>
    public Button resetMenuBtn;
    public Transform uiTran;
    Transform stereoCameraTran;
    void InitLog()
	{
		//隐藏调试对象
		for (int i = 0; i < testObjs.Length; i++)
		{
			if (IsTest)
				testObjs[i].SetActive(true);
			else
				testObjs[i].SetActive(false);
		}
        if (IsTest)
		{
#if UNITY_EDITOR
			localLogPath = "D:/LenQiy/Log/";
#elif UNITY_ANDROID
			localLogPath = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android", StringComparison.Ordinal)) + "LenQiy/Log/";
#endif
            //是否自动显示新消息
            dontShowNewBtn.onClick.AddListener(()=> {
                isAutoShowNew = false;
                dontShowNewBtn.gameObject.SetActive(false);
                showNewBtn.gameObject.SetActive(true);
            });
            showNewBtn.onClick.AddListener(()=> {
                isAutoShowNew = true;
                dontShowNewBtn.gameObject.SetActive(true);
                showNewBtn.gameObject.SetActive(false);
            });
            isAutoShowNew = true;
            dontShowNewBtn.gameObject.SetActive(true);
            showNewBtn.gameObject.SetActive(false);
            //end
            //重置UI位置
            resetMenuBtn.onClick.AddListener(ResetMenuPos);
            stereoCameraTran = XRCameraManager.Instance.stereoCamera.transform;

            topBtn.onClick.AddListener(() => { SetLogTextPos(true); });
			bottomBtn.onClick.AddListener(() => { SetLogTextPos(false); });

            lastBtn.onClick.AddListener(() => { SetLogTextStr(true); });
            nextBtn.onClick.AddListener(() => { SetLogTextStr(false); });
            qingkongBtn.onClick.AddListener(Clear);
            //#if !UNITY_EDITOR
            Application.logMessageReceivedThreaded += AddLogThread;
//#endif
        }
	}
    void AddLogThread(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Log)
        {
            isShow = true;
            string temp = string.Format("{0}{1}", condition + "      *************      " + "\n", stackTrace);
            for (int i = 0; i < strList.Count; i++)
            {
                if (strList[i].str.Equals(temp))
                {
                    strList[i].num++;
                    if (isAutoShowNew)
                        startCount = i;
                    return;
                }
            }
            if (type == LogType.Log && !condition.Contains("MyLog"))
                return;
            LogData logData = new LogData();
            logData.str = temp;
            logData.logType = type;

            //logText.text += type.ToString() + " ";
            strList.Add(logData);
            //strList.Add("----------------------------------------\n");
            if (strList.Count > startCount)
            {
                startCount = strList.Count - 1;
            }
        }
    }
    //重置UI位置
    void ResetMenuPos()
    {
        uiTran.position = stereoCameraTran.position;
        uiTran.forward = stereoCameraTran.forward;
        uiTran.eulerAngles = new Vector3(0, uiTran.eulerAngles.y, 0);
    }

    bool isShow = false;
    private void Update()
    {
        if (logText && isShow && startCount>=0 && startCount < strList.Count)
        {
            logText.text = strList[startCount].str + "==============数量：" + strList[startCount].num;
            if (strList[startCount].logType == LogType.Log)
                logText.color = Color.white;
            else
                logText.color = Color.red;
            isShow = false;

            logNumText.text = "log: " + (startCount + 1) + "/" + strList.Count;
        }

        Vector3 cameraPos = new Vector3(stereoCameraTran.position.x, 0, stereoCameraTran.position.z);
        Vector3 uipos = new Vector3(uiTran.position.x, 0, uiTran.position.z);

        //判断距离超过3米或者夹角较大时出现复位按钮
        if (Vector3.Distance(cameraPos, uipos) > 3f || Vector3.Angle(stereoCameraTran.forward, uiTran.forward) > 75f)
        {
            resetMenuBtn.gameObject.SetActive(true);
        }
        else
            resetMenuBtn.gameObject.SetActive(false);
    }
    void Clear()
    {
        startCount = 0;
        isShow = false;
        for (int i = 0; i < strList.Count; i++)
        {
            strList[i].str = null;
            strList[i] = null;
        }
        strList.Clear();
        logText.text = "";
    }
    /// <summary>
    /// 显示日志信息
    /// </summary>
    public Text logText;

    public Text logNumText;

    //位置偏移
    public Button topBtn;
	public Button bottomBtn;

    //内容切换
    public Button lastBtn;
    public Button nextBtn;

    public Button qingkongBtn;
    //内容切换
    void SetLogTextStr(bool islast)
    {
        startCount += islast ? -1 : 1;
        if (startCount < 0)
            startCount = 0;
        if (startCount > strList.Count - 1)
            startCount = strList.Count - 1;

        isShow = true;
    }

    //位置偏移
    void SetLogTextPos(bool isTop)
    {
        if (isTop)
        {
            logText.transform.localPosition += new Vector3(0,200,0);
        }
        else
            logText.transform.localPosition -= new Vector3(0, 200, 0);
    }

    string localLogPath;
	/// <summary>
	/// 程序退出时保存log文件
	/// </summary>
	private void OnApplicationQuit()
	{
		//SaveLogFile();
	}

	#region 获取网络、刷新目标文件实时显示
	static AndroidJavaObject _ajo;
	static AndroidJavaObject AJO
	{
		get
		{
			if (_ajo == null)
				_ajo = new AndroidJavaObject("com.icetone.javatools.MyJavaTools");
			return _ajo;
		}
	}
	/// <summary>
	/// 判断是否有网
	/// </summary>
	/// <returns></returns>
	public static bool IsNet()
	{
		return AJO.Call<bool>("isOnline");
	}

	/// <summary>
	/// 刷新目标路径文件
	/// </summary>
	public static void ScanFile(string filePath)
	{
		AJO.Call("scanFile", filePath);
	}

	#endregion

	void SaveLogFile()
	{
		//获取当前时间
		int hour = DateTime.Now.Hour;
		int minute = DateTime.Now.Minute;
		int second = DateTime.Now.Second;
		int year = DateTime.Now.Year;
		int month = DateTime.Now.Month;
		int day = DateTime.Now.Day;
		string time = string.Format("{0:D2}：{1:D2}：{2:D2}-{3:D4}、{4:D2}、{5:D2} ", hour, minute, second, year, month, day);

		localLogPath += time + "log.txt";
		string direName = Path.GetDirectoryName(localLogPath);

		if (!Directory.Exists(direName)) Directory.CreateDirectory(direName);

		FileStream fs = new FileStream(localLogPath, FileMode.Create);
		StreamWriter sw = new StreamWriter(fs);

		sw.WriteLine(time);
		sw.WriteLine("------------------------------------------------------");
		for (int i = 0; i < strList.Count; i++)
		{
			sw.WriteLine(strList[i].str);
			sw.WriteLine("------------------------------------------------------");
		}

		//清空缓冲区
		sw.Flush();
		//关闭流
		sw.Close();
		sw.Dispose();

		fs.Close();
		fs.Dispose();

		ScanFile(localLogPath);
	}
    
#endregion
    
	#region 测试网络
	/// <summary>
	/// 测试网络
	/// </summary>
	void Start()
	{
		InitLog();
	}
    #endregion
}
