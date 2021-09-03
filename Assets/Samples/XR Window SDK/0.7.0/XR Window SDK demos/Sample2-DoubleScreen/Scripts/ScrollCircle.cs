/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XR.Samples
{
    public class ScrollCircle : ScrollRect
    {
        protected float mRadius = 0f;
        protected override void Start()
        {
            base.Start();
            //计算摇杆块的半径
            mRadius = (transform as RectTransform).sizeDelta.x * 0.5f;
        }
        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            var contentPostion = this.content.anchoredPosition;
            if (contentPostion.magnitude > mRadius)
            {
                contentPostion = contentPostion.normalized * mRadius;
                SetContentAnchoredPosition(contentPostion);
            }
            EasyJoystick.Instance.JoystickTouch = contentPostion;
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            EasyJoystick.Instance.JoystickTouch = Vector2.zero;
        }
    }
    public class EasyJoystick
    {
        private static EasyJoystick instance;
        public static EasyJoystick Instance
        {
            get
            {
                if (null == instance)
                    instance = new EasyJoystick();
                return instance;
            }
        }
        public Vector2 JoystickTouch { get; set; }
    }
}