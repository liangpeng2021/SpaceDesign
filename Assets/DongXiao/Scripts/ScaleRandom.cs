using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 随机缩放，/*create by 梁鹏 2021-8-6 */
/// </summary>
public class ScaleRandom : MonoBehaviour
{
    Transform selfTran;
    float timeCount;
    //随机间隔
    public float timeInternal = 0.5f;
    //插值速度
    public float lerpSpeed = 1f;
    //范围
    public float floatMin=0.8f;
    public float floatMax=1.2f;

    float curValue = 0;
    float targetvalue = 0;

    void Start()
    {
        selfTran = GetComponent<Transform>();
        SetFloat(curValue);
        RandomFloat();
    }
    void SetFloat(float value)
    {
        selfTran.localScale=Vector3.one* value;
    }

    void RandomFloat()
    {
        targetvalue = Random.Range(floatMin, floatMax);
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount > timeInternal)
        {
            timeCount = 0;
            RandomFloat();
        }
        curValue = Mathf.Lerp(curValue, targetvalue, Time.deltaTime * lerpSpeed);
        SetFloat(curValue);
    }
}
