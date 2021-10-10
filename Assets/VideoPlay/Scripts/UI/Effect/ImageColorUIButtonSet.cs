using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// UI颜色效果响应  create by liangpeng 2020-7-7
/// </summary>
public class ImageColorUIButtonSet : ButtonSetFather
{
	//目标颜色
	public Color _focusColor = Color.cyan;
	public Color _pressColor = Color.gray;

	[HideInInspector]
	public Color normalColor;

	//图片组件
	Image image;

    private void OnDestroy()
    {
        image = null;
    }

    public override void OnClickDownRespons()
	{
		base.OnClickDownRespons();
        if (image)
            image.color = _pressColor;
	}
	
	public override void OnFocusRespons()
	{
        
        base.OnFocusRespons();
        if (image)
            image.color = _focusColor;
    }

	public override void OnLoseFocusRespons()
	{
        //if (gameObject.name=="")
        //Debug.Log();
		base.OnLoseFocusRespons();
		if (image)
			image.color = normalColor;
    }
    
    // Start is called before the first frame update
    public override void OnInit()
	{
		base.OnInit();
		//获取基础参数
		image = _offsetTran.GetComponent<Image>();
		if (image != null)
		{
            if (normalColor.a == 0)
            {
                normalColor = image.color;
            }
            else
            { 
                image.color = normalColor;
            }
        }
		else
			Debug.Log(gameObject.name);
	}
}
