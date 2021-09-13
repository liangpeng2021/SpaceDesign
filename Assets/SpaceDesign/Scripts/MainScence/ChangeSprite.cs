using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 进入退出修改效果，/*create by 梁鹏 2021-8-31 */
/// </summary>

public class ChangeSprite : MonoBehaviour
{
    /// <summary>
    /// 悬停颜色
    /// </summary>
    public Sprite focusSprite;
    /// <summary>
    /// 正常颜色
    /// </summary>
    public Sprite normalSprite;
    
    ButtonRayReceiver buttonRayReceiver;

    Image image;
    private void Start()
    {
        image = GetComponent<Image>();
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        if (buttonRayReceiver == null)
        {
            buttonRayReceiver = GetComponent<ButtonRayReceiver>();
        }
        if (buttonRayReceiver)
        {
            buttonRayReceiver.onPointerEnter.AddListener(OnPointEnter);
            buttonRayReceiver.onPointerExit.AddListener(OnPointExit);
        }
    }

    private void OnDisable()
    {
        if (buttonRayReceiver)
        {
            buttonRayReceiver.onPointerEnter.RemoveListener(OnPointEnter);
            buttonRayReceiver.onPointerExit.RemoveListener(OnPointExit);
        }
    }

    void OnPointEnter()
    {
        image.sprite = focusSprite;
    }

    void OnPointExit()
    {
        image.sprite = normalSprite;
    }
}
