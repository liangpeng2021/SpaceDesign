using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制图片大小和透明度  Created by CL on 2021.9.6
/// Scale；   Alpha
/// </summary>

public class ScaleAlphaCtrl : MonoBehaviour
{
    Transform selfTran;

    public float lerpSpeed = 1f;

    //Scale
    public float floatMinS = 0.0f;
    public float floatMaxS = 1.1f;
    //Alpha
    public float floatMinA = 0.0f;
    public float floatMaxA = 1.0f;

    float curScaleValue = 0;
    float curAlpha = 0;
    Color curColor;

    void Start()
    {
        selfTran = GetComponent<Transform>();
        curColor = GetComponent<Image>().color;

        SetScale(curScaleValue);
        SetAlpha(curAlpha);
    }

    void Update()
    {    

        if ((floatMaxS - curScaleValue <=0.1f)|| (curAlpha - floatMinA <= 0.1f))
        {
            curScaleValue = floatMinS;
            curAlpha = floatMaxA;

            SetScale(curScaleValue);
            SetAlpha(curAlpha);
        }
        curScaleValue = Mathf.Lerp(curScaleValue, floatMaxS, Time.deltaTime * lerpSpeed);
        curAlpha = Mathf.Lerp(curAlpha, floatMinA, Time.deltaTime * lerpSpeed);

        SetScale(curScaleValue);
        SetAlpha(curAlpha);
    }


    void SetScale(float value)
    {
        selfTran.localScale = Vector3.one * value;
    }

    void SetAlpha(float value)
    {
        GetComponent<Image>().color = new Color(curColor.r, curColor.g, curColor.b, value);
    }  
}
