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
        //Debug.Log("Label:"+Label+ "Clear");
        if (roomDatasList != null)
        {
            for (int i = 0; i < roomDatasList.Count; i++)
            {
                roomDatasList[i].Clear();
            }
            roomDatasList.Clear();
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
        if (sPointsList != null)
        {
            sPointsList.Clear();
        }

        if (ObjectList != null)
        {
            ObjectList.Clear();
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
}

//------------ Modify by zh ------------
/// <summary>
/// Icon触点的触发距离数据
/// </summary>
public  class IconDisData
{
    //植物
    public float PlanteFar = 5f;
    public float PlanteMiddle = 0f;
    //CPE信息显示（灯泡控制）
    public float ShowCPEFar = 5f;
    public float ShowCPEMiddle = 0f;
    //视频通话
    public float PhoneFar = 5f;
    public float PhoneCalling = 1.5f;
    public float PhoneMissAndReCall = 2f;
    public float PhoneTalking = 4f;
    //京东台灯购买
    public float TaiDengFar = 5f;
    public float TaiDengMiddle = 2f;
    //书籍
    public float MagazineFar = 5f;
    public float MagazineMiddle = 2.5f;
    //翻译
    public float TranslateFar = 5f;
    public float TranslateMiddle = 2.5f;
    //视频（电视、2D、3D容积）
    public float VideoFar = 5f;
    public float VideoMiddle = 3.5f;
    //音乐
    public float MusicFar = 5f;
    public float MusicMiddle = 0f;
    //桌面游戏
    public float DeskGameFar = 5f;
    public float DeskGameMiddle = 0f;
    //虚拟人教练
    public float VirtualCoachFar = 5f;
    public float VirtualCoachMiddle = 3f;
    //卡丁车（单车，骑车）
    public float KartingFar = 5f;
    public float KartingMiddle = 2f;
    //跳操（大屏跳操）
    public float AerobicsFar = 5f;
    public float AerobicsMiddle = 2f;
    //冰箱
    public float BingXiangFar = 5f;
    public float BingXiangMiddle = 3f;
    //菜谱（厨房）
    public float ChuFangFar = 5f;
    public float ChuFangMiddle = 2f;
}
//------------------End------------------