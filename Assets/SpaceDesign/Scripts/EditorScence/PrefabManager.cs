﻿using OXRTK.ARHandTracking;
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
    
    void CreatePrefabs2D()
    {
        for (int i = 0; i < prefabDatas.Length; i++)
        {
            GameObject obj3d = prefabDatas[i].prefab3D;
            string str = prefabDatas[i].id;
            GameObject obj2d = prefabDatas[i].Obj2D;
            prefabDatas[i].Obj2D.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { CreatePrefab3D(obj2d, str, obj3d); });
        }
    }
    
    void CreatePrefab3D(GameObject obj2d, string id, GameObject prefab3d)
    {
        obj2d.GetComponent<Image>().color = Color.gray;
        obj2d.GetComponent<BoxCollider>().enabled = false;
        EditorControl.Instance.roomManager.Create3DObj(obj2d.transform.position, id, prefab3d);
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        CreatePrefabs2D();
    }

    private void OnEnable()
    {
        backtoRoomBtn.onPinchDown.AddListener(BackToRoom);
        deleteObjBtn.onPinchDown.AddListener(DeleteObj);
    }

    private void OnDisable()
    {
        backtoRoomBtn.onPinchDown.RemoveListener(BackToRoom);
        deleteObjBtn.onPinchDown.RemoveListener(DeleteObj);
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