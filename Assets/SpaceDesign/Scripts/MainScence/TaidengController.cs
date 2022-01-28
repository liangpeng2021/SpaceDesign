using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制台灯UI显示，/*create by 梁鹏 2021-9-3 */
/// </summary>
namespace SpaceDesign
{
    public class TaidengController : MonoBehaviour
    {
        /// <summary>
        /// 台灯模型，展示的大模型
        /// </summary>
        public GameObject taidengModel;
        /// <summary>
        /// 开启购买流程
        /// </summary>
        public ButtonRayReceiver startpayRayReceiver;
        ButtonTouchableReceiver startPayTouch;

        /// <summary>
        /// 默认显示的对象
        /// </summary>
        public GameObject firstObj;
        /// <summary>
        /// 射线碰到以后第二次显示的对象
        /// </summary>
        public GameObject secondObj;
        /// <summary>
        /// 开始购买后显示的对象
        /// </summary>
        public GameObject payObj;

        BoundingBox boundingBox;
        BoxCollider boxCollider;
        GameObject boundingChild;

        /// <summary>
        /// 立即购买按钮
        /// </summary>
        public ButtonRayReceiver justpayRayReceiver;
        ButtonTouchableReceiver justPayTouch;

        /// <summary>
        /// 购买成功对象
        /// </summary>
        public GameObject paySuccessObj;
        /// <summary>
        /// 配送中
        /// </summary>
        public GameObject peisongObj;
        /// <summary>
        /// 台灯旁边的UI（不跟随台灯旋转）
        /// </summary>
        public Transform taiDengUI;
        /// <summary>
        /// 台灯旁边UI的父节点
        /// </summary>
        public Transform tdUIParent;

        //-----  CL 音效  -----------------------
        public AudioSource audioSource;
        public AudioClip successClip;
        public AudioClip zhixiangClip;
        //---------------------------------------

        //------------ Modify by zh ------------
        /// <summary>
        /// 台灯灯泡控制
        /// </summary>
        public LightController lightCtr;
        /// <summary>
        /// 购买界面的台灯的碰撞框(第二次显示的对象，3秒自动隐藏UI，碰触台灯再出现)
        /// </summary>
        public ButtonRayReceiver btnBuyUIBoxCollider;
        /// <summary>
        /// 台灯的灯光，跟随第二次显示的对象一起开启关闭（但是台灯单独旋转，不属于其子节点）
        /// </summary>
        public GameObject pointLight;
        /// <summary>
        /// 显示的状态【1-(Mark)默认空间放置界面】【2-购买界面】【3-确认购买界面】【4-支付成功】【5-配送】
        /// </summary>
        public int iShowUIState = 0;
        /// <summary>
        /// 是否显示购买界面的UI（3秒不管自动隐藏）
        /// </summary>
        public bool bShowPayUI = false;
        /// <summary>
        /// 台灯控制中（位移旋转等操作）
        /// </summary>
        public bool bTranslating;
        /// <summary>
        /// 是否显示台灯的包围盒（项目启动的初始值必须是true）
        /// </summary>
        [SerializeField]
        bool bBoundBoxShow = true;
        //台灯原始位置
        Vector3 taidengOriPos;
        //台灯原始旋转
        Vector3 taidengOriEur;
        //台灯原始缩放
        Vector3 taidengOriSca;
        //台灯UI原始位置
        Vector3 taidengUIOriPos;
        //台灯摆放后，计时3秒隐藏UI
        float _fNoOprTime;
        //------------------End------------------

        void Awake()
        {
            taidengOriPos = transform.localPosition;
            taidengOriEur = transform.localEulerAngles;
            taidengOriSca = transform.localScale;

            taidengUIOriPos = taiDengUI.localPosition;
        }

        private void OnEnable()
        {
            startpayRayReceiver.onPinchDown.AddListener(StartPay);
            if (startPayTouch == null)
            {
                startPayTouch = startpayRayReceiver.GetComponent<ButtonTouchableReceiver>();
            }
            if (startPayTouch != null)
            {
                if (startPayTouch.onPressUp == null)
                    startPayTouch.onPressUp = new UnityEngine.Events.UnityEvent();
                startPayTouch.onPressUp.AddListener(StartPay);
            }

            justpayRayReceiver.onPinchDown.AddListener(PaySuccess);
            if (justPayTouch == null)
            {
                justPayTouch = justpayRayReceiver.GetComponent<ButtonTouchableReceiver>();
            }
            if (justPayTouch != null)
            {
                if (justPayTouch.onPressUp == null)
                    justPayTouch.onPressUp = new UnityEngine.Events.UnityEvent();
                justPayTouch.onPressUp.AddListener(PaySuccess);
            }

            if (boundingBox == null)
                boundingBox = GetComponent<BoundingBox>();
            if (boundingBox)
            {
                boundingBox.onTranslateStart.AddListener(PointDown);
                boundingBox.onScaleStart.AddListener(PointDown);
                boundingBox.onRotateStart.AddListener(PointDown);
            }
            else
                Debug.Log("MyLog::找不到台灯BoundingBox");

            btnBuyUIBoxCollider.onPointerEnter.AddListener(() =>
            {
                bShowPayUI = true;
                SetTranslating(true);
            });
            btnBuyUIBoxCollider.onPointerExit.AddListener(() =>
            {
                SetTranslating(false);
            });

            Init();
        }

        private void OnDisable()
        {
            justpayRayReceiver.onPinchDown.RemoveListener(PaySuccess);
            if (startPayTouch != null && startPayTouch.onPressUp != null)
                startPayTouch.onPressUp.RemoveAllListeners();

            startpayRayReceiver.onPinchDown.RemoveListener(StartPay);
            if (justPayTouch != null && justPayTouch.onPressUp != null)
                justPayTouch.onPressUp.RemoveAllListeners();

            if (boundingBox)
            {
                boundingBox.onTranslateStart.RemoveListener(PointDown);
                boundingBox.onScaleStart.RemoveListener(PointDown);
                boundingBox.onRotateStart.RemoveListener(PointDown);
            }

            btnBuyUIBoxCollider.onPointerEnter.RemoveAllListeners();
            btnBuyUIBoxCollider.onPointerExit.RemoveAllListeners();
        }
        //有操作
        void PointDown()
        {
            TaidengManager.Inst.noOperationTime = 0;
        }

        private void Update()
        {
            if (iShowUIState == 0)
            {
                DisableBoundBox();
                return;
            }

            if (iShowUIState == 2)
            {
                if (bShowPayUI)
                {
                    EnableBoundBox();

                    if (bTranslating == false)
                    {
                        _fNoOprTime += Time.deltaTime;
                        if (_fNoOprTime > 3)
                        {
                            _fNoOprTime = 0;
                            bShowPayUI = false;
                            secondObj.SetActive(false);
                            btnBuyUIBoxCollider.gameObject.SetActive(true);
                        }
                    }
                }
                else
                    DisableBoundBox();
            }
            else
            {
                DisableBoundBox();
            }
        }

        private void OnDestroy()
        {
            startpayRayReceiver = null;
            firstObj = null;
            secondObj = null;
            pointLight = null;
            payObj = null;
            boundingBox = null;
            justpayRayReceiver = null;
        }

        public void ShowMark(bool bShow)
        {
            if (bShow)
            {
                iShowUIState = 1;

                transform.localPosition = taidengOriPos;
                transform.localEulerAngles = taidengOriEur;
                transform.localScale = taidengOriSca;


                taidengModel.SetActive(true);
                taiDengUI.localPosition = taidengUIOriPos;
                taiDengUI.localEulerAngles = Vector3.zero;
                taiDengUI.gameObject.SetActive(true);
            }
            else
            {
                //------------ Modify by zh ------------
                //关闭是mark延迟关闭的，如果点击放置按钮后，台灯Mark没扫到就不隐藏了
                if (iShowUIState != 1)
                    return;
                //------------------End------------------

                taidengModel.SetActive(false);
                taiDengUI.gameObject.SetActive(false);
                //print("隐藏");

            }
        }

        public void Init()
        {
            bShowPayUI = false;
            DisableBoundBox();
            SetTranslating(false);
            iShowUIState = 0;
            firstObj.SetActive(true);
            secondObj.SetActive(false);
            lightCtr.SetLight(false);
            pointLight.SetActive(false);
            payObj.SetActive(false);
            peisongObj.SetActive(false);
            paySuccessObj.SetActive(false);

            startpayRayReceiver.gameObject.SetActive(false);
            paySuccessObj.SetActive(false);
            btnBuyUIBoxCollider.gameObject.SetActive(false);
            taiDengUI.SetParent(tdUIParent);
            taiDengUI.localPosition = taidengUIOriPos;
            taiDengUI.localEulerAngles = Vector3.zero;

            taidengModel.SetActive(false);
            taiDengUI.gameObject.SetActive(false);
        }

        /// <summary>
        /// 移动控制的包围盒首次实例化出来（会有先粗后细的初始化），先隐藏MeshRender
        /// </summary>
        bool bBoundingFirstInst = false;
        /// <summary>
        /// 移动控制的包围盒MeshRender是否隐藏
        /// </summary>
        bool bShowBoundingMeshRender = false;

        /// <summary>
        /// 隐藏台灯移动旋转功能
        /// </summary>
        void DisableBoundBox()
        {
            if (bBoundBoxShow == false)
                return;
            bBoundBoxShow = false;

            if (boundingBox == null)
                boundingBox = GetComponent<BoundingBox>();
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();
            if (boundingChild == null)
                boundingChild = transform.Find("BoundingBox")?.gameObject;
            if (boundingBox)
                boundingBox.enabled = false;
            if (boxCollider)
                boxCollider.enabled = false;
            if (boundingChild)
            {
                HandleBounding();
                boundingChild.SetActive(false);
            }
        }
        /// <summary>
        /// 打开台灯移动旋转功能
        /// </summary>
        void EnableBoundBox()
        {
            if (bBoundBoxShow == true)
                return;
            bBoundBoxShow = true;

            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();

            if (boundingChild == null)
                boundingChild = transform.Find("BoundingBox")?.gameObject;
            if (boundingBox == null)
                boundingBox = GetComponent<BoundingBox>();
            if (boundingBox)
                boundingBox.enabled = true;
            if (boxCollider)
                boxCollider.enabled = true;
            if (boundingChild)
            {
                HandleBounding();
                boundingChild.SetActive(true);
            }
            if (iShowUIState == 2)
            {
                if (btnBuyUIBoxCollider.gameObject.activeSelf == true)
                    btnBuyUIBoxCollider.gameObject.SetActive(false);
                if (secondObj.activeSelf == false)
                    secondObj.SetActive(true);
            }
        }

        void HandleBounding()
        {
            if (bBoundingFirstInst == false)
            {
                bBoundingFirstInst = true;
                MeshRenderer[] mrs = boundingChild.GetComponentsInChildren<MeshRenderer>();
                foreach (var v in mrs)
                    v.enabled = false;
            }
            if (bShowBoundingMeshRender == false)
            {
                MeshRenderer[] mrs = boundingChild.GetComponentsInChildren<MeshRenderer>();
                foreach (var v in mrs)
                    v.enabled = true;
                bShowBoundingMeshRender = true;
            }
        }

        /// <summary>
        /// 放置台灯
        /// </summary>
        public void PlaceTaideng()
        {
            iShowUIState = 2;
            btnBuyUIBoxCollider.gameObject.SetActive(true);

            taiDengUI.localEulerAngles = Vector3.zero;
            taiDengUI.gameObject.SetActive(true);
            taidengModel.SetActive(true);
            firstObj.SetActive(false);
            startpayRayReceiver.gameObject.SetActive(true);
            secondObj.SetActive(true);
            lightCtr.SetLight(true);
            pointLight.SetActive(true);
            bShowPayUI = true;

            TaidengManager.Inst.noOperationTime = 0;
        }

        /// <summary>
        /// 开始购买
        /// </summary>
        void StartPay()
        {
            iShowUIState = 3;
            btnBuyUIBoxCollider.gameObject.SetActive(false);
            taidengModel.SetActive(false);
            secondObj.SetActive(false);
            lightCtr.SetLight(false);
            pointLight.SetActive(false);
            payObj.SetActive(true);
            startpayRayReceiver.gameObject.SetActive(false);
            bShowPayUI = false;
            TaidengManager.Inst.noOperationTime = 0;
        }

        void PaySuccess()
        {
            iShowUIState = 4;
            payObj.SetActive(false);
            ShowPaySucess();
            TaidengManager.Inst.noOperationTime = 0;
        }

        void ShowPaySucess()
        {
            paySuccessObj.SetActive(true);
            //---CL  ----------------------------------
            audioSource.PlayOneShot(successClip);
            //-----------------------------------------
            Invoke("ShowPeisong", 3f);
        }

        void ShowPeisong()
        {
            iShowUIState = 5;
            paySuccessObj.SetActive(false);

            //---CL  ----------------------------------
            audioSource.Stop();
            //-----------------------------------------

            peisongObj.SetActive(true);
            //---CL  ----------------------------------
            audioSource.PlayOneShot(zhixiangClip);
            //-----------------------------------------
            Invoke("HidePeisong", 3f);
        }

        void HidePeisong()
        {
            peisongObj.SetActive(false);
            //---CL  ----------------------------------
            audioSource.Stop();
            //-----------------------------------------

            Init();
            //配送完毕，重新开启mark
            TaidengManager.Inst.SetMark(true);
        }

        //------------ Modify by zh ------------
        /// <summary>
        /// 台灯开始旋转，把旁边的UI父节点放出去
        /// </summary>
        public void TaiDengStartRot()
        {
            taiDengUI.SetParent(tdUIParent.parent);
        }
        /// <summary>
        /// 台灯旋转结束，把旁边的UI父节点放回来
        /// </summary>
        public void TarDengEndRot()
        {
            taiDengUI.SetParent(tdUIParent);
        }
        public void SetTranslating(bool bTstling)
        {
            bTranslating = bTstling;
            if (bTranslating == true)
                _fNoOprTime = 0;

        }
        //------------------End------------------

    }
}

//using OXRTK.ARHandTracking;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using SpaceDesign;

///// <summary>
///// 控制台灯UI显示，/*create by 梁鹏 2021-9-3 */
///// </summary>
//public class TaidengController : MonoBehaviour
//{
//    /// <summary>
//    /// 台灯模型，展示的大模型
//    /// </summary>
//    public GameObject taidengModel;
//    /// <summary>
//    /// 开启购买流程
//    /// </summary>
//    public ButtonRayReceiver startpayRayReceiver;

//    /// <summary>
//    /// 默认显示的对象
//    /// </summary>
//    public GameObject firstObj;
//    /// <summary>
//    /// 射线碰到以后第二次显示的对象
//    /// </summary>
//    public GameObject secondObj;
//    /// <summary>
//    /// 开始购买后显示的对象
//    /// </summary>
//    public GameObject payObj;

//    bool showBoundBox = false;
//    BoundingBox boundingBox;
//    BoxCollider boxCollider;
//    GameObject boundingChild;

//    /// <summary>
//    /// 立即购买按钮
//    /// </summary>
//    public ButtonRayReceiver justpayRayReceiver;
//    /// <summary>
//    /// 购买成功对象
//    /// </summary>
//    public GameObject paySuccessObj;
//    /// <summary>
//    /// 配送中
//    /// </summary>
//    public GameObject peisongObj;

//    bool hideBoxScale = false;

//    private void Update()
//    {
//        if (showBoundBox)
//        {
//            Debug.Log("EnableBoundBox");
//            EnableBoundBox();
//        }
//        else
//            DisableBoundBox();
//    }

//    private void OnDestroy()
//    {
//        startpayRayReceiver = null;
//        firstObj = null;
//        secondObj = null;
//        payObj = null;
//        boundingBox = null;
//        justpayRayReceiver = null;
//    }

//    public void Init()
//    {
//        firstObj.SetActive(true);
//        secondObj.SetActive(false);
//        payObj.SetActive(false);
//        peisongObj.SetActive(false);
//        paySuccessObj.SetActive(false);

//        startpayRayReceiver.gameObject.SetActive(false);
//        paySuccessObj.SetActive(false);
//        taidengModel.SetActive(true);
//        //showFirst = false;
//    }

//    private void OnEnable()
//    {
//        startpayRayReceiver.onPinchDown.AddListener(StartPay);
//        justpayRayReceiver.onPinchDown.AddListener(PaySuccess);

//        if (boundingBox == null)
//            boundingBox = GetComponent<BoundingBox>();
//        if (boundingBox)
//        {
//            boundingBox.onTranslateStart.AddListener(PointDown);
//            boundingBox.onScaleStart.AddListener(PointDown);
//            boundingBox.onRotateStart.AddListener(PointDown);
//        }
//        else
//            Debug.Log("MyLog::找不到台灯BoundingBox");

//        Init();
//    }

//    private void OnDisable()
//    {
//        justpayRayReceiver.onPinchDown.RemoveListener(PaySuccess);
//        startpayRayReceiver.onPinchDown.RemoveListener(StartPay);

//        if (boundingBox)
//        {
//            boundingBox.onTranslateStart.RemoveListener(PointDown);
//            boundingBox.onScaleStart.RemoveListener(PointDown);
//            boundingBox.onRotateStart.RemoveListener(PointDown);
//        }
//    }
//    //有操作
//    void PointDown()
//    {
//         TaidengManager.Inst.noOperationTime = 0;
//    }
//    /// <summary>
//    /// 隐藏台灯移动旋转功能
//    /// </summary>
//    void DisableBoundBox()
//    {
//        if (boxCollider == null)
//            boxCollider = GetComponent<BoxCollider>();

//        if (boundingChild == null)
//        {
//            if (transform.Find("BoundingBox"))
//                boundingChild = transform.Find("BoundingBox").gameObject;
//        }
//        if (boundingBox == null)
//            boundingBox = GetComponent<BoundingBox>();

//        if (boundingBox)
//            boundingBox.enabled = false;
//        if (boundingChild)
//            boundingChild.SetActive(false);
//        if (boxCollider)
//            boxCollider.enabled = false;
//    }
//    /// <summary>
//    /// 打开台灯移动旋转功能
//    /// </summary>
//    void EnableBoundBox()
//    {
//        if (boxCollider == null)
//            boxCollider = GetComponent<BoxCollider>();

//        if (boundingChild == null)
//        {
//            if (transform.Find("BoundingBox"))
//                boundingChild = transform.Find("BoundingBox").gameObject;
//        }
//        if (boundingBox == null)
//            boundingBox = GetComponent<BoundingBox>();

//        if (boxCollider)
//            boxCollider.enabled = true;
//        if (boundingChild)
//            boundingChild.SetActive(true);
//        if (!hideBoxScale && boundingBox)
//        {
//            if (boundingBox.cornerObjects != null)
//            {
//                for (int i = 0; i < boundingBox.cornerObjects.Length; i++)
//                {
//                    boundingBox.cornerObjects[i].SetActive(false);
//                }
//                hideBoxScale = true;
//            }
//        }
//    }

//    /// <summary>
//    /// 放置台灯
//    /// </summary>
//    public void PlaceTaideng()
//    {
//        //if (!showFirst)
//        //{
//        //    showFirst = true;
//        //}
//        firstObj.SetActive(false);
//        startpayRayReceiver.gameObject.SetActive(true);
//        secondObj.SetActive(true);
//        showBoundBox = true;
//        TaidengManager.Inst.noOperationTime = 0;
//    }
//    /// <summary>
//    /// 开始购买
//    /// </summary>
//    void StartPay()
//    {
//        taidengModel.SetActive(false);
//        secondObj.SetActive(false);
//        payObj.SetActive(true);
//        startpayRayReceiver.gameObject.SetActive(false);
//        showBoundBox = false;
//        TaidengManager.Inst.noOperationTime = 0;
//    }

//    void PaySuccess()
//    {
//        payObj.SetActive(false);
//        ShowPaySucess();

//        TaidengManager.Inst.noOperationTime = 0;
//    }

//    void ShowPaySucess()
//    {
//        paySuccessObj.SetActive(true);
//        Invoke("ShowPeisong", 3f);
//    }

//    void ShowPeisong()
//    {
//        paySuccessObj.SetActive(false);
//        peisongObj.SetActive(true);
//        Invoke("HidePeisong", 5f);
//    }

//    void HidePeisong()
//    {
//        peisongObj.SetActive(false);
//    }
//}

