/* Create by zh at 2021-10-20

    CPEShow控制脚本

 */

using OXRTK.ARHandTracking;
using System.Collections;
using UnityEngine;

namespace SpaceDesign
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
        void Awake()
        {
            animIconFar = traIcon.GetComponent<Animator>();
            btnIcon = traIcon.GetComponent<ButtonRayReceiver>();
        }
        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
            btnIcon.onPinchDown.AddListener(ClickIcon);
            btnLightOn.onPinchDown.AddListener(LightOff);
            btnLightOff.onPinchDown.AddListener(LightOn);
            btnQuit.onPinchDown.AddListener(Hide);
            sliderLamp.onValueChanged.AddListener(LightSlider);
            sliderLamp.onInteractionEnd.AddListener(SetBrightness);
            timelineShow.SetActive(false);
            timelineHide.SetActive(false);
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
            btnIcon.onPinchDown.RemoveAllListeners();
            btnLightOn.onPinchDown.RemoveAllListeners();
            btnLightOff.onPinchDown.RemoveAllListeners();
            btnQuit.onPinchDown.RemoveAllListeners();
            sliderLamp.onValueChanged.RemoveAllListeners();
            sliderLamp.onInteractionEnd.RemoveAllListeners();
            timelineShow.SetActive(false);
            timelineHide.SetActive(false);
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

            float _fFar = LoadPrefab.IconDisData.ShowCPEFar;

            if (_dis > _fFar)
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
            //print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

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
            //bIconLarge = false;

            //远距离=>中距离
            //Icon从静态变成动态
            //Icon的自旋转动画开启
            foreach (var v in animIconMiddle)
                v.enabled = false;
            //Icon自身上下浮动开启
            animIconFar.enabled = false;

            timelineHide.SetActive(false);
            timelineShow.SetActive(true);

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.zero);
                //if ((bIconLarge == true) || (_fDis < fThreshold))
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
            //bIconLarge = true;
            //中距离=>远距离

            timelineShow.SetActive(false);
            timelineHide.SetActive(true);

            yield return new WaitForSeconds(1.4f);

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traIcon.localScale, Vector3.one);
                //if ((bIconLarge == false) || (_fDis < fThreshold))
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
        ////Icon放大
        //bool bIconLarge;
        /// <summary>
        /// 点击Icon
        /// </summary>
        public void ClickIcon()
        {
            if (bUIChanging)
                return;
            if (curPlayerPosState != PlayerPosState.Far)
            {
                StopCoroutine("IEFarToMiddle");
                StartCoroutine("IEFarToMiddle");
            }
        }

        #endregion

        #region 重交互，大UI，近距离
        [Header("===重交互，大UI，近距离")]
        //UI的变化速度
        public float fUISpeed = 5;

        //不用Timelin的PlayableDirector的Play和Stop来控制，否则在近处关闭按钮后，远离还会触发一次（需要加更多逻辑来进行判断）
        //Timeline：显示
        public GameObject timelineShow;
        //Timeline：隐藏
        public GameObject timelineHide;

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
        //灯泡亮度
        public float fBrightness = 1;
        //现实灯泡，开启状态
        public bool bLampOn = true;

        public void Hide()
        {
            if (bUIChanging)
                return;
            StopCoroutine("IEMiddleToFar");
            StartCoroutine("IEMiddleToFar");
        }

        public void SetBrightness()
        {
            fBrightness = sliderLamp.sliderValue;
            LightSliderCPE();
        }
        public void LightOn()
        {
            if (fBrightness <= 0)
                fBrightness = 1;
            sliderLamp.sliderValue = fBrightness;
            LightSliderCPE();
        }

        public void LightOff()
        {
            sliderLamp.sliderValue = 0;
            LightSliderCPE();
        }

        public void LightSlider(float f)
        {
            f = 0.4f + f * 0.6f;
            SetPropBlock(f);
            if (f <= 0.4f)
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
        }

        #region 控制灯光实物
        public struct LightData
        {
            public int ErrorCode;
            public int LampId;
            public int Brightness;
            public string Status;
        }

        void ClickLight(string str)
        {
            //print("发送灯光：" + str);
            //开启新协程
            IEnumerator enumerator = YoopInterfaceSupport.SendDataToCPE<LightData>(YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.cpeipport] + "iot/lamp/setting?" + str,
                //回调
                (sd) => { Debug.Log("MyLog::灯" + str + ":" + sd.Status); });

            ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }


        public void LightSliderCPE()
        {
            //发送信息状态【0：亮度】【1：开】【2：关】
            int _iSendType = 0;

            if (sliderLamp.sliderValue <= 0)
            {
                _iSendType = 2;
                bLampOn = false;
            }
            else
            {
                fBrightness = sliderLamp.sliderValue;

                if (bLampOn)
                {
                    _iSendType = 0;
                }
                else
                {
                    bLampOn = true;
                    _iSendType = 1;

                    Invoke("_DelaySetSlider", 0.5f);
                }
            }

            switch (_iSendType)
            {
                case 0://【0：亮度】
                    _DelaySetSlider();
                    break;
                case 1://【1：开】
                    ClickLight("id=1&action=on");
                    break;
                case 2://【2：关】
                    ClickLight("id=1&action=off");
                    break;
            }

        }
        //设置抬起（关闭状态下，设置值大于0，先调用开启，再赋值）
        void _DelaySetSlider()
        {
            string valuetest = (fBrightness * 100).ToString("f0");
            valuetest = "id=1&action=setBrightness&value=" + valuetest;
            ClickLight(valuetest);
        }
        #endregion

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