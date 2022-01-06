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
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            AddButtonRayEvent();

            Init();
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            RemoveButtonRayEvent();
        }

        private void Init()
        {
            timeline.StartPause();
            timeline.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(false);

            //挥手碰撞框消失
            waveInteractionGazeHandle.gameObject.SetActive(false);
        }

        void Start()
        {
            v3OriPos = this.transform.position;
            Init();
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

            float _fFar = LoadPrefab.IconDisData.ChuFangFar;
            float _fMid = LoadPrefab.IconDisData.ChuFangMiddle;

            if (_dis > _fFar)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                curPlayerPosState = PlayerPosState.Far;
            }
            else if (_dis <= _fFar && _dis > _fMid)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                //从近距离到中距离，大于2m切换状态
                if (lastPPS == PlayerPosState.Close)
                {
                    if (_dis > (_fMid + 1))
                        curPlayerPosState = PlayerPosState.Middle;
                }//否则1.5m就切换状态
                else
                {
                    curPlayerPosState = PlayerPosState.Middle;
                }
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

            //RefreshPosState(lastPPS);
        }
        //edit by lp,不用协程处理测试

        public enum MoveState
        {
            MiddleToFar,
            MiddleToClose,
            CloseToMiddle
        }

        MoveState moveState=MoveState.MiddleToFar;

        void Update()
        {
            switch (moveState)
            {
                case MoveState.MiddleToClose:
                    MiddleToClose();
                    break;
                case MoveState.CloseToMiddle:
                    CloseToMiddle();
                    break;
            }
        }

        void FarToMiddle()
        {
            //远距离=>中距离
            //Icon从静态变成动态
            //Icon的自旋转动画开启
            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动开启
            animIconFar.enabled = true;
            traIcon.gameObject.SetActive(true);
        }

        void MiddleToClose()
        {
            float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);

            if (_fDis < fThreshold)
            {
                traIcon.localScale = Vector3.zero;
            }
            else
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                return;
            }
            
            timeline.gameObject.SetActive(true);
            timeline.SetCurTimelineData("提示出现");
            backBtn.gameObject.SetActive(false);

            //UI变化结束
            bUIChanging = false;
        }

        void MiddleToFar()
        {
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
        }

        void CloseToMiddle()
        {
            float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
            if (_fDis < fThreshold)
            {
                traIcon.localScale = Vector3.one;
            }
            else
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                return;
            }

            timeline.gameObject.SetActive(false);

            OnQuit();

            //UI变化结束
            bUIChanging = false;
        }

        void RefreshPosState(PlayerPosState lastPPS)
        {
            if (lastPPS == PlayerPosState.Far)
            {
                if (curPlayerPosState == PlayerPosState.Middle)/// 远距离=>中距离
                    FarToMiddle();
                else if (curPlayerPosState == PlayerPosState.Close)/// 远距离=>近距离
                {
                    FarToMiddle();
                    //UI开始变化
                    bUIChanging = true;
                    moveState = MoveState.MiddleToClose;
                }
            }
            else if (lastPPS == PlayerPosState.Middle)
            {
                if (curPlayerPosState == PlayerPosState.Close)/// 中距离=>近距离
                {
                    //UI开始变化
                    bUIChanging = true;
                    moveState = MoveState.MiddleToClose;
                } 
                else if (curPlayerPosState == PlayerPosState.Far)/// 中距离=>远距离
                    MiddleToFar();
            }
            else if (lastPPS == PlayerPosState.Close)
            {
                if (curPlayerPosState == PlayerPosState.Middle)/// 近距离=>中距离
                {
                    //UI开始变化
                    bUIChanging = true;
                    moveState = MoveState.CloseToMiddle;
                }
                else if (curPlayerPosState == PlayerPosState.Far)/// 近距离=>远距离
                {
                    MiddleToFar();
                }
            }
        }
        //end

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
            timeline.SetCurTimelineData("提示出现");
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
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;

        void AddButtonRayEvent()
        {
            qiukuiBtn.onPinchDown.AddListener(GotoQiukui);
            //触摸
            //if (qiukuiTouch == null)
            //{
            //    qiukuiTouch = qiukuiBtn.GetComponent<ButtonTouchableReceiver>();
            //}
            //if (qiukuiTouch != null)
            //{
            //    if (qiukuiTouch.onPressDown == null)
            //        qiukuiTouch.onPressDown = new UnityEngine.Events.UnityEvent();
            //    qiukuiTouch.onPressDown.AddListener(GotoQiukui);
            //}

            fanqieBtn.onPinchDown.AddListener(GotoFanqie);
            //if (fanqieTouch == null)
            //{
            //    fanqieTouch = fanqieBtn.GetComponent<ButtonTouchableReceiver>();
            //}
            //if (fanqieTouch != null)
            //{
            //    if (fanqieTouch.onPressDown == null)
            //        fanqieTouch.onPressDown = new UnityEngine.Events.UnityEvent();
            //    fanqieTouch.onPressDown.AddListener(GotoFanqie);
            //}

            bocaiBtn.onPinchDown.AddListener(GotoBocai);
            //if (bocaiTouch == null)
            //{
            //    bocaiTouch = bocaiBtn.GetComponent<ButtonTouchableReceiver>();
            //}
            //if (bocaiTouch != null)
            //{
            //    if (bocaiTouch.onPressDown == null)
            //        bocaiTouch.onPressDown = new UnityEngine.Events.UnityEvent();
            //    bocaiTouch.onPressDown.AddListener(GotoBocai);
            //}

            backBtn.onPinchDown.AddListener(BackToCaipu);
            //if (backTouchBtn == null)
            //{
            //    backTouchBtn = backBtn.GetComponent<ButtonTouchableReceiver>();
            //}
            //if (backTouchBtn != null)
            //{
            //    if (backTouchBtn.onPressDown == null)
            //        backTouchBtn.onPressDown = new UnityEngine.Events.UnityEvent();
            //    backTouchBtn.onPressDown.AddListener(BackToCaipu);
            //}

            showCaipuBtn.onPinchDown.AddListener(ShowCaipu);
            //if (showTouchCaipuBtn == null)
            //{
            //    showTouchCaipuBtn = showCaipuBtn.GetComponent<ButtonTouchableReceiver>();
            //}
            //if (showTouchCaipuBtn != null)
            //{
            //    if (showTouchCaipuBtn.onPressDown == null)
            //        showTouchCaipuBtn.onPressDown = new UnityEngine.Events.UnityEvent();
            //    showTouchCaipuBtn.onPressDown.AddListener(ShowCaipu);
            //}

            //挥手,射线
            waveInteractionGazeHandle.g_OnHandWave.AddListener(WaveHandle);

        }

        void RemoveButtonRayEvent()
        {
            qiukuiBtn.onPinchDown.RemoveAllListeners();
            //if (qiukuiTouch != null && qiukuiTouch.onPressDown != null)
            //    qiukuiTouch.onPressDown.RemoveAllListeners();

            fanqieBtn.onPinchDown.RemoveAllListeners();
            //if (fanqieTouch != null && fanqieTouch.onPressDown != null)
            //    fanqieTouch.onPressDown.RemoveAllListeners();

            bocaiBtn.onPinchDown.RemoveAllListeners();
            //if (bocaiTouch != null && bocaiTouch.onPressDown != null)
            //    bocaiTouch.onPressDown.RemoveAllListeners();

            backBtn.onPinchDown.RemoveAllListeners();
            //if (backTouchBtn != null && backTouchBtn.onPressDown != null)
            //    backTouchBtn.onPressDown.RemoveAllListeners();

            showCaipuBtn.onPinchDown.RemoveAllListeners();
            //if (showTouchCaipuBtn != null && showTouchCaipuBtn.onPressDown != null)
            //    showTouchCaipuBtn.onPressDown.RemoveAllListeners();

            //挥手
            //射线
            waveInteractionGazeHandle.g_OnHandWave.RemoveAllListeners();

        }

        #endregion

        void OnQuit()
        {
            //timeline.SetCurTimelineData("菜谱消失");
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
        //ButtonTouchableReceiver qiukuiTouch;

        /// <summary>
        /// 番茄
        /// </summary>
        public ButtonRayReceiver fanqieBtn;
        //ButtonTouchableReceiver fanqieTouch;

        /// <summary>
        /// 菠菜
        /// </summary>
        public ButtonRayReceiver bocaiBtn;
        //ButtonTouchableReceiver bocaiTouch;

        /// <summary>
        /// 返回
        /// </summary>
        public ButtonRayReceiver backBtn;
        //ButtonTouchableReceiver backTouchBtn;

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
        /// 显示菜谱按钮
        /// </summary>
        public ButtonRayReceiver showCaipuBtn;
        //ButtonTouchableReceiver showTouchCaipuBtn;
        /// <summary>
        /// 显示菜谱
        /// </summary>
        void ShowCaipu()
        {
            timeline.SetCurTimelineData("提示消失",
                () =>
                {
                    timeline.SetCurTimelineData("显示菜谱");
                });
        }

        /// <summary>
        /// 返回到菜单选择界面
        /// </summary>
        void BackToCaipu()
        {
            if (curCai.Equals(""))
                return;
            //Debug.Log(curCai);
            //先播放当前的菜流程消失，再显示开始菜谱
            backBtn.gameObject.SetActive(false);
            timeline.SetCurTimelineData(curCai + "消失",
                () =>
                {
                    timeline.SetCurTimelineData("显示菜谱");
                });

            //挥手碰撞框消失
            waveInteractionGazeHandle.gameObject.SetActive(false);
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

                    //挥手碰撞框出现
                    waveInteractionGazeHandle.gameObject.SetActive(true);
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
                () =>
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

        #region 挥手,不稳定
        public WaveInteractionGazeHandle waveInteractionGazeHandle;

        void WaveHandle(Vector2Int dir)
        {
            if (dir.x < 0)
                ChangeLiuChengLastAnimation(true);
            else if (dir.x > 0)
            {
                ChangeLiuChengLastAnimation(false);
            }
        }
        #endregion

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