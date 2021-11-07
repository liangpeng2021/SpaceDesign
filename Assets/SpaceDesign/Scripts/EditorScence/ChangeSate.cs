using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 属性被修改时执行相关逻辑，/*create by 梁鹏 2021-9-15 */
/// </summary>
public class ChangeSate : MonoBehaviour
{
    BoundingBox boundingBox;
    
    [HideInInspector]
    public int index;

    bool isDisableEdgeObj=false;

    GameObject deleteObj;
    ButtonRayReceiver deleteBtn; 
    
    private void OnDestroy()
    {
        boundingBox = null;
    }

    public void HightLightOff()
    {
        InitDeleteObj();
        if (deleteObj)
            deleteObj.SetActive(false);
    }

    void InitDeleteObj()
    {
        if (deleteObj == null)
        {
            deleteObj = Instantiate(EditorControl.Instance.prefabManager.deletePrefab);
            deleteBtn = deleteObj.transform.GetChild(0).GetComponent<ButtonRayReceiver>();

            deleteObj.transform.SetParent(this.transform);
            deleteObj.transform.localScale = Vector3.one;

            deleteObj.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (boundingBox == null)
        {
            boundingBox = GetComponent<BoundingBox>();
        }
        if (boundingBox)
        {
            boundingBox.onTranslateStart.AddListener(HasChange);
            boundingBox.onScaleStart.AddListener(HasChange);
            boundingBox.onRotateStart.AddListener(HasChange);
        }
        else
        {
            Debug.Log("MyLog::缺少BoundingBox");
        }

        InitDeleteObj();

        if (deleteBtn)
            deleteBtn.onPinchDown.AddListener(DeleteThis);
    }

    private void OnDisable()
    {
        if(boundingBox)
        {
            boundingBox.onTranslateStart.RemoveListener(HasChange);
            boundingBox.onScaleStart.RemoveListener(HasChange);
            boundingBox.onRotateStart.RemoveListener(HasChange);
        }

        if (deleteBtn)
            deleteBtn.onPinchDown.RemoveAllListeners();
    }

    /// <summary>
    /// 有修改
    /// </summary>
    void HasChange()
    {
        EditorControl.Instance.roomManager.ShowRoomObj(index);
    }
    /// <summary>
    /// 删除
    /// </summary>
    void DeleteThis()
    {
        EditorControl.Instance.roomManager.RemoveCurRoomObj();
    }

    public void HightLightOn()
    {
        InitDeleteObj();

        if (deleteObj)
            deleteObj.SetActive(true);
    }

    private void Update()
    {
        //更新删除按钮的位置和方向
        if (deleteObj && deleteObj.activeInHierarchy)
        {
            deleteObj.transform.forward = XR.XRCameraManager.Instance.stereoCamera.transform.forward;
            deleteObj.transform.eulerAngles = new Vector3(0, deleteObj.transform.eulerAngles.y, 0);
        }

        //获取不到时，update里面继续获取
        if (isDisableEdgeObj)
            return;
        if (boundingBox == null)
        {
            boundingBox = GetComponent<BoundingBox>();
        }
        if (boundingBox == null)
            return;
        if (boundingBox.edgeObjects == null)
            return;
        for (int i = 0; i < boundingBox.edgeObjects.Length; i++)
        {
            //x、z不旋转，隐藏
            if (i < 4 || (i >= 8 && i < 12))
            {
                boundingBox.edgeObjects[i].SetActive(false);
            }
            isDisableEdgeObj = true;
        }
    }
}
