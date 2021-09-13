using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 修改灯的材质，/*create by 梁鹏 2021-9-3 */
/// </summary>
public class ChangeLightObjMat : MonoBehaviour
{
    [System.Serializable]
    public struct ChangeLightData
    {
        public ButtonTouchableReceiver buttonRayReceiver;
        public Color color;
        public Image image;
    }

    public ChangeLightData[] lightData;

    public MeshRenderer lightRender;
    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = lightRender.material;
        for (int i = 0; i < lightData.Length; i++)
        {
            if (lightData[i].buttonRayReceiver)
            {
                int num = i;
                lightData[i].buttonRayReceiver.onClick.AddListener(()=>
                {
                    ClickButtonRay(num);
                });
            }
        }
    }

    void ClickButtonRay(int num)
    {
        Debug.Log("MyLog::ClickButtonRay:" +num);
        for (int i = 0; i < lightData.Length; i++)
        {
            lightData[i].image.enabled = (i == num);
            if (i == num)
            {
                mat.color = lightData[i].color;
            }
        }
    }
}
