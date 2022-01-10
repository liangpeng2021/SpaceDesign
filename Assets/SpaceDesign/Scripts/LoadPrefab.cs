using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XR;
/// <summary>
/// 存储预设，/*create by 梁鹏 2021-10-22 */
/// </summary>
public class LoadPrefab : MonoBehaviour
{
    public Canvas[] canvas;

    //版本号
    public Text version;

    public Prefab3D[] prefab3Ds;

    public static Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();

    //------------ Modify by zh ------------
    public static IconDisData IconDisData = new IconDisData();
    public static Vector3 IconSize = new Vector3(1.7f, 1.7f, 1.7f);
    //------------------End------------------

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        version.text = Application.version;

        Camera eventCamera = XRCameraManager.Instance.eventCamera;
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i].worldCamera = eventCamera;
        }


        for (int i = 0; i < prefab3Ds.Length; i++)
        {
            prefabDic.Add(prefab3Ds[i].id, prefab3Ds[i].prefab3d);
        }
    }
}
