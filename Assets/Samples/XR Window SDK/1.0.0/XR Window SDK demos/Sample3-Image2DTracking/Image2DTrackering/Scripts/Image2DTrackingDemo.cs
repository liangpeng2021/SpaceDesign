using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XR;
public class Image2DTrackingDemo : TrackableEventHandler
{
    public GameObject targetModel;
    public Image StatusImage;

    private void Awake()
    {
        SetModelVisible(false);
    }

    public void StartTrack()
    {
        Image2DTrackingManager.Instance.TrackStart();
    }

    public void StopTrack()
    {
        Image2DTrackingManager.Instance.TrackStop();
    }
    /// <summary>
    /// 切换模型
    /// </summary>
    public void ChangeTracker() {

        //Image2DTrackingManager.Instance.ChangeTracker("传入FeamName", "传入Feam所在文件夹名称");

    }

    public override void OnAddTacker()
    {
        base.OnAddTacker();
        Debug.Log("Image2DTrackingDemoLog:OnAddTacker");
    }

    public override void OnGetTrackerInfo()
    {
        base.OnGetTrackerInfo();
        Debug.Log("Image2DTrackingDemoLog:OnGetTrackerInfo");
        StatusImage.color = Color.green;
    }

    public override void OnStart()
    {
        base.OnStart();
        Debug.Log("Image2DTrackingDemoLog:OnStart");
        SetModelVisible(false);
        StatusImage.color = Color.white;
    }

    public override void OnStop()
    {
        base.OnStop();
        Debug.Log("Image2DTrackingDemoLog:OnStop");
        SetModelVisible(false);
        StatusImage.color = Color.white;
    }

    public override void OnFindTarget()
    {
        base.OnFindTarget();
        Debug.Log("Image2DTrackingDemoLog:OnFindTarget");
        SetModelVisible(true);
    }

    public override void OnLossTarget()
    {
        base.OnLossTarget();
        Debug.Log("Image2DTrackingDemoLog:OnLossTarget");
        SetModelVisible(false);
    }


    private void SetModelVisible(bool isVisible)
    {
        targetModel.SetActive(isVisible);
    }
}