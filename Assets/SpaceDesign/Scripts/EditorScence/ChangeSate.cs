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
    bool isLightOn;
    GameObject lightObj;

    private void Start()
    {
        EditorControl.Instance.HasChange();
    }

    private void OnDestroy()
    {
        lightObj = null;

        boundingBox = null;
    }

    public void HightLightOff()
    {
        isLightOn = false;
    }

    void Init()
    {
        lightObj = transform.Find("BoundingBox/BoundsVisualization").gameObject;
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
        EditorControl.Instance.HasChange();
    }

    public void HightLightOn()
    {
        isLightOn = true;
    }

    private void Update()
    {
        if (lightObj == null)
            Init();
        if (lightObj == null)
            return;
        lightObj.SetActive(isLightOn);
    }
}
