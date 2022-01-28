//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using XR;
//namespace SpaceDesign
//{
//    public class Image2DTrackingMagazine : TrackableEventHandler
//    {
//        //Mark识别后，目标出现（移动）的对象，现在把要用的移动到目标位置
//        private Transform objTarget;
//        //有切换场景，所以这里不能在Awake只找一次
//        private Transform objTargetModel
//        {
//            get
//            {
//                if (objTarget == null)
//                {
//                    objTarget = Image2DTrackingManager.Instance?.transform.Find("root/child");
//                }
//                return objTarget;
//            }
//        }
//        public Texture texture;
//        private bool bCallback = false;
//        //原来的父节点
//        private Transform oriParent;
//        /// <summary>
//        /// 是否识别到
//        /// </summary>
//        public bool bMarking = false;
//        private void Awake()
//        {
//            oriParent = transform.parent;
//            //objTargetModel = XRCameraManager.Instance.stereoCamera.transform.Find("TrackingManager/root/child").gameObject;
//            //objTargetModel = Image2DTrackingManager.Instance.transform.Find("root/child");
//            //objShow3 = objTargetModel.Find("Capsule").gameObject;
//            SetModelVisible(false);

//            //v3PosOri = MagazineManage.Inst.transform.position ;
//            //v3RotOri = MagazineManage.Inst.transform.eulerAngles;
//        }

//        public void StartTrack()
//        {
//            if (JudgeGo() == false)
//                return;

//            Debug.Log("MagazineMark::StartTrackMagazine");
//#if UNITY_EDITOR
//            return;
//#endif
//            if (Image2DTrackingManager.Instance == null)
//                return;

//            StopTrack();

//            bCallback = true;

//            Image2DTrackingManager.Instance.m_TrackerPath = "Magazine";
//            Image2DTrackingManager.Instance.m_feamName = "1bb2712acfe06c2b85bca344893cf383_21102021003912";
//            Image2DTrackingManager.Instance.m_TargetTexture = texture;
//            Image2DTrackingManager.Instance.TrackStart();
//        }

//        public void StopTrack()
//        {
//            //if (JudgeGo() == false)
//            //    return;

//            Debug.Log("MagazineMark::StopTrack");
//#if UNITY_EDITOR
//            SetModelVisible(false);
//            return;
//#endif
//            if (Image2DTrackingManager.Instance == null)
//                return;

//            if (String.IsNullOrEmpty(Image2DTrackingManager.Instance.m_TrackerPath))
//                return;
//            Image2DTrackingManager.Instance.TrackStop();
//            Image2DTrackingManager.Instance.m_TrackerPath = null;
//            Image2DTrackingManager.Instance.m_feamName = null;
//            Image2DTrackingManager.Instance.m_TargetTexture = null;
//        }

//        public override void OnAddTacker()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnAddTacker();
//            if (bCallback == false) return;
//            Debug.Log("MagazineMark:OnAddTacker");
//        }

//        public override void OnGetTrackerInfo()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnGetTrackerInfo();
//            if (bCallback == false) return;
//            Debug.Log("MagazineMark:OnGetTrackerInfo");
//        }

//        public override void OnStart()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnStart();
//            if (bCallback == false) return;
//            Debug.Log("MagazineMark:OnStart");
//            SetModelVisible(false);
//        }

//        public override void OnStop()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnStop();
//            Debug.Log("MagazineMark:OnStop");
//            SetModelVisible(false);

//            bCallback = false;
//        }

//        public override void OnFindTarget()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnFindTarget();
//            if (bCallback == false) return;
//            Debug.Log("MagazineMark:OnFindTarget");
//            SetModelVisible(true);
//        }

//        public override void OnLossTarget()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnLossTarget();
//            if (bCallback == false) return;
//            Debug.Log("MagazineMark:OnLossTarget");
//            SetModelVisible(false);
//        }

//        //Vector3 v3PosOri;
//        //Vector3 v3RotOri;

//        //Vector3 v3PosOff = new Vector3(0, 0.1f, -0.25f);
//        //Vector3 v3RotOff = new Vector3(0, 180f, 0);
//        //private void LateUpdate()
//        //{
//        //    if (bMarking)
//        //    {
//        //        if (MagazineManage.Inst == null)
//        //            return;

//        //        MagazineManage.Inst.transform.position = objTargetModel.position + v3PosOff;
//        //        MagazineManage.Inst.transform.eulerAngles = objTargetModel.eulerAngles + v3RotOff;
//        //    }
//        //}


//        private void SetModelVisible(bool isVisible)
//        {
//            if (JudgeGo() == false)
//                return;

//            bMarking = isVisible;
//            if (objTargetModel == null)
//                return;

//            if (MagazineManage.Inst == null)
//                return;

//            Debug.Log("MagazineMark:bMarking:" + bMarking);

//            if (bMarking)
//            {
//                MagazineManage.Inst.transform.SetParent(objTargetModel);
//                MagazineManage.Inst.transform.localPosition = new Vector3(0, 0.1f, -0.25f);
//                //MagazineManage.Inst.transform.localPosition = new Vector3(0, 0, -0.25f);
//                MagazineManage.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);
//            }
//            else
//            {
//                //MagazineManage.Inst.transform.position = v3PosOri;
//                //MagazineManage.Inst.transform.eulerAngles = v3RotOri;
//                MagazineManage.Inst.transform.SetParent(oriParent);
//                //看不到后隐藏对象
//                MagazineManage.Inst.OnQuit();
//            }
//            //OnQuit函数中会隐藏btnCheckDetail对象，这里如果是开启要放在最下面
//            MagazineManage.Inst.btnCheckDetail.gameObject.SetActive(bMarking);
//        }

//        bool JudgeGo()
//        {
//            if (MagazineManage.Inst.curPlayerPosState == PlayerPosState.Close)
//            {
//                return true;
//            }
//            else
//            {
//                bMarking = false;
//                return false;
//            }
//        }
//    }
//}