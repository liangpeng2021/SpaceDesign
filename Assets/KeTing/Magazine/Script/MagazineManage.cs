/* Create by zh at 2021-10-11

    杂志控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;

namespace SpaceDesign.Magazine
{
    public class MagazineManage : MonoBehaviour
    {
        static MagazineManage inst;
        public static MagazineManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<MagazineManage>();
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
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnCheckDetail.onPinchDown.AddListener(OnCheckDetail);
            btnQuit.onPinchDown.AddListener(OnQuit);
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnCheckDetail.onPinchDown.RemoveAllListeners();
            btnQuit.onPinchDown.RemoveAllListeners();
            timelineHide.SetActive(false);
            timelineShow.SetActive(false);
        }

        void Start()
        {
            v3OriPos = this.transform.position;
            //开始的时候要把Icon对象父节点清空，Mark定位的时候，Icon不跟随移动
            //traIconRoot.SetParent(null);
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

            if (_dis > 5f)
            {
                curPlayerPosState = PlayerPosState.Far;
                if (lastPPS == PlayerPosState.Far)
                    return;
            }
            else if (_dis <= 5f && _dis > 2.5f)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            else if (_dis <= 2.5f)
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

            //启动Mark
            markTrackMagazine.enabled = true;
            markTrackMagazine.StartTrack();

            //UI变化结束
            bUIChanging = false;
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            //UI开始变化
            bUIChanging = true;

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

            markTrackMagazine.StopTrack();
            markTrackMagazine.enabled = false;

            OnQuit();

            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化
        [Header("===Icon变化")]
        //Icon的对象的总根节点
        public Transform traIconRoot;
        //Icon的对象
        public Transform traIcon;
        //Icon对象的AR手势Button按钮
        private ButtonRayReceiver btnIcon;
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
            if (curPlayerPosState == PlayerPosState.Close)
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
        //Mark追踪对象，杂志
        public Image2DTrackingMagazine markTrackMagazine;
        public GameObject timelineShow;
        public GameObject timelineHide;
        //查看翻译按钮
        public ButtonRayReceiver btnCheckDetail;
        //查看翻译按钮的父节点
        public GameObject objCheckDetailParent;
        //退出翻译按钮
        public ButtonRayReceiver btnQuit;

        /// <summary>
        /// 查看详情按钮响应
        /// </summary>
        public void OnCheckDetail()
        {
            //btnCheckDetail.transform.localScale = Vector3.zero;
            objCheckDetailParent.SetActive(false);
            //btnCheckDetail.gameObject.SetActive(false);
            timelineShow.SetActive(true);
            timelineHide.SetActive(false);
            //markTrackMagazine.StopTrack();
            //markTrackMagazine.enabled = false;
        }

        /// <summary>
        /// 关闭详情界面响应
        /// </summary>
        public void OnQuit()
        {
            objCheckDetailParent.SetActive(true);
            btnCheckDetail.gameObject.SetActive(false);
            if (timelineShow.activeSelf == true)
                timelineHide.SetActive(true);
            timelineShow.SetActive(false);
            //markTrackMagazine.StopTrack();
            //markTrackMagazine.enabled = false;
        }
        #endregion
    }
}