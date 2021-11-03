using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
namespace SpaceDesign.Translate
{
    public class Image2DTrackingTranslate : TrackableEventHandler
    {
        //Mark识别后，目标出现（移动）的对象，现在把要用的移动到目标位置
        private GameObject objTargetModel;
        //Mark识别后，显隐的对象
        public GameObject objBtnShow;
        public Texture texture;
        bool bCallback = false;
        //原来的父节点
        private Transform oriParent;
        private void Awake()
        {
            oriParent = transform.parent;
            //objTargetModel = XRCameraManager.Instance.stereoCamera.transform.Find("TtackingManager/root/child").gameObject;
            objTargetModel = Image2DTrackingManager.Instance.transform.Find("root/child").gameObject;
            SetModelVisible(false);
        }

        public void StartTrack()
        {
#if UNITY_EDITOR
            return;
#endif
            StopTrack();
            bCallback = true;

            //Image2DTrackingManager.Instance.m_TrackerPath = "Translate";
            //Image2DTrackingManager.Instance.m_feamName = "c2dd24647f17a05fdc273e7aa95bc674_21102021003932";
            Image2DTrackingManager.Instance.m_TrackerPath = "Translate";
            Image2DTrackingManager.Instance.m_feamName = "7c47ed6410de1f914d06e1075c52e1e8_01112021175446";
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
            //objTargetModel.SetActive(isVisible);
            if (isVisible)
            {
                TranslateManage.Inst.transform.SetParent(objTargetModel.transform);
                TranslateManage.Inst.transform.localPosition = new Vector3(0, 0.02f, -0.25f);
                TranslateManage.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);
            }
            else
            {
                TranslateManage.Inst.transform.SetParent(oriParent);
            }
            objBtnShow.SetActive(isVisible);
        }
    }
}