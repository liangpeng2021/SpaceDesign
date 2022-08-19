using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理台灯效果，/*create by 梁鹏 2021-10-27 */
/// </summary>
namespace SpaceDesign
{
    public class TaidengManager : MonoBehaviour
    {
        static TaidengManager inst;
        public static TaidengManager Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<TaidengManager>();
                return inst;
            }
        }
        //Mark识别图片
        public Texture textureMark;

        //该对象父节点
        public Transform traParent;
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;

        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        public Vector3 v3OriPos;
        /// <summary>
        /// 无操作时间
        /// </summary>
        [HideInInspector]
        public float noOperationTime = 0;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================
        void Awake()
        {
            if (traParent == null)
                traParent = transform.parent;
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
            btnIconTouch = traIcon.GetComponent<ButtonTouchableReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            AddButtonRayEvent();
        }
        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            RemoveButtonRayEvent();
        }

        void Start()
        {
            //taidengController.gameObject.SetActive(false);
            v3OriPos = traIconRoot.position;// this.transform.position;

            traIconRoot.SetParent(transform.parent);
            tt.gameObject.SetActive(false);
        }
        void OnDestroy()
        {
            StopAllCoroutines();
            if (traIconRoot != null)
            {
                GameObject obj = traIconRoot.gameObject;
                if (obj != null)
                    DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            //if (bUIChanging)
            //    return;
            Vector3 _v3 = v3OriPos;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);

            //tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            float _fFar = LoadPrefab.IconDisData.TaiDengFar;
            float _fMid = LoadPrefab.IconDisData.TaiDengMiddle;

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

            OnQuit();

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
            //初始化
            taidengController.gameObject.SetActive(true);
            taidengController.Init();

            //启动Mark
            SetMark(true);

            //UI变化结束
            bUIChanging = false;

            //timelineShow.SetActive(true);
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            //UI开始变化
            bUIChanging = true;
            //近距离=>中距离

            Vector3 _v3Icon = LoadPrefab.IconSize;
            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, _v3Icon, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, _v3Icon);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = _v3Icon;
                    break;
                }
                yield return 0;
            }

            OnQuit();

            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离
        [Header("===Icon变化，远距离")]
        //Icon的对象的总根节点
        public Transform traIconRoot;
        //Icon的对象
        public Transform traIcon;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        ButtonTouchableReceiver btnIconTouch;

        //吸引态，上下移动动画
        private Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        /// <summary>
        /// 关闭按钮
        /// </summary>
        public ButtonRayReceiver closeBtn;
        ButtonTouchableReceiver closeBtnTouch;

        /// <summary>
        /// 空间放置按钮
        /// </summary>
        public ButtonRayReceiver placeBtn;
        ButtonTouchableReceiver placeBtnTouch;

        /// <summary>
        /// mark放置后控制脚本
        /// </summary>
        public TaidengController taidengController;

        /// <summary>
        /// 点击Icon
        /// </summary>
        void ClickIcon()
        {
            if (bUIChanging)
                return;

            noOperationTime = 0;
            if (curPlayerPosState == PlayerPosState.Close)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
            }
        }

        void AddButtonRayEvent()
        {
            btnIcon.onPinchDown.AddListener(ClickIcon);
            if (btnIconTouch != null)
            {
                btnIconTouch.onPressUp.AddListener(ClickIcon);
            }

            closeBtn.onPinchDown.AddListener(OnClose);
            if (closeBtnTouch == null)
            {
                closeBtnTouch = closeBtn.GetComponent<ButtonTouchableReceiver>();
            }
            if (closeBtnTouch != null)
            {
                closeBtnTouch.onPressUp.AddListener(OnClose);
            }

            placeBtn.onPinchDown.AddListener(OnCheckPlace);
            if (placeBtnTouch == null)
            {
                placeBtnTouch = placeBtn.GetComponent<ButtonTouchableReceiver>();
            }
            if (placeBtnTouch != null)
            {
                placeBtnTouch.onPressUp.AddListener(OnCheckPlace);
            }
        }

        void RemoveButtonRayEvent()
        {
            btnIcon.onPinchDown.RemoveListener(ClickIcon);
            btnIconTouch.onPressUp.RemoveListener(ClickIcon);

            closeBtn.onPinchDown.RemoveListener(OnClose);
            if (closeBtnTouch != null && closeBtnTouch.onPressUp != null)
                closeBtnTouch.onPressUp.RemoveListener(OnClose);

            placeBtn.onPinchDown.RemoveListener(OnCheckPlace);
            if (placeBtnTouch != null && placeBtnTouch.onPressUp != null)
                placeBtnTouch.onPressUp.RemoveListener(OnCheckPlace);
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;
        ////Mark追踪对象
        //public Image2DTrackingTaideng image2DTrackingTaideng;
        public GameObject timelineShow;

        /// <summary>
        /// 空间放置按钮响应
        /// </summary>
        void OnCheckPlace()
        {
            noOperationTime = 0;

            //timelineShow.SetActive(false);
            SetMark(false);

            taidengController.PlaceTaideng();
        }

        void OnClose()
        {
            //OnQuit();
            //StopCoroutine("IECloseToMiddle");
            //StartCoroutine("IECloseToMiddle");

            taidengController.Init();
            //if (taidengController.gameObject.activeSelf == true)
            //    taidengController.gameObject.SetActive(false);

            SetMark(true);
        }

        /// <summary>
        /// 关闭界面响应
        /// </summary>
        void OnQuit()
        {
            noOperationTime = 0;

            //timelineShow.SetActive(false);
            SetMark(false);

            taidengController.Init();
            //if (taidengController.gameObject.activeSelf == true)
            //    taidengController.gameObject.SetActive(false);
        }


        public void SetMark(bool bOpen)
        {
            if (bOpen)
            {
                MarkManage.Inst.StartTrack(MarkType.Taideng, textureMark);
                //image2DTrackingTaideng.enabled = true;
                //image2DTrackingTaideng.StartTrack();
            }
            else
            {
                if (MarkManage.Inst.curMarkType == MarkType.Taideng)
                    MarkManage.Inst.StopTrack(MarkType.Taideng);
                //image2DTrackingTaideng.StopTrack();
                //image2DTrackingTaideng.enabled = false;
            }
        }

        #endregion
    }
}