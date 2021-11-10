/* Create by zh at 2021-10-19

    游戏控制脚本

    奇幻森林：          com.gabor.artowermotion
    忒依亚传说(moba):   com.baymax.omoba
    AR动物园:           com.xyani.findanimals

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;

namespace SpaceDesign.DeskGame
{
    public class DeskGameManage : MonoBehaviour
    {
        static DeskGameManage inst;
        public static DeskGameManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<DeskGameManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        ////播放模型
        //public Transform traModel;
        //Icon、UI等正在切换中
        private bool bUIChanging = false;
        //运动阈值
        private float fThreshold = 0.1f;
        //对象初始位置
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================
        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnGame01.onPinchDown.AddListener(() => { CallApp("com.gabor.artowermotion"); });
            btnGame02.onPinchDown.AddListener(() => { CallApp("com.baymax.omoba"); });
            btnGame03.onPinchDown.AddListener(() => { CallApp("com.xyani.findanimals"); });
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnGame01.onPinchDown.RemoveAllListeners();
            btnGame02.onPinchDown.RemoveAllListeners();
            btnGame03.onPinchDown.RemoveAllListeners();
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);
        }

        void OnDestroy() { StopAllCoroutines(); }

        void Start() { v3OriPos = this.transform.position; }

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

            if (_dis > 5f)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
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
            print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

            if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 远距离=>中距离
                yield return IEFarToMiddle();
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Far)
            {
                /// 中距离=>远距离
                yield return IEMiddleToFar();
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
                v.enabled = false;
            //Icon自身上下浮动开启
            animIconFar.enabled = false;
            traIcon.gameObject.SetActive(true);

            timelineShow.SetActive(true);
            timelineHide.SetActive(false);

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
            traIcon.gameObject.SetActive(true);

            timelineShow.SetActive(false);
            timelineHide.SetActive(true);

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动关闭
            animIconFar.enabled = true;

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离
        [Header("===Icon变化，远距离")]
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        //Icon的对象
        public Transform traIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Middle)
            {
                Show();
            }
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;
        //Timeline：显示
        public GameObject timelineShow;
        //Timeline：隐藏
        public GameObject timelineHide;
        //游戏1按钮
        public ButtonRayReceiver btnGame01;
        //游戏2按钮
        public ButtonRayReceiver btnGame02;
        //游戏3按钮
        public ButtonRayReceiver btnGame03;

        public void Hide()
        {
            StopCoroutine("IEMiddleToFar");
            StartCoroutine("IEMiddleToFar");
        }
        public void Show()
        {
            StopCoroutine("IEFarToMiddle");
            StartCoroutine("IEFarToMiddle");
        }

        public void CallApp(string strPackName)
        {
            //print($"拉起应用:{strPackName}");
            XR.AppManager.StartApp(strPackName);
        }
        #endregion

    }
}