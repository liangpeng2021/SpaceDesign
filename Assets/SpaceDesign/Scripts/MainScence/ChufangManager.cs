using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理台灯的部分效果，/*create by 梁鹏 2021-10-18 */
/// </summary>
namespace SpaceDesign
{
    public class ChufangManager : MonoBehaviour
    {
        static ChufangManager inst;
        public static ChufangManager Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<ChufangManager>();
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
        public Vector3 v3OriPos;

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
            AddButtonRayEvent();
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            RemoveButtonRayEvent();
        }

        void Start()
        {
            v3OriPos = this.transform.position;
            timeline.StartPause();
            timeline.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(false);
        }
        void OnDestroy() { StopAllCoroutines(); }

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

            if (_dis > 5f)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                curPlayerPosState = PlayerPosState.Far;
            }
            else if (_dis <= 5f && _dis > 2f)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                //从近距离到中距离，大于2m切换状态
                if (lastPPS == PlayerPosState.Close)
                {
                    if (_dis > 3f)
                        curPlayerPosState = PlayerPosState.Middle;
                }//否则1.5m就切换状态
                else
                {
                    curPlayerPosState = PlayerPosState.Middle;
                }
            }
            else if (_dis <= 1.5f)
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
            timeline.gameObject.SetActive(true);
            timeline.SetCurTimelineData("显示菜谱");
            backBtn.gameObject.SetActive(false);

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
            timeline.gameObject.SetActive(false);

            OnQuit();

            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离
        [Header("===Icon变化，远距离")]
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        private Animator animIconFar;
        /// <summary>
        /// 触点按钮
        /// </summary>
        private ButtonRayReceiver btnIcon;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        //Icon的移动速度
        public float fIconSpeed = 1;

        /// <summary>
        /// 点击Icon
        /// </summary>
        void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Close)
                StartCoroutine(IEMiddleToClose());
        }

        void AddButtonRayEvent()
        {
            btnIcon.onPinchDown.AddListener(ClickIcon);
            qiukuiBtn.onPinchDown.AddListener(GotoQiukui);
            fanqieBtn.onPinchDown.AddListener(GotoFanqie);
            bocaiBtn.onPinchDown.AddListener(GotoBocai);
            backBtn.onPinchDown.AddListener(BackToCaipu);

            //waveInteractionGazeHandle.g_OnHandWave.AddListener(WaveHandle);
            //waveInteractionGazeHandle.g_OnHandVirtualPress.AddListener(()=> { ChangePressState(true); });
            //waveInteractionGazeHandle.g_OnHandVirtualRelease.AddListener(() => { ChangePressState(false); });
        }

        void RemoveButtonRayEvent()
        {
            btnIcon.onPinchDown.RemoveAllListeners();
            qiukuiBtn.onPinchDown.RemoveAllListeners();
            fanqieBtn.onPinchDown.RemoveAllListeners();
            bocaiBtn.onPinchDown.RemoveAllListeners();
            backBtn.onPinchDown.RemoveAllListeners();

            //waveInteractionGazeHandle.g_OnHandWave.RemoveAllListeners();
            //waveInteractionGazeHandle.g_OnHandVirtualPress.RemoveAllListeners();
            //waveInteractionGazeHandle.g_OnHandVirtualRelease.RemoveAllListeners();
        }

        #endregion

        void OnQuit()
        {

        }

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;
        /// <summary>
        /// 控制Timeline
        /// </summary>
        public TimelineControl timeline;

        /// <summary>
        /// 秋葵按钮
        /// </summary>
        public ButtonRayReceiver qiukuiBtn;
        /// <summary>
        /// 番茄
        /// </summary>
        public ButtonRayReceiver fanqieBtn;
        /// <summary>
        /// 菠菜
        /// </summary>
        public ButtonRayReceiver bocaiBtn;
        /// <summary>
        /// 返回
        /// </summary>
        public ButtonRayReceiver backBtn;
        /// <summary>
        /// 当前点击的菜谱流程
        /// </summary>
        string curCai = "";
        /// <summary>
        /// 索引
        /// </summary>
        int lastIndex;
        int curIndex;
        /// <summary>
        /// 菜谱第一条文字，手动修改enable
        /// </summary>
        public Text[] firstTexts;

        /// <summary>
        /// 返回到菜单选择界面
        /// </summary>
        void BackToCaipu()
        {
            if (curCai.Equals(""))
                return;
            //Debug.Log(curCai);
            //先播放当前的菜流程消失，再显示开始菜谱
            timeline.SetCurTimelineData(curCai + "消失",
                () =>
                {
                    backBtn.gameObject.SetActive(false);
                    timeline.SetCurTimelineData("显示菜谱");
                });

        }
        /// <summary>
        /// 秋葵
        /// </summary>
        void GotoQiukui()
        {
            curCai = "秋葵";
            // 转到菜谱内的流程
            GotoCaipuliucheng();
        }
        /// <summary>
        /// 转到菜谱内的流程
        /// </summary>
        void GotoCaipuliucheng()
        {
            timeline.SetCurTimelineData("菜谱消失",
                () =>
                {
                    backBtn.gameObject.SetActive(true);
                    timeline.SetCurTimelineData(curCai + "出现");
                });

            lastIndex = curIndex = 1;

            for (int i = 0; i < firstTexts.Length; i++)
            {
                firstTexts[i].enabled = true;
            }
        }
        /// <summary>
        /// 切换具体的菜谱流程
        /// </summary>
        public void ChangeLiuChengLastAnimation(bool isNext)
        {
            if (isNext)
            {
                curIndex++;
                if (curIndex > 5)
                    curIndex = 1;
            }
            else
            {
                curIndex--;
                if (curIndex < 1)
                {
                    curIndex = 5;
                }
            }

            if (curIndex == 1)
            {
                for (int i = 0; i < firstTexts.Length; i++)
                {
                    firstTexts[i].enabled = true;
                }
            }

            timeline.SetCurTimelineData(curCai + lastIndex.ToString() + "-" + curIndex.ToString(),
                ()=>
                {
                    if (curIndex != 1)
                    {
                        for (int i = 0; i < firstTexts.Length; i++)
                        {
                            firstTexts[i].enabled = false;
                        }
                    }
                });
            lastIndex = curIndex;
        }

        //#region 挥手,不稳定
        //public WaveInteractionGazeHandle waveInteractionGazeHandle;
        //bool isPressed;
        //void WaveHandle(Vector2Int dir)
        //{
        //    //if (isPressed)
        //    {
        //        Debug.Log("MyLog::WaveHandle:" + dir.ToString());
        //    }
        //}

        //void ChangePressState(bool press)
        //{
        //    isPressed = press;
        //}

        //#endregion
        
        /// <summary>
        /// 番茄
        /// </summary>
        void GotoFanqie()
        {
            //Debug.Log("GotoFanqie");
            curCai = "番茄";
            // 转到菜谱内的流程
            GotoCaipuliucheng();
        }
        /// <summary>
        /// 菠菜
        /// </summary>
        void GotoBocai()
        {
            curCai = "菠菜";
            // 转到菜谱内的流程
            GotoCaipuliucheng();
        }
        #endregion
    }
}