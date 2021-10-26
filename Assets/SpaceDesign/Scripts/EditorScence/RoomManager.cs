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
    #region 翻页
    /// <summary>
    /// 每页数量
    /// </summary>
    int pageCount;
    /// <summary>
    /// 场景列表界面
    /// </summary>
    public ButtonRayReceiver[] roomBtns;
    Text[] texts;
    Material[] materials;

    /// <summary>
    /// 总页数
    /// </summary>
    int totalPageRoomNum = -1;
    /// <summary>
    /// 当前在编辑的场景所在页，每页3个
    /// </summary>
    int curPageRoomId = 0;
    /// <summary>
    /// 当前在编辑的场景在当前页的下标
    /// </summary>
    int curRoomBtnIndex = -1;

    /// <summary>
    /// 页数
    /// </summary>
    public Text pageNumText;
    /// <summary>
    /// 翻到上页
    /// </summary>
    public ButtonRayReceiver turnPageLastBtn;
    /// <summary>
    /// 翻到下页
    /// </summary>
    public ButtonRayReceiver turnPageNextBtn;
    /// <summary>
    /// 选中后出现对应按钮
    /// </summary>
    public Transform seletTran;
    /// <summary>
    /// 重置页面和按钮
    /// </summary>

    private void ResetPage()
    {
        if (!isInitPage)
            InitPage();

        seletTran.gameObject.SetActive(false);
        pageNumText.gameObject.SetActive(false);
        turnPageLastBtn.gameObject.SetActive(false);
        turnPageNextBtn.gameObject.SetActive(false);

        for (int i = 0; i < roomBtns.Length; i++)
        {
            roomBtns[i].gameObject.SetActive(false);
        }
        if (curRoomBtnIndex > -1 && curRoomBtnIndex < roomBtns.Length)
        {
            materials[curRoomBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
        }
        curRoomBtnIndex = -1;
        curPageRoomId = 0;
        totalPageRoomNum = -1;
    }

    /// <summary>
    /// 翻到上一页
    /// </summary>
    void TurnLastPage()
    {
        //选中状态去掉
        if (curRoomBtnIndex > -1 && curRoomBtnIndex < materials.Length)
            materials[curRoomBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
        seletTran.gameObject.SetActive(false);
        //下一页按钮打开
        turnPageNextBtn.gameObject.SetActive(true);
        //当前页数修改
        curPageRoomId--;

        for (int i = 0; i < roomBtns.Length; i++)
        {
            //显示按钮
            roomBtns[i].gameObject.SetActive(true);
            //更新名字
            texts[i].text = curScenceData.roomDatasList[curPageRoomId * pageCount + i].roomName;
        }
        //如果是第一页，关掉上一页按钮
        if (curPageRoomId == 0)
            turnPageLastBtn.gameObject.SetActive(false);
        ///更新页数显示
        pageNumText.text = (curPageRoomId + 1).ToString() + "/" + (totalPageRoomNum + 1).ToString();
    }
    /// <summary>
    /// 翻到下一页
    /// </summary>
    void TurnNextPage()
    {
        //选中状态去掉
        if (curRoomBtnIndex > -1 && curRoomBtnIndex < materials.Length)
            materials[curRoomBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
        seletTran.gameObject.SetActive(false);
        //上一页按钮打开
        turnPageLastBtn.gameObject.SetActive(true);
        //当前页数修改
        curPageRoomId++;
        //当前页还剩多少个
        int index = curScenceData.roomDatasList.Count - 1 - curPageRoomId * pageCount;

        for (int i = 0; i < roomBtns.Length; i++)
        {
            //没满的关掉
            if (i > index)
            {
                roomBtns[i].gameObject.SetActive(false);
            }
            else
            {
                //满的更新
                roomBtns[i].gameObject.SetActive(true);
                texts[i].text = curScenceData.roomDatasList[curPageRoomId * pageCount + i].roomName;
            }
        }
        //如果是最后一页，下一页按钮关掉
        if (curPageRoomId == totalPageRoomNum)
            turnPageNextBtn.gameObject.SetActive(false);
        //更新页数显示
        pageNumText.text = (curPageRoomId + 1).ToString() + "/" + (totalPageRoomNum + 1).ToString();
    }

    /// <summary>
    /// 设置页面内的按钮
    /// </summary>
    void SetRoomBtnState()
    {
        //控制按钮显示和隐藏
        int index;
        if ((curScenceData.roomDatasList.Count - 1) <= 0)
            index = curScenceData.roomDatasList.Count - 1;
        else
            index = (curScenceData.roomDatasList.Count - 1) % pageCount;

        for (int i = 0; i < roomBtns.Length; i++)
        {
            if (i > (curScenceData.roomDatasList.Count - 1 - curPageRoomId * pageCount))
                roomBtns[i].gameObject.SetActive(false);
            else
            {
                roomBtns[i].gameObject.SetActive(true);
                //名称赋值
                texts[i].text = curScenceData.roomDatasList[curPageRoomId * pageCount + i].roomName;
            }
        }

        //翻页和页码更新
        if (index >= 0)
        {
            if (curPageRoomId == 0)
            {
                turnPageLastBtn.gameObject.SetActive(false);
            }
            else
                turnPageLastBtn.gameObject.SetActive(true);

            if (curPageRoomId == totalPageRoomNum)
                turnPageNextBtn.gameObject.SetActive(false);
            else
                turnPageNextBtn.gameObject.SetActive(true);

            pageNumText.gameObject.SetActive(true);
            pageNumText.text = (curPageRoomId + 1).ToString() + "/" + (totalPageRoomNum + 1).ToString();
        }
        else
        {
            pageNumText.gameObject.SetActive(false);
            for (int i = 0; i < roomBtns.Length; i++)
            {
                roomBtns[i].gameObject.SetActive(false);
            }

            turnPageLastBtn.gameObject.SetActive(false);
            turnPageNextBtn.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 赋值总页数
    /// </summary>
    /// <param name="count"></param>
    void SetTotalPageNum(int count)
    {
        if (count <= 0)
        {
            totalPageRoomNum = -1;
            return;
        }

        if (count % pageCount == 0)
        {
            totalPageRoomNum = count / pageCount - 1;
        }
        else
            totalPageRoomNum = count / pageCount;
    }

    bool isInitPage=false;
    void InitPage()
    {
        pageCount = roomBtns.Length;
        texts = new Text[roomBtns.Length];
        materials = new Material[roomBtns.Length];

        for (int i = 0; i < roomBtns.Length; i++)
        {
            texts[i] = roomBtns[i].transform.Find("Text (TMP)").GetComponent<Text>();
            materials[i] = roomBtns[i].transform.Find("Cube/BackBoard").GetComponent<MeshRenderer>().material;
        }

        seletTran.gameObject.SetActive(false);
        pageNumText.gameObject.SetActive(false);
        turnPageLastBtn.gameObject.SetActive(false);
        turnPageNextBtn.gameObject.SetActive(false);

        for (int i = 0; i < roomBtns.Length; i++)
        {
            roomBtns[i].gameObject.SetActive(false);
        }

        isInitPage = true;
    }

    #endregion

    /// <summary>
    /// 编辑模式3D物体父节点
    /// </summary>
    public Transform editorParent;
    
    #region 房间管理
    
    /// <summary>
    /// 房间预设
    /// </summary>
    public GameObject room3DPrefab;
    
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

        for (int i = 0; i < roomBtns.Length; i++)
        {
            int num = i;
            roomBtns[i].onPinchDown.AddListener(() => { ShowRoom(num); });
        }

        turnPageLastBtn.onPinchDown.AddListener(TurnLastPage);
        turnPageNextBtn.onPinchDown.AddListener(TurnNextPage);
    }

    void RemoveRoomEvent()
    {
        createRoomBtn.onPinchDown.RemoveListener(Create2DRoom);
        editRoomBtn.onPinchDown.RemoveListener(EditObj);
        deleteRoomBtn.onPinchDown.RemoveListener(DeleteRoom);

        for (int i = 0; i < roomBtns.Length; i++)
        {
            roomBtns[i].onPinchDown.RemoveAllListeners();
        }

        turnPageLastBtn.onPinchDown.RemoveAllListeners();
        turnPageNextBtn.onPinchDown.RemoveAllListeners();
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
            //EditorControl.Instance.editBtn.transform.parent.parent.gameObject.SetActive(false);
        }
        else
        {
            //恢复界面
            transform.parent.gameObject.SetActive(true);
            editorParent.gameObject.SetActive(true);
            EditorControl.Instance.setName.gameObject.SetActive(false);
            EditorControl.Instance.keyBoardManager.gameObject.SetActive(false);
            //EditorControl.Instance.editBtn.transform.parent.parent.gameObject.SetActive(true);
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
            Create3DRoom(roomName);
            //赋值总页数
            SetTotalPageNum(curScenceData.roomDatasList.Count);

            //控制整体按钮显示和隐藏
            SetRoomBtnState();

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

        room3DList[room3DList.Count - 1].gameObject.SetActive(false);
    }

    /// <summary>
    /// 切换显示当前房间
    /// </summary>
    /// <param name="name"></param>
    void ShowRoom(int roomid)
    {
        //表示选中状态
        seletTran.gameObject.SetActive(true);

        for (int i = 0; i < room3DList.Count; i++)
        {
            room3DList[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < roomBtns.Length; i++)
        {
            if (i== roomid)
            {
                curRoomBtnIndex = i;
                seletTran.position = roomBtns[i].transform.position;
                materials[i].SetColor("_BaseColor", EditorControl.Instance.chooseColor);
                room3DList[curPageRoomId*pageCount+i].gameObject.SetActive(true);
            }
            else
            {
                materials[i].SetColor("_BaseColor", EditorControl.Instance.normalColor);
            }
        }
    }
    /// <summary>
    /// 房间编辑，生成预设
    /// </summary>
    void EditObj()
    {
        int curRoomListId= (curPageRoomId * pageCount + curRoomBtnIndex);
        if (curRoomListId == -1)
            return;
        EditorControl.Instance.ToEditObj();
    }
    
    /// <summary>
    /// 删除房间
    /// </summary>
    void DeleteRoom()
    {
        int curRoomListId = (curPageRoomId * pageCount + curRoomBtnIndex);
        if (curRoomListId == -1)
            return;
        if (curRoomListId < room3DList.Count && curScenceData.roomDatasList.Contains(room3DList[curRoomListId].roomDatas))
        {
            //删除时取消选中状态
            if (curRoomBtnIndex > -1 && curRoomBtnIndex < materials.Length)
                materials[curRoomBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
            //如果删的是第一个位置的，更新当前页
            if (curRoomBtnIndex == 0)
            {
                curRoomBtnIndex = pageCount - 1;
                curPageRoomId--;
                if (curPageRoomId < 0)
                    curPageRoomId = 0;
            }

            curScenceData.roomDatasList.Remove(room3DList[curRoomListId].roomDatas);

            //恢复房间内的物体的UI
            room3DList[curRoomListId].ResetObjUI();
            
            Destroy(room3DList[curRoomListId].gameObject);
            room3DList.RemoveAt(curRoomListId);

            //赋值总页数
            SetTotalPageNum(curScenceData.roomDatasList.Count);
            //如果总页数到头了
            if (curScenceData.roomDatasList.Count == 0)
            {
                curRoomBtnIndex = -1;
            }
            SetRoomBtnState();
            //删除时取消选中状态
            seletTran.gameObject.SetActive(false);
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
        int curRoomListId = (curPageRoomId * pageCount + curRoomBtnIndex);
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
        int curRoomListId = (curPageRoomId * pageCount + curRoomBtnIndex);

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
        int curRoomListId = (curPageRoomId * pageCount + curRoomBtnIndex);
        if (curRoomListId == -1)
        {
            Debug.Log("MyLog::缺少curRoomControl");
            return null;
        }

        return room3DList[curRoomListId].RemoveObj();
    }
    #endregion

    private void Start()
    {
        if (!isInitPage)
            InitPage();
    }

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
        }
        //赋值总页数
        SetTotalPageNum(curScenceData.roomDatasList.Count);
        
        SetRoomBtnState();
    }
    
    /// <summary>
    /// 销毁原来的房间，清空场景
    /// </summary>
    public void DestroyOldScence()
    {
        //重置2Dicon
        ResetPage();

        //销毁3D房间
        for (int i = 0; i < room3DList.Count; i++)
        {
            Destroy(room3DList[i].gameObject);
        }
        room3DList.Clear();
    }
}
