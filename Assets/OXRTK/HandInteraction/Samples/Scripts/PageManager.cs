using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
    public GameObject[] pages;
    private void Start()
    {
        UpdatePage(0);
    }

    public void UpdatePage(int id)
    {
        if (pages == null)
            return;
        if (id >= pages.Length)
            return;

        for(int i = 0; i < pages.Length; i++)
        {
            if (i == id)
                pages[i].SetActive(true);
            else
                pages[i].SetActive(false);
        }
    }
}
