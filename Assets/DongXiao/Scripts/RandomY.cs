using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomY : MonoBehaviour
{
    float timeCount;
    //随机间隔
    public float timeInternal = 3f;
    //插值速度
    public float lerpSpeed = 3f;
    //范围
    public float floatMin;
    public float floatMax;

    float curValue = 0;
    float targetvalue = 0;
    private void Start()
    {
        SetFloat(curValue);
        RandomFloat();
    }

    void SetFloat(float value)
    {     
        transform.localScale = new Vector3(transform.localScale.x, value, transform.localScale.z);
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