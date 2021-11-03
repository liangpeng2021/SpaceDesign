/* Create by zh at 2021-10-19

    植物控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;

namespace SpaceDesign.Plante
{
    public class PlanteManage : MonoBehaviour
    {
        static PlanteManage inst;
        public static PlanteManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<PlanteManage>();
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
        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchUp.AddListener(ClickIcon);
            btnQuit.onPinchUp.AddListener(Hide);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchUp.RemoveAllListeners();
            btnQuit.onPinchUp.RemoveAllListeners();
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
        //吸引态，上下移动动画
        private Animator animIconFar;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
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
        //关闭按钮
        public ButtonRayReceiver btnQuit;

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
        #endregion

    }
}