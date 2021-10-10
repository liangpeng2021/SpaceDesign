using UnityEngine;
using OXRTK.ARHandTracking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.UI;
using XR;
using UnityEngine.SceneManagement;

/// <summary>
/// 控制编辑场景界面切换，/*create by 梁鹏 2021-9-14 */
/// </summary>
public class EditorControl : MonoBehaviour
{
    UserDataSupport yoopUserData;

    #region 赋值相机

    public Transform uiTran;
    public Canvas[] canvas;

    void InitCanvas()
    {
        uiTran.parent = XRCameraManager.Instance.stereoCamera.transform;
        uiTran.localEulerAngles = Vector3.zero;
        uiTran.localPosition = Vector3.zero;
        uiTran.localScale = Vector3.one;

        Camera eventCamera = XRCameraManager.Instance.eventCamera;
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i].worldCamera = eventCamera;
        }
    }

    #endregion

    public static EditorControl Instance;

    public GameObject editorUIObj;
    public GameObject previewUIObj;

    /// <summary>
    /// 预览
    /// </summary>
    public ButtonRayReceiver previewBtn;

    /// <summary>
    /// 保存
    /// </summary>
    public ButtonRayReceiver saveBtn;

    /// <summary>
    /// 返回编辑模式
    /// </summary>
    public ButtonRayReceiver backBtn;

    /// <summary>
    /// 退出程序
    /// </summary>
    public ButtonRayReceiver quitBtn;
    
    LoadScence loadScence;
    
    /// <summary>
    /// 预览模式父节点
    /// </summary>
    public Transform previewParent;
    /// <summary>
    /// 预设管理
    /// </summary>
    public PrefabManager prefabManager;
    /// <summary>
    /// 房间管理
    /// </summary>
    public RoomManager roomManager;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        loadScence = GetComponent<LoadScence>();
        BackToEditor();
        ToEditRoom();
        InitSave();
        InitTip();

        previewBtn.gameObject.SetActive(false);
        saveBtn.gameObject.SetActive(false);

        InitCanvas();

        yoopUserData = DontDestroyInstance.instance.transform.Find("UserDataSupport").GetComponent<UserDataSupport>();
    }
    private void OnEnable()
    {
        previewBtn.onPinchDown.AddListener(LoadPreview);
        saveBtn.onPinchDown.AddListener(SaveScenceData);
        backBtn.onPinchDown.AddListener(BackToEditor);
        quitBtn.onPinchDown.AddListener(QuitScence);
    }

    private void OnDisable()
    {
        previewBtn.onPinchDown.RemoveListener(LoadPreview);
        saveBtn.onPinchDown.RemoveListener(SaveScenceData);
        backBtn.onPinchDown.RemoveListener(BackToEditor);
        quitBtn.onPinchDown.RemoveListener(QuitScence);
    }
    
    public void LoadPreview()
    {
        roomManager.editorParent.gameObject.SetActive(false);
        previewParent.gameObject.SetActive(true);

        editorUIObj.SetActive(false);
        previewUIObj.SetActive(true);

        loadScence.LoadGameObjectData();
    }

    public void BackToEditor()
    {
        roomManager.editorParent.gameObject.SetActive(true);
        previewParent.gameObject.SetActive(false);

        editorUIObj.SetActive(true);
        previewUIObj.SetActive(false);

        loadScence.ClearChild();
    }
    
    void QuitScence()
    {
        uiTran.parent = null;
        SceneManager.LoadScene(0);
        //Application.Quit();
    }
    /// <summary>
    /// 切换到房间管理
    /// </summary>
    public void ToEditRoom()
    {
        prefabManager.gameObject.SetActive(false);
        roomManager.gameObject.SetActive(true);
    }
    /// <summary>
    /// 切换到对象管理
    /// </summary>
    public void ToEditObj()
    {
        prefabManager.gameObject.SetActive(true);
        roomManager.gameObject.SetActive(false);
    }

    #region 保存数据
    /// <summary>
    /// 保存数据的间隔时间
    /// </summary>
    public float saveInternalTime = 3f;

    float timeCount = 0;
    [HideInInspector]
    public string path;
    void InitSave()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            //动态请求安装权限
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.WRITE_EXTERNAL_STORAGE");
#endif
        ////获取当前时间
        //int hour = DateTime.Now.Hour;
        //int minute = DateTime.Now.Minute;
        //int second = DateTime.Now.Second;
        //int year = DateTime.Now.Year;
        //int month = DateTime.Now.Month;
        //int day = DateTime.Now.Day;

        ////格式化显示当前时间
        //string timestr= string.Format("{0:D2}-{1:D2}-{2:D2} " + "{3:D4}-{4:D2}-{5:D2}", hour, minute, second, year, month, day);
        path = GetPth();
        path = Path.Combine(path,"scence.scn");
    }

    void SaveScenceData()
    {
        roomManager.SaveRoomData();
        //BitConverter方式
        MySerial(path, roomManager.scenceData);
        previewBtn.gameObject.SetActive(true);
        saveBtn.gameObject.SetActive(false);
    }
    /// <summary>
    /// 有修改，保存按钮出现，预览按钮消失
    /// </summary>
    public void HasChange()
    {
        saveBtn.gameObject.SetActive(true);
        previewBtn.gameObject.SetActive(false);
    }

    /// <summary>
    /// 序列化（存储path路径下的文件），将数据存储到文件
    /// </summary>
    void MySerial(string path, ScenceData gameObjectDatas)
    {
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, gameObjectDatas);

        fs.Flush();
        fs.Close();
        fs.Dispose();
    }

    /// <summary>
    /// 获取配置文件的路径
    /// </summary>
    public static string GetPth()
    {
        string path;
#if UNITY_ANDROID && !UNITY_EDITOR
                path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android", StringComparison.Ordinal));
#else
        path = Application.streamingAssetsPath.Substring(0, Application.streamingAssetsPath.IndexOf("opporoom", StringComparison.Ordinal));
#endif
        path= Path.Combine(path, "LenQiy", "Scences");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }
    #endregion

    private void Update()
    {
        //SaveScenceData();

        TestInput();
    }

    #region 提示
    public GameObject tipObj;
    Text tipText;

    void InitTip()
    {
        HideTip();
        if (tipText == null)
            tipText = tipObj.transform.GetChild(0).GetComponent<Text>();
    }

    public void ShowTipKeep(string str)
    {
        tipObj.SetActive(true);
        
        tipText.text = str;
    }

    public void ShowTipTime(string str,float time)
    {
        ShowTipKeep(str);

        Invoke("HideTip", time);
    }

    void HideTip()
    {
        tipObj.SetActive(false);
    }

    void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("MyLog::KeyCode.A");
            ShowTipTime("A", 2f);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("MyLog::KeyCode.S");
            ShowTipTime("S", 2f);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("MyLog::KeyCode.W");
            ShowTipTime("W", 2f);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("MyLog::KeyCode.D");
            ShowTipTime("D", 2f);
        } 
    }
    #endregion
}
