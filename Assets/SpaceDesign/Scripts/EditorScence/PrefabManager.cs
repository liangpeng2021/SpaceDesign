using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 预设管理，/*create by 梁鹏 2021-9-18 */
/// </summary>
public class PrefabManager : MonoBehaviour
{
    #region 翻页
    /// <summary>
    /// 总页数
    /// </summary>
    int totalPageNum = 3;
    /// <summary>
    /// 当前在编辑的场景所在页，每页3个
    /// </summary>
    int curPageId = 0;
    /// <summary>
    /// 所有页面
    /// </summary>
    public GameObject[] pages;

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

    void Awake()
    {
        totalPageNum = pages.Length - 1;
    }
    /// <summary>
    /// 翻到上一页
    /// </summary>
    void TurnLastPage()
    {
        //下一页按钮打开
        turnPageNextBtn.gameObject.SetActive(true);
        //当前页数修改
        curPageId--;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == curPageId);
        }
        //如果是第一页，关掉上一页按钮
        if (curPageId == 0)
            turnPageLastBtn.gameObject.SetActive(false);
        //更新页数显示
        pageNumText.text = (curPageId + 1).ToString() + "/" + (totalPageNum + 1).ToString();
    }
    /// <summary>
    /// 翻到下一页
    /// </summary>
    void TurnNextPage()
    {
        //上一页按钮打开
        turnPageLastBtn.gameObject.SetActive(true);
        //当前页数修改
        curPageId++;
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == curPageId);
        }
        //如果是最后一页，下一页按钮关掉
        if (curPageId == totalPageNum)
            turnPageNextBtn.gameObject.SetActive(false);
        //更新页数显示
        pageNumText.text = (curPageId + 1).ToString() + "/" + (totalPageNum + 1).ToString();
    }

    void InitPage()
    {
        pageNumText.gameObject.SetActive(true);
        turnPageLastBtn.gameObject.SetActive(false);
        turnPageNextBtn.gameObject.SetActive(true);
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == 0);
        }
        //更新页数显示
        pageNumText.text = (curPageId + 1).ToString() + "/" + (totalPageNum + 1).ToString();
    }
    #endregion

    public GameObject deletePrefab;
    
    public ButtonRayReceiver backtoRoomBtn;
    #region 预设管理

    public PrefabData[] prefabDatas;
    Dictionary<string, Material> materialDic = new Dictionary<string, Material>();

    public Dictionary<string, GameObject> editorPrefabDic = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> icon2DDic = new Dictionary<string, GameObject>();
    private void OnDestroy()
    {
        deletePrefab = null;
        backtoRoomBtn = null;

        for (int i = 0; i < prefabDatas.Length; i++)
        {
            prefabDatas[i].Clear();
        }
        prefabDatas = null;

        materialDic.Clear();
        editorPrefabDic.Clear();
        icon2DDic.Clear();
    }

    void Add2DObjEvent()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            //添加事件
            string str = prefabDatas[i].id;
            int index = i;
            prefabDatas[i].Obj2D.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { CreatePrefab3D(index, str); });

            if (!editorPrefabDic.ContainsKey(str))
                editorPrefabDic.Add(str, prefabDatas[i].prefab3D);
        }

        turnPageLastBtn.onPinchDown.AddListener(TurnLastPage);
        turnPageNextBtn.onPinchDown.AddListener(TurnNextPage);
    }

    void Remove2DObjEvent()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            prefabDatas[i].Obj2D.GetComponent<ButtonRayReceiver>().onPinchDown.RemoveAllListeners();
        }

        turnPageLastBtn.onPinchDown.RemoveAllListeners();
        turnPageNextBtn.onPinchDown.RemoveAllListeners();
    }

    void CreatePrefab3D(int index, string str)
    {
        GameObject obj3d = prefabDatas[index].prefab3D;

        GameObject obj2d = prefabDatas[index].Obj2D;

        materialDic[str].SetColor("_BaseColor", Color.black);

        obj2d.GetComponent<BoxCollider>().enabled = false;
        EditorControl.Instance.roomManager.Create3DObj(obj2d.transform.position, str, obj3d);
    }
    /// <summary>
    /// 让所有按钮处于激活状态
    /// </summary>
    public void Reset2DImage()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            prefabDatas[i].Obj2D.GetComponent<BoxCollider>().enabled = true;
        }

        foreach (var i in materialDic)
        {
            i.Value.SetColor("_BaseColor", EditorControl.Instance.normalColor);
        }
    }
    /// <summary>
    /// 让按钮处于不激活状态
    /// </summary>
    /// <param name="id"></param>
    public void DisableObj2DImage(string id)
    {
        materialDic[id].SetColor("_BaseColor", Color.black);
        icon2DDic[id].GetComponent<BoxCollider>().enabled = false;
    }
    #endregion

    public void InitPrefabDic()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            if (!editorPrefabDic.ContainsKey(prefabDatas[i].id))
                editorPrefabDic.Add(prefabDatas[i].id, prefabDatas[i].prefab3D);
            if (!icon2DDic.ContainsKey(prefabDatas[i].id))
            {
                icon2DDic.Add(prefabDatas[i].id, prefabDatas[i].Obj2D);
            }
            if (!materialDic.ContainsKey(prefabDatas[i].id))
            {
                materialDic.Add(prefabDatas[i].id, prefabDatas[i].Obj2D.transform.Find("Cube/BackBoard").GetComponent<MeshRenderer>().material);
            }
        }

        InitPage();
    }

    private void OnEnable()
    {
        backtoRoomBtn.onPinchDown.AddListener(BackToRoom);

        Add2DObjEvent();
    }

    private void OnDisable()
    {
        backtoRoomBtn.onPinchDown.RemoveListener(BackToRoom);

        Remove2DObjEvent();
    }

    void BackToRoom()
    {
        EditorControl.Instance.ToEditRoom();
    }
    /// <summary>
    /// 删除物体时还原列表
    /// </summary>
    /// <param name="id"></param>
    public void ResetObjUI(string id)
    {
        if (id != null)
        {
            icon2DDic[id].GetComponent<BoxCollider>().enabled = true;
            materialDic[id].SetColor("_BaseColor", EditorControl.Instance.normalColor);
        }
    }

    //public void SetDeleteObjPos(Transform parent)
    //{
    //    if (deleteObjBtn == null)
    //    {
    //        deleteObjBtn = Instantiate(deletePrefab).GetComponent<ButtonRayReceiver>();
    //        deleteObjBtn.onPinchDown.AddListener(DeleteObj);
    //    }
    //    if (deleteObjBtn != null)
    //    {
    //        deleteObjBtn.transform.SetParent(parent);
    //        deleteObjBtn.transform.localEulerAngles = Vector3.zero;
    //        deleteObjBtn.transform.localPosition = new Vector3(0, -0.228f, -0.152f);
    //        deleteObjBtn.transform.localScale = Vector3.one * 0.001f;
    //    }
    //}
}
