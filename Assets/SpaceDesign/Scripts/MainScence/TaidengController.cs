using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceDesign;

/// <summary>
/// 控制台灯UI显示，/*create by 梁鹏 2021-9-3 */
/// </summary>
namespace SpaceDesign.Lamp
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

        /// <summary>
        /// 默认显示的对象
        /// </summary>
        public GameObject firstObj;
        /// <summary>
        /// 射线碰到以后第二次显示的对象
        /// </summary>
        public GameObject secondObj;
        /// <summary>
        /// 购买界面的台灯的碰撞框(第二次显示的对象，3秒自动隐藏UI，碰触台灯再出现)
        /// </summary>
        public GameObject objBuyUIBoxCollider;
        /// <summary>
        /// 台灯的灯光，跟随第二次显示的对象一起开启关闭（但是台灯单独旋转，不属于其子节点）
        /// </summary>
        public GameObject pointLight;
        /// <summary>
        /// 开始购买后显示的对象
        /// </summary>
        public GameObject payObj;
        //台灯原始位置
        Vector3 taidengOriPos;
        //台灯原始旋转
        Vector3 taidengOriEur;
        //台灯原始缩放
        Vector3 taidengOriSca;

        //台灯UI原始位置
        Vector3 taidengUIOriPos;

        public bool showBoundBox { get; set; }

        BoundingBox boundingBox;
        BoxCollider boxCollider;
        GameObject boundingChild;

        /// <summary>
        /// 立即购买按钮
        /// </summary>
        public ButtonRayReceiver justpayRayReceiver;
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

        bool hideBoxScale = false;

        //-----  CL 音效  -----------------------
        public AudioSource audioSource;
        public AudioClip successClip;
        public AudioClip zhixiangClip;
        //---------------------------------------

        //------------ Modify by zh ------------
        /// <summary>
        /// 显示的状态【1-(Mark)默认空间放置界面】【2-购买界面】【3-确认购买界面】【4-支付成功】【5-配送】
        /// </summary>
        public int iShowUIState = 0;
        /// <summary>
        /// 台灯控制中（位移旋转等操作）
        /// </summary>
        public bool bTranslating;
        //------------------End------------------


        void Awake()
        {
            taidengOriPos = transform.localPosition;
            taidengOriEur = transform.localEulerAngles;
            taidengOriSca = transform.localScale;

            taidengUIOriPos = taiDengUI.localPosition;
        }

        //台灯摆放后，计时3秒隐藏UI
        float _fNoOprTime;

        private void Update()
        {
            if (iShowUIState == 0)
            {
                DisableBoundBox();
                return;
            }

            if (iShowUIState == 1 || iShowUIState == 2)
            {
                if (showBoundBox)
                {
                    //Debug.Log("EnableBoundBox");
                    EnableBoundBox();

                    if (bTranslating == false)
                    {
                        if (iShowUIState == 2)
                        {
                            _fNoOprTime += Time.deltaTime;
                            if (_fNoOprTime > 3)
                            {
                                _fNoOprTime = 0;
                                showBoundBox = false;
                                secondObj.SetActive(false);
                                objBuyUIBoxCollider.SetActive(true);
                            }
                        }
                    }
                }
                else
                    DisableBoundBox();

            }
            else
            {
                DisableBoundBox();
                iShowUIState = 0;
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


                //翻译按钮缩放，代替显隐
                taidengModel.SetActive(true);
                taiDengUI.localPosition = taidengUIOriPos;
                taiDengUI.localEulerAngles = Vector3.zero;
                taiDengUI.gameObject.SetActive(true);
            }
            else
            {
                //翻译按钮不能隐藏，要缩放到0，防止隐藏动画播放未完成bug
                taidengModel.SetActive(false);
                taiDengUI.gameObject.SetActive(false);
                //print("隐藏");

            }
        }

        public void Init()
        {
            showBoundBox = false;

            SetTranslating(false);
            iShowUIState = 0;
            firstObj.SetActive(true);
            secondObj.SetActive(false);
            pointLight.SetActive(false);
            payObj.SetActive(false);
            peisongObj.SetActive(false);
            paySuccessObj.SetActive(false);

            startpayRayReceiver.gameObject.SetActive(false);
            paySuccessObj.SetActive(false);
            objBuyUIBoxCollider.SetActive(false);
            taiDengUI.SetParent(tdUIParent);
            taiDengUI.localPosition = taidengUIOriPos;
            taiDengUI.localEulerAngles = Vector3.zero;
        }

        private void OnEnable()
        {
            startpayRayReceiver.onPinchDown.AddListener(StartPay);
            justpayRayReceiver.onPinchDown.AddListener(PaySuccess);

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

            Init();
        }

        private void OnDisable()
        {
            justpayRayReceiver.onPinchDown.RemoveListener(PaySuccess);
            startpayRayReceiver.onPinchDown.RemoveListener(StartPay);

            if (boundingBox)
            {
                boundingBox.onTranslateStart.RemoveListener(PointDown);
                boundingBox.onScaleStart.RemoveListener(PointDown);
                boundingBox.onRotateStart.RemoveListener(PointDown);
            }
        }
        //有操作
        void PointDown()
        {
            TaidengManager.Inst.noOperationTime = 0;
        }
        /// <summary>
        /// 隐藏台灯移动旋转功能
        /// </summary>
        void DisableBoundBox()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();

            if (boundingChild == null)
            {
                if (transform.Find("BoundingBox"))
                    boundingChild = transform.Find("BoundingBox").gameObject;
            }
            if (boundingBox == null)
                boundingBox = GetComponent<BoundingBox>();

            if (boundingBox && boundingBox.enabled)
                boundingBox.enabled = false;
            if (boundingChild && boundingChild.activeSelf)
                boundingChild.SetActive(false);
            if (boxCollider && boxCollider.enabled)
                boxCollider.enabled = false;
        }
        /// <summary>
        /// 打开台灯移动旋转功能
        /// </summary>
        void EnableBoundBox()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();

            if (boundingChild == null)
            {
                if (transform.Find("BoundingBox"))
                    boundingChild = transform.Find("BoundingBox").gameObject;
            }
            if (boundingBox == null)
                boundingBox = GetComponent<BoundingBox>();
            if (boundingBox && (boundingBox.enabled == false))
                boundingBox.enabled = true;
            if (boxCollider)
                boxCollider.enabled = true;
            if (boundingChild)
                boundingChild.SetActive(true);
            if (!hideBoxScale && boundingBox)
            {
                if (boundingBox.cornerObjects != null)
                {
                    for (int i = 0; i < boundingBox.cornerObjects.Length; i++)
                    {
                        boundingBox.cornerObjects[i].SetActive(false);
                    }
                    hideBoxScale = true;
                    //print("hideBoxScale = true");

                }
            }
            if (iShowUIState == 2)
            {
                if (objBuyUIBoxCollider.activeSelf == true)
                    objBuyUIBoxCollider.SetActive(false);
                if (secondObj.activeSelf == false)
                    secondObj.SetActive(true);
            }

            //if (boundingBox.edgeObjects == null)
            //    return;
            //for (int i = 0; i < boundingBox.edgeObjects.Length; i++)
            //{
            //    //x、z不旋转
            //    if (i < 4 || (i >= 8 && i < 12))
            //    {
            //        boundingBox.edgeObjects[i].SetActive(false);
            //    }
            //}
        }

        /// <summary>
        /// 放置台灯
        /// </summary>
        public void PlaceTaideng()
        {
            iShowUIState = 2;
            objBuyUIBoxCollider.SetActive(true);

            //if (!showFirst)
            //{
            //    showFirst = true;
            //}
            taiDengUI.localEulerAngles = Vector3.zero;
            taiDengUI.gameObject.SetActive(true);
            taidengModel.SetActive(true);
            firstObj.SetActive(false);
            startpayRayReceiver.gameObject.SetActive(true);
            secondObj.SetActive(true);
            pointLight.SetActive(true);
            showBoundBox = true;

            TaidengManager.Inst.noOperationTime = 0;
        }

        /// <summary>
        /// 开始购买
        /// </summary>
        void StartPay()
        {
            iShowUIState = 3;
            objBuyUIBoxCollider.SetActive(false);
            taidengModel.SetActive(false);
            //print("隐藏");

            secondObj.SetActive(false);
            pointLight.SetActive(false);
            payObj.SetActive(true);
            startpayRayReceiver.gameObject.SetActive(false);
            showBoundBox = false;
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