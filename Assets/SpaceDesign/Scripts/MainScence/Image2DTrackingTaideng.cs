//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using XR;
//using UnityEngine.SceneManagement;

//namespace SpaceDesign
//{
//    public class Image2DTrackingTaideng : TrackableEventHandler
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
//        ////Mark识别后，显隐的对象
//        //public GameObject objModel;
//        ////Mark识别后，显隐的对象2号
//        //public GameObject objUI;
//        public Texture texture;
//        bool bCallback = false;
//        //原来的父节点
//        private Transform oriParent;

//        bool bMarking = false;
//        Vector3 v3OriPos;
//        Vector3 v3OriRot;

//        private void Awake()
//        {
//            oriParent = transform.parent;
//            SetModelVisible(false);
//        }

//        public void StartTrack()
//        {
//            if (JudgeGo() == false)
//                return;

//            Debug.Log("TaiDengMark:StartTrackTaideng");
//#if UNITY_EDITOR
//            return;
//#endif
//            if (Image2DTrackingManager.Instance == null)
//                return;
//            //Debug.Log("MyLog::StartTrack");
//            StopTrack();

//            bCallback = true;

//            //Image2DTrackingManager.Instance.m_TrackerPath = "Taideng";
//            //Image2DTrackingManager.Instance.m_feamName = "3c11f9a3e98c523a45f23f07b76586ab_28062021145112";
//            Image2DTrackingManager.Instance.m_TrackerPath = "Taideng";
//            Image2DTrackingManager.Instance.m_feamName = "fdf32e7865fe1f195eee4f5444e8ef65_30102021172742";
//            Image2DTrackingManager.Instance.m_TargetTexture = texture;
//            Image2DTrackingManager.Instance.TrackStart();
//        }

//        public void StopTrack()
//        {
//            //if (JudgeGo() == false)
//            //    return;

//            Debug.Log("TaiDengMark:StopTrack");
//#if UNITY_EDITOR
//            return;
//#endif
//            if (Image2DTrackingManager.Instance == null)
//                return;

//            if (Image2DTrackingManager.Instance.m_TrackerPath == null || Image2DTrackingManager.Instance.m_TrackerPath == "")
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
//            Debug.Log("TaiDengMark:OnAddTacker");
//        }

//        public override void OnGetTrackerInfo()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnGetTrackerInfo();
//            if (bCallback == false) return;
//            Debug.Log("TaiDengMark:OnGetTrackerInfo");
//        }

//        public override void OnStart()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnStart();
//            if (bCallback == false) return;
//            Debug.Log("TaiDengMark:OnStart");
//            SetModelVisible(false);
//        }

//        public override void OnStop()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnStop();
//            Debug.Log("TaiDengMark:OnStop");
//            SetModelVisible(false);

//            bCallback = false;
//        }

//        public override void OnFindTarget()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnFindTarget();
//            if (bCallback == false) return;
//            Debug.Log("TaiDengMark:OnFindTarget");
//            SetModelVisible(true);
//        }

//        public override void OnLossTarget()
//        {
//            if (JudgeGo() == false)
//                return;

//            base.OnLossTarget();
//            if (bCallback == false) return;
//            Debug.Log("TaiDengMark:OnLossTarget");
//            SetModelVisible(false);
//        }

//        //是否开始计时（隐藏前先计时，确定没看到就隐藏）
//        bool bTiming;
//        float fTiming;
//        private void LateUpdate()
//        {
//            if (bMarking)
//            {
//                if (TaidengManager.Inst == null)
//                    return;

//                TaidengManager.Inst.transform.position = objTargetModel.position;

//                Vector3 _v3 = PlayerManage.Inst.transform.position;
//                Transform _tra = TaidengManager.Inst.transform;
//                _v3.y = _tra.position.y;
//                _tra.LookAt(_v3, Vector3.up);
//                _tra.forward = -_tra.forward;
//            }
//        }

//        void Update()
//        {
//            if (bTiming)
//            {
//                fTiming += Time.deltaTime;
//                if (fTiming > 3)
//                {
//                    fTiming = 0;
//                    bTiming = false;

//                    Debug.Log("TaiDengMark：计时够3秒了");

//                    if (TaidengManager.Inst == null)
//                        return;

//                    //TaidengManager.Inst.transform.SetParent(oriParent);
//                    TaidengManager.Inst.taidengController.ShowMark(false);
//                    Debug.Log("TaiDengMark：对象隐藏了");

//                }
//            }
//        }

//        private void SetModelVisible(bool isVisible)
//        {
//            if (JudgeGo() == false)
//                return;

//            Debug.Log("TaiDengMark:SetModelVisible-" + bMarking);

//            bMarking = isVisible;
//            if (objTargetModel == null)
//                return;

//            if (TaidengManager.Inst == null)
//                return;

//            fTiming = 0;

//            Debug.Log("TaiDengMark：计时清零：bMarking" + bMarking + "---fTiming :" + fTiming);

//            //objTargetModel.SetActive(isVisible);
//            if (bMarking)
//            {
//                bTiming = false;
//                //TaidengManager.Inst.transform.SetParent(objTargetModel);
//                //TaidengManager.Inst.transform.localPosition = new Vector3(0, -0.15f, 0);
//                //TaidengManager.Inst.transform.localEulerAngles = new Vector3(0, 180f, 0);
//                TaidengManager.Inst.taidengController.ShowMark(true);
//            }
//            else
//            {
//                bTiming = true;
//                //不直接隐藏，计时3秒，还没看到，直接隐藏
//            }
//        }

//        //void SetIconParent()
//        //{
//        //    //Debug.Log("MyLog::GetActiveScene:"+ SceneManager.GetActiveScene().name.Equals("EditorScence"));
//        //    //Debug.Log("MyLog::TaidengManager:" + TaidengManager.Inst);
//        //    //Debug.Log("MyLog::EditorControl:" + EditorControl.Instance);
//        //    //Debug.Log("MyLog::loadPreviewScence:" + EditorControl.Instance.loadPreviewScence);
//        //    //Debug.Log("MyLog::ObjParent:" + EditorControl.Instance.loadPreviewScence.ObjParent);
//        //    if (SceneManager.GetActiveScene().name.Equals("EditorScence"))
//        //        TaidengManager.Inst.transform.SetParent(EditorControl.Instance.loadPreviewScence.ObjParent);
//        //    else
//        //        TaidengManager.Inst.transform.SetParent(null);
//        //}

//        bool JudgeGo()
//        {
//            if (TaidengManager.Inst.curPlayerPosState == PlayerPosState.Close)
//            {
//                return true;
//            }
//            else
//            {
//                bTiming = false;
//                fTiming = 0;
//                bMarking = false;
//                return false;
//            }
//        }
//    }
//}
