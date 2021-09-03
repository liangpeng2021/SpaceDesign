using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 进入退出修改效果，/*create by 梁鹏 2021-8-31 */
/// </summary>
public enum ChangeType
{
    material,
    image,
}
public class ChangeColor : MonoBehaviour
{
    /// <summary>
    /// 悬停颜色
    /// </summary>
    public Color focusColor=new Color(0.18f,0.78f,0.42f);
    /// <summary>
    /// 正常颜色
    /// </summary>
    public Color normalColor=Color.white;

    public ChangeType changeType = ChangeType.image;

    ButtonRayReceiver buttonRayReceiver;

    Image image;
    Material mat;
    private void Start()
    {
        switch (changeType)
        {
            case ChangeType.image:
                image = GetComponent<Image>();
                break;
            case ChangeType.material:
                mat = GetComponent<MeshRenderer>().material;
                break;
        }
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        if (buttonRayReceiver == null)
        {
            buttonRayReceiver = GetComponent<ButtonRayReceiver>();
        }

        buttonRayReceiver.onPointerEnter.AddListener(OnPointEnter);
        buttonRayReceiver.onPointerExit.AddListener(OnPointExit);
    }

    private void OnDisable()
    {
        buttonRayReceiver.onPointerEnter.RemoveListener(OnPointEnter);
        buttonRayReceiver.onPointerExit.RemoveListener(OnPointExit);
    }

    void OnPointEnter()
    {
        switch (changeType)
        {
            case ChangeType.image:
                image.color = focusColor;
                break;
            case ChangeType.material:
                mat.color = focusColor;
                break;
        }
    }

    void OnPointExit()
    {
        switch (changeType)
        {
            case ChangeType.image:
                image.color = normalColor;
                break;
            case ChangeType.material:
                mat.color = normalColor;
                break;
        }
    }
}
