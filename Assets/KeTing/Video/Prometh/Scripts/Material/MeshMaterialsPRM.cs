using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using prometheus;

public class MeshMaterialsPRM : MonoBehaviour
{
    Renderer mRendererComponent;
    public List<Material> materials;
    // Start is called before the first frame update
    void Start()
    {
        mRendererComponent = GetComponent<MeshPlayerPRM>().rendererComponent;
    }

    public void SetMaterial(int i)
    {
        mRendererComponent.material = materials[i];
    }
}
