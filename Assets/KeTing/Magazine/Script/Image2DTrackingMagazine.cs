using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
namespace SpaceDesign.Magazine
{
    public class Image2DTrackingMagazine : TrackableEventHandler
    {
        //Mark识别后，目标出现（移动）的对象，现在把要用的移动到目标位置
        private Transform objTarget;
        //有切换场景，所以这里不能在Awake只找一次
        private Transform objTargetModel
        {
            get
            {
                if (objTarget == null)
                {
                    objTarget = Image2DTrackingManager.Instance.transform.Find("root/child");
                }
                return objTarget;
            }
        }
        //Mark识别后，显隐的对象
        public GameObject objBtnShow;
        public Texture texture;
        private bool bCallback = false;
        //原来的父节点
        private Transform oriParent;
        private void Awake()
        {
            oriParent = transform.parent;
            //objTargetModel = XRCameraManager.Instance.stereoCamera.transform.Find("TrackingManager/root/child").gameObject;
            //objTargetModel = Image2DTrackingManager.Instance.transform.Find("root/child");
            //objShow3 = objTargetModel.Find("Capsule").gameObject;
            SetModelVisible(false);

            Image2DTrackingManager.Instance.OnAddTacker += AddTrack;
        }

        void AddTrack()
        {

        }

        public void StartTrack()
        {
            Debug.Log("MyLog::StartTrackMagazine");
#if UNITY_EDITOR
            return;
#endif

            StopTrack();

            bCallback = true;

            Image2DTrackingManager.Instance.m_TrackerPath = "Magazine";
            Image2DTrackingManager.Instance.m_feamName = "1bb2712acfe06c2b85bca344893cf383_21102021003912";
            Image2DTrackingManager.Instance.m_TargetTexture = texture;
            Image2DTrackingManager.Instance.TrackStart();
        }

        public void StopTrack()
        {
#if UNITY_EDITOR
            return;
#endif

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
            if (objTargetModel == null)
                return;

            //objTargetModel.SetActive(isVisible);
            if (isVisible)
            {
                MagazineManage.Inst.transform.SetParent(objTargetModel);
                MagazineManage.Inst.transform.localPosition = new Vector3(0, 0.02f, -0.25f);
                //MagazineManage.Inst.transform.localPosition = new Vector3(0, 0, -0.25f);
                MagazineManage.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);


                //按钮缩放，代替显隐
                //objBtnShow.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                objBtnShow.SetActive(true);
                //objBtnShow.transform.localScale = Vector3.one;
            }
            else
            {
                MagazineManage.Inst.transform.SetParent(oriParent);

                //按钮不能隐藏，要缩放到0，防止隐藏动画播放未完成bug
                //objBtnShow.transform.localScale = Vector3.zero;
                objBtnShow.SetActive(false);
            }
            //objBtnShow.SetActive(isVisible);
        }
    }
}