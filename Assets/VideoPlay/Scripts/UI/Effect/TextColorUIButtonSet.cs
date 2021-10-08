

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// UI颜色效果响应  create by liangpeng 2020-7-7
/// </summary>
public class TextColorUIButtonSet : ButtonSetFather
{
	//目标颜色
	public Color _focusColor = Color.cyan;
	public Color _pressColor = Color.gray;

	[HideInInspector]
	public Color normalColor;

	//文字组件
	Text _text;

	public override void OnClickDownRespons()
	{
		base.OnClickDownRespons();
		_text.color = _pressColor;
	}
	
	public override void OnFocusRespons()
	{
		//if (gameObject.name == "faxian")
		//{
		//	Debug.Log(1);
		//}
		base.OnFocusRespons();
		_text.color = _focusColor;
	}

	public override void OnLoseFocusRespons()
	{
		base.OnLoseFocusRespons();
		_text.color = normalColor;
	}

	// Start is called before the first frame update
	public override void OnInit()
	{
		//if (gameObject.name == "name")
		//{
		//	Debug.Log(1);
		//}
		base.OnInit();
		//获取基础参数
		_text =_offsetTran.GetComponent<Text>();
		if (_text != null)
		{
            if (normalColor.a == 0)
                normalColor = _text.color;
            else
                _text.color = normalColor;
        }
		//if (gameObject.name.Equals("name"))
		//	Debug.Log(1);
	}
}
