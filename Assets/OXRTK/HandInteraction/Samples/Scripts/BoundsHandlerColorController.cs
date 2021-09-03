using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsHandlerColorController : MonoBehaviour
{
    Color m_HighlightColor;
    Color m_PinchColor;
    Color m_NormalColor;
    public void Init(Color highlightedColor, Color pressedColor)
    {
        if (transform.childCount > 0 && transform.GetChild(0).GetComponent<MeshRenderer>() != null)
        {
            if (transform.GetChild(0).GetComponent<MeshRenderer>().material.HasProperty("_Color"))
            {
                m_NormalColor = transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_Color");
            }
        }
        m_HighlightColor = highlightedColor;
        m_PinchColor = pressedColor;

        if (gameObject.GetComponent<BoundingBoxTouchableReceiverHelper>() != null)
        {
            gameObject.GetComponent<BoundingBoxTouchableReceiverHelper>().onInteractionEnabled += OnInteractionEnabled;
            gameObject.GetComponent<BoundingBoxTouchableReceiverHelper>().onInteractionDisabled += OnInteractionDisabled;
            gameObject.GetComponent<BoundingBoxTouchableReceiverHelper>().onPinchDown += OnPinchDown;
            gameObject.GetComponent<BoundingBoxTouchableReceiverHelper>().onPinchUp += OnPinchUp;
        }

        if (gameObject.GetComponent<BoundingBoxRayReceiverHelper>() != null)
        {
            gameObject.GetComponent<BoundingBoxRayReceiverHelper>().onPointerEnter += OnInteractionEnabled;
            gameObject.GetComponent<BoundingBoxRayReceiverHelper>().onPointerExit += OnInteractionDisabled;
            gameObject.GetComponent<BoundingBoxRayReceiverHelper>().onPinchDown += OnPinchDown;
            gameObject.GetComponent<BoundingBoxRayReceiverHelper>().onPinchUp += OnPinchUp;
        }
    }

    void OnInteractionEnabled()
    {
        SetHandlerColor(m_HighlightColor);
    }

    void OnInteractionDisabled()
    {
        SetHandlerColor(m_NormalColor);
    }

    void OnPinchDown()
    {
        SetHandlerColor(m_PinchColor);
    }

    void OnPinchUp()
    {
        SetHandlerColor(m_HighlightColor);
    }

    void SetHandlerColor(Color col)
    {
        if (transform.childCount > 0 && transform.GetChild(0).GetComponent<MeshRenderer>() != null)
        {
            if (transform.GetChild(0).GetComponent<MeshRenderer>().material.HasProperty("_Color"))
            {
                transform.GetChild(0).GetComponent<MeshRenderer>().material.color = col;
            }
        }
    }
}
