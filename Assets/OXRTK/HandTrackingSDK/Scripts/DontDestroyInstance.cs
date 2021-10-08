using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

public class DontDestroyInstance : MonoBehaviour
{
    public static DontDestroyInstance instance = null;
    public DontDestroyInstance()
    {

    }

    public XRManager xrManager;
    public XRWindowBase xrWindow;

    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            instance = this;
            xrManager.gameObject.SetActive(true);
            xrWindow.gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
        }
    }
}
