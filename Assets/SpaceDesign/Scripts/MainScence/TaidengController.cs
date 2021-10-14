using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制台灯UI显示，/*create by 梁鹏 2021-9-3 */
/// </summary>
public class TaidengController : MonoBehaviour
{
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
    /// 开始购买后显示的对象
    /// </summary>
    public GameObject payObj;

    bool showFirst = false;
    BoundingBox boundingBox;
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

    bool hideBoxScale = false;

    private void Update()
    {
        if (!hideBoxScale && GetComponent<BoundingBox>())
        {
            boundingBox = GetComponent<BoundingBox>();
            for (int i = 0; i < boundingBox.cornerObjects.Length; i++)
            {
                boundingBox.cornerObjects[i].SetActive(false);
            }
            hideBoxScale = true;
        }
    }

    private void OnDestroy()
    {
        
    }

    void Init()
    {
        firstObj.SetActive(true);
        secondObj.SetActive(false);
        payObj.SetActive(false);
        peisongObj.SetActive(false);
        paySuccessObj.SetActive(false);

        startpayRayReceiver.gameObject.SetActive(false);
        paySuccessObj.SetActive(false);
        showFirst = false;
    }

    private void OnEnable()
    {
        startpayRayReceiver.onPinchDown.AddListener(StartPay);
        justpayRayReceiver.onPinchDown.AddListener(PaySuccess);

        if (boundingBox == null)
            boundingBox = GetComponent<BoundingBox>();
        if (boundingBox)
        {
            boundingBox.onTranslateStart.AddListener(PointEnter);
            boundingBox.onScaleStart.AddListener(PointEnter);
            boundingBox.onRotateStart.AddListener(PointEnter);
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
            boundingBox.onTranslateStart.RemoveListener(PointEnter);
            boundingBox.onScaleStart.RemoveListener(PointEnter);
            boundingBox.onRotateStart.RemoveListener(PointEnter);
        }
    }

    void PointEnter()
    {

        if (!showFirst)
        {
            firstObj.SetActive(false);
            startpayRayReceiver.gameObject.SetActive(true);
            secondObj.SetActive(true);
            showFirst = true;
        }
    }
    /// <summary>
    /// 开始购买
    /// </summary>
    void StartPay()
    {
        secondObj.SetActive(false);
        payObj.SetActive(true);
        startpayRayReceiver.gameObject.SetActive(false);
    }

    void PaySuccess()
    {
        payObj.SetActive(false);
        Invoke("ShowPaySucess",2f);
    }

    void ShowPaySucess()
    {
        paySuccessObj.SetActive(true);
        Invoke("ShowPeisong", 2f);
    }

    void ShowPeisong()
    {
        paySuccessObj.SetActive(false);
        peisongObj.SetActive(true);
    }
}
