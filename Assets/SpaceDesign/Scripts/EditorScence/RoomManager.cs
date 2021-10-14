using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 房间管理和物体管理,/*create by 梁鹏 2021-9-18 */
/// </summary>
public class RoomManager : MonoBehaviour
{
    /// <summary>
    /// 编辑模式3D物体父节点
    /// </summary>
    public Transform editorParent;
    
    #region 房间管理
    /// <summary>
    /// 房间列表界面
    /// </summary>
    public Transform roomIconParentTran;
    /// <summary>
    /// 2D房间预设
    /// </summary>
    public GameObject room2DPrefab;

    /// <summary>
    /// 房间预设
    /// </summary>
    public GameObject room3DPrefab;
    /// <summary>
    /// 当前在编辑的房间
    /// </summary>
    [HideInInspector]
    public int curRoomListId=-1;
    /// <summary>
    /// 创建房间
    /// </summary>
    public ButtonRayReceiver createRoomBtn;

    /// <summary>
    /// 编辑房间
    /// </summary>
    public ButtonRayReceiver editRoomBtn;

    /// <summary>
    /// 删除房间
    /// </summary>
    public ButtonRayReceiver deleteRoomBtn;
    
    //int roomNum=0;
    [HideInInspector]
    public List<RoomControl> room3DList =new List<RoomControl>();
    List<Image> roomIconList = new List<Image>();

    /// <summary>
    /// 场景数据
    /// </summary>
    ScenceData curScenceData;

    public ScenceData ScenceData
    {
        get
        {
            return curScenceData;
        }
        set
        {
            curScenceData = value;
        }
    }

    void AddRoomEvent()
    {
        createRoomBtn.onPinchDown.AddListener(Create2DRoom);
        editRoomBtn.onPinchDown.AddListener(EditObj);
        deleteRoomBtn.onPinchDown.AddListener(DeleteRoom);
    }

    void RemoveRoomEvent()
    {
        createRoomBtn.onPinchDown.RemoveListener(Create2DRoom);
        editRoomBtn.onPinchDown.RemoveListener(EditObj);
        deleteRoomBtn.onPinchDown.RemoveListener(DeleteRoom);
    }
    /// <summary>
    /// 去到起名界面，或者从起名界面回来
    /// </summary>
    void ToOrBackName(bool isToName)
    {
        if (isToName)
        {
            //打开起名界面
            EditorControl.Instance.setName.gameObject.SetActive(true);
            EditorControl.Instance.keyBoardManager.gameObject.SetActive(true);
            editorParent.gameObject.SetActive(false);
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            //恢复界面
            transform.parent.gameObject.SetActive(true);
            editorParent.gameObject.SetActive(true);
            EditorControl.Instance.setName.gameObject.SetActive(false);
            EditorControl.Instance.keyBoardManager.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 创建2D icon
    /// </summary>
    void Create2DRoom()
    {
        EditorControl.Instance.setName.StartSetName(
            () =>
            {
                ToOrBackName(false);
            },
        (roomName) => {
            if (roomIconList.Count > 0)
                roomIconList[roomIconList.Count - 1].color = Color.white;

            GameObject obj = Instantiate(room2DPrefab);
            obj.transform.parent = roomIconParentTran;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            //表示选中状态
            roomIconList.Add(obj.GetComponent<Image>());
            roomIconList[roomIconList.Count - 1].color = Color.green;

            int index = roomIconList.Count - 1;

            obj.transform.GetChild(0).GetComponent<Text>().text = roomName;
            Create3DRoom(roomName);

            obj.name = roomName;
            obj.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { ShowRoom(index); });

            //恢复界面
            ToOrBackName(false);
        });

        ToOrBackName(true);
    }

    /// <summary>
    /// 创建3D房间
    /// </summary>
    /// <param name="name"></param>
    void Create3DRoom(string name)
    {
        //if (room3DList.Count > 0)
        //    room3DList[room3DList.Count - 1].gameObject.SetActive(false);

        GameObject obj = Instantiate(room3DPrefab);
        obj.transform.parent = editorParent;
        
        RoomControl roomControl = obj.GetComponent<RoomControl>();
        roomControl.roomDatas = new RoomDatas();
        roomControl.roomDatas.roomName = name;
        roomControl.CreatePoint();
        //压入链表
        if (curScenceData == null)
            curScenceData = new ScenceData();
        curScenceData.roomDatasList.Add(roomControl.roomDatas);

        room3DList.Add(roomControl);
        curRoomListId = room3DList.Count - 1;
        //设置选中状态
        ShowRoom(curRoomListId);
    }

    /// <summary>
    /// 切换显示当前房间
    /// </summary>
    /// <param name="name"></param>
    void ShowRoom(int roomid)
    {
        //表示选中状态
        //Debug.Log(name);
        //for (int i = 0; i < roomIconList.Count; i++)
        //{
        //    if (roomIconList[i].gameObject.name.Equals(name))
        //    {
        //        curRoomListId = i;
        //        roomIconList[i].color = Color.green;
        //        room3DList[i].gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        roomIconList[i].color = Color.white;
        //        room3DList[i].gameObject.SetActive(false);
        //    }
        //}

        for (int i = 0; i < roomIconList.Count; i++)
        {
            if (i== roomid)
            {
                curRoomListId = i;
                roomIconList[i].color = Color.green;
                room3DList[i].gameObject.SetActive(true);
            }
            else
            {
                roomIconList[i].color = Color.white;
                room3DList[i].gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 房间编辑，生成预设
    /// </summary>
    void EditObj()
    {
        if (curRoomListId == -1)
            return;
        
        EditorControl.Instance.ToEditObj();
    }
    
    /// <summary>
    /// 删除房间
    /// </summary>
    void DeleteRoom()
    {
        if (curRoomListId == -1)
            return;
        if (curRoomListId < room3DList.Count && curScenceData.roomDatasList.Contains(room3DList[curRoomListId].roomDatas))
        {
            curScenceData.roomDatasList.Remove(room3DList[curRoomListId].roomDatas);

            //恢复房间内的物体的UI
            room3DList[curRoomListId].ResetObjUI();

            Destroy(roomIconList[curRoomListId].gameObject);
            roomIconList.RemoveAt(curRoomListId);
            Destroy(room3DList[curRoomListId].gameObject);
            room3DList.RemoveAt(curRoomListId);

            if (roomIconList.Count>0)
                ShowRoom(roomIconList.Count - 1);
        }
    }

    public void SaveRoomData()
    {
        for (int i = 0; i < room3DList.Count; i++)
        {
            room3DList[i].SaveData();
        }
    }

    #endregion

    private void OnEnable()
    {
        AddRoomEvent();
    }

    private void OnDisable()
    {
        RemoveRoomEvent();
    }

    #region 管理房间内物体
    public void Create3DObj(Vector3 pos, string id, GameObject prefab3d)
    {
        if (curRoomListId == -1)
        {
            Debug.Log("MyLog::缺少curRoomControl");
            return;
        }
        //Debug.Log("MyLog::id:"+id);
        room3DList[curRoomListId].CreatePrefab3D(pos, id, prefab3d);
    }
    /// <summary>
    /// 显示当前房间当前编辑的对象
    /// </summary>
    public void ShowRoomObj(int index)
    {
        if (curRoomListId == -1)
        {
            Debug.Log("MyLog::缺少curRoomControl");
            return;
        }

        room3DList[curRoomListId].SetShowObjNum(index);
    }
    /// <summary>
    /// 删除当前操作的对象
    /// </summary>
    public string RemoveCurRoomObj()
    {
        if (curRoomListId == -1)
        {
            Debug.Log("MyLog::缺少curRoomControl");
            return null;
        }

        return room3DList[curRoomListId].RemoveObj();
    }
    #endregion

    /// <summary>
    /// 编辑模式下根据场景数据加载房间
    /// </summary>
    public void LoadScenceData(ScenceData scenceData)
    {
        DestroyOldScence();
        //还原物体预设UI显示
        EditorControl.Instance.prefabManager.Reset2DImage();
        curScenceData = scenceData;
        
        //生成新房间
        for (int i = 0; i < scenceData.roomDatasList.Count; i++)
        {
            //实例化3D物体
            GameObject obj = Instantiate(room3DPrefab);
            obj.transform.parent = editorParent;

            RoomControl roomControl = obj.GetComponent<RoomControl>();
            roomControl.roomDatas = scenceData.roomDatasList[i];
            
            room3DList.Add(roomControl);
            
            roomControl.SetRoomFromData(scenceData.roomDatasList[i], EditorControl.Instance.prefabManager.editorPrefabDic);
            //实例化2Dicon
            Load2DRoomIcon(scenceData.roomDatasList[i].roomName);
        }

        curRoomListId = room3DList.Count - 1;
        ShowRoom(curRoomListId);
    }

    void Load2DRoomIcon(string name)
    {
        //实例化物体
        GameObject obj = Instantiate(room2DPrefab);
        obj.transform.parent = roomIconParentTran;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        //表示选中状态
        roomIconList.Add(obj.GetComponent<Image>());

        obj.name = name;
        obj.transform.GetChild(0).GetComponent<Text>().text = name;

        int index = roomIconList.Count - 1;
        obj.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { ShowRoom(index); });
        
    }

    /// <summary>
    /// 销毁原来的房间，清空场景
    /// </summary>
    public void DestroyOldScence()
    {
        //销毁房间2Dicon
        for (int i = 0; i < roomIconList.Count; i++)
        {
            roomIconList[i].GetComponent<ButtonRayReceiver>().onPinchDown.RemoveAllListeners();
            Destroy(roomIconList[i].gameObject);
        }
        roomIconList.Clear();
        
        //销毁3D房间
        for (int i = 0; i < room3DList.Count; i++)
        {
            Destroy(room3DList[i].gameObject);
        }
        room3DList.Clear();

        if (curScenceData != null)
        {
            curScenceData.Clear();
            curScenceData = null;
        }
        //当前房间ID设为-1
        curRoomListId = -1;
    }
}
