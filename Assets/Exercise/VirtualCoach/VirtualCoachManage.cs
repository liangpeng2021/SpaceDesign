/* Create by zh at 2021-10-27

    虚拟人控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;

namespace SpaceDesign.VirtualCoach
{
    public class VirtualCoachManage : MonoBehaviour
    {
        static VirtualCoachManage inst;
        public static VirtualCoachManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<VirtualCoachManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //初次体验（虚拟人只出现一次）
        private bool bFirstTime = true;
        //Icon、UI等正在切换中
        private bool bUIChanging = false;
        //虚拟人物介绍中（实时面向用户）
        public bool bIntroducing;
        //运动阈值
        private float fThreshold = 0.1f;
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
            btnIcon.onPinchDown.AddListener(ClickIcon);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);
        }

        void Start() { v3OriPos = this.transform.position; }
        void OnDestroy() { StopAllCoroutines(); }
        void LateUpdate()
        {
            if (bIntroducing)
            {
                Vector3 _v3 = PlayerManage.Inst.transform.position;
                _v3.y = transform.position.y;
                transform.LookAt(_v3, Vector3.up);
                transform.forward = -transform.forward;
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
            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;
            //print($"目标的距离:{_dis}--lastPPS:{lastPPS}--curPPS:{curPlayerPosState}");

            float _fFar = LoadPrefab.IconDisData.VirtualCoachFar;
            float _fMid = LoadPrefab.IconDisData.VirtualCoachMiddle;

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
            bIntroducing = false;
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
            bIntroducing = false;
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
            //UI开始变化
            bUIChanging = true;

            //开始介绍
            bIntroducing = true;

            timelineShow.SetActive(true);
            timelineHide.SetActive(false);

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

            yield return new WaitForSeconds(1);

            //UI变化结束
            bUIChanging = false;

            yield return new WaitForSeconds(18f);
            //等待讲话16秒之后，关闭动画

            //介绍结束
            bIntroducing = false;

            yield return IECloseToMiddle();
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            bIntroducing = false;
            //UI开始变化
            bUIChanging = true;

            objMenuUI.SetActive(false);

            timelineShow.SetActive(false);
            timelineHide.SetActive(true);

            //近距离=>中距离
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

            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离
        [Header("===Icon变化，远距离")]
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //人物的说话框，人物隐藏的时候，对话框也要隐藏
        public GameObject objMenuUI;
        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (bUIChanging)
                return;

            if (curPlayerPosState != PlayerPosState.Far)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
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
        #endregion
    }
}