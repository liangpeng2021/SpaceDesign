using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 存储预设，/*create by 梁鹏 2021-10-22 */
/// </summary>
public class LoadPrefab : MonoBehaviour
{
    public Prefab3D[] prefab3Ds;

    public Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();

    public static LoadPrefab Instance;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefabDic.Add(prefab3Ds[i].id, prefab3Ds[i].prefab3d);
        }
    }
}
