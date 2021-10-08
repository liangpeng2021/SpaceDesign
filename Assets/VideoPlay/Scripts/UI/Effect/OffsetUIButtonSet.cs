
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VR空间中的UI事件处理，create by liangpeng 2019-09-04，
/// </summary>
//挂在按钮上
public abstract class ButtonSetBase : MonoBehaviour
{
	public abstract void OnInit();       //初始化
	public abstract void OnFocusRespons();       //指向响应
    public abstract void OnLoseFocusRespons();   //离开响应
    public abstract void OnClickDownRespons();       //按下响应
    public abstract void OnClickUpRespons();       //抬起响应
}

public class ButtonSetFather : ButtonSetBase
{
	/// <summary>
	/// 是否影响子节点
	/// </summary>
	public bool _synchronizeChild;
	//受影响的子节点OffsetUIButtonSet类
	List<ButtonSetBase> _buttonSetBaseList = new List<ButtonSetBase>();

	/// <summary>
	/// 偏移的矩阵，为空时默认是自己,多段移动时，可以中间加个节点，分别控制
	/// </summary>
	public Transform _offsetTran;

    protected void OnEnable()
    {
        if (!isInit)
            OnInit();
    }

    protected bool isInit = false;

	public override void OnInit()
	{
        if (isInit)
            return;
		//偏移
		if (_offsetTran == null)
			_offsetTran = this.transform;

		//获取需要显示效果的子节点数据
		if (_synchronizeChild)
		{
			for (int i = 0; i < _offsetTran.childCount; i++)
			{
				ButtonSetBase _buttonSetBase = _offsetTran.GetChild(i).GetComponent<ButtonSetBase>();
				if (_buttonSetBase != null)
				{
					_buttonSetBase.OnInit();
					_buttonSetBaseList.Add(_buttonSetBase);
				}
			}
		}

		isInit = true;
	}
	public override void OnFocusRespons()
	{
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			this.GetComponent<ButtonSetBase>().OnInit();

		if (_synchronizeChild)
		{
			for (int i = 0; i < _buttonSetBaseList.Count; i++)
			{
				_buttonSetBaseList[i].OnFocusRespons();
			}
		}
	}
	public override void OnLoseFocusRespons()
	{
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			this.GetComponent<ButtonSetBase>().OnInit();

		if (_synchronizeChild)
		{
			for (int i = 0; i < _buttonSetBaseList.Count; i++)
			{
				_buttonSetBaseList[i].OnLoseFocusRespons();
			}
		}
	}
	public override void OnClickDownRespons()
	{
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			this.GetComponent<ButtonSetBase>().OnInit();

		if (_synchronizeChild)
		{
			for (int i = 0; i < _buttonSetBaseList.Count; i++)
			{
				_buttonSetBaseList[i].OnClickDownRespons();
			}
		}
	}
	public override void OnClickUpRespons()
	{
		//防止因对象被隐藏而没有初始化
		if (!isInit)
			this.GetComponent<ButtonSetBase>().OnInit();

		if (_synchronizeChild)
		{
			for (int i = 0; i < _buttonSetBaseList.Count; i++)
			{
				_buttonSetBaseList[i].OnClickUpRespons();
			}
		}
		OnFocusRespons();
	}

	//private void OnDisable()
	//{
	//	OnLoseFocusRespons();
	//}
}

/// <summary>
/// UI偏移加高亮响应
/// </summary>
public class OffsetUIButtonSet : ButtonSetFather
{
	/// <summary>
	/// 偏移
	/// </summary>
	//悬停的偏移值
	public Vector3 Focusoffset = new Vector3(0,0,-30f);
    //按下的偏移值
    public Vector3 pressoffset = new Vector3(0, 0, -15f);
	
	//初始位置
	Vector3 startLocalPos;
	Vector3 targetPos;
	bool starMove = false;
	//end

	public override void OnInit()
    {
		base.OnInit();
		startLocalPos = _offsetTran.localPosition;
	}

    /// <summary>
    /// 点击按下响应
    /// </summary>
    public override void OnClickDownRespons()
    {
		base.OnClickDownRespons();
		targetPos = pressoffset+ startLocalPos;
	}

    /// <summary>
    /// 悬停响应
    /// </summary>
    public override void OnFocusRespons()
    {
		base.OnFocusRespons();
		starMove = true;
		targetPos = Focusoffset+ startLocalPos;
	}

    /// <summary>
    /// 悬停离开响应
    /// </summary>
    public override void OnLoseFocusRespons()
    {
		base.OnLoseFocusRespons();
		starMove = true;
		targetPos = startLocalPos;
	}
	
	private void Update()
	{
		if (starMove)
		{
			////防止因对象被隐藏而没有初始化
			//if (!isInit)
			//	this.GetComponent<ButtonSetBase>().OnInit();

			if (Vector3.Distance(_offsetTran.localPosition, targetPos) > 1f)
			{
				_offsetTran.localPosition = Vector3.Lerp(_offsetTran.localPosition, targetPos, Time.deltaTime * 8f);
			}
			else
			{
				_offsetTran.localPosition = targetPos;
				starMove = false;
			}
		}
	}

	//private void OnDisable()
	//{
	//	if (!isInit)
	//		this.GetComponent<ButtonSetBase>().OnInit();
	//	starMove = false;
	//	_offsetTran.localPosition = startLocalPos;
	//}
}
