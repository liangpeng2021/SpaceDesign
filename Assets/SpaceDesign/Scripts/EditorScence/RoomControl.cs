using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
/// <summary>
/// 房间控制，/*create by 梁鹏 2021-9-18 */
/// </summary>
public class GameObjectData
{
    public string id;
    public GameObject obj;
    public GameObjectData(string id, GameObject obj)
    {
        this.id = id;
        this.obj = obj;
    }
}

public class RoomControl : MonoBehaviour
{
    [HideInInspector]
    public RoomDatas roomDatas;

    public GameObject pointPrefab;
    /// <summary>
    /// 四个角落
    /// </summary>
    List<Transform> pointTranList = new List<Transform>();

    /// <summary>
    /// 中间的连线
    /// </summary>
    public LineRenderer line;
    /// <summary>
    /// 相机位置
    /// </summary>
    Transform eyeTran;
    /// <summary>
    /// 保存生成的对象
    /// </summary>
    public Transform objParent;
    /// <summary>
    /// 区间
    /// </summary>
    float xmin, xmax, zmin, zmax;
    List<GameObjectData> objList = new List<GameObjectData>();
    
    int curObjIndex=-1;

    public GameObject textMesh;
    /// <summary>
    /// 房间位置，用来实例化物体
    /// </summary>
    Vector3 roomPos;
    /// <summary>
    /// 是否为预览模式
    /// </summary>
    bool isPreview = false;
    
    private void OnDestroy()
    {
        //Debug.Log("删除房间");
        for (int i = 0; i < objList.Count; i++)
        {
            objList[i].id = null;
            Destroy(objList[i].obj);
            objList[i] = null;
        }
        objList.Clear();

        roomDatas.Clear();
        roomDatas = null;

        pointPrefab = null;

        objParent = null;
        line = null;
        eyeTran = null;
        for (int i = 0; i < pointTranList.Count; i++)
        {
            Destroy(pointTranList[i].gameObject);
            pointTranList[i] = null;
        }
        pointTranList.Clear();
        pointPrefab = null;
        textMesh = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (eyeTran == null)
            eyeTran = XRCameraManager.Instance.stereoCamera.transform;
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
    }

    public void CreatePoint()
    {
        float startMax = 5f;
#if UNITY_EDITOR
        startMax = 5f;
#else

#endif
        roomPos = XRCameraManager.Instance.stereoCamera.transform.position+new Vector3(0,-0.5f,0.5f);
        
        textMesh.transform.position = roomPos;

        for (int i = 0; i < 4; i++)
        {
            GameObject obj = Instantiate(pointPrefab,this.transform);
            obj.transform.localScale = Vector3.one * 0.1f;
            obj.transform.localRotation = Quaternion.identity;
            
            pointTranList.Add(obj.transform);
        }
        
        pointTranList[0].position = roomPos + new Vector3(-startMax, 0, -startMax);
        pointTranList[1].position = roomPos + new Vector3(-startMax, 0, startMax);
        pointTranList[2].position = roomPos + new Vector3(startMax, 0, startMax);
        pointTranList[3].position = roomPos + new Vector3(startMax, 0, -startMax);

        roomPos.y += 0.5f;
    }
    /// <summary>
    /// 编辑模式下设置线隐藏和出现
    /// </summary>
    public void SetLineActive(bool active)
    {
        line.gameObject.SetActive(active);
    }

    void LoadPoint(List<SPoint> sPoints)
    {
        textMesh.transform.position = Vector3.zero;
        
        //生成四周锚点
        for (int i = 0; i < sPoints.Count; i++)
        {
            GameObject obj = Instantiate(pointPrefab);

            obj.transform.parent = this.transform;
            obj.transform.localScale = Vector3.one * 0.1f;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.position = new Vector3(sPoints[i].posx, sPoints[i].posy, sPoints[i].posz);
            
            pointTranList.Add(obj.transform);
            //计算提示文字的位置
            textMesh.transform.position += obj.transform.position;
        }
        //放在中心位置
        textMesh.transform.position /= sPoints.Count;
    }

    // Update is called once per frame
    void Update()
    {
        ShowPointObj();
    }

    void ShowPointObj()
    {
        //根据房间角落位置，求出范围区间
        //给初值
        float y = 0;
        if (pointTranList.Count > 0)
        {
            xmin = pointTranList[0].position.x;
            xmax = pointTranList[0].position.x;
            zmin = pointTranList[0].position.z;
            zmax = pointTranList[0].position.z;

            y = pointTranList[0].position.y;
        }
        //求区间
        for (int i = 0; i < pointTranList.Count; i++)
        {
            if (xmin > pointTranList[i].position.x)
                xmin = pointTranList[i].position.x;
            if (xmax < pointTranList[i].position.x)
                xmax = pointTranList[i].position.x;
            if (zmin > pointTranList[i].position.z)
                zmin = pointTranList[i].position.z;
            if (zmax < pointTranList[i].position.z)
                zmax = pointTranList[i].position.z;
        }
        SetObjActive();
        //预览模式
        if (isPreview)
        {
            //预览模式下不画线
            line.gameObject.SetActive(false);
            textMesh.SetActive(false);
        }
        else
        {
            //编辑模式下画线
            line.positionCount = 5;
            line.SetPosition(0, new Vector3(xmin, y, zmin));
            line.SetPosition(1, new Vector3(xmin, y, zmax));
            line.SetPosition(2, new Vector3(xmax, y, zmax));
            line.SetPosition(3, new Vector3(xmax, y, zmin));
            line.SetPosition(4, new Vector3(xmin, y, zmin));

            //是否物体在房间范围内，不在就提示一下
            if (IsObjInRoom())
                textMesh.SetActive(false);
            else
                textMesh.SetActive(true);
        }
        
    }

    void SetObjActive()
    {
        if (eyeTran == null)
            eyeTran = XRCameraManager.Instance.stereoCamera.transform;
        // 判断眼镜是否在房间里
        if (IsInRoom(eyeTran.position))
        {
            if (!isPreview && !EditorControl.Instance.prefabManager.gameObject.activeInHierarchy)
            {
                objParent.gameObject.SetActive(false);
            }
            else
                objParent.gameObject.SetActive(true);
        }
        else
            objParent.gameObject.SetActive(false);
    }

    bool IsObjInRoom()
    {
        for (int i = 0; i < objList.Count; i++)
        {
            if (!IsInRoom(objList[i].obj.transform.position))
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// 判断是否在房间里
    /// </summary>
    bool IsInRoom(Vector3 pos)
    {
        if (pos.x > xmin && pos.x < xmax && pos.z > zmin && pos.z < zmax)
        {
            return true;
        }

        return false;
    }

    public void CreatePrefab3D(Vector3 pos, string id, GameObject prefab3d)
    {
        try
        {
            GameObject obj = Instantiate(prefab3d);

            obj.transform.parent = objParent;
            obj.transform.position = eyeTran.position+new Vector3(0,0,0.8f);
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            GameObjectData gameObjectData = new GameObjectData(id, obj);

            objList.Add(gameObjectData);
            //更新选中状态，赋值索引
            SetShowObjNum(objList.Count-1);
            obj.GetComponent<ChangeSate>().index = objList.Count - 1;
        }
        catch (Exception e)
        {
            Debug.Log("MyLog::实例化3D预设："+e);
        }
    }

    void LoadPrefab3D(List<ObjectData> ObjectList,Dictionary<string,GameObject> prefabDics)
    {
        try
        {
            //实例化房间内的物体
            for (int i = 0; i < ObjectList.Count; i++)
            {
                if (prefabDics.ContainsKey(ObjectList[i].id))
                {
                    //生成物体
                    GameObject obj = Instantiate(prefabDics[ObjectList[i].id]);
                    obj.transform.parent = objParent;
                    ObjectData objectData = ObjectList[i];
                    obj.transform.position = new Vector3(objectData.posx, objectData.posy, objectData.posz);
                    obj.transform.eulerAngles = new Vector3(objectData.eulerx, objectData.eulery, objectData.eulerz);
                    obj.transform.localScale = new Vector3(objectData.scalex, objectData.scaley, objectData.scalez);
                    //对应的UI变为不可点击
                    EditorControl.Instance.prefabManager.DisableObj2DImage(roomDatas.ObjectList[i].id);

                    GameObjectData gameObjectData = new GameObjectData(ObjectList[i].id, obj);

                    objList.Add(gameObjectData);
                    //赋值索引
                    obj.GetComponent<ChangeSate>().index = objList.Count - 1;
                }
            }
            
            //更新选中状态，赋值索引
            if (objList.Count>0)
                SetShowObjNum(objList.Count - 1);
        }
        catch (Exception e)
        {
            Debug.Log("MyLog::实例化3D预设：" + e);
        }
    }

    /// <summary>
    /// 根据索引设置选中状态
    /// </summary>
    /// <param name="index"></param>
    public void SetShowObjNum(int index)
    {
        if (index != -1)
        {
            if (index==curObjIndex)
                return;
            
            if (curObjIndex != -1)
                objList[curObjIndex].obj.GetComponent<ChangeSate>().HightLightOff();
            curObjIndex = index;

            objList[curObjIndex].obj.GetComponent<ChangeSate>().HightLightOn();
        }
    }

    public string RemoveObj()
    {
        string id = null;
        if (curObjIndex == -1)
        {
            //Debug.Log("缺少curprefabID");
            return id;
        }

        id = objList[curObjIndex].id;
        Destroy(objList[curObjIndex].obj);
        objList[curObjIndex].id = null;
        objList[curObjIndex] = null;
        objList.RemoveAt(curObjIndex);
        curObjIndex = objList.Count - 1;
        if (curObjIndex!=-1)
            objList[curObjIndex].obj.GetComponent<ChangeSate>().HightLightOn();
        
        return id;
    }

    public void ResetObjUI()
    {
        PrefabManager prefabManager = EditorControl.Instance.prefabManager;
        for (int i = 0; i < objList.Count; i++)
        {
            prefabManager.ResetObjUI(objList[i].id);
        }
    }
    
    public void SaveData()
    {
        roomDatas.ObjectList.Clear();
        roomDatas.sPointsList.Clear();

        for (int i = 0; i < objList.Count; i++)
        {
            ObjectData objectData = new ObjectData();

            objectData.id = objList[i].id;
            objectData.posx = objList[i].obj.transform.position.x;
            objectData.posy = objList[i].obj.transform.position.y;
            objectData.posz = objList[i].obj.transform.position.z;
            objectData.eulerx = objList[i].obj.transform.eulerAngles.x;
            objectData.eulery = objList[i].obj.transform.eulerAngles.y;
            objectData.eulerz = objList[i].obj.transform.eulerAngles.z;
            objectData.scalex = objList[i].obj.transform.localScale.x;
            objectData.scaley = objList[i].obj.transform.localScale.y;
            objectData.scalez = objList[i].obj.transform.localScale.z;

            roomDatas.ObjectList.Add(objectData);
        }
        
        //Debug.Log(roomDatas.ObjectList.Count);

        for (int i = 0; i < pointTranList.Count; i++)
        {
            SPoint sPoint = new SPoint();
            sPoint.posx = pointTranList[i].position.x;
            sPoint.posy = pointTranList[i].position.y;
            sPoint.posz = pointTranList[i].position.z;

            roomDatas.sPointsList.Add(sPoint);
        }
        //Debug.Log(roomDatas.sPointsList.Count);
    }

    public void SetRoomFromData(RoomDatas roomDatas, Dictionary<string, GameObject> prefabDics)
    {
        LoadPoint(roomDatas.sPointsList);

        LoadPrefab3D(roomDatas.ObjectList, prefabDics);

        SetObjActive();
    }

    /// <summary>
    /// 根据房间数据设置和生成物体和锚点
    /// </summary>
    /// <param name="roomDatas"></param>
    public void SetRoomData(RoomDatas roomDatas, Dictionary<string,GameObject> prefabDics)
    {
        //是否为预览模式
        isPreview = true;

        //根据房间角落位置，求出范围区间
        //给初值
        float y = 0;
        for (int i = 0; i < roomDatas.sPointsList.Count; i++)
        {
            if (i == 0)
            {
                xmin = roomDatas.sPointsList[i].posx;
                xmax = roomDatas.sPointsList[i].posx;
                zmin = roomDatas.sPointsList[i].posz;
                zmax = roomDatas.sPointsList[i].posz;

                y = roomDatas.sPointsList[i].posy;
            }
            //求区间
            if (xmin > roomDatas.sPointsList[i].posx)
                xmin = roomDatas.sPointsList[i].posx;
            if (xmax < roomDatas.sPointsList[i].posx)
                xmax = roomDatas.sPointsList[i].posx;
            if (zmin > roomDatas.sPointsList[i].posz)
                zmin = roomDatas.sPointsList[i].posz;
            if (zmax < roomDatas.sPointsList[i].posz)
                zmax = roomDatas.sPointsList[i].posz;
        }

        //实例化房间内的物体
        for (int i = 0; i < roomDatas.ObjectList.Count; i++)
        {
            if (prefabDics.ContainsKey(roomDatas.ObjectList[i].id))
            {
                //生成物体
                GameObject obj = Instantiate(prefabDics[roomDatas.ObjectList[i].id]);
                obj.transform.parent = objParent;
                ObjectData objectData = roomDatas.ObjectList[i];
                obj.transform.position = new Vector3(objectData.posx, objectData.posy, objectData.posz);
                obj.transform.eulerAngles = new Vector3(objectData.eulerx, objectData.eulery, objectData.eulerz);
                obj.transform.localScale = new Vector3(objectData.scalex, objectData.scaley, objectData.scalez);
            }
        }
    }
}
