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
    BoundingBoxRayReceiverHelper boundingBoxRayReceiverHelper;
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
    // Start is called before the first frame update
    void Start()
    {
        firstObj.SetActive(true);
        secondObj.SetActive(false);
        payObj.SetActive(false);
        peisongObj.SetActive(false);
        paySuccessObj.SetActive(false);

        startpayRayReceiver.gameObject.SetActive(false);
        paySuccessObj.SetActive(false);

        if (boundingBoxRayReceiverHelper == null)
            boundingBoxRayReceiverHelper = GetComponent<BoundingBoxRayReceiverHelper>();
        if (boundingBoxRayReceiverHelper)
            GetComponent<BoundingBoxRayReceiverHelper>().onPointerEnter += PointEnter;
    }

    private void OnEnable()
    {
        startpayRayReceiver.onPinchDown.AddListener(StartPay);
        justpayRayReceiver.onPinchDown.AddListener(PaySuccess);
    }

    private void OnDisable()
    {
        justpayRayReceiver.onPinchDown.RemoveListener(PaySuccess);
        startpayRayReceiver.onPinchDown.RemoveListener(StartPay);
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
