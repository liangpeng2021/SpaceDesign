using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 根据随机值显示UI，/*create by 梁鹏 2021-8-6 */
/// </summary>
public class RandomShow : MonoBehaviour
{
    public Text valueText;
    public Image[] valueImage;

    float timeCount;
    //随机间隔
    public float timeInternal=3f;
    //插值速度
    public float lerpSpeed=3f;
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
        for (int i = 0; i < valueImage.Length; i++)
        {
            valueImage[i].fillAmount = value;
        }
        if (valueText)
            valueText.text = (value * 100).ToString("f0") + "%";
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
        curValue = Mathf.Lerp(curValue, targetvalue,Time.deltaTime* lerpSpeed);
        SetFloat(curValue);
    }
}
