using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 多按钮控制  Created by CL 2021.9.18
/// </summary>

public class BtnCtrl : MonoBehaviour
{
    //public List<Transform> transforms; 
    public Transform typeParent;
    //private Transform lastTra;

    void Start()
    {
    }

    public void BtnSwitch(Transform tra)
    {

        for (int i = 0; i < typeParent.childCount; i++)
        {
            typeParent.GetChild(i).gameObject.SetActive(false);
        }

        //lastTra.gameObject.SetActive(false);
        tra.gameObject.SetActive(true);;
    } 
}