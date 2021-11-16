using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 随机坐标值  Created by CL;
/// </summary>

public class RectPosCtrl : MonoBehaviour
{
    public float minX;
    public float maxX;

    public float minY;
    public float maxY;

    float posX = 0;
    float posY = 0;

    float timeCount;
    //随机间隔
    public float timeInternal = 3f;

    void Start()
    {
        SetPos(posX, posY);
        RandomFloat();
    }


    void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount > timeInternal)
        {
            timeCount = 0;
            RandomFloat();
            SetPos(posX, posY);
        }    
    }

    public void SetPos(float x, float y)
    {      
        //transform.position = new Vector3(x, y, transform.position.z);
        transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(x, y, 0);
    }

    public void RandomFloat()
    {
        posX = Random.Range(minX, maxX);

        posY = Random.Range(minY, maxY);
    }
}
