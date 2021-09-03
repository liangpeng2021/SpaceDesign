using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 控制灯的开关，/*create by 梁鹏 2021-9-3 */
/// </summary>
public class LightController : MonoBehaviour
{
    public Sprite lightoff;
    public Sprite lightOn;
    bool islightOn;
    Image image;
    public GameObject lightObj;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = lightOn;
    }

    public void SetLightOnOrOff()
    {
        islightOn = !islightOn;
        if (islightOn)
            image.sprite = lightoff;
        else
            image.sprite = lightOn;
        lightObj.SetActive(islightOn);
    }
}
