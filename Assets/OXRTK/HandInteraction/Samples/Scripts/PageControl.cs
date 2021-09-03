using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageControl : MonoBehaviour
{
    public PageManager pageManager;
    public int pageId = 0;

    public void OnButtonClick()
    {
        if(pageManager != null)
        {
            pageManager.UpdatePage(pageId);
        }
    }
}
