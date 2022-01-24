using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDesign
{
    public class VideoManage2 : MonoBehaviour
    {
        static VideoManage2 inst;
        public static VideoManage2 Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<VideoManage2>();
                return inst;
            }
        }

        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        [SerializeField]
        private Vector3 v3OriPos;
        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================

        [Header("===Icon变化，远距离")]
        //Icon的对象
        public Transform traIcon;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;

        //卡丁车模式（绑定在卡丁车界面）
        public bool bKarting = false;
        //加载中等待的UI对象
        public GameObject objLoadingUI;

        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            HideLoadingUI();
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            HideLoadingUI();
        }

        void Start()
        {
            HideLoadingUI();
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            VideoUICtr.Inst.AutoSmall(pos);
            //if (bUIChanging == true)
            //    return;

            Vector3 _v3 = traIcon.position;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"目标的距离:{_dis}");

            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            float _fFar = LoadPrefab.IconDisData.VideoFar;
            float _fMid = LoadPrefab.IconDisData.VideoMiddle;

            if (_dis > _fFar)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else if (_dis <= _fFar && _dis > _fMid)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            else if (_dis <= _fMid)
            {
                curPlayerPosState = PlayerPosState.Close;
                if (lastPPS == PlayerPosState.Close)
                    return;
            }

            StopCoroutine("IERefreshPos");
            StartCoroutine("IERefreshPos", lastPPS);
        }


        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            //print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

            if (lastPPS == PlayerPosState.Far)
            {
                if (curPlayerPosState == PlayerPosState.Middle)/// 远距离=>中距离
                    yield return IEFarToMiddle();/// 远距离=>中距离
                else if (curPlayerPosState == PlayerPosState.Close)/// 远距离=>近距离
                {
                    yield return IEFarToMiddle();
                    yield return IEMiddleToClose();
                }
            }
            else if (lastPPS == PlayerPosState.Middle)
            {
                if (curPlayerPosState == PlayerPosState.Close)/// 中距离=>近距离
                    yield return IEMiddleToClose();

                else if (curPlayerPosState == PlayerPosState.Far)/// 中距离=>远距离
                    yield return IEMiddleToFar();
            }
            else if (lastPPS == PlayerPosState.Close)
            {
                if (curPlayerPosState == PlayerPosState.Middle)/// 近距离=>中距离
                    yield return IECloseToMiddle();
                else if (curPlayerPosState == PlayerPosState.Far)/// 近距离=>远距离
                {
                    yield return IECloseToMiddle();
                    yield return IEMiddleToFar();
                }
            }
            yield return 0;
        }

        /// <summary>
        /// 远距离=>中距离
        /// </summary>
        IEnumerator IEFarToMiddle()
        {
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (VideoUICtr.Inst.bOpenVideo)
                yield break;

            //UI开始变化
            bUIChanging = true;

            //远距离=>中距离
            //Icon从静态变成动态
            //Icon的自旋转动画开启
            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动开启
            animIconFar.enabled = true;
            traIcon.gameObject.SetActive(true);

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 中距离=>远距离
        /// </summary>
        IEnumerator IEMiddleToFar()
        {
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (VideoUICtr.Inst.bOpenVideo)
                yield break;

            //UI开始变化
            bUIChanging = true;

            //中距离=>远距离
            //Icon从动态变成静态
            //Icon的自旋转动画关闭
            foreach (var v in animIconMiddle)
            {
                v.Play(0, -1, 0f);
                v.Update(0);
                v.enabled = false;
            }
            //Icon自身上下浮动关闭
            animIconFar.enabled = false;
            traIcon.gameObject.SetActive(true);

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 中距离=>近距离
        /// </summary>
        IEnumerator IEMiddleToClose()
        {
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (VideoUICtr.Inst.bOpenVideo)
                yield break;

            //UI开始变化
            bUIChanging = true;

            //中距离=>近距离
            if (VideoUICtr.Inst.curVideoUIType != VideoUIType.Reminder)
                VideoUICtr.Inst.SetReminderUI(false, true);

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }
            //UI变化结束
            bUIChanging = false;

            yield return 0;
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            //开启电视的模式下（无论TV还是AR），都不判断了
            if (VideoUICtr.Inst.bOpenVideo)
                yield break;

            //UI开始变化
            bUIChanging = true;

            VideoUICtr.Inst.objWindowsBtnParent.SetActive(false);


            //中距离=>近距离
            if (VideoUICtr.Inst.curVideoUIType == VideoUIType.Reminder)
                VideoUICtr.Inst.SetReminderUI(false, false);
            //===========================================================================
            //这里为什么这么做忘记了，待修改
            //else if (bExpand == true)
            //    SetExpand(false);
            //===========================================================================

            Vector3 _v3Icon = LoadPrefab.IconSize;
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, _v3Icon, fUISpeed * 2f * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, _v3Icon);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = _v3Icon;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;
            yield return 0;
        }

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (bUIChanging)
                return;

            if (curPlayerPosState == PlayerPosState.Close)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
            }
        }

        /// <summary>
        /// TV模式下，播放后，隐藏加载提示UI（2D，3D根据各自的播放流程判断隐藏）
        /// </summary>
        void HideLoadingUI()
        {
            //print("隐藏了");

            if (objLoadingUI.activeSelf == true)
                objLoadingUI.SetActive(false);
        }
    }
}