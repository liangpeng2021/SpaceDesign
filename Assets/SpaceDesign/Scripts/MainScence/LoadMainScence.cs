using OXRTK.ARHandTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XR;

/// <summary>
/// 根据配置文件加载主场景 /*create by 梁鹏 2021-10-9 */
/// </summary>
public class LoadMainScence : MonoBehaviour
{
    public Prefab3D[] prefab3Ds;

    Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();

    ScenceData objectDatas;

    public GameObject roomPrefab;

    public Button backto;

    public Canvas[] canvas;

    #region 拉起其他应用
    /// <summary>
    /// 奇幻森林按钮
    /// </summary>
    public ButtonRayReceiver artowermotionBtn;
    /// <summary>
    /// 忒依亚传说
    /// </summary>
    public ButtonRayReceiver omobaBtn;
    /// <summary>
    /// AR动物园
    /// </summary>
    public ButtonRayReceiver findanimalsBtn;

    /// <summary>
    /// 测试demo
    /// </summary>
    public ButtonRayReceiver testBtn;

    /// <summary>
    /// 奇幻森林
    /// </summary>
    void ClickLaqiArtowermotion()
    {
        AppManager.StartApp("com.gabor.artowermotion");
    }
    /// <summary>
    /// 忒依亚传说（moba)
    /// </summary>
    void ClickLaqiOmoba()
    {
        AppManager.StartApp("com.baymax.omoba");
    }
    /// <summary>
    /// AR动物园
    /// </summary>
    void ClickLaqiFindanimals()
    {
        AppManager.StartApp("com.xyani.findanimals");
        //Debug.Log("MyLog::ClickLaqi:"+ CallApp("com.LenQiy.TestInput"));
    }

    void ClickTestInput()
    {
        AppManager.StartApp("com.LenQiy.TestInput");
    }

    void AddLaqiEvent()
    {
        findanimalsBtn.onPinchDown.AddListener(ClickLaqiFindanimals);
        omobaBtn.onPinchDown.AddListener(ClickLaqiOmoba);
        artowermotionBtn.onPinchDown.AddListener(ClickLaqiArtowermotion);
        testBtn.onPinchDown.AddListener(ClickTestInput);
    }

    void RemoveLaqiEvent()
    {
        artowermotionBtn.onPinchDown.RemoveAllListeners();
        omobaBtn.onPinchDown.RemoveAllListeners();
        findanimalsBtn.onPinchDown.RemoveAllListeners();
        testBtn.onPinchDown.RemoveAllListeners();
    }

    #endregion

    private void OnEnable()
    {
        backto.onClick.AddListener(QuitApp);
        AddLaqiEvent();
    }

    private void OnDisable()
    {
        backto.onClick.RemoveListener(QuitApp);
        RemoveLaqiEvent();
    }

    void BackToLoginScence()
    {
        SceneManager.LoadScene(0);
    }

    void QuitApp()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        roomPrefab = null;
        
        objectDatas.Clear();
        objectDatas = null;

        prefabDic.Clear();

        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefab3Ds[i].id = null;
            prefab3Ds[i].prefab3d = null;
            prefab3Ds[i] = null;
        }

        backto = null;

        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i] = null;
        }
        canvas = null;

        artowermotionBtn = null;
        omobaBtn = null;
        findanimalsBtn = null;
    }

    private void Start()
    {
        Camera eventCamera = XRCameraManager.Instance.eventCamera;
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i].worldCamera = eventCamera;
        }

        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefabDic.Add(prefab3Ds[i].id, prefab3Ds[i].prefab3d);
        }
        LoadGameObjectData();
    }
    
    void LoadGameObjectData()
    {
        //BitConverter方式
        string path = EditorControl.GetPth();
        path = Path.Combine(path, "scence.scn");
        //Debug.Log("MyLog::path" + path);
        objectDatas = MyDeSerial(path);

        if (objectDatas == null)
        {
            return;
        }

        for (int i = 0; i < objectDatas.roomDatasList.Count; i++)
        {
            GameObject obj = Instantiate(roomPrefab);
            RoomControl roomControl = obj.GetComponent<RoomControl>();
            if (objectDatas.roomDatasList[i] != null)
                roomControl.SetRoomData(objectDatas.roomDatasList[i], prefabDic);
            else
            {
                Debug.Log("MyLog::第" + i.ToString()+"房间为空");
            }
        }
        Debug.Log("MyLog::objectDatas.roomDatasList.Count:" + objectDatas.roomDatasList.Count);
    }

    ScenceData MyDeSerial(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }
        ScenceData gameObjectDatas = null;
        using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            gameObjectDatas = bf.Deserialize(fileStream) as ScenceData;
        }

        return gameObjectDatas;
    }

    //检测是否需要切换到编辑模式
    int clickNum;
    float timeCount;
    private void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount < 3f)
        {
            if (XRInput.Instance.GetMouseButtonDown(0) || Input.GetMouseButtonDown(0))
            {
                clickNum++;
            }
            //1秒内连续点击超过3次，进入编辑模式
            if (clickNum > 5)
            {
                timeCount = 0;
                clickNum = 0;
                SceneManager.LoadScene("EditorScence");
            }
        }
        else
        {
            timeCount = 0;
            clickNum = 0;
        }
    }
}
