using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 控制灯的开关，/*create by 梁鹏 2021-9-3 */
/// </summary>
namespace SpaceDesign
{
    public class LightController : MonoBehaviour
    {
        public Sprite lightoff;
        public GameObject lightoffFocusObj;

        public Sprite lightOn;
        public GameObject lightOnFocusObj;

        bool islightOn;
        Image image;
        public GameObject lightObj;
        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<Image>();
            image.sprite = lightOn;
            islightOn = true;
            lightObj.SetActive(true);
        }
        /// <summary>
        /// 修改按钮精灵
        /// </summary>
        public void SetLightOnOrOff()
        {
            islightOn = !islightOn;
            if (islightOn)
                image.sprite = lightOn;
            else
                image.sprite = lightoff;
            lightObj.SetActive(islightOn);
        }
        /// <summary>
        /// 修改悬停状态
        /// </summary>
        public void OnPointEnter()
        {
            TaidengManager.Inst.taidengController.SetTranslating(true);
            if (islightOn)
            {
                lightOnFocusObj.SetActive(false);
                lightoffFocusObj.SetActive(true);
            }
            else
            {
                lightOnFocusObj.SetActive(true);
                lightoffFocusObj.SetActive(false);
            }
        }

        public void OnPointExit()
        {
            TaidengManager.Inst.taidengController.SetTranslating(false);
            lightOnFocusObj.SetActive(false);
            lightoffFocusObj.SetActive(false);
        }
    }
}