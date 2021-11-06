using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 卡丁车计圈、读数、特效   Created by CL on 2021.11.5
/// </summary>

public class CircleCountManage : MonoBehaviour
{
    // 道路检测点
    public GameObject[] checkPoints;
    public bool is0;
    public bool is1;
    public bool is2;

    public int curCircleCount = 0;
    public ParticleSystem winEffect;

    // 圈数记录
    public Sprite[] sprites;
    public GameObject geWei;
    public GameObject shiWei;
    public GameObject baiWei;

    private Image geImage;
    private Image shiImage;
    private Image baiImage;

    public GameObject circleAnimObj;
    private Animation circleAnimation;

    //音效
    public AudioSource audioSource;
    public AudioClip winClip;

    void Start()
    {
        geImage = geWei.GetComponentInChildren<Image>();
        shiImage = shiWei.GetComponentInChildren<Image>();
        baiImage = baiWei.GetComponentInChildren<Image>();

        circleAnimation = circleAnimObj.GetComponent<Animation>();
        circleAnimation.Stop();
        winEffect.Stop(true);

        shiWei.SetActive(false);
        baiWei.SetActive(false);       
    }

    void Update()
    {
        CaculateImageNum(curCircleCount);
    }


    /// <summary>
    /// 人物跑圈计算
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //碰到最后一个，全true则整圈，否则回归原点
        if(other.name == checkPoints[checkPoints.Length-1].name)
        {
            if(is0 && is1)
            {
                curCircleCount += 1;

                audioSource.PlayOneShot(winClip);
                winEffect.Play(true);           
                ShowNum();
            }

            is0 = false;
            is1 = false;
        }

        else if (other.name == checkPoints[0].name)
        {         
            is0 = true;
            is1 = false;  // 中途返回，或反着跑，控制正方向
        }

        else if (other.name == checkPoints[1].name)
        {
            if (!is0)
                return;

            is1 = true;
        }
    }

    /// <summary>
    /// 数值变化
    /// </summary>
    /// <param name="num"></param>
    public void CaculateImageNum(int num)
    {
        if(curCircleCount >= 0 && curCircleCount <= 9)
        {
            shiWei.SetActive(false);
            baiWei.SetActive(false);

            geImage.sprite = sprites[num];
        }

        else if(curCircleCount >= 10 && curCircleCount <= 99)
        {
            shiWei.SetActive(true);
            baiWei.SetActive(false);

            geImage.sprite = sprites[(int)(num/10)];
            shiImage.sprite = sprites[num % 10];
        }

        else if (curCircleCount >= 100 && curCircleCount <= 999)
        {
            baiWei.SetActive(true);

            geImage.sprite = sprites[num % 10];
            shiImage.sprite = sprites[(num / 10) % 10];
            baiImage.sprite = sprites[num / 100];
        }
    }


    public void ShowNum()
    {
        circleAnimation.Play();
        Invoke("HideNum", 3f);
    }

    public void HideNum()
    {
        circleAnimation.Stop();
        audioSource.Stop();
    }
}
