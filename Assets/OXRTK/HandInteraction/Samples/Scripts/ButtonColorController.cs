using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonColorController : MonoBehaviour
{
    public GameObject pressableHandler;

    public Color highlightColor = new Color(0.99f, 1f, 0.45f, 1);

    public Color pressedColor = Color.white;
    Color m_NormalColor;

    void Start()
    {
        if (pressableHandler != null)
        {
            if (pressableHandler.GetComponent<MeshRenderer>() != null)
            {
                if (pressableHandler.GetComponent<MeshRenderer>().material.HasProperty("_Color"))
                    m_NormalColor = pressableHandler.GetComponent<MeshRenderer>().material.GetColor("_Color");
            }
        }
    }
    public void OnInteractionEnabled()
    {
        SetHandlerColor(highlightColor);
    }

    public void OnInteractionDisabled()
    {
        SetHandlerColor(m_NormalColor);
    }

    public void OnStartPress()
    {
        SetHandlerColor(pressedColor);
    }
    
    public void OnEndPress()
    {
        SetHandlerColor(highlightColor);
    }


    void SetHandlerColor(Color col)
    {
        if (pressableHandler != null && pressableHandler.GetComponent<MeshRenderer>() != null)
        {
            if (pressableHandler.GetComponent<MeshRenderer>().material.HasProperty("_Color"))
            {
                pressableHandler.GetComponent<MeshRenderer>().material.color = col;
            }
        }
    }
}
