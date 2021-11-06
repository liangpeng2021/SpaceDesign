/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace XR.Samples
{
    public class LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public UnityEvent OnLongPress;
        private bool isDown = false;
        private float pressTime = 0;

        private void Update()
        {
            if (isDown)
            {
                if ((Time.time - pressTime) > 0.2f)//长按
                {
                    OnLongPress?.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            pressTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isDown = false;
        }
    }
}