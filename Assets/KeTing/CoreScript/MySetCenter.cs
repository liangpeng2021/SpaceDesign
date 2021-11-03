using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySetCenter : MonoBehaviour
{


    [Tooltip("设置：画的总对象中心点（EachBoothCtr），设为画的中心点（article）")]
    [HideInInspector]
    public Transform targetCenter;
    //[ContextMenu("设置：画的总对象中心点（EachBoothCtr），设为画的中心点（article）")]
    public void SetCenterPoint()
    {
        Transform tempParent = new GameObject("tempParent").transform;
        Transform t = transform;

        Transform[] childs;

        childs = new Transform[t.childCount];
        for (int i = 0; i < t.childCount; i++)
        {
            childs[i] = t.GetChild(i);
        }

        Vector3 centerPos = targetCenter.position;
        tempParent.parent = t.parent;
        tempParent.localPosition = t.localPosition;
        tempParent.localRotation = t.localRotation;
        tempParent.localScale = t.localScale;

        foreach (Transform v in childs)
        {
            v.SetParent(tempParent);
        }

        t.position = centerPos;

        foreach (Transform v in childs)
        {
            v.SetParent(t);
        }

        DestroyImmediate(tempParent.gameObject);
    }


}
