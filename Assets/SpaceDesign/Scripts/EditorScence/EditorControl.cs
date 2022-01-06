using UnityEngine;
using OXRTK.ARHandTracking;
using System.IO;

using UnityEngine.UI;
using XR;
using UnityEngine.SceneManagement;
using SpaceDesign;

/// <summary>
/// 控制编辑场景界面切换，/*create by 梁鹏 2021-9-14 */
/// </summary>
public class EditorControl : MonoBehaviour
{
    #region 赋值相机

    public Transform uiTran;
    public Canvas[] canvas;

    void InitCanvas()
    {
        resetMenuParent = resetMenuBtn.transform.parent.parent;

        stereoCameraTran = XRCameraManager.Instance.stereoCamera.transform;

        resetMenuParent.parent = stereoCameraTran;
        resetMenuParent.localEulerAngles = Vector3.zero;
        resetMenuParent.localPosition = Vector3.zero;
        resetMenuParent.localScale = Vector3.one;

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
    /// 菜单，点击复位所有按钮
    /// </summary>
    public ButtonRayReceiver resetMenuBtn;
    Transform resetMenuParent;
    Transform stereoCameraTran;

    /// <summary>
    /// 预览
    /// </summary>
    public ButtonRayReceiver previewBtn;

    /// <summary>
    /// 返回编辑模式
    /// </summary>
    public ButtonRayReceiver backToEditorBtn;
    
    [HideInInspector]
    public LoadPreviewScence loadPreviewScence;

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
    /// <summary>
    /// 场景管理
    /// </summary>
    public ScenceManager scenceManager;
    /// <summary>
    /// 返回到上一级目录
    /// </summary>
    public ButtonRayReceiver backtoTopBtn;

    /// <summary>
    /// 键盘管理
    /// </summary>
    public KeyBoardManager keyBoardManager;

    /// <summary>
    /// 创建时起名
    /// </summary>
    public SetName setName;
    [HideInInspector]
    public Color chooseColor;
    [HideInInspector]
    public Color normalColor;

    private void OnDestroy()
    {
        uiTran = null;
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i] = null;
        }

        editorUIObj = null;
        previewUIObj = null;
        previewBtn = null;
        backToEditorBtn = null;
        loadPreviewScence = null;
        previewParent = null;
        prefabManager = null;
        roomManager = null;
        scenceManager = null;
        backtoTopBtn = null;
        keyBoardManager = null;
        setName = null;
        path = null;
        tipObj = null;
        tipText = null;

        Instance = null;
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        ColorUtility.TryParseHtmlString("#6070FFFF", out chooseColor);
        ColorUtility.TryParseHtmlString("#6B00B0FF", out normalColor);

        loadPreviewScence = GetComponent<LoadPreviewScence>();
        
        InitSave();
        InitTip();
        
        InitCanvas();

        ToEditRoom();
        BackToScenceList();

        prefabManager.InitPrefabDic();
    }
    private void OnEnable()
    {
        previewBtn.onPinchDown.AddListener(LoadPreview);
        backToEditorBtn.onPinchDown.AddListener(BackToEditor);
        
        backtoTopBtn.onPinchDown.AddListener(BackToScenceList);
        resetMenuBtn.onPinchDown.AddListener(ResetMenuPos);
    }

    private void OnDisable()
    {
        previewBtn.onPinchDown.RemoveListener(LoadPreview);
        backToEditorBtn.onPinchDown.RemoveListener(BackToEditor);
        
        backtoTopBtn.onPinchDown.RemoveListener(BackToScenceList);
        resetMenuBtn.onPinchDown.RemoveAllListeners();
    }

    void ResetMenuPos()
    {
        uiTran.position = stereoCameraTran.position;
        uiTran.forward = stereoCameraTran.forward;
        uiTran.eulerAngles = new Vector3(0, uiTran.eulerAngles.y,0);
    }

    void Update()
    {
        Vector3 cameraPos = new Vector3(stereoCameraTran.position.x,0, stereoCameraTran.position.z);
        Vector3 uipos = new Vector3(uiTran.position.x,0,uiTran.position.z);
        
        //判断距离超过3米或者夹角较大时出现复位按钮
        if (Vector3.Distance(cameraPos, uipos)>3f || Vector3.Angle(stereoCameraTran.forward, uiTran.forward) >75f)
        {
            resetMenuParent.gameObject.SetActive(true);
        }
        else
            resetMenuParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 返回到场景列表目录
    /// </summary>
    void BackToScenceList()
    {
        roomManager.SaveRoomData();
        scenceManager.gameObject.SetActive(true);
        roomManager.gameObject.SetActive(false);
        roomManager.editorParent.gameObject.SetActive(false);
    }
   
    /// <summary>
    /// 点击预览按钮
    /// </summary>
    public void LoadPreview()
    {
        if (roomManager.ScenceData == null)
            return;

        roomManager.SaveRoomData();
        roomManager.editorParent.gameObject.SetActive(false);
        previewParent.gameObject.SetActive(true);

        editorUIObj.SetActive(false);
        previewUIObj.SetActive(true);

        loadPreviewScence.LoadGameObjectData(roomManager.ScenceData);
    }

    public void BackToEditor()
    {
        roomManager.editorParent.gameObject.SetActive(true);
        previewParent.gameObject.SetActive(false);

        editorUIObj.SetActive(true);
        previewUIObj.SetActive(false);

        loadPreviewScence.ClearChild();
    }
    
    void QuitScence()
    {
        Application.Quit();
    }

    void BackToStarScence()
    {
        //------------ Modify by zh ------------
        PlayerManage.InitPlayerPosEvt();
        //------------------End------------------
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 切换到房间管理
    /// </summary>
    public void ToEditRoom()
    {
        scenceManager.gameObject.SetActive(false);
        prefabManager.gameObject.SetActive(false);
        roomManager.gameObject.SetActive(true);
        roomManager.editorParent.gameObject.SetActive(true);
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
        path = PathConfig.GetPth();
        path = Path.Combine(path,"scence.scn");
    }

    #endregion

    #region 提示
    public GameObject tipObj;
    Text tipText;

    void InitTip()
    {
        HideTip();
    }

    public void ShowTipKeep(string str)
    {
        tipObj.SetActive(true);
        if (tipText == null)
            tipText = tipObj.transform.GetChild(0).GetComponent<Text>();
        tipText.text = str;
    }

    public void ShowTipTime(string str,float time)
    {
        ShowTipKeep(str);

        Invoke("HideTip", time);
    }

    public void HideTip()
    {
        tipObj.SetActive(false);
    }
    #endregion
}
