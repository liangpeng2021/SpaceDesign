using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDesign
{
    /// <summary>
    /// The class for slate scrolling control. <br>
    /// 控制面板滚动交互的类。
    /// </summary>
    public class ScrollViewSlateController : ChufangSlateController
    {
        BoxCollider boxCollider;
        RectTransform rectTransform;

        public Scrollbar scrollbar;
        float valuestart;
        
        private void Start()
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            rectTransform = GetComponent<RectTransform>();
        }
        
        public override void UpdatePinchPointerStart(Vector3 pointOnSlate)
        {
            startPoint = pointOnSlate;
            valuestart = scrollbar.value;
        }

        private void Update()
        {
            boxCollider.size = new Vector3(rectTransform.sizeDelta.x, 200 * scrollbar.size, 10);
        }

        public override void UpdatePinchPointerEnd()
        {
            
        }
        public override void UpdatePinchPointer(Vector3 pointOnSlate)
        {
            endPoint = pointOnSlate;
            scrollbar.value = Mathf.Clamp((endPoint.y - startPoint.y) * 8f + valuestart, 0, 1);
            //scrollbar.value = (endPoint.y - startPoint.y) * 8f + valuestart;
        }
    }
}