using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 关闭旋转和缩放，只有位移
/// </summary>
public class SetAnchorPoint : MonoBehaviour
{
    GameObject boundingBoxObj;
    float startY;
    bool hasDestroy;

    private void OnDestroy()
    {
        boundingBoxObj = null;
    }

    private void Start()
    {
        startY = transform.position.y;
        DestroyBoundBox();
    }

    void DestroyBoundBox()
    {
        if (transform.childCount == 0)
            return;
        if (!hasDestroy && boundingBoxObj == null)
        {
            boundingBoxObj = transform.GetChild(0).gameObject;
            if (boundingBoxObj)
            {
                Destroy(boundingBoxObj);
                hasDestroy = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        DestroyBoundBox();
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        //y轴不动
        pos.y = startY;
        transform.position = pos;
    }
}
