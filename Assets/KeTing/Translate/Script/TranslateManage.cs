/* Create by zh at 2021-10-12

    翻译控制脚本

 */

using OXRTK.ARHandTracking;
using UnityEngine;

namespace SpaceDesign
{
    public class TranslateManage : MonoBehaviour
    {
        static TranslateManage inst;
        public static TranslateManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<TranslateManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //Icon、UI等正在切换中
        public bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        [SerializeField]
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================
        //edit by lp,添加近场点击事件
        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
            //add by lp
            btnIconTouch = btnIcon.GetComponent<ButtonTouchableReceiver>();
            btnCheckTranslateTouch = btnCheckTranslate.GetComponent<ButtonTouchableReceiver>();
            btnQuitTouch = btnQuit.GetComponent<ButtonTouchableReceiver>();
            //end
        }
        void OnEnable()
        {
            bUIChanging = true;
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnCheckTranslate.onPinchDown.AddListener(OnCheckTranslate);
            btnQuit.onPinchDown.AddListener(OnQuit);
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);

            //add by lp
            btnIconTouch.onPressUp.AddListener(ClickIcon);
            btnCheckTranslateTouch.onPressUp.AddListener(OnCheckTranslate);
            btnQuitTouch.onPressUp.AddListener(OnQuit);
            //end
        }

        void OnDisable()
        {
            bUIChanging = false;
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnCheckTranslate.onPinchDown.RemoveAllListeners();
            btnQuit.onPinchDown.RemoveAllListeners();
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);

            //add by lp
            btnIconTouch.onPressUp.RemoveListener(ClickIcon);
            btnCheckTranslateTouch.onPressUp.RemoveListener(OnCheckTranslate);
            btnQuitTouch.onPressUp.RemoveListener(OnQuit);
            //end
        }
        //end

        void Start()
        {
            v3OriPos = traIconRoot.position;// this.transform.position;
            //开始的时候要把Icon对象父节点清空，Mark定位的时候，Icon不跟随移动
            traIconRoot.SetParent(transform.parent);
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
            //if (traIconRoot != null)
            //{
            //    traIconRoot.SetParent(transform.parent);
            //}
        }

        void Update()
        {
            if (bUIChanging)
            {
                switch (curPlayerPosState)
                {
                    case PlayerPosState.Far:
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
                        traIcon.localScale = LoadPrefab.IconSize;
                        traIcon.gameObject.SetActive(true);

                        OnQuit();

                        bUIChanging = false;

                        break;
                    case PlayerPosState.Middle:
                        //Icon从静态变成动态
                        //Icon的自旋转动画开启
                        foreach (var v in animIconMiddle)
                            v.enabled = true;

                        animIconFar.enabled = true;
                        traIcon.gameObject.SetActive(true);

                        SetIcon(true);

                        break;
                    case PlayerPosState.Close:

                        //近距离
                        SetIcon(false);

                        break;
                }
            }
        }

        Vector3 v3IconTarget = Vector3.one;
        void SetIcon(bool bShow)
        {
            if (bShow)
                v3IconTarget = LoadPrefab.IconSize;
            else
                v3IconTarget = Vector3.zero;

            float _fDis = Vector3.Distance(traIcon.localScale, v3IconTarget);
            bUIChanging = (_fDis >= fThreshold);
            if (bUIChanging == true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, v3IconTarget, fUISpeed * Time.deltaTime);
            }
            else
            {
                traIcon.localScale = v3IconTarget;

                //启动Mark
                if (markTrackTranslate == null)
                    markTrackTranslate = FindObjectOfType<Image2DTrackingTranslate>();
                if (markTrackTranslate != null)
                {
                    if (bShow)
                    {
                        markTrackTranslate.StopTrack();
                        markTrackTranslate.enabled = false;

                        OnQuit();
                    }
                    else
                    {
                        markTrackTranslate.enabled = true;
                        markTrackTranslate.StartTrack();
                    }
                }
            }
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
            //print($"目标的距离:{_dis}");
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            float _fFar = LoadPrefab.IconDisData.TranslateFar;
            float _fMid = LoadPrefab.IconDisData.TranslateMiddle;

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

            bUIChanging = true;
        }

        #region Icon变化
        [Header("===Icon变化")]
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
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (bUIChanging)
                return;
            bUIChanging = true;
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;
        //Mark追踪对象，杂志
        public Image2DTrackingTranslate markTrackTranslate;
        public GameObject timelineShow;
        public GameObject timelineHide;
        //查看翻译按钮
        public ButtonRayReceiver btnCheckTranslate;
        ButtonTouchableReceiver btnCheckTranslateTouch;

        //查看翻译按钮的父节点
        public GameObject objCheckTranslateParent;
        //退出翻译按钮
        public ButtonRayReceiver btnQuit;
        ButtonTouchableReceiver btnQuitTouch;

        /// <summary>
        /// 查看翻译按钮响应
        /// </summary>
        public void OnCheckTranslate()
        {
            objCheckTranslateParent.SetActive(false);
            timelineShow.SetActive(true);
            timelineHide.SetActive(false);
        }

        /// <summary>
        /// 关闭翻译界面响应
        /// </summary>
        public void OnQuit()
        {
            objCheckTranslateParent.SetActive(true);
            btnCheckTranslate.gameObject.SetActive(markTrackTranslate.bMarking);
            if (timelineShow.activeSelf == true)
                timelineHide.SetActive(true);
            timelineShow.SetActive(false);
        }
        #endregion
    }
}