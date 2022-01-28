//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using XR;
//namespace SpaceDesign
//{
//    public class Image2DTrackingTranslate : TrackableEventHandler
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
//            //objTargetModel = XRCameraManager.Instance.stereoCamera.transform.Find("TtackingManager/root/child").gameObject;
//            //objTargetModel = Image2DTrackingManager.Instance.transform.Find("root/child");
//            //objShow3 = objTargetModel.Find("Capsule").gameObject;
//            SetModelVisible(false);

//            //v3PosOri = TranslateManage.Inst.transform.position ;
//            //v3RotOri = TranslateManage.Inst.transform.eulerAngles;
//        }

//        public void StartTrack()
//        {
//            if (JudgeGo() == false)
//                return;

//            Debug.Log("TranslateMark:StartTrackTranslate");
//#if UNITY_EDITOR
//            SetModelVisible(true);
//            return;
//#endif
//            if (Image2DTrackingManager.Instance == null)
//                return;

//            StopTrack();
//            bCallback = true;
//            //白纸
//            //Image2DTrackingManager.Instance.m_TrackerPath = "Translate";
//            //Image2DTrackingManager.Instance.m_feamName = "7c47ed6410de1f914d06e1075c52e1e8_01112021175446";
//            //深海
//            //Image2DTrackingManager.Instance.m_TrackerPath = "Translate";
//            //Image2DTrackingManager.Instance.m_feamName = "95e8a7c9d1b01e2720be6ba323479b32_06012022095734";
//            //OPPO介绍
//            Image2DTrackingManager.Instance.m_TrackerPath = "Translate";
//            Image2DTrackingManager.Instance.m_feamName = "c409698f155ec8bd0a7ce037f7fa0d96_17012022094900";

//            Image2DTrackingManager.Instance.m_TargetTexture = texture;
//            Image2DTrackingManager.Instance.TrackStart();
//        }

//        public void StopTrack()
//        {
//            //if (JudgeGo() == false)
//            //    return;

//            Debug.Log("TranslateMark:StopTrack");
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
//            Debug.Log("TranslateMark:OnAddTacker");
//        }

//        public override void OnGetTrackerInfo()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnGetTrackerInfo();
//            if (bCallback == false) return;
//            Debug.Log("TranslateMark:OnGetTrackerInfo");
//        }

//        public override void OnStart()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnStart();
//            if (bCallback == false) return;
//            Debug.Log("TranslateMark:OnStart");
//            SetModelVisible(false);
//        }

//        public override void OnStop()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnStop();
//            Debug.Log("TranslateMark:OnStop");
//            SetModelVisible(false);

//            bCallback = false;
//        }

//        public override void OnFindTarget()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnFindTarget();
//            if (bCallback == false) return;
//            Debug.Log("TranslateMark:OnFindTarget");
//            SetModelVisible(true);
//        }

//        public override void OnLossTarget()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnLossTarget();
//            if (bCallback == false) return;
//            Debug.Log("TranslateMark:OnLossTarget");
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
//        //        if (TranslateManage.Inst == null)
//        //            return;

//        //        TranslateManage.Inst.transform.position = objTargetModel.position + v3PosOff;
//        //        TranslateManage.Inst.transform.eulerAngles = objTargetModel.eulerAngles + v3RotOff;
//        //    }
//        //}

//        private void SetModelVisible(bool isVisible)
//        {
//            if (JudgeGo() == false)
//                return;

//            bMarking = isVisible;
//            if (objTargetModel == null)
//                return;
//            if (TranslateManage.Inst == null)
//                return;

//            Debug.Log("TranslateMark:bMarking" + bMarking);
//            if (bMarking)
//            {
//                TranslateManage.Inst.transform.SetParent(objTargetModel);
//                TranslateManage.Inst.transform.localPosition = new Vector3(0, 0.1f, -0.25f);
//                //TranslateManage.Inst.transform.localPosition = new Vector3(0, 0 - 0.25f);
//                TranslateManage.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);
//            }
//            else
//            {
//                //TranslateManage.Inst.transform.position = v3PosOri;
//                //TranslateManage.Inst.transform.eulerAngles = v3RotOri;
//                TranslateManage.Inst.transform.SetParent(oriParent);
//                //看不到后隐藏对象
//                TranslateManage.Inst.OnQuit();
//            }
//            //OnQuit函数中会隐藏btnCheckDetail对象，这里如果是开启要放在最下面
//            TranslateManage.Inst.btnCheckTranslate.gameObject.SetActive(bMarking);
//        }

//        bool JudgeGo()
//        {
//            if (TranslateManage.Inst.curPlayerPosState == PlayerPosState.Close)
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