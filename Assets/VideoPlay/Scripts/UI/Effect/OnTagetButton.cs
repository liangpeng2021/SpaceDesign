using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// VR空间中的UI事件处理，create by liangpeng 2019-09-04，
/// </summary>

//挂在按钮上
public class OnTagetButton : MonoBehaviour
{
    //定义事件
    public UnityEvent OnInit,OnClickDwon, OnFocus, OnLoseFocus, OnClickUp;

	/// <summary>
	/// 父级菜单，悬停时父级跟着一起悬停，出界时，父级一起出界，若跑到了父级上，则父级先出界再悬停
	/// </summary>
	public OnTagetButton onTagetButton_Parent;

	private bool isInit = false;
    ButtonSetBase Bsf;
    ButtonRayReceiver buttonRayReceiver;

    private void Awake()
    {
        //这里可以设置每个物体的层级方便控制点击层级
        //this.transform.gameObject.layer = 30;
    }

    private void OnEnable()
    {
        if (Bsf == null)
        {
            Bsf = this.GetComponent<ButtonSetBase>();  //实例化父类
        }
        if (buttonRayReceiver == null)
        {
            buttonRayReceiver = gameObject.GetComponent<ButtonRayReceiver>();
        }
        if (Bsf)
        {
            OnClickDwon.AddListener(Bsf.OnClickDownRespons);               //监听按下事件
            OnClickUp.AddListener(Bsf.OnClickUpRespons);    //监听抬起事件
            OnFocus.AddListener(Bsf.OnFocusRespons);               //鉴听指向事件
            OnLoseFocus.AddListener(Bsf.OnLoseFocusRespons);       //监听移开事件
        }

        //oppo ar添加事件
        if (buttonRayReceiver)
        {
            buttonRayReceiver.onPinchDown.AddListener(GetClickDown);
            buttonRayReceiver.onPinchUp.AddListener(GetClickUp);
            buttonRayReceiver.onPointerEnter.AddListener(GetFocus);
            buttonRayReceiver.onPointerExit.AddListener(LoseFocus);
        }
    }

    private void OnDisable()
    {
        if (Bsf)
        {
            OnClickDwon.RemoveListener(Bsf.OnClickDownRespons);               //监听按下事件
            OnClickUp.RemoveListener(Bsf.OnClickUpRespons);    //监听抬起事件
            OnFocus.RemoveListener(Bsf.OnFocusRespons);               //鉴听指向事件
            OnLoseFocus.RemoveListener(Bsf.OnLoseFocusRespons);       //监听移开事件
        }
        if (buttonRayReceiver)
        {
            buttonRayReceiver.onPinchDown.RemoveListener(GetClickDown);
            buttonRayReceiver.onPinchUp.RemoveListener(GetClickUp);
            buttonRayReceiver.onPointerEnter.RemoveListener(GetFocus);
            buttonRayReceiver.onPointerExit.RemoveListener(LoseFocus);
        }
    }

    private void Start()
	{
		OnInit.Invoke();
		isInit = true;
        
    }
    bool isAdd;
    
    public void GetFocus()
    {
        //if (gameObject.name == "thumbnail")
        //    Debug.Log(123);
        //防止因对象被隐藏而没有初始化
        if (!isInit)
			Start();

		OnFocus.Invoke();      //响应指向
		if (onTagetButton_Parent)
			onTagetButton_Parent.GetFocus();
	}
    public void LoseFocus()
    {
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			Start();
		OnLoseFocus.Invoke();  //响应移开
		if (onTagetButton_Parent)
			onTagetButton_Parent.LoseFocus();
	}
    public void GetClickDown()
    {
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			Start();
		OnClickDwon.Invoke();       //响应按下
    }
    public void GetClickUp()
    {
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			Start();
		OnClickUp.Invoke();       //响应抬起
    }
}

