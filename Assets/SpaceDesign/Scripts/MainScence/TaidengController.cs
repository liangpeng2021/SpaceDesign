using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceDesign;

/// <summary>
/// 控制台灯UI显示，/*create by 梁鹏 2021-9-3 */
/// </summary>
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

    public bool showBoundBox = false;
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

    void Awake()
    {
        taidengOriPos = transform.localPosition;
        taidengOriEur = transform.localEulerAngles;
        taidengOriSca = transform.localScale;
    }

    //台灯摆放后，计时3秒隐藏UI
    float _fTime;
    private void Update()
    {
        if (showBoundBox)
        {
            //Debug.Log("EnableBoundBox");
            EnableBoundBox();

            _fTime += Time.deltaTime;
            if (_fTime >= 3)
            {
                _fTime = 0;
                showBoundBox = false;
            }

        }
        else
            DisableBoundBox();
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

    public void ResetTaidengTra()
    {
        transform.localPosition = taidengOriPos;
        transform.localEulerAngles = taidengOriEur;
        transform.localScale = taidengOriSca;
    }

    public void Init()
    {
        showBoundBox = false;

        firstObj.SetActive(true);
        secondObj.SetActive(false);
        pointLight.SetActive(false);
        payObj.SetActive(false);
        peisongObj.SetActive(false);
        paySuccessObj.SetActive(false);

        startpayRayReceiver.gameObject.SetActive(false);
        paySuccessObj.SetActive(false);
        //taidengModel.SetActive(true);
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

        if (boundingBox)
            boundingBox.enabled = false;
        if (boundingChild)
            boundingChild.SetActive(false);
        if (boxCollider)
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
            }
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
        //if (!showFirst)
        //{
        //    showFirst = true;
        //}
        taiDengUI.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
        //taiDengUI.gameObject.SetActive(true);
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
        taidengModel.SetActive(false);
        secondObj.SetActive(false);
        pointLight.SetActive(false);
        payObj.SetActive(true);
        startpayRayReceiver.gameObject.SetActive(false);
        showBoundBox = false;
        TaidengManager.Inst.noOperationTime = 0;
    }

    void PaySuccess()
    {
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
}
