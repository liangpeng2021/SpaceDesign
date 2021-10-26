/* Create by zh at 2021-10-20

    CPEShow控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SpaceDesign.CPEShow
{
    public class CPEShowManage : MonoBehaviour
    {
        static CPEShowManage inst;
        public static CPEShowManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<CPEShowManage>();
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
        private Vector3 v3OriPos;

        //===========================================================================
        //临时测距
        public TextMesh tt;
        //===========================================================================

        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchUp.AddListener(ClickIcon);
            btnLightOn.onPinchUp.AddListener(LightOff);
            btnLightOff.onPinchUp.AddListener(LightOn);
            btnQuit.onPinchUp.AddListener(Hide);
            sliderLamp.onValueChanged.AddListener(LightSlider);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchUp.RemoveAllListeners();
            btnLightOn.onPinchUp.RemoveAllListeners();
            btnLightOff.onPinchUp.RemoveAllListeners();
            btnQuit.onPinchUp.RemoveAllListeners();
            sliderLamp.onValueChanged.RemoveAllListeners();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        void Start()
        {
            v3OriPos = this.transform.position;
        }

        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        Show();
        //    }
        //    if (Input.GetKeyDown(KeyCode.Alpha2))
        //    {
        //        Hide();
        //    }
        //    if (Input.GetKeyDown(KeyCode.Alpha3))
        //    {
        //        LightOn();
        //    }
        //    if (Input.GetKeyDown(KeyCode.Alpha4))
        //    {
        //        LightOff();
        //    }
        //}

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
            else //if (_dis <= 5f && _dis > 1.5f)
            {
                curPlayerPosState = PlayerPosState.Middle;
                if (lastPPS == PlayerPosState.Middle)
                    return;
            }
            //else if (_dis <= 1.5f)
            //{
            //    curPlayerPosState = PlayerPosState.Close;
            //    if (lastPPS == PlayerPosState.Close)
            //        return;
            //}

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
            //else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Close)
            //{
            //    /// 中距离=>近距离
            //    yield return IEMiddleToClose();
            //}
            //else if (lastPPS == PlayerPosState.Close && curPlayerPosState == PlayerPosState.Middle)
            //{
            //    /// 近距离=>中距离
            //    yield return IECloseToMiddle(false);
            //}
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

            timelineShow.gameObject.SetActive(true);
            timelineHide.gameObject.SetActive(false);

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

            bUIShow = true;

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

            if (bUIShow == true)
            {
                timelineShow.gameObject.SetActive(false);
                timelineHide.gameObject.SetActive(true);
            }

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

            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动关闭
            animIconFar.enabled = true;

            bUIShow = false;

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离（大于5米，静态）（小于5米，大于1.5米，动态）
        [Header("===Icon变化，原距离（大于5米，或者小于5米，大于1.5米）")]
        //Icon的对象
        public Transform traIcon;
        //Icon对象的AR手势Button按钮
        public ButtonRayReceiver btnIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //Icon的移动速度
        public float fIconSpeed = 1;

        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (curPlayerPosState == PlayerPosState.Middle)
                StartCoroutine(IEFarToMiddle());
        }

        #endregion

        #region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;
        //Timeline：显示
        public PlayableDirector timelineShow;
        //Timeline：隐藏
        public PlayableDirector timelineHide;
        public bool bUIShow = false;

        //开灯对象的AR手势Button按钮
        public ButtonRayReceiver btnLightOn;
        //关灯对象的AR手势Button按钮
        public ButtonRayReceiver btnLightOff;
        //退出按钮
        public ButtonRayReceiver btnQuit;
        //灯泡亮度的滑动条
        public PinchSlider sliderLamp;

        //灯光的材质球Render
        [SerializeField]
        private MeshRenderer lampRender;
        //优化render
        private MaterialPropertyBlock matPropBlock;

        public void Hide()
        {
            StartCoroutine(IEMiddleToFar());
        }
        public void Show()
        {
            StartCoroutine(IEFarToMiddle());
        }
        public void LightOn()
        {
            float _f = sliderLamp.sliderValue;
            if (_f <= 0)
                _f = 1;
            sliderLamp.sliderValue = _f;

            //===========================================================================
            //CPE发送：开灯
            //===========================================================================
        }

        public void LightOff()
        {
            sliderLamp.sliderValue = 0;
            //===========================================================================
            //CPE发送：关灯
            //===========================================================================
        }

        public void LightSlider(float f)
        {
            f = 0.4f + f * 0.6f;
            SetPropBlock(f);
            if (f <= 0.41f)
            {
                if (btnLightOn.gameObject.activeSelf == true)
                    btnLightOn.gameObject.SetActive(false);
                if (btnLightOff.gameObject.activeSelf == false)
                    btnLightOff.gameObject.SetActive(true);
            }
            else
            {
                if (btnLightOn.gameObject.activeSelf == false)
                    btnLightOn.gameObject.SetActive(true);
                if (btnLightOff.gameObject.activeSelf == true)
                    btnLightOff.gameObject.SetActive(false);
            }

            //===========================================================================
            //CPE发送：灯光亮度
            //===========================================================================
        }

        /// <summary>
        /// 设置材质属性，自定义颜色
        /// </summary>
        void SetPropBlock(float f)
        {
            if (lampRender == null)
                return;
            matPropBlock = new MaterialPropertyBlock();
            lampRender.GetPropertyBlock(matPropBlock);
            matPropBlock.SetColor("_EmissionColor", Color.white * f);
            lampRender.SetPropertyBlock(matPropBlock);
        }

        #endregion

    }
}