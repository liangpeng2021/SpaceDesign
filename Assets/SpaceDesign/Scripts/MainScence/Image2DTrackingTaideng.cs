﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
using UnityEngine.SceneManagement;

namespace SpaceDesign
{
    public class Image2DTrackingTaideng : TrackableEventHandler
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
        public GameObject objModel;
        //Mark识别后，显隐的对象2号
        public GameObject objUI;
        public Texture texture;
        bool bCallback = false;
        //原来的父节点
        private Transform oriParent;
        private void Awake()
        {
            oriParent = transform.parent;
            //objTargetModel = XRCameraManager.Instance.stereoCamera.transform.Find("TtackingManager/root/child").gameObject;
            //objTargetModel = Image2DTrackingManager.Instance.transform.Find("root/child");
            //objShow3 = objTargetModel.Find("Capsule").gameObject;
            SetModelVisible(false);
        }

        public void StartTrack()
        {
#if UNITY_EDITOR
            return;
#endif
            //Debug.Log("MyLog::StartTrack");
            StopTrack();

            bCallback = true;

            //Image2DTrackingManager.Instance.m_TrackerPath = "Taideng";
            //Image2DTrackingManager.Instance.m_feamName = "3c11f9a3e98c523a45f23f07b76586ab_28062021145112";
            Image2DTrackingManager.Instance.m_TrackerPath = "Taideng";
            Image2DTrackingManager.Instance.m_feamName = "fdf32e7865fe1f195eee4f5444e8ef65_30102021172742";
            Image2DTrackingManager.Instance.m_TargetTexture = texture;
            Image2DTrackingManager.Instance.TrackStart();
        }

        public void StopTrack()
        {
#if UNITY_EDITOR
            return;
#endif
            //Debug.Log("MyLog::StopTrack");
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
                //Debug.Log("MyLog::objTargetModel:" + objTargetModel);
                TaidengManager.Inst.transform.SetParent(objTargetModel);
                TaidengManager.Inst.transform.localPosition = new Vector3(0, -0.15f, 0);
                TaidengManager.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);
                //TaidengManager.Inst.transform.localScale = new Vector3(1f, 1f, 1f);

                TaidengManager.Inst.taidengController.ResetTaidengTra();

                //翻译按钮缩放，代替显隐
                objModel.SetActive(true);
                objUI.transform.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
                //objShow2.transform.localScale = Vector3.one;
            }
            else
            {
                //Invoke("SetIconParent", 0.1f);
                TaidengManager.Inst.transform.SetParent(oriParent);

                //翻译按钮不能隐藏，要缩放到0，防止隐藏动画播放未完成bug
                objModel.SetActive(false);
                objUI.transform.localScale = Vector3.zero;
            }
            //#if UNITY_EDITOR
            //            return;
            //#endif
            //            objBtnShow.SetActive(isVisible);
            //            objShow2.SetActive(isVisible);
        }

        //void SetIconParent()
        //{
        //    //Debug.Log("MyLog::GetActiveScene:"+ SceneManager.GetActiveScene().name.Equals("EditorScence"));
        //    //Debug.Log("MyLog::TaidengManager:" + TaidengManager.Inst);
        //    //Debug.Log("MyLog::EditorControl:" + EditorControl.Instance);
        //    //Debug.Log("MyLog::loadPreviewScence:" + EditorControl.Instance.loadPreviewScence);
        //    //Debug.Log("MyLog::ObjParent:" + EditorControl.Instance.loadPreviewScence.ObjParent);
        //    if (SceneManager.GetActiveScene().name.Equals("EditorScence"))
        //        TaidengManager.Inst.transform.SetParent(EditorControl.Instance.loadPreviewScence.ObjParent);
        //    else
        //        TaidengManager.Inst.transform.SetParent(null);
        //}
    }
}
