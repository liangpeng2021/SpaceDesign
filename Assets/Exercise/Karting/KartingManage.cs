/* Create by zh at 2021-10-11

    卡丁车整体控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SpaceDesign
{
    public class KartingManage : MonoBehaviour
    {
        static KartingManage inst;
        public static KartingManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<KartingManage>();
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

        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnStart.onPinchDown.AddListener(OnStartGame);
            btnEnd.onPinchDown.AddListener(OnEndGame);
            //btnQuit.onPinchDown.AddListener(Hide);
            //add by lp
            InitWindow();
            AddWindowEvent();
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnStart.onPinchDown.RemoveAllListeners();
            btnEnd.onPinchDown.RemoveAllListeners();
            //btnQuit.onPinchDown.RemoveAllListeners();
            RemoveWindowEvent();
        }

        void Start()
        {
            v3OriPos = this.transform.position;
            objKarting.SetActive(false);
            traReadyUI.localScale = Vector3.zero;
            traPlayUI.localScale = Vector3.zero;
            //objKartingWindows.SetActive(false);
            //audioSources = GetComponentsInChildren<AudioSource>();

            //add by lp
            InitWindow();
        }
        void OnDestroy()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            //if (bUIChanging == true)
            //    return;

            Vector3 _v3 = v3OriPos;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;
            //print($"目标的距离:{_dis}--lastPPS:{lastPPS}--curPPS:{curPlayerPosState}");

            float _fFar = LoadPrefab.IconDisData.KartingFar;
            float _fMid = LoadPrefab.IconDisData.KartingMiddle;

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
            //UI开始变化
            bUIChanging = true;
            OutSwap(true);

            //中距离=>远距离
            //Icon从动态变成静态
            //Icon的自旋转动画关闭
            foreach (var v in animIconMiddle)
                v.enabled = false;
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
            //UI开始变化
            bUIChanging = true;

            //中距离=>近距离
            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                traReadyUI.localScale = Vector3.Lerp(traReadyUI.localScale, _v3, fUISpeed * Time.deltaTime);

                float _fDis = Vector3.Distance(traReadyUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    //这里界面先放大再缩小，所以这里不用赋值位置最大值（否则会跳一下的感觉）
                    break;
                }
                yield return 0;
            }

            //先放大，再缩小，需要一个停顿感觉
            yield return new WaitForSeconds(0.1f);
            _v3 = Vector3.one;

            while (true)
            {
                traReadyUI.localScale = Vector3.Lerp(traReadyUI.localScale, _v3, fUISpeed * Time.deltaTime);

                float _fDis = Vector3.Distance(traReadyUI.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traReadyUI.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            yield return new WaitForSeconds(1);
            objHelp.SetActive(true);

            //UI变化结束
            bUIChanging = false;

            //add by lp
            InSwap();
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            //add by lp
            OutSwap(true);
            //UI开始变化
            bUIChanging = true;

            KartingCPE.Inst.EndGame();
            objKarting.SetActive(false);

            objHelp.SetActive(false);

            //近距离=>中距离
            Vector3 _v3Icon = LoadPrefab.IconSize;
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, _v3Icon, fUISpeed * Time.deltaTime);
                traPlayUI.localScale = Vector3.Lerp(traPlayUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                traReadyUI.localScale = Vector3.Lerp(traReadyUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, _v3Icon);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = _v3Icon;
                    traPlayUI.localScale = Vector3.zero;
                    traReadyUI.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离（大于5米，静态）（小于5米，大于1.5米，动态）
        [Header("===Icon变化，远距离（大于5米，或者小于5米，大于1.5米）")]
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
        //Icon对象的AR手势Button按钮
        public ButtonRayReceiver btnIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (bUIChanging == true)
                return;
            if (curPlayerPosState == PlayerPosState.Close)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
            }
        }

        #endregion

        #region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;
        //public ButtonRayReceiver btnQuit;
        private bool bUIShow = false;

        //开始按钮
        public ButtonRayReceiver btnStart;
        //结束按钮
        public ButtonRayReceiver btnEnd;

        //帮助界面动画
        public GameObject objHelp;
        //卡丁车游戏对象
        public GameObject objKarting;

        //准备界面对象
        public Transform traReadyUI;
        //玩游戏界面对象
        public Transform traPlayUI;
        //卡丁车初始位置
        public Vector3 v3PlayerOriPos;
        //卡丁车对象
        public Transform traPlayer;

        void OnStartGame()
        {
            if (bUIChanging == true)
                return;

            traPlayer.localPosition = v3PlayerOriPos;
            traPlayer.localEulerAngles = Vector3.zero;
            KartingCPE.Inst.StartGame();
            //objReadyUI.SetActive(false);
            objKarting.SetActive(true);
            StopCoroutine("IEStartGame");
            StartCoroutine("IEStartGame");
        }

        IEnumerator IEStartGame()
        {
            //UI开始变化
            bUIChanging = true;

            //近距离=>中距离
            while (true)
            {
                traReadyUI.localScale = Vector3.Lerp(traReadyUI.localScale, Vector3.zero, fUISpeed * 0.8f * Time.deltaTime);
                traPlayUI.localScale = Vector3.Lerp(traPlayUI.localScale, Vector3.one, fUISpeed * 0.8f * Time.deltaTime);
                float _fDis = Vector3.Distance(traPlayUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traReadyUI.localScale = Vector3.zero;
                    traPlayUI.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;
        }

        void OnEndGame()
        {
            if (bUIChanging == true)
                return;

            StopCoroutine("IECloseToMiddle");
            StartCoroutine("IECloseToMiddle");
        }

        #endregion

        #region 和视频窗口切换,add by lp
        public ButtonRayReceiver swapBtn;
        public TimelineControl swapTimeline;
        /// <summary>
        /// 是否切换
        /// </summary>
        bool isSwap;
        /// <summary>
        /// 是否在区域内
        /// </summary>
        bool isInArea;
        /// <summary>
        /// 初始化
        /// </summary>
        void InitWindow()
        {
            OutSwap(false);
        }
        /// <summary>
        /// 添加事件
        /// </summary>
        void AddWindowEvent()
        {
            swapBtn.onPinchDown.AddListener(SwapWindow);
        }

        void RemoveWindowEvent()
        {
            swapBtn.onPinchDown.RemoveAllListeners();
        }
        /// <summary>
        /// 切换窗口
        /// </summary>
        void SwapWindow()
        {
            if (bUIChanging == true)
                return;

            //if (VideoManage.Inst == null)
            //    return;
            //if (VideoManage.Inst.bTV == true)
            //    return;
            if (VideoUICtr.Inst == null)
                return;
            if (VideoUICtr.Inst.bTV == true)
                return;
            if (VideoUICtr.Inst.videoAutoRotate.bMove == false)
                return;

            isSwap = !isSwap;

            swapBtn.GetComponent<BoxCollider>().enabled = false;

            if (isSwap)
            {
                swapTimeline.SetCurTimelineData("大到小", () =>
                 {
                     swapBtn.GetComponent<BoxCollider>().enabled = true;
                 });
                objHelp.SetActive(false);
                btnEnd.gameObject.SetActive(false);
                btnStart.gameObject.SetActive(false);
            }
            else
            {
                swapTimeline.SetCurTimelineData("小到大",
                () =>
                {
                    btnEnd.gameObject.SetActive(true);
                    traReadyUI.gameObject.SetActive(true);
                    objHelp.SetActive(true);
                    btnStart.gameObject.SetActive(true);

                    swapBtn.GetComponent<BoxCollider>().enabled = true;
                });
            }
            SetVideo(true);

        }
        /// <summary>
        /// 退出时调用
        /// </summary>
        void OutSwap(bool bSetVideo)
        {
            isInArea = false;
            //复原
            if (isSwap)
            {
                swapTimeline.SetCurTimelineData("小到大",
                () =>
                {
                    btnEnd.gameObject.SetActive(true);
                    traReadyUI.gameObject.SetActive(true);
                    objHelp.SetActive(true);
                    btnStart.gameObject.SetActive(true);

                    isSwap = false;
                    swapTimeline.StartPause();
                });
            }
            else
                swapTimeline.StartPause();
            //TODO,修改视频节点
            if (bSetVideo)
                SetVideo(false);

        }
        /// <summary>
        /// 进入时调用
        /// </summary>
        void InSwap()
        {
            //判断当前是视频跟随状态
            //if (VideoManage.Inst == null)
            //    return;
            //if (VideoManage.Inst.bTV == true)
            //    return;
            if (VideoUICtr.Inst == null)
                return;
            if (VideoUICtr.Inst.bTV == true)
                return;
            if (VideoUICtr.Inst.videoAutoRotate.bMove == false)
                return;

            isInArea = true;
            //TODO,修改视频节点
            SetVideo(true);

        }


        //------------ Modify by zh ------------
        public Transform traVideoBig;
        public Transform traVideoSmall;
        void SetVideo(bool bKarting)
        {
            //VideoManage _vm = VideoManage.Inst;
            VideoUICtr _vm = VideoUICtr.Inst;

            if (_vm == null)
                return;
            if (_vm.bTV == true)
                return;
            if (_vm.traVideoExpand == null)
                return;

            _vm.bKarting = bKarting;

            Transform _traTarget;

            if (bKarting)
            {
                if (isSwap)
                {
                    _vm.colliderBtnARWindows.enabled = true;
                    //卡丁小框，视频大框
                    _traTarget = traVideoBig;
                    _vm.HandleVideo(VideoState.Play);
                    //这里视频非缩小态（离开卡丁用到）
                    _vm.bExpandSmall = false;
                    //_vm.btnPlay.onPinchDown.Invoke();
                }
                else
                {
                    _vm.colliderBtnARWindows.enabled = false;
                    //_vm.SetExpand(false);
                    _vm.SetExpandUI(false, false);
                    //这里视频是缩小态（离开卡丁用到）
                    _vm.bExpandSmall = true;

                    //卡丁大框，视频小框
                    _traTarget = traVideoSmall;
                    _vm.HandleVideo(VideoState.Pause);
                    //_vm.btnPause.onPinchDown.Invoke();
                }
            }
            else
            {
                _vm.colliderBtnARWindows.enabled = true;

                Transform traVideoOriParent;

                //if (_vm.bSizeSmall)
                if (_vm.bExpandSmall)
                    traVideoOriParent = _vm.traFollowSmallParent;
                else
                    traVideoOriParent = _vm.traFollowNormalParent;

                _traTarget = traVideoOriParent;
            }

            _vm.btnTV.transform.parent.gameObject.SetActive(!bKarting);

            //卡丁车状态不显示视频的关闭按钮、缩放按钮、固定位置按钮
            //不能直接显隐父节点objWindowsBtnParent，信息界面呼出的时候会开启objWindowsBtnParent对象
            //_vm.objWindowsBtnParent.SetActive(!bKarting);
            _vm.btnVideoClose.gameObject.SetActive(!bKarting);
            _vm.btnVideoSize.gameObject.SetActive(!bKarting);
            _vm.btnVideoMove.gameObject.SetActive(!bKarting);
            _vm.btnQuit.gameObject.SetActive(!bKarting);
            //视频只有移动状态才能和卡丁车结合，所以不移动的按钮肯定是Disable的
            _vm.btnVideoNotMove.gameObject.SetActive(false);

            _vm.traVideoExpand.SetParent(_traTarget);
            _vm.traVideoExpand.localScale = Vector3.one;
            _vm.traVideoExpand.localPosition = Vector3.zero;
            _vm.traVideoExpand.localRotation = Quaternion.identity;
        }
        //------------------End------------------
        #endregion
    }
}