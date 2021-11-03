using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXDemo : MonoBehaviour
{

    public List<GameObject> vfxs;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickVFX(int id)
    {
        for (int i=0;i<vfxs.Count;i++) {
            vfxs[i].SetActive(false);
        }
        vfxs[id].SetActive(true);
    }
}
