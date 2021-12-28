/* Create by zh at 2021-10-11

    大屏跳操整体控制脚本（WebSockets）

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;

namespace SpaceDesign.Aerobics
{
    public class AerobicsManage : MonoBehaviour
    {
        static AerobicsManage inst;
        public static AerobicsManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<AerobicsManage>();
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
        //这里不带端口号，端口号在7777,8888,9999切换尝试
        [SerializeField]
        private string url = "ws://192.168.1.100";//"ws://192.168.1.100:7777";
        //WebSocket控制
        public MyWebSocket myWebSocket;
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
            btnStart.onPinchDown.AddListener(OnStartGame);
            btnQuit.onPinchDown.AddListener(OnQuit);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnStart.onPinchDown.RemoveAllListeners();
            btnQuit.onPinchDown.RemoveAllListeners();
        }
        void OnDestroy() { StopAllCoroutines(); }

        void Start()
        {
            v3OriPos = this.transform.position;

            traAnimConnect.localScale = Vector3.zero;
            traReadyUI.localScale = Vector3.zero;
            btnStart.transform.localScale = Vector3.zero;
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

            float _fFar = LoadPrefab.IconDisData.AerobicsFar;
            float _fMid = LoadPrefab.IconDisData.AerobicsMiddle;

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
            float _fSp;
            while (true)
            {
                _fSp = fUISpeed * Time.deltaTime;
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, _fSp);
                btnStart.transform.localScale = Vector3.Lerp(btnStart.transform.localScale, _v3, _fSp);

                float _fDis = Vector3.Distance(btnStart.transform.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    //这里界面先放大再缩小，所以这里不用赋值位置最大值（否则会跳一下的感觉）
                    //btnStart.transform.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //先放大，再缩小，需要一个停顿感觉
            yield return new WaitForSeconds(0.1f);
            _v3 = Vector3.one;

            while (true)
            {
                btnStart.transform.localScale = Vector3.Lerp(btnStart.transform.localScale, _v3, fUISpeed * Time.deltaTime);

                float _fDis = Vector3.Distance(btnStart.transform.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    btnStart.transform.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            yield return new WaitForSeconds(1);

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

            float _fSp;
            //近距离=>中距离
            while (true)
            {
                _fSp = fUISpeed * Time.deltaTime;
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, _fSp);
                btnStart.transform.localScale = Vector3.Lerp(btnStart.transform.localScale, Vector3.zero, _fSp);
                traReadyUI.localScale = Vector3.Lerp(traReadyUI.localScale, Vector3.zero, _fSp);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    btnStart.transform.localScale = Vector3.zero;
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
        //开始按钮
        public ButtonRayReceiver btnStart;
        //退出按钮
        public ButtonRayReceiver btnQuit;
        //连接界面对象动画
        public Transform traAnimConnect;
        //准备界面对象
        public Transform traReadyUI;

        void OnStartGame()
        {
            StopCoroutine("IEConnecting");
            StartCoroutine("IEConnecting");
            myWebSocket.InitAndConnect(url, CallbackStart);
        }

        /// <summary>
        /// 连接WebSocket中
        /// </summary>
        IEnumerator IEConnecting()
        {
            //UI开始变化
            bUIChanging = true;

            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);
            while (true)
            {
                traAnimConnect.localScale = Vector3.Lerp(traAnimConnect.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traAnimConnect.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    break;
                }
                yield return 0;
            }

            yield return new WaitForSeconds(0.1f);

            _v3 = Vector3.one;
            while (true)
            {
                traAnimConnect.localScale = Vector3.Lerp(traAnimConnect.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traAnimConnect.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    traAnimConnect.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //UI变化结束
            bUIChanging = false;

        }

        void CallbackStart()
        {
            StopCoroutine("IEStartGame");
            StartCoroutine("IEStartGame");
        }

        IEnumerator IEStartGame()
        {
            btnQuit.gameObject.SetActive(false);

            //UI开始变化
            bUIChanging = true;

            //连接成功后，等一秒（如果连接很快，等一下"连接中"的动画）
            yield return new WaitForSeconds(1);

            myWebSocket.StartGame();

            Vector3 _v3 = new Vector3(1.2f, 1.2f, 1.2f);
            while (true)
            {
                btnStart.transform.localScale = Vector3.Lerp(btnStart.transform.localScale, Vector3.zero, fUISpeed * 2 * Time.deltaTime);
                traAnimConnect.localScale = Vector3.Lerp(traAnimConnect.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                traReadyUI.localScale = Vector3.Lerp(traReadyUI.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traAnimConnect.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    btnStart.transform.localScale = Vector3.zero;
                    traAnimConnect.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

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

            //UI变化结束
            bUIChanging = false;

            yield return new WaitForSeconds(25);
            btnQuit.gameObject.SetActive(true);
        }

        void OnQuit()
        {
            myWebSocket.EndGame();

            StopCoroutine("IECloseToMiddle");
            StartCoroutine("IECloseToMiddle");
        }

        #endregion
    }
}