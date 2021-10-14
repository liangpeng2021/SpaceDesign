using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// /*create by 梁鹏 2021-9-13，根据配置文件加载场景 */
/// </summary>
[System.Serializable]
public class Prefab3D
{
    public string id;
    public GameObject prefab3d;
    public void Clear()
    {
        id = null;
        prefab3d = null;
    }
}

public class LoadPreviewScence : MonoBehaviour
{
    /// <summary>
    /// 生成的对象的父节点
    /// </summary>
    public Transform ObjParent;

    public Prefab3D[] prefab3Ds;

    Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>(); 
    
    public GameObject roomPrefab;
    
    private void OnDestroy()
    {
        roomPrefab = null;
        
        prefabDic.Clear();
        prefabDic = null;

        ObjParent = null;
        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefab3Ds[i].id = null;
            prefab3Ds[i].prefab3d = null;
            prefab3Ds[i] = null;
        }
    }

    private void Start()
    {
        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefabDic.Add(prefab3Ds[i].id, prefab3Ds[i].prefab3d);
        }
    }

    public void ClearChild()
    {
        int childCount = ObjParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(ObjParent.GetChild(0).gameObject);
        }
    }
    public void LoadGameObjectData(ScenceData objectDatas)
    {
        //BitConverter方式
        //string path = EditorControl.GetPth();
        //path = Path.Combine(path, "scence.scn");

        //objectDatas = MyDeSerial(path);

        if (objectDatas == null)
        {
            Debug.Log("MyLog::数据为空");
        }

        //Debug.Log(objectDatas.roomDatasList.Count);

        for (int i = 0; i < objectDatas.roomDatasList.Count; i++)
        {
            GameObject obj = Instantiate(roomPrefab);
            RoomControl roomControl = obj.GetComponent<RoomControl>();
            obj.transform.parent = ObjParent;
            roomControl.SetRoomData(objectDatas.roomDatasList[i], prefabDic);
        }
    }

    /// <summary>
    /// 反序列化（读取path路径下的文件），将数据从文件读取出来
    /// </summary>
    public static ScenceData MyDeSerial(string path)
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
    
}
