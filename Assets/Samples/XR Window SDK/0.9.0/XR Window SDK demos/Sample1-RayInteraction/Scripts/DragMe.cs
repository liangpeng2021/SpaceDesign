/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XR.Samples
{
    public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RayInteractionFullscreen rayInteractionFullscreen;
        private Collider thisCollider;

        void Start()
        {
            rayInteractionFullscreen = FindObjectOfType<RayInteractionFullscreen>();
            thisCollider = GetComponent<Collider>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            thisCollider.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = rayInteractionFullscreen.RayInfo.position.ToVector3_FlipZ();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            thisCollider.enabled = true;
        }
    }
}