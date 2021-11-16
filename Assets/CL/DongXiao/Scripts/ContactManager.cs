using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContactManager : MonoBehaviour
{
    public GameObject[] chuDian;
    public GameObject cam;

    public Button jietong;
    public Button guaduan;
    public GameObject laidian;
    // Start is called before the first frame update
    void Start()
    {
        laidian.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //5米以外
        {
            for (int i = 0; i < chuDian.Length; i++)
            {
                chuDian[i].SetActive(true);
                chuDian[i].GetComponent<Animator>().enabled = false;
            }
        }
        //5米以内
        {
            for (int i = 0; i < chuDian.Length; i++)
            {
                chuDian[i].SetActive(true);
                chuDian[i].GetComponent<Animator>().enabled = true;
            }
        }
        //1.5米以内
        {
            for (int i = 0; i < chuDian.Length; i++)
            {
                chuDian[i].SetActive(false);
            }
            laidian.SetActive(true);
        }
    }
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }
}
