﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
namespace SpaceDesign.Magazine
{
    public class Image2DTrackingMagazine : TrackableEventHandler
    {
        //Mark识别后，目标出现（移动）的对象，现在把要用的移动到目标位置
        private GameObject objTargetModel;
        //Mark识别后，显隐的对象
        public GameObject objBtnShow;
        public Texture texture;
        bool bCallback = false;
        private void Awake()
        {
            objTargetModel = XRCameraManager.Instance.stereoCamera.transform.Find("TtackingManager/root/child").gameObject;
            SetModelVisible(false);
        }

        public void StartTrack()
        {
            StopTrack();

            bCallback = true;

            Image2DTrackingManager.Instance.m_TrackerPath = "Magazine";
            Image2DTrackingManager.Instance.m_feamName = "1bb2712acfe06c2b85bca344893cf383_21102021003912";
            Image2DTrackingManager.Instance.m_TargetTexture = texture;
            Image2DTrackingManager.Instance.TrackStart();
        }

        public void StopTrack()
        {
            Image2DTrackingManager.Instance.TrackStop();
        }

        public override void OnAddTacker()
        {
            base.OnAddTacker();
            if (bCallback == false) return;
            Debug.Log("Image2DTrackingDemoLog:OnAddTacker");
        }

        public override void OnGetTrackerInfo()
        {
            base.OnGetTrackerInfo();
            if (bCallback == false) return;
            Debug.Log("Image2DTrackingDemoLog:OnGetTrackerInfo");
        }

        public override void OnStart()
        {
            base.OnStart();
            if (bCallback == false) return;
            Debug.Log("Image2DTrackingDemoLog:OnStart");
            SetModelVisible(false);
        }

        public override void OnStop()
        {
            base.OnStop();
            Debug.Log("Image2DTrackingDemoLog:OnStop");
            SetModelVisible(false);

            bCallback = false;
        }

        public override void OnFindTarget()
        {
            base.OnFindTarget();
            if (bCallback == false) return;
            Debug.Log("Image2DTrackingDemoLog:OnFindTarget");
            SetModelVisible(true);
        }

        public override void OnLossTarget()
        {
            base.OnLossTarget();
            if (bCallback == false) return;
            Debug.Log("Image2DTrackingDemoLog:OnLossTarget");
            SetModelVisible(false);
        }


        private void SetModelVisible(bool isVisible)
        {
            //objTargetModel.SetActive(isVisible);
            if (isVisible)
            {
                MagazineManage.Inst.transform.SetParent(objTargetModel.transform);
                MagazineManage.Inst.transform.localPosition = new Vector3(0, -0.05f, -0.3f);
                MagazineManage.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);
            }
            else
            {
                MagazineManage.Inst.transform.SetParent(null);
            }
            objBtnShow.SetActive(isVisible);
        }
    }
}