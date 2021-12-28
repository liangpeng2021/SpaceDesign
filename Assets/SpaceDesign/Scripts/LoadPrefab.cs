using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 存储预设，/*create by 梁鹏 2021-10-22 */
/// </summary>
public class LoadPrefab : MonoBehaviour
{
    public Prefab3D[] prefab3Ds;

    public static Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();

    //------------ Modify by zh ------------
    public static IconDisData IconDisData = new IconDisData();
    //------------------End------------------

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefabDic.Add(prefab3Ds[i].id, prefab3Ds[i].prefab3d);
        }
    }
}
