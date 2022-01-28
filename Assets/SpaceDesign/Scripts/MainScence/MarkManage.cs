using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

namespace SpaceDesign
{
    public enum MarkType
    {
        None,
        Magazine,
        Translate,
        Taideng,
    }

    public class MarkManage : TrackableEventHandler
    {
        static MarkManage inst;
        public static MarkManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<MarkManage>();
                return inst;
            }
        }

        //Mark识别后，目标出现（移动）的对象，现在把要用的移动到目标位置
        private Transform objTarget;
        //有切换场景，所以这里不能在Awake只找一次
        private Transform objTargetModel
        {
            get
            {
                if (objTarget == null)
                {
                    objTarget = Image2DTrackingManager.Instance?.transform.Find("root/child");
                }
                return objTarget;
            }
        }
        //当前的Mark对象
        public MarkType curMarkType = MarkType.None;

        MagazineManage magazine { get { return MagazineManage.Inst; } }
        TranslateManage translate { get { return TranslateManage.Inst; } }
        TaidengManager taideng { get { return TaidengManager.Inst; } }

        public static bool bVisibleMagazine = false;
        public static bool bVisibleTranslate = false;
        public static bool bVisibleTaideng = false;

        void OnEnable()
        {
            Log("OnEnable-Init");
            Init();
        }
        void OnDisable()
        {
            Log("OnDisable-Init");
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            curMarkType = MarkType.None;
            bVisibleMagazine = false;
            bVisibleTranslate = false;
            bVisibleTaideng = false;
            bTiming = false;
            fTiming = 0;
        }

        //是否开始计时（隐藏前先计时(手部临时遮挡等)，一定时间后确定没看到就隐藏）
        bool bTiming = false;
        float fTiming = 0;

        void Log(string s)
        {
            Debug.Log("Mark:" + s);
        }

        public void StartTrack(MarkType markType, Texture targetTexture)
        {
            //先停止一下之前的
            StopTrack(curMarkType);

            curMarkType = markType;
            if (curMarkType == MarkType.None)
            {
                Log("StartTrack-不执行-curMarkType:" + curMarkType);
                return;
            }

            Log("StartTrack-curMarkType:" + curMarkType);

#if UNITY_EDITOR
            return;
#endif

            if (Image2DTrackingManager.Instance == null)
            {
                Log("StartTrack-Image2DTrackingManager为空");
                return;
            }

            string m_TrackerPath = null;
            string m_feamName = null;
            switch (curMarkType)
            {
                case MarkType.Magazine:
                    m_TrackerPath = "Magazine";
                    //外乡人（A4尺寸）
                    m_feamName = "1bb2712acfe06c2b85bca344893cf383_21102021003912";
                    break;
                case MarkType.Translate:
                    m_TrackerPath = "Translate";
                    //FromeMe
                    //m_feamName = "7c47ed6410de1f914d06e1075c52e1e8_01112021175446";
                    //冰山
                    //m_feamName = "95e8a7c9d1b01e2720be6ba323479b32_06012022095734";
                    //OPPO介绍
                    //m_feamName = "c409698f155ec8bd0a7ce037f7fa0d96_17012022094900";
                    //数字孪生
                    m_feamName = "a10b4fde2bc9012393019c60d2e067e1_27012022161904";

                    break;
                case MarkType.Taideng:
                    m_TrackerPath = "Taideng";
                    m_feamName = "fdf32e7865fe1f195eee4f5444e8ef65_30102021172742";
                    break;
            }

            if (string.IsNullOrEmpty(m_TrackerPath))
            {
                Log("StartTrack-m_TrackerPath为空");
                return;
            }
            if (string.IsNullOrEmpty(m_feamName))
            {
                Log("StartTrack-m_feamName为空");
                return;
            }
            if (targetTexture == null)
            {
                Log("StartTrack-targetTexture为空");
                return;
            }

            Image2DTrackingManager.Instance.m_TrackerPath = m_TrackerPath;
            Image2DTrackingManager.Instance.m_feamName = m_feamName;
            Image2DTrackingManager.Instance.m_TargetTexture = targetTexture;
            Image2DTrackingManager.Instance.TrackStart();

        }

        public void StopTrack(MarkType markType)
        {
            if (curMarkType == MarkType.None || markType == MarkType.None)
            {
                Log("StopTrack-不执行-curMarkType:" + curMarkType);
                return;
            }

            if (Image2DTrackingManager.Instance == null)
            {
                Log("StopTrack-Image2DTrackingManager为空");
                return;
            }

            Log("StopTrack-curMarkType:" + curMarkType);

#if UNITY_EDITOR
            return;
#endif

            if (Image2DTrackingManager.Instance == null)
                return;

            if (string.IsNullOrEmpty(Image2DTrackingManager.Instance.m_TrackerPath))
                return;
            Image2DTrackingManager.Instance.TrackStop();
            Image2DTrackingManager.Instance.m_TrackerPath = null;
            Image2DTrackingManager.Instance.m_feamName = null;
            Image2DTrackingManager.Instance.m_TargetTexture = null;
        }

        public override void OnAddTacker()
        {
            base.OnAddTacker();
            Log("OnAddTacker-curMarkType:" + curMarkType);
        }

        public override void OnGetTrackerInfo()
        {
            base.OnGetTrackerInfo();
            Log("OnGetTrackerInfo-curMarkType:" + curMarkType);
        }

        public override void OnStart()
        {
            base.OnStart();
            Log("OnStart-curMarkType:" + curMarkType);
            SetModelVisible(false);
        }

        public override void OnStop()
        {
            base.OnStop();
            Log("OnStop-curMarkType:" + curMarkType);
            SetModelVisible(false);
            Init();
        }

        public override void OnFindTarget()
        {
            base.OnFindTarget();
            Log("OnFindTarget-curMarkType:" + curMarkType);
            SetModelVisible(true);
        }

        public override void OnLossTarget()
        {
            base.OnLossTarget();
            Log("OnLossTarget-curMarkType:" + curMarkType);
            SetModelVisible(false);
        }

        private void LateUpdate()
        {
            if (bVisibleTaideng)
            {
                if (taideng == null)
                    return;

                taideng.transform.position = objTargetModel.position;

                Vector3 _v3 = PlayerManage.Inst.transform.position;
                Transform _tra = taideng.transform;
                _v3.y = _tra.position.y;
                _tra.LookAt(_v3, Vector3.up);
                _tra.forward = -_tra.forward;
            }
        }

        void Update()
        {
            if (bTiming)
            {
                fTiming += Time.deltaTime;
                if (fTiming > 3)
                {
                    fTiming = 0;
                    bTiming = false;

                    Log("TaiDengMark：计时够3秒了");

                    if (taideng == null)
                        return;
                    taideng.taidengController.ShowMark(false);
                    Log("TaiDengMark：对象隐藏了");

                }
            }
        }

        private void SetModelVisible(bool isVisible)
        {
            if (curMarkType == MarkType.None)
            {
                Log("SetModelVisible-不执行-curMarkType:" + curMarkType);
                return;
            }

            Log($"SetModelVisible-curMarkType:{curMarkType}");

            if (objTargetModel == null)
                return;

            switch (curMarkType)
            {
                case MarkType.Magazine:
                    bVisibleMagazine = isVisible;
                    if (magazine == null)
                    {
                        Log("SetModelVisible-magazine为空");
                        return;
                    }

                    if (bVisibleMagazine)
                    {
                        magazine.transform.SetParent(objTargetModel);
                        magazine.transform.localPosition = new Vector3(0, 0.1f, -0.25f);
                        magazine.transform.localEulerAngles = new Vector3(0, 180f, 0);
                    }
                    else
                    {
                        magazine.transform.SetParent(magazine.traParent);
                        //看不到后隐藏对象
                        magazine.OnQuit();
                    }
                    //OnQuit函数中会隐藏btnCheckDetail对象，这里如果是开启要放在最下面
                    magazine.btnCheckDetail.gameObject.SetActive(bVisibleMagazine);

                    break;
                case MarkType.Translate:
                    bVisibleTranslate = isVisible;
                    if (translate == null)
                    {
                        Log("SetModelVisible-translate为空");
                        return;
                    }

                    if (bVisibleTranslate)
                    {
                        translate.transform.SetParent(objTargetModel);
                        translate.transform.localPosition = new Vector3(0, 0.1f, -0.25f);
                        translate.transform.localEulerAngles = new Vector3(0, 180f, 0);
                    }
                    else
                    {
                        translate.transform.SetParent(translate.traParent);
                        //看不到后隐藏对象
                        translate.OnQuit();
                    }
                    //OnQuit函数中会隐藏btnCheckDetail对象，这里如果是开启要放在最下面
                    translate.btnCheckTranslate.gameObject.SetActive(bVisibleTranslate);

                    break;
                case MarkType.Taideng:
                    fTiming = 0;
                    bVisibleTaideng = isVisible;
                    Log("SetModelVisible：台灯计时清零");

                    if (bVisibleTaideng)
                    {
                        bTiming = false;
                        taideng.taidengController.ShowMark(true);
                    }
                    else
                    {
                        bTiming = true;
                        //不直接隐藏，计时3秒，还没看到，直接隐藏
                    }
                    break;
            }
        }
    }
}