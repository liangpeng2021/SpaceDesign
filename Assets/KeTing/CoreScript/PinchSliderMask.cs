/* Create by zh at 2021-10-30

    Oppo拖拽条，背景填充脚本

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchSliderMask : MonoBehaviour
{
    RectTransform rt;

    public void SetSlider(float f)
    {
        if (rt == null)
            rt = GetComponent<RectTransform>();
        //位置信息，从左到右【146,930】
        float _f = 146f + 784f * f;
        rt.sizeDelta = new Vector2(_f, rt.sizeDelta.y);
    }
}
