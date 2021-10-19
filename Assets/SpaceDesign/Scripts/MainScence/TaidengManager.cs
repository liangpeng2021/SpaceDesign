using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 管理台灯的部分效果，/*create by 梁鹏 2021-10-18 */
/// </summary>
public class TaidengManager : MonoBehaviour
{
    //人物和Icon的距离状态
    public PlayerPosState curPlayerPosState = PlayerPosState.Far;
    //Icon、UI等正在切换中
    bool bUIChanging = false;
    //运动阈值
    float fThreshold = 0.05f;

    //吸引态，上下移动动画
    public Animator animIconFar;
    //轻交互，半球动画+音符动画
    public Animator[] animIconMiddle;
    /// <summary>
    /// 台灯和扫描片
    /// </summary>
    public GameObject taidengObj;

    //Icon的移动速度
    public float fIconMoveSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        animIconFar.enabled = true;
        taidengObj.transform.parent = XR.XRCameraManager.Instance.stereoCamera.transform;

        taidengObj.SetActive(false);
    }
    void OnEnable()
    {
        PlayerManage.refreshPlayerPosEvt += RefreshPos;
    }

    void OnDisable()
    {
        PlayerManage.refreshPlayerPosEvt -= RefreshPos;
    }
    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 刷新位置消息
    /// </summary>
    public void RefreshPos(Vector3 pos)
    {
        if (bUIChanging == true)
            return;

        Vector3 _v3 = transform.position;
        _v3.y = pos.y;
        float _dis = Vector3.Distance(_v3, pos);
        
        PlayerPosState lastPPS = curPlayerPosState;

        if (_dis >= 5f)
        {
            if (lastPPS == PlayerPosState.Far)
                return;
            curPlayerPosState = PlayerPosState.Far;
        }
        else if (_dis < 5f && _dis > 1.5f)
        {
            if (lastPPS == PlayerPosState.Middle)
                return;
            curPlayerPosState = PlayerPosState.Middle;
        }
        else
        {
            if (lastPPS == PlayerPosState.near)
                return;
            curPlayerPosState = PlayerPosState.near;
        }
        
        StartCoroutine("IERefreshPos", lastPPS);
    }

    /// <summary>
    /// UI等刷新位置消息
    /// </summary>
    IEnumerator IERefreshPos(PlayerPosState lastPPS)
    {
        //UI开始变化
        bUIChanging = true;

        if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
        {
            yield return IEFarToMiddle();
        }
        else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Far)
        {
            yield return IEMiddleToFar();
        }
        else if (lastPPS == PlayerPosState.near && curPlayerPosState == PlayerPosState.Middle)
        {
            yield return IEMiddleToNear();
        }

        yield return 0;
        //UI变化结束
        bUIChanging = false;
    }

    /// <summary>
    /// 中距离=>近距离
    /// </summary>
    IEnumerator IEMiddleToNear()
    {
        animIconFar.enabled = false;

        for (int i = 0; i < animIconMiddle.Length; i++)
        {
            animIconMiddle[i].enabled = false;
        }
        Vector3 targetPos = new Vector3(animIconFar.transform.localPosition.x, animIconFar.transform.localPosition.y-0.5f, animIconFar.transform.localPosition.z);
        while (true)
        {
            animIconFar.transform.localPosition = Vector3.MoveTowards(animIconFar.transform.localPosition, targetPos, fIconMoveSpeed * Time.deltaTime);
            animIconFar.transform.localScale = Vector3.MoveTowards(animIconFar.transform.localScale,Vector3.zero, fIconMoveSpeed * Time.deltaTime);
            float _fDis = Vector3.Distance(animIconFar.transform.localScale, Vector3.zero);
            if (_fDis < fThreshold)
            {
                animIconFar.transform.localPosition = targetPos;
                animIconFar.transform.localScale = Vector3.zero;
                taidengObj.SetActive(true);
                break;
            }

            yield return 0;
        }
    }

    /// <summary>
    /// 远距离=>中距离
    /// </summary>
    IEnumerator IEFarToMiddle()
    {
        animIconFar.enabled = false;

        for (int i = 0; i < animIconMiddle.Length; i++)
        {
            animIconMiddle[i].enabled = true;
        }

        yield return 0;
    }
    /// <summary>
    /// 中距离=>远距离
    /// </summary>
    IEnumerator IEMiddleToFar()
    {
        animIconFar.enabled = true;
        for (int i = 0; i < animIconMiddle.Length; i++)
        {
            animIconMiddle[i].enabled = false;
        }
        yield return 0;
    }
}
