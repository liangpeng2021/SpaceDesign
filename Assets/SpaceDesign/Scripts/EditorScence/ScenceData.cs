using OXRTK.ARHandTracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// /*create by 梁鹏 2021-9-9 */，加载预设
/// </summary>
//预设数据
[System.Serializable]
public struct PrefabData
{
    //id
    public string id;
    //2D预设
    public GameObject Obj2D;
    //3D预设
    public GameObject prefab3D;

    public void Clear()
    {
        id = null;
        Obj2D = null;
        prefab3D = null;
    }
}
[System.Serializable]
public class ScenceData
{
    public string Label;
    public List<RoomDatas> roomDatasList = new List<RoomDatas>();
    public void Clear()
    {
        Label = null;
        if (roomDatasList != null)
        {
            for (int i = 0; i < roomDatasList.Count; i++)
            {
                roomDatasList[i].Clear();
            }
            roomDatasList.Clear();
            roomDatasList = null;
        }
    }
}

[System.Serializable]
public class RoomDatas
{
    public string roomName;
    public List<SPoint> sPointsList = new List<SPoint>();
    public List<ObjectData> ObjectList = new List<ObjectData>();
    public void Clear()
    {
        roomName = null;
        if (sPointsList != null)
        {
            sPointsList.Clear();
            sPointsList = null;
        }

        if (ObjectList != null)
        {
            for (int i = 0; i < ObjectList.Count; i++)
            {
                ObjectList[i].Clear();
            }
            ObjectList.Clear();
            ObjectList = null;
        }
    }
}
[System.Serializable]
public struct SPoint
{
    public float posx;
    public float posy;
    public float posz;
}

/// <summary>
/// 要保存的对象数据
/// </summary>
[System.Serializable]
public struct ObjectData
{
    public string id;

    public float posx;
    public float posy;
    public float posz;

    public float eulerx;
    public float eulery;
    public float eulerz;

    public float scalex;
    public float scaley;
    public float scalez;

    public void Clear()
    {
        id = null;
    }
}

