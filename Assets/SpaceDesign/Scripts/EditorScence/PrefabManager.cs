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
    public ButtonRayReceiver deleteObjBtn;
    public ButtonRayReceiver backtoRoomBtn;
    #region 预设管理

    /// <summary>
    /// 房间内的物体编辑界面
    /// </summary>
    public GameObject objPrefabUIList;

    public PrefabData[] prefabDatas;
    
    public Dictionary<string, GameObject> editorPrefabDic = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> icon2DDic = new Dictionary<string, GameObject>();
    private void OnDestroy()
    {
        deleteObjBtn = null;
        backtoRoomBtn = null;
        objPrefabUIList = null;

        for (int i = 0; i < prefabDatas.Length; i++)
        {
            prefabDatas[i].Clear();
        }
        prefabDatas = null;
    }

    void Add2DObjEvent()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            //添加事件
            GameObject obj3d = prefabDatas[i].prefab3D;
            string str = prefabDatas[i].id;
            GameObject obj2d = prefabDatas[i].Obj2D;
            prefabDatas[i].Obj2D.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { CreatePrefab3D(obj2d, str, obj3d); });

            if (!editorPrefabDic.ContainsKey(str))
                editorPrefabDic.Add(str, prefabDatas[i].prefab3D);
        }
    }

    void Remove2DObjEvent()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            GameObject obj3d = prefabDatas[i].prefab3D;
            string str = prefabDatas[i].id;
            GameObject obj2d = prefabDatas[i].Obj2D;
            prefabDatas[i].Obj2D.GetComponent<ButtonRayReceiver>().onPinchDown.RemoveAllListeners();
        }
    }
    
    void CreatePrefab3D(GameObject obj2d, string id, GameObject prefab3d)
    {
        obj2d.GetComponent<Image>().color = Color.gray;
        obj2d.GetComponent<BoxCollider>().enabled = false;
        EditorControl.Instance.roomManager.Create3DObj(obj2d.transform.position, id, prefab3d);
    }
    /// <summary>
    /// 让所有按钮处于激活状态
    /// </summary>
    public void Reset2DImage()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            prefabDatas[i].Obj2D.GetComponent<Image>().color = Color.white;
            prefabDatas[i].Obj2D.GetComponent<BoxCollider>().enabled = true;
        }
    }
    /// <summary>
    /// 让按钮处于不激活状态
    /// </summary>
    /// <param name="id"></param>
    public void DisableObj2DImage(string id)
    {
        icon2DDic[id].GetComponent<Image>().color = Color.gray;
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
        }
    }

    private void OnEnable()
    {
        backtoRoomBtn.onPinchDown.AddListener(BackToRoom);
        deleteObjBtn.onPinchDown.AddListener(DeleteObj);
        Add2DObjEvent();
    }

    private void OnDisable()
    {
        backtoRoomBtn.onPinchDown.RemoveListener(BackToRoom);
        deleteObjBtn.onPinchDown.RemoveListener(DeleteObj);
        Remove2DObjEvent();
    }

    void BackToRoom()
    {
        EditorControl.Instance.ToEditRoom();
    }
    //删除当前编辑的物体
    public void DeleteObj()
    {
        string id= EditorControl.Instance.roomManager.RemoveCurRoomObj();
        ResetObjUI(id);
    }

    public void ResetObjUI(string id)
    {
        if (id != null)
        {
            for (int i = 0; i < prefabDatas.Length; i++)
            {
                if (prefabDatas[i].id.Equals(id))
                {
                    prefabDatas[i].Obj2D.GetComponent<Image>().color = Color.white;
                    prefabDatas[i].Obj2D.GetComponent<BoxCollider>().enabled = true;
                    break;
                }
            }
        }
    }
}
