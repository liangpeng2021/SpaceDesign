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
    //bool isLightOn;
    //GameObject lightObj;
    
    private void OnDestroy()
    {
        //lightObj = null;

        boundingBox = null;
    }

    //public void HightLightOff()
    //{
    //    //isLightOn = false;
    //}

    //void Init()
    //{
    //    if (transform.Find("BoundingBox/BoundsVisualization"))
    //        lightObj = transform.Find("BoundingBox/BoundsVisualization").gameObject;
    //}

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
    }

    private void OnDisable()
    {
        if(boundingBox)
        {
            boundingBox.onTranslateStart.RemoveListener(HasChange);
            boundingBox.onScaleStart.RemoveListener(HasChange);
            boundingBox.onRotateStart.RemoveListener(HasChange);
        }
    }

    /// <summary>
    /// 有修改
    /// </summary>
    void HasChange()
    {
        EditorControl.Instance.roomManager.ShowRoomObj(index);
    }

    public void HightLightOn()
    {
        //isLightOn = true;

        //if (lightObj == null)
        //    Init();
        //if (lightObj == null)
        //    return;
        EditorControl.Instance.prefabManager.SetDeleteObjPos(transform);
    }

    private void Update()
    {
        //if (lightObj == null)
        //    Init();
        //if (lightObj == null)
        //    return;
        //lightObj.SetActive(isLightOn);
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
            //x、z不旋转
            if (i < 4 || (i >= 8 && i < 12))
            {
                boundingBox.edgeObjects[i].SetActive(false);
            }
        }
    }
}
