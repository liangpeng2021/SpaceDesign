using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
/// <summary>
/// 总控制脚本，/*create by 梁鹏 2021-9-6 */
/// </summary>
public class GameManager : MonoBehaviour
{
    public Transform eyeTran;
    /// <summary>
    /// 需要显示的物体
    /// </summary>
    public Transform[] showTrans;
    
    /// <summary>
    /// 显示的物体
    /// </summary>
    public Transform child;
    // Start is called before the first frame update
    void Start()
    {
        Image2DTrackingManager.Instance.TrackStart();
    }
    
    public void StopTrack()
    {
        Image2DTrackingManager.Instance.TrackStop();
        child.SetParent(null, true);
        //child.localScale = Vector3.one * 0.2f;
        //child.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        for (int i = 0; i < showTrans.Length; i++)
        {
            if (Vector3.Distance(showTrans[i].position,eyeTran.position) < 1f)
            {
                showTrans[i].gameObject.SetActive(true);
            }
            else
                showTrans[i].gameObject.SetActive(false);
        }
    }
}
