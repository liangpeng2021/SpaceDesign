using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjFormatAnalyzerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var go = ObjFormatAnalyzerFactory.AnalyzeToGameObject(@"E:\LP\opporoom\testLoadAsset\long.obj");
    }
}
