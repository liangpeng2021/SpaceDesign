using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
///  create by liangpeng 2020-8-4 按键管理
/// </summary>
public class KeyBoardManager : MonoBehaviour
{
	/// <summary>
	///	单词状态
	/// </summary>
	public enum WordState
	{
		small,
		bigTemp,
		big,
	}

	/// <summary>
	/// 键盘相关父节点
	/// </summary>
	public Transform keybordTran;

	/// <summary>
	/// 大写字母父节点
	/// </summary>
	Transform bigTran;

	/// <summary>
	/// 小写字母父节点
	/// </summary>
	Transform smallTran;

	/// <summary>
	/// 字母父节点
	/// </summary>
	Transform numTran;

	/// <summary>
	/// 符号父节点
	/// </summary>
	Transform symbolTran;

	WordState wordState = WordState.small;

	#region 配合编辑器使用，实例化预设

	public GameObject keyPrefab;

	/// <summary>
	///实例化对象
	/// </summary>
	public void InitGameObject()
	{
        bigTran = transform.Find("keyboard/word/big");
        smallTran = transform.Find("keyboard/word/small");
        numTran = transform.Find("keyboard/num");
        symbolTran = transform.Find("keyboard/symbolPar");

        InitGameobjectforChild(bigTran);
		InitGameobjectforChild(smallTran);
		InitGameobjectforChild(numTran);
		InitGameobjectforChild(symbolTran);
	}

	void InitGameobjectforChild(Transform tran)
	{
		for (int i = 0; i < tran.childCount; i++)
		{
			GameObject obj = GameObject.Instantiate(keyPrefab);
			obj.transform.SetParent(tran.GetChild(i), false);
			obj.name = tran.GetChild(i).name;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;

			obj.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = obj.name;
		}
	}

	/// <summary>
	/// 删除实例化的对象
	/// </summary>
	public void DestroyGameObject()
	{
		bigTran = transform.Find("keyboard/word/big");
		smallTran = transform.Find("keyboard/word/small");
		numTran = transform.Find("keyboard/num");
		symbolTran = transform.Find("keyboard/symbolPar");

		DestroyGameObjectforchild(bigTran);
		DestroyGameObjectforchild(smallTran);
		DestroyGameObjectforchild(numTran);
		DestroyGameObjectforchild(symbolTran);
	}

	void DestroyGameObjectforchild(Transform tran)
	{
		for (int i = 0; i < tran.childCount; i++)
		{
			if (tran.GetChild(i).childCount>0 && tran.GetChild(i).GetChild(0).gameObject)
				DestroyImmediate(tran.GetChild(i).GetChild(0).gameObject);
		}
	}
	#endregion

	OnTagetButton spaceBtn;
	OnTagetButton comBtn;
	Transform symbol;
	OnTagetButton symbolBtn;
	OnTagetButton capitalBtn;
	OnTagetButton backspaceBtn;
	OnTagetButton enterBtn;
	OnTagetButton atBtn;
	OnTagetButton qingkongBtn;

	/// <summary>
	/// 输入文本
	/// </summary>
	Text _inputText;
	
	public Text InputText
	{
		get
		{
			return _inputText;
		}
		set
		{
			_inputText = value;
		}
	}

	/// <summary>
	/// 光标闪烁脚本
	/// </summary>
	public Flash _flash;

	void InitTran()
	{
		wordObj = keybordTran.Find("word").gameObject;
		bigTran = keybordTran.Find("word/big");
		smallTran = keybordTran.Find("word/small");
		numTran = keybordTran.Find("num");

		symbolTran = keybordTran.Find("symbolPar");

		spaceBtn = keybordTran.Find("space").GetComponent<OnTagetButton>();
		comBtn = keybordTran.Find("com").GetComponent<OnTagetButton>();
		symbol = keybordTran.Find("symbol");
		symbolBtn = symbol.GetComponent<OnTagetButton>();
		symbolText = symbol.Find("GameObject").GetChild(1).GetComponent<Text>();
		symbolText.text = "#+=";
		capitalBtn = keybordTran.Find("word/capital").GetComponent<OnTagetButton>();
		backspaceBtn = keybordTran.Find("other/backspace").GetComponent<OnTagetButton>();
		enterBtn = keybordTran.Find("other/enter").GetComponent<OnTagetButton>();
		atBtn = keybordTran.Find("word/at").GetComponent<OnTagetButton>();
		qingkongBtn = keybordTran.Find("qingkong").GetComponent<OnTagetButton>();

		bigTran.gameObject.SetActive(false);
		smallTran.gameObject.SetActive(true);
		symbolTran.gameObject.SetActive(false);
		wordObj.SetActive(true);
		
		InitEvent();
		
	}
	
	/// <summary>
	/// 不同界面的光标和清空按钮有不同，这里分别处理下
	/// </summary>
	Flash normalflash;

	public void SetClearBtnAndGuangbiao(Flash flash,OnTagetButton btn)
	{
		qingkongBtn.gameObject.SetActive(false);
		_flash.gameObject.SetActive(false);
		normalflash = _flash;
		_flash = flash;
		btn.OnClickDwon.AddListener(ClearString);
	}

	public void ResetClearBtnAndGuangbiao(OnTagetButton btn)
	{
		_flash = normalflash;
		btn.OnClickDwon.RemoveListener(ClearString);
		qingkongBtn.gameObject.SetActive(true);
		_flash.gameObject.SetActive(true);
	}

	/// <summary>
	/// 注册事件
	/// </summary>
	void InitEvent()
	{
		spaceBtn.OnClickDwon.AddListener(() => ClickStringButton(" "));

		comBtn.OnClickDwon.AddListener(() => ClickStringButton(".com"));
		
		symbolBtn.OnClickDwon.AddListener(GotoSymbol);
		
		capitalBtn.OnClickDwon.AddListener(Capitalorlowercase);
		
		backspaceBtn.OnClickDwon.AddListener(BackSpaceString);
		enterBtn.OnClickDwon.AddListener(ConfirmCurInput);

		atBtn.OnClickDwon.AddListener(() => ClickStringButton("@"));
		
		qingkongBtn.OnClickDwon.AddListener(ClearString);

		InitChildEvent(bigTran);
		InitChildEvent(smallTran);
		InitChildEvent(numTran);
		InitChildEvent(symbolTran);
		
	}

	void InitChildEvent(Transform tran)
	{
		for (int i = 0; i < tran.childCount; i++)
		{
			if (tran.GetChild(i).childCount > 0)
			{
				string namestr = tran.GetChild(i).GetChild(0).name;
				
				//if (tran.GetChild(i).GetChild(0).GetComponent<OnTagetButton>() == null)
				//	Debug.Log(tran.name);
				tran.GetChild(i).GetChild(0).GetComponent<OnTagetButton>().OnClickDwon.AddListener(() => ClickStringButton(namestr));
			}
		}
	}

	void RemoveEvent()
	{
		spaceBtn.OnClickDwon.RemoveListener(() => ClickStringButton(" "));

		comBtn.OnClickDwon.RemoveListener(() => ClickStringButton(".com"));

		symbolBtn.OnClickDwon.RemoveListener(GotoSymbol);

		capitalBtn.OnClickDwon.RemoveListener(Capitalorlowercase);

		backspaceBtn.OnClickDwon.RemoveListener(BackSpaceString);
		enterBtn.OnClickDwon.RemoveListener(ConfirmCurInput);

		atBtn.OnClickDwon.RemoveListener(() => ClickStringButton("@"));

		qingkongBtn.OnClickDwon.RemoveListener(ClearString);

		RemoveChildEvent(bigTran);
		RemoveChildEvent(smallTran);
		RemoveChildEvent(numTran);
		RemoveChildEvent(symbolTran);
	}

	void RemoveChildEvent(Transform tran)
	{
		for (int i = 0; i < tran.childCount; i++)
		{
			if (tran.GetChild(i).childCount > 0)
			{
				string namestr = tran.GetChild(i).GetChild(0).name;

				//if (tran.GetChild(i).GetChild(0).GetComponent<OnTagetButton>() == null)
				//	Debug.Log(tran.name);
				tran.GetChild(i).GetChild(0).GetComponent<OnTagetButton>().OnClickDwon.RemoveListener(() => ClickStringButton(namestr));
			}
		}
	}
	
	/// <summary>
	/// 输入的字符
	/// </summary>
	void ClickStringButton(string name)
	{
		if (wordState == WordState.bigTemp)
		{
			SetWordSmall();
		}

		//Debug.Log(name);
		_inputText.text += name;
		//_inputText.color = Color.white;
		SetGuangbiaoPos();
	}

	/// <summary>
	/// 设置光标位置
	/// </summary>
	void SetGuangbiaoPos()
	{
		float width = CalculateLengthOfText(_inputText);
		_flash.transform.localPosition = new Vector3(width - _inputText.rectTransform.sizeDelta.x * 0.5f + 5, 0, 0);
	}

	/// <summary>
	/// 设置光标所在父级文本
	/// </summary>
	public void SetGuangBiaoParent()
	{
		_flash.transform.SetParent(_inputText.transform, false);
		Invoke("SetGuangbiaoPos",0.1f);
		//float width = CalculateLengthOfText(_inputText);
		
		//_flash.transform.localPosition = new Vector3(width - _inputText.rectTransform.sizeDelta.x * 0.5f+5, 0, 0);
	}

	/// <summary>
	/// 获取文本字符串宽度
	/// </summary>
	/// <param name="textComp"></param>
	/// <returns></returns>
	public static float CalculateLengthOfText(Text textComp)
	{
		Font font = textComp.font;
		int fontsize = textComp.fontSize;
		string text = textComp.text;
		font.RequestCharactersInTexture(text, fontsize, textComp.fontStyle);
		CharacterInfo characterInfo;
		float width = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			font.GetCharacterInfo(text[i], out characterInfo, fontsize);
			//width+=characterInfo.width; unity5.x提示此方法将来要废弃
			width += characterInfo.advance;
		}

		return width;
	}
	[Header("符号")]
	[HideInInspector]
	public Text symbolText;
	GameObject wordObj;
	bool isSymbol = false;

	/// <summary>
	/// 确认输入的回调
	/// </summary>
	System.Action confirmInputAction;

	/// <summary>
	/// 切换为符号面板
	/// </summary>
	void GotoSymbol()
	{
		isSymbol = !isSymbol;
		if (isSymbol)
		{
			wordObj.SetActive(false);
			symbolTran.gameObject.SetActive(true);

			symbolText.text = "ABC";
		}
		else
		{
			wordObj.SetActive(true);
			symbolTran.gameObject.SetActive(false);

			symbolText.text = "#+=";
		}
		
		Debug.Log("GotoSymbol");
	}

	#region 大小写
	[Header("大小写")]
	public GameObject smallObj;
	public GameObject bigObj;
	public GameObject bigtempObj;

	/// <summary>
	/// 切换大小写
	/// </summary>
	void Capitalorlowercase()
	{
		if (wordState == WordState.small)
		{
			SetWordBigTemp();
		}
		else if (wordState == WordState.bigTemp)
		{
			SetWordBig();
		}
		else
		{
			SetWordSmall();
		}
		
		//Debug.Log("Capitalorlowercase");
	}

	/// <summary>
	/// 设置小写
	/// </summary>
	void SetWordSmall()
	{
		smallObj.SetActive(true);
		bigObj.SetActive(false);
		bigtempObj.SetActive(false);

		bigTran.gameObject.SetActive(false);
		smallTran.gameObject.SetActive(true);
		wordState = WordState.small;
	}

	/// <summary>
	/// 设置临时大写
	/// </summary>
	void SetWordBigTemp()
	{
		smallObj.SetActive(false);
		bigObj.SetActive(false);
		bigtempObj.SetActive(true);

		bigTran.gameObject.SetActive(true);
		smallTran.gameObject.SetActive(false);
		wordState = WordState.bigTemp;
	}

	/// <summary>
	/// 设置大写
	/// </summary>
	void SetWordBig()
	{
		smallObj.SetActive(false);
		bigObj.SetActive(true);
		bigtempObj.SetActive(true);

		bigTran.gameObject.SetActive(true);
		smallTran.gameObject.SetActive(false);
		wordState = WordState.big;
	}
	#endregion

	/// <summary>
	/// 退格
	/// </summary>
	void BackSpaceString()
	{
		//Debug.Log("BackSpaceString");
		if (_inputText.text.Length > 0)
		{
			_inputText.text = _inputText.text.Remove(_inputText.text.Length - 1, 1);
			SetGuangbiaoPos();
		} 
		
	}

	/// <summary>
	/// 确认当前输入，回车功能
	/// </summary>
	void ConfirmCurInput()
	{
		confirmInputAction?.Invoke();
	}

	/// <summary>
	/// 清空输入
	/// </summary>
	public void ClearString()
	{
		_inputText.text = "";
		SetGuangbiaoPos();
	}

	/// <summary>
	/// 初始化，赋值文本框，以及文本是否赋空
	/// </summary>
	public void InitKeyboard(Text text,bool isNull,System.Action action)
	{
		_inputText = text;
		_inputText.text = isNull?"": text.text;
		SetGuangBiaoParent();
		confirmInputAction = action;
	}

	private void Awake()
	{
		InitTran();
	}
}
