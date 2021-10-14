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

    #region test
    public Button testlaqi;
    void ClickLaqi()
    {
        Debug.Log("MyLog::ClickLaqi:"+ CallApp("com.LenQiy.TestInput"));
    }

    public bool CallApp(string packageName)
    {
        try
        {
            using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject curActivity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packMag = curActivity.Call<AndroidJavaObject>("getPackageManager"))
            using (AndroidJavaObject intent = packMag.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName))
            {
                Debug.Log("MyLog::intent:"+ intent);
                if (intent != null)
                {
                    curActivity.Call("startActivity", intent);
                    return true;
                }
            }
            Debug.Log("MyLog::未执行");
            return false;
            //debugText.text += "\n拉起成功";
        }
        catch
        {
            Debug.Log("MyLog::using失败");
            return false;
        }
    }
    #endregion

    private void OnEnable()
    {
        backto.onClick.AddListener(QuitApp);
        testlaqi.onClick.AddListener(ClickLaqi);
    }

    private void OnDisable()
    {
        backto.onClick.RemoveListener(QuitApp);
        testlaqi.onClick.RemoveListener(ClickLaqi);
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
        Debug.Log("MyLog::path" + path);
        objectDatas = MyDeSerial(path);

        if (objectDatas == null)
        {
            Debug.Log("MyLog::配置数据为空");
            return;
        }
        Debug.Log("MyLog::objectDatas："+ objectDatas);
        for (int i = 0; i < objectDatas.roomDatasList.Count; i++)
        {
            GameObject obj = Instantiate(roomPrefab);
            RoomControl roomControl = obj.GetComponent<RoomControl>();
            roomControl.SetRoomData(objectDatas.roomDatasList[i], prefabDic);
        }
        Debug.Log("MyLog::objectDatas.roomDatasList.Count:" + objectDatas.roomDatasList.Count);
    }

    ScenceData MyDeSerial(string path)
    {
        if (!File.Exists(path))
        {
            Debug.Log("MyLog::!File.Exists(path)：null");
            return null;
        }
        ScenceData gameObjectDatas = null;
        using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
        {
            Debug.Log("MyLog::fileStream:"+ fileStream);
            BinaryFormatter bf = new BinaryFormatter();
            gameObjectDatas = bf.Deserialize(fileStream) as ScenceData;
            Debug.Log("MyLog::gameObjectDatas.Label:" + gameObjectDatas.Label);
            Debug.Log("MyLog::gameObjectDatas.roomDatasList:" + gameObjectDatas.roomDatasList);
        }

        return gameObjectDatas;
    }

    //检测是否需要切换到编辑模式
    int clickNum;
    float timeCount;
    private void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount < 1f)
        {
            if (XRInput.Instance.GetMouseButtonDown(0) || Input.GetMouseButtonDown(0))
            {
                clickNum++;
            }
            //1秒内连续点击超过3次，进入编辑模式
            if (clickNum > 2)
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
