using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 修改灯的材质，/*create by 梁鹏 2021-9-3 */
/// </summary>
namespace SpaceDesign
{
    public class ChangeLightObjMat : MonoBehaviour
    {
        [System.Serializable]
        public struct ChangeLightData
        {
            public ButtonRayReceiver buttonRayReceiver;
            public Color color;
        }

        public ChangeLightData[] lightData;

        public Transform selectTran;
        public Transform focusTran;

        public MeshRenderer lightRender;
        //优化render
        private MaterialPropertyBlock matPropBlock;

        Material mat1;
        Material mat2;

        private void Start()
        {
            mat1 = lightRender.materials[1];
            mat2 = lightRender.materials[4];
        }

        private void OnEnable()
        {
            for (int i = 0; i < lightData.Length; i++)
            {
                if (lightData[i].buttonRayReceiver)
                {
                    int num = i;
                    lightData[i].buttonRayReceiver.onPinchDown.AddListener(() =>
                    {
                        ClickButtonRay(num);
                    });

                    lightData[i].buttonRayReceiver.onPointerEnter.AddListener(() =>
                    {
                        FocusOnOrOff(num, true);
                    });

                    lightData[i].buttonRayReceiver.onPointerExit.AddListener(() =>
                    {
                        //------------ Modify by zh ------------
                        //FocusOnOrOff(num, true);
                        FocusOnOrOff(num, false);
                        //------------------End------------------
                    });
                }
            }
        }

        void FocusOnOrOff(int num, bool isOn)
        {
            //------------ Modify by zh ------------
            //focusTran.gameObject.SetActive(isOn);
            //if (isOn)
            //    focusTran.position = lightData[num].buttonRayReceiver.transform.position;

            focusTran.gameObject.SetActive(true);
            focusTran.position = lightData[num].buttonRayReceiver.transform.position;
            TaidengManager.Inst.taidengController.SetTranslating(isOn);
            //------------------End------------------
        }

        private void OnDisable()
        {
            for (int i = 0; i < lightData.Length; i++)
            {
                lightData[i].buttonRayReceiver.onPinchDown.RemoveAllListeners();
                lightData[i].buttonRayReceiver.onPointerEnter.RemoveAllListeners();
                lightData[i].buttonRayReceiver.onPointerExit.RemoveAllListeners();
            }
        }

        void ClickButtonRay(int num)
        {
            SetPropBlock(lightData[num].color);
            selectTran.position = lightData[num].buttonRayReceiver.transform.position;
        }

        /// <summary>
        /// 设置材质属性，自定义颜色
        /// </summary>
        void SetPropBlock(Color f)
        {
            if (lightRender == null)
                return;
            mat1.color = f;
            mat2.color = f;
            //matPropBlock = new MaterialPropertyBlock();
            //lightRender.GetPropertyBlock(matPropBlock);
            //matPropBlock.SetColor("_Color",f);
            //lightRender.SetPropertyBlock(matPropBlock);
        }
    }
}

