using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ttt : MonoBehaviour
{
    public TextMesh t;
    // Start is called before the first frame update
    void Start()
    {
        if (t == null)
            t = GetComponentInChildren<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        t.text = $"pos:{transform.localPosition}\n" +
            $"eur:{transform.localEulerAngles}\n" +
            $"sca:{transform.localScale}";
    }
}
