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
    public Transform roomParentTran;
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

    int roomNum=0;
    [HideInInspector]
    public List<RoomControl> room3DList =new List<RoomControl>();
    List<Image> roomIconList = new List<Image>();

    /// <summary>
    /// 场景数据
    /// </summary>
    [HideInInspector]
    public ScenceData scenceData = new ScenceData();
    void AddRoomEvent()
    {
        createRoomBtn.onPinchDown.AddListener(CreateRoom);
        editRoomBtn.onPinchDown.AddListener(EditObj);
        deleteRoomBtn.onPinchDown.AddListener(DeleteRoom);
    }

    void RemoveRoomEvent()
    {
        createRoomBtn.onPinchDown.RemoveListener(CreateRoom);
        editRoomBtn.onPinchDown.RemoveListener(EditObj);
        deleteRoomBtn.onPinchDown.RemoveListener(DeleteRoom);
    }
    /// <summary>
    /// 创建2D icon
    /// </summary>
    void CreateRoom()
    {
        if (roomIconList.Count > 0)
            roomIconList[roomIconList.Count - 1].color = Color.white;

        roomNum++;
        GameObject obj = Instantiate(room2DPrefab);
        obj.transform.parent = roomParentTran;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        //表示选中状态
        roomIconList.Add(obj.GetComponent<Image>());
        roomIconList[roomIconList.Count - 1].color = Color.green;

        string roomName= "room_" + roomNum.ToString();
        obj.transform.GetChild(0).GetComponent<Text>().text = roomName;
        Create3DRoom(roomName);

        obj.name = roomName;
        obj.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(()=> { ShowRoom(roomName); });
    }

    /// <summary>
    /// 创建3D房间
    /// </summary>
    /// <param name="name"></param>
    void Create3DRoom(string name)
    {
        GameObject obj = Instantiate(room3DPrefab);
        obj.transform.parent = editorParent;
        
        RoomControl roomControl = obj.GetComponent<RoomControl>();
        roomControl.roomDatas = new RoomDatas();
        roomControl.roomDatas.roomName = name;
        roomControl.CreatePoint();
        
        scenceData.roomDatasList.Add(roomControl.roomDatas);

        if (room3DList.Count > 0)
            room3DList[room3DList.Count - 1].gameObject.SetActive(false);

        room3DList.Add(roomControl);
        curRoomListId = room3DList.Count - 1;

        EditorControl.Instance.HasChange();
    }

    /// <summary>
    /// 切换显示当前房间
    /// </summary>
    /// <param name="name"></param>
    void ShowRoom(string name)
    {
        //表示选中状态
        //Debug.Log(name);
        for (int i = 0; i < roomIconList.Count; i++)
        {
            if (roomIconList[i].gameObject.name.Equals(name))
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
        if (curRoomListId < room3DList.Count && scenceData.roomDatasList.Contains(room3DList[curRoomListId].roomDatas))
        {
            scenceData.roomDatasList.Remove(room3DList[curRoomListId].roomDatas);

            //恢复房间内的物体的UI
            room3DList[curRoomListId].ResetObjUI();

            Destroy(roomIconList[curRoomListId].gameObject);
            roomIconList.RemoveAt(curRoomListId);
            Destroy(room3DList[curRoomListId].gameObject);
            room3DList.RemoveAt(curRoomListId);

            ShowRoom(roomIconList[roomIconList.Count - 1].gameObject.name);
        }

        EditorControl.Instance.HasChange();
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

    public void SaveRoomData()
    {
        for (int i = 0; i < room3DList.Count; i++)
        {
            room3DList[i].SaveData();
        }
    }
}
