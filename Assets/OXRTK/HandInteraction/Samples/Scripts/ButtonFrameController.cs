using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFrameController : MonoBehaviour
{
    public GameObject frameObject;
    public GameObject backBoard;

    public float highlightAlpha = 0.25f;
    public float pressedAlpha = 0.5f;

    public Color highlightColor;
    public Color pressedColor;
    public Color clickedColor;
    
    float m_NormalAlpha;
    Color m_NormalColor;

    void Start()
    {
        if (frameObject != null)
        {
            if (frameObject.GetComponent<MeshRenderer>() != null)
            {
                if (frameObject.GetComponent<MeshRenderer>().material.HasProperty("_BorderOpacity"))
                    m_NormalAlpha = frameObject.GetComponent<MeshRenderer>().material.GetFloat("_BorderOpacity");
                //pressableHandler.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        
        if (backBoard != null)
        {
            if (backBoard.GetComponent<MeshRenderer>() != null)
            {
                if (backBoard.GetComponent<MeshRenderer>().material.HasProperty("_BaseColor"))
                    m_NormalColor = backBoard.GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
            }
        }
    }
    public void OnInteractionEnabled()
    {
        if(frameObject != null)
        {
            if(frameObject.GetComponent<MeshRenderer>() != null)
            {
                frameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        SetHandlerAlpha(highlightAlpha);
        SetBackColor(highlightColor);
    }

    public void OnInteractionDisabled()
    {
        if(frameObject != null)
        {
            if (frameObject.GetComponent<MeshRenderer>() != null)
            {
                frameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        SetHandlerAlpha(m_NormalAlpha);
        SetBackColor(m_NormalColor);
    }

    public void OnStartPress()
    {
        SetHandlerAlpha(pressedAlpha);
        SetBackColor(pressedColor);
    }

    public void OnEndPress()
    {
        SetHandlerAlpha(highlightAlpha);
        SetBackColor(highlightColor);
    }

    public void OnClick()
    {
        //SetHandlerAlpha(highlightAlpha);
        SetBackColor(clickedColor);
    }


    void SetHandlerAlpha(float alpha)
    {
        if (frameObject != null && frameObject.GetComponent<MeshRenderer>() != null)
        {
            if (frameObject.GetComponent<MeshRenderer>().material.HasProperty("_BorderOpacity"))
            {
                frameObject.GetComponent<MeshRenderer>().material.SetFloat("_BorderOpacity", alpha);
            }
        }
    }
    
    void SetBackColor(Color col)
    {
        if (backBoard != null && backBoard.GetComponent<MeshRenderer>() != null)
        {
            if (backBoard.GetComponent<MeshRenderer>().material.HasProperty("_BaseColor"))
            {
                backBoard.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", col);
            }
        }
    }
}
