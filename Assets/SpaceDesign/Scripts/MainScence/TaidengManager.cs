using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理冰箱效果，/*create by 梁鹏 2021-10-27 */
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
        public float noOperationTime=0;
        
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
            taidengController.gameObject.SetActive(false);
            v3OriPos = this.transform.position;
            timelineShow.SetActive(false);
            //开始的时候要把Icon对象父节点清空，Mark定位的时候，Icon不跟随移动
            Invoke("SetIconParent", 0.1f);
        }

        void SetIconParent()
        {
            if (SceneManager.GetActiveScene().name.Equals("EditorScence"))
                traIconRoot.SetParent(EditorControl.Instance.loadPreviewScence.ObjParent);
            else
                traIconRoot.SetParent(null);
        }
        
        public TextMesh tt;

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
            else if (_dis <= 5f && _dis > 1.5f)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                curPlayerPosState = PlayerPosState.Middle;
            }
            else if (_dis <= 1.5f)
            {
                if (lastPPS == PlayerPosState.Close)
                    return;
                curPlayerPosState = PlayerPosState.Close;
            }

            StartCoroutine("IERefreshPos", lastPPS);
        }
        
        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            //print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");
            
            if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 远距离=>中距离
                yield return IEFarToMiddle();
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Close)
            {
                /// 中距离=>近距离
                yield return IEMiddleToClose();
            }
            else if (lastPPS == PlayerPosState.Close && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 近距离=>中距离
                yield return IECloseToMiddle();
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
                v.enabled = false;
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

            //中距离=>近距离

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, 0.1f);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }

            //启动Mark
            image2DTrackingTaideng.enabled = true;
            image2DTrackingTaideng.StartTrack();
            //初始化
            taidengController.gameObject.SetActive(true);
            taidengController.Init();

            //UI变化结束
            bUIChanging = false;

            timelineShow.SetActive(true);
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
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, 0.1f);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

            OnQuit();

            //UI变化结束
            bUIChanging = false;
        }

#region Icon变化，远距离（大于5米，静态）（小于5米，大于1.5米，动态）
        [Header("===Icon变化，原距离（大于5米，或者小于5米，大于1.5米）")]
        //Icon的对象的总根节点
        public Transform traIconRoot;
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //Icon的移动速度
        public float fIconSpeed = 1;
        /// <summary>
        /// 触点按钮
        /// </summary>
        public ButtonRayReceiver chudianBtn;
        /// <summary>
        /// 关闭按钮
        /// </summary>
        public ButtonRayReceiver closeBtn;
        /// <summary>
        /// 空间放置按钮
        /// </summary>
        public ButtonRayReceiver placeBtn;
        /// <summary>
        /// mark放置后控制脚本
        /// </summary>
        public TaidengController taidengController;

        /// <summary>
        /// 点击Icon
        /// </summary>
        void ClickIcon()
        {
            noOperationTime = 0;
            if (curPlayerPosState == PlayerPosState.Close)
                StartCoroutine(IEMiddleToClose());
        }

        void AddButtonRayEvent()
        {
            chudianBtn.onPinchDown.AddListener(ClickIcon);
            closeBtn.onPinchDown.AddListener(OnClose);
            placeBtn.onPinchDown.AddListener(OnCheckPlace);
        }

        void RemoveButtonRayEvent()
        {
            chudianBtn.onPinchDown.RemoveListener(ClickIcon);
            closeBtn.onPinchDown.RemoveListener(OnClose);
            placeBtn.onPinchDown.RemoveListener(OnCheckPlace);
        }

#endregion

#region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;
        //Mark追踪对象
        public Image2DTrackingTaideng image2DTrackingTaideng;
        public GameObject timelineShow;
        
        /// <summary>
        /// 空间放置按钮响应
        /// </summary>
        void OnCheckPlace()
        {
            noOperationTime = 0;

            timelineShow.SetActive(false);

            image2DTrackingTaideng.StopTrack();
            image2DTrackingTaideng.enabled = false;

            taidengController.PlaceTaideng();
        }

        void OnClose()
        {
            OnQuit();
            StartCoroutine("IECloseToMiddle");
        }

        /// <summary>
        /// 关闭界面响应
        /// </summary>
        void OnQuit()
        {
            noOperationTime = 0;

            timelineShow.SetActive(false);
            image2DTrackingTaideng.StopTrack();
            image2DTrackingTaideng.enabled = false;

            taidengController.gameObject.SetActive(false);
        }
#endregion
    }
}