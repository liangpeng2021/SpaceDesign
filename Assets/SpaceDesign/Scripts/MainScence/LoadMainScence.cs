using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public ButtonRayReceiver backto;

    public Canvas[] canvas;

    private void OnEnable()
    {
        backto.onPinchDown.AddListener(BackToLoginScence);
    }

    private void OnDisable()
    {
        backto.onPinchDown.RemoveListener(BackToLoginScence);
    }

    void BackToLoginScence()
    {
        SceneManager.LoadScene("LoginScence");
    }

    private void OnDestroy()
    {
        roomPrefab = null;

        if (objectDatas != null)
        {
            for (int i = 0; i < objectDatas.roomDatasList.Count; i++)
            {
                objectDatas.roomDatasList[i].sPointsList.Clear();

                objectDatas.roomDatasList[i].ObjectList.Clear();
            }
        }

        objectDatas = null;

        prefabDic.Clear();
        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefab3Ds[i].id = null;
            prefab3Ds[i].prefab3d = null;
            prefab3Ds[i] = null;
        }

        backto = null;
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

        objectDatas = LoadScence.MyDeSerial(path);

        if (objectDatas == null)
        {
            Debug.Log("MyLog::数据为空");
        }
        
        for (int i = 0; i < objectDatas.roomDatasList.Count; i++)
        {
            GameObject obj = Instantiate(roomPrefab);
            RoomControl roomControl = obj.GetComponent<RoomControl>();
            roomControl.SetRoomData(objectDatas.roomDatasList[i], prefabDic);
        }
    }
}
