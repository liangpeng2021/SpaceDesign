using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理冰箱的部分效果，/*create by 梁鹏 2021-10-18 */
/// </summary>
namespace SpaceDesign
{
    public class BingxiangManager : MonoBehaviour
    {
        public struct DoorState
        {
            public int ErrorCode;
            public int DoorId;
            public string Status;
        }

        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;
        //对象初始位置
        public Vector3 v3OriPos;
        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================


        private bool isBought;  // 是否已购买


        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            AddButtonRayEvent();

            bingxiangTimeline.StartPause();
            bingxiangTimeline.gameObject.SetActive(false);

            AddTestEvent();
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            RemoveButtonRayEvent();

            bingxiangTimeline.gameObject.SetActive(false);

            RemoveTestEvent();

            startTip = false;
        }

        void Start()
        {
            v3OriPos = this.transform.position;
            bingxiangTimeline.StartPause();
            bingxiangTimeline.gameObject.SetActive(false);

            for (int i = 0; i < payHideObjs.Length; i++)
            {
                payHideObjs[i].SetActive(true);
            }
            for (int i = 0; i < payShowObjs.Length; i++)
            {
                payShowObjs[i].SetActive(false);
            }

            //  ------------CL----------------------
            isBought = false;
            //--------------------------------------
        }
        void OnDestroy() { StopAllCoroutines(); }

        #region 测试
        [Header("测试")]
        public ButtonRayReceiver opentBtn;
        public ButtonRayReceiver closeBtn;

        /// <summary>
        /// 添加测试事件
        /// </summary>
        void AddTestEvent()
        {
            opentBtn.onPinchDown.AddListener(() => { JudgeOpen("False"); });
            closeBtn.onPinchDown.AddListener(() => { JudgeOpen("True"); });
        }

        void RemoveTestEvent()
        {
            opentBtn.onPinchDown.RemoveAllListeners();
            closeBtn.onPinchDown.RemoveAllListeners();
        }

        #endregion

        private void Update()
        {
            timeCount += Time.deltaTime;
            if (timeCount > 0.5f)
            {
                timeCount = 0;
                ClickGetDoorState();
            }
        }

        float timeCount = 0;

        string lastDoorState = "";
        /// <summary>
        /// 查看冰箱是否打开
        /// </summary>
        void ClickGetDoorState()
        {
            //Debug.Log("GetDoorState:"+ YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.cpeipport] + "iot/sensor/door?id=1");
            //开启新协程
            IEnumerator enumerator = YoopInterfaceSupport.SendDataToCPE<DoorState>(/*wwwFrom, */YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.cpeipport] + "iot/sensor/door?id=1",
                //回调
                (sd) =>
                {
                    //Debug.Log("MyLog::DoorState:" + sd.Status);
                    JudgeOpen(sd.Status);
                }
                );
            StartCoroutine(enumerator);
            //ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }
        /// <summary>
        /// 根据是否开门设置冰箱UI
        /// </summary>
        /// <param name="state"></param>
        void JudgeOpen(string state)
        {
            if (state == null)
            {
                //Debug.Log("MyLog::冰箱Status为空");
                return;
            }
            if (state.Equals(lastDoorState))
                return;
            //保存之前的状态
            lastDoorState = state;

            //近处才触发
            if (curPlayerPosState != PlayerPosState.Close)
                return;

            //发生变化的瞬间触发一次
            SetOpenAnimation();
        }
        /// <summary>
        /// 直接的变化，没有过渡
        /// </summary>
        void SetOpenAnimation()
        {
            //true为门磁合上
            if (lastDoorState.Equals("True"))
            {
                dangaoObj.SetActive(false);
                niunaiObj.SetActive(false);

                if (isBought)
                    SetTimelineData("已购买门外信息", null);
                else
                    SetTimelineData("未购买门外信息", null);
            }
            else
            {
                dangaoObj.SetActive(true);
                niunaiObj.SetActive(true);

                startTip = false;
                SetTimelineData("开冰箱", null);
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

            tt.text = _dis.ToString();

            PlayerPosState lastPPS = curPlayerPosState;

            float _fFar = LoadPrefab.IconDisData.BingXiangFar;
            float _fMid = LoadPrefab.IconDisData.BingXiangMiddle;

            if (_dis > _fFar)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                bingxiangTimeline.gameObject.SetActive(false);
                curPlayerPosState = PlayerPosState.Far;
            }
            else if (_dis <= _fFar && _dis > _fMid)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                curPlayerPosState = PlayerPosState.Middle;
            }
            else if (_dis <= _fMid)
            {
                //Debug.Log(lastPPS);
                if (lastPPS == PlayerPosState.Close)
                    return;
                curPlayerPosState = PlayerPosState.Close;
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
            OnQuit();
            //if (OnQuit())
            //    yield return new WaitForSeconds(0.5f);
            
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

            startTip = false;
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

            //true为门磁合上
            if (lastDoorState.Equals("True") || lastDoorState.Equals(""))
            {
                //出现提示
                SetTimelineData("开头提示", null);
                startTip = true;
                //4秒后，出现信息
                Invoke("TipToInfo", 4.75f);
            }
            else
            {
                startTip = false;
                SetTimelineData("开冰箱", null);
                dangaoObj.SetActive(true);
                niunaiObj.SetActive(true);
            }

            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 开始提示，提示到一半如果距离变化，关闭
        /// </summary>
        bool startTip;
        
        /// <summary>
        /// 是否购买来切换不同的信息
        /// </summary>
        void TipToInfo()
        {
            //退出执行
            //Debug.Log("TipToInfo:startTip:" + startTip);
            if (!startTip)
                return;
            startTip = false;

            SetTimelineData("关闭开头提示", () =>
             {
                 if (isBought)
                 {
                    //出现已购买对应的门外信息
                    SetTimelineData("已购买门外信息", null);
                 }
                 else
                 {
                    //出现未购买对应的门外信息
                    SetTimelineData("未购买门外信息", null);
                 }
             });
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle()
        {
            //UI开始变化
            bUIChanging = true;
            OnQuit();
            //if (OnQuit())
            //    yield return new WaitForSeconds(0.5f);
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
            //UI变化结束
            bUIChanging = false;

            startTip = false;
            StopCoroutine("TipToInfo");
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
        /// <summary>
        /// 控制Timeline
        /// </summary>
        public TimelineControl bingxiangTimeline;
        
        /// <summary>
        /// 点击Icon
        /// </summary>
        void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Close)
            {
                StopCoroutine("IEMiddleToClose");
                StartCoroutine("IEMiddleToClose");
            }
        }

        void AddButtonRayEvent()
        {
            btnIcon.onPinchDown.AddListener(ClickIcon);
            closePayBtn.onPinchDown.AddListener(OnClosePay);
            directPayBtn.onPinchDown.AddListener(DirectPay);
            payBtn.onPinchDown.AddListener(ChoosePay);
            gochoose.onPinchDown.AddListener(GoChoose);
        }

        void RemoveButtonRayEvent()
        {
            btnIcon.onPinchDown.RemoveAllListeners();
            closePayBtn.onPinchDown.RemoveAllListeners();
            directPayBtn.onPinchDown.RemoveAllListeners();
            payBtn.onPinchDown.RemoveAllListeners();
            gochoose.onPinchDown.RemoveAllListeners();
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;

        /// <summary>
        /// 购买关闭按钮
        /// </summary>
        public ButtonRayReceiver closePayBtn;
        /// <summary>
        /// 一键复购
        /// </summary>
        public ButtonRayReceiver directPayBtn;
        /// <summary>
        /// 选择后购买
        /// </summary>
        public ButtonRayReceiver payBtn;
        /// <summary>
        /// 进入选择界面
        /// </summary>
        public ButtonRayReceiver gochoose;
        /// <summary>
        /// 蛋糕和牛奶对象，退出时处理
        /// </summary>
        public GameObject dangaoObj;
        public GameObject niunaiObj;
        /// <summary>
        /// 购买后隐藏对象
        /// </summary>
        public GameObject[] payHideObjs;
        /// <summary>
        /// 购买后显示对象
        /// </summary>
        public GameObject[] payShowObjs;

        void OnClosePay()
        {
            //bingxiangTimeline.SetCurTimelineData("关闭购买");
            SetTimelineData("关闭购买", null);
        }

        void GoChoose()
        {
            //bingxiangTimeline.SetCurTimelineData("点击可乐");
            SetTimelineData("点击可乐", null);
        }

        void DirectPay()
        {
            //bingxiangTimeline.SetCurTimelineData("一键复购");
            SetTimelineData("一键复购", null);
            isBought = true;

            for (int i = 0; i < payHideObjs.Length; i++)
            {
                payHideObjs[i].SetActive(false);
            }
            for (int i = 0; i < payShowObjs.Length; i++)
            {
                payShowObjs[i].SetActive(true);
            }
        }

        void ChoosePay()
        {
            //bingxiangTimeline.SetCurTimelineData("点击购买");
            SetTimelineData("点击购买", null);
            isBought = true;

            for (int i = 0; i < payHideObjs.Length; i++)
            {
                payHideObjs[i].SetActive(false);
            }
            for (int i = 0; i < payShowObjs.Length; i++)
            {
                payShowObjs[i].SetActive(true);
            }
        }
        #endregion

        /// <summary>
        /// 关闭界面响应
        /// </summary>
        bool OnQuit()
        {
            //提示到一半如果距离变化，不继续执行
            startTip = false;
            dangaoObj.SetActive(false);
            niunaiObj.SetActive(false);
            bingxiangTimeline.gameObject.SetActive(false);
            //switch (curTimelineState)
            //{
            //    case "开头提示":
            //        SetTimelineData("关闭开头提示", null);
            //        return true;
            //    case "开冰箱":
            //        SetTimelineData("关闭开冰箱信息", null);
            //        return true;
            //    case "点击可乐":
            //        SetTimelineData("关闭点击可乐信息", null);
            //        return true;
            //    case "未购买门外信息":
            //        SetTimelineData("关闭未购买门外信息", null);
            //        return true;
            //    case "已购买门外信息":
            //        SetTimelineData("关闭已购买门外信息", null);
            //        return true;
            //    case "一键复购":
            //    case "点击购买":
            //        bingxiangTimeline.gameObject.SetActive(false);
            //        break;
            //    default:
            //        break;
            //}

            return false;
        }

        /// <summary>
        /// 当前Timeline的状态
        /// </summary>
        public string curTimelineState;

        void SetTimelineData(string state, System.Action action)
        {
            //Debug.Log("-----------进入设置");
            curTimelineState = state;
            bingxiangTimeline.gameObject.SetActive(true);
            bingxiangTimeline.SetCurTimelineData(curTimelineState, action);
        }
    }
}