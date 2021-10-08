using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

///b*[^:b#/]+.*$,用于统计代码行数,pico SDK 18327

/// <summary>
/// 账户相关UI的控制  create by liangpeng 2020-8-3
/// </summary>

public class UserUI : MonoBehaviour
{
	#region 登录注册界面
	[Header("登录注册界面")]
	//切换时的颜色
	public Color NormalColor = Color.gray;

	public Color ClickDownColor = Color.white;

	//登录功能切换
	public Transform dengluzhuceTran;

	/// <summary>
	/// 登录、注册、找回密码三子界面的切换按钮
	/// </summary>
	public TextColorUIButtonSet[] BtnSet;

	/// <summary>
	/// 登录、注册、找回密码三子界面的切换按钮的文字
	/// </summary>
	Text[] BtnText;
	
	/// <summary>
	/// 键盘控制脚本
	/// </summary>
	[SerializeField]
	KeyBoardManager _keyBoardManager;

	/// <summary>
	/// 账号信息与后台交互
	/// </summary>
	[SerializeField]
	UserManager userManager;

	/// <summary>
	/// 文本父节点
	/// </summary>
	public Transform[] inputTran;

	int curInputID = -1;//0为登录界面，1位注册界面，2为找回密码界面

	/// <summary>
	/// 输入文本
	/// </summary>
	List<Text> inputTexts=new List<Text>();

	/// <summary>
	/// 当前文本下标
	/// </summary>
	int curTextId=0;
	
	/// <summary>
	/// 错误提示
	/// </summary>
	public Text _errorTipText;
	public string[] errorTipStr;
	
	//下一个和最终的确认按钮
	OnTagetButton okBtn;
	OnTagetButton nextBtn;
	[HideInInspector]
	public BoxCollider nextBtnBox;

	/// <summary>
	/// 返回上一级
	/// </summary>
	public OnTagetButton backtolastBtn;

	/// <summary>
	/// 验证码读秒
	/// </summary>
	public Text dumiaoText;
	BoxCollider dumiaoCol;
    
	public UserData yoopUserData;
    
	void InitDengluZhuce()
	{
		BtnText = new Text[BtnSet.Length];
		for (int i = 0; i < BtnSet.Length; i++)
		{
			BtnText[i] = BtnSet[i].GetComponent<Text>();

			OnTagetButton onTagetButton = BtnText[i].GetComponent<OnTagetButton>();
			int temid = i;
			onTagetButton.OnClickDwon.AddListener(()=> {ClickBtn(temid); });
		}
        
		okBtn = dengluzhuceTran.Find("OK").GetComponent<OnTagetButton>();

		nextBtn= dengluzhuceTran.Find("next").GetComponent<OnTagetButton>();
		nextBtnBox = nextBtn.GetComponent<BoxCollider>();

		okBtn.OnClickDwon.AddListener(ConfirmOK);
		nextBtn.OnClickDwon.AddListener(ConfirmToNext);
		nextBtn.gameObject.SetActive(true);
		okBtn.gameObject.SetActive(false);
		
		backtolastBtn.OnClickDwon.AddListener(BackToInput);

		dumiaoCol = dumiaoText.GetComponent<BoxCollider>();

		OnTagetButton chongxinfasongBtn = dumiaoText.GetComponent<OnTagetButton>();
		chongxinfasongBtn.OnClickDwon.AddListener(SendYanzhengmaAgain);

        ClickBtn(0);
    }
    
	void ResetInputText()
	{
		for (int i = 0; i < inputTran.Length; i++)
		{
			for (int k = 0; k < inputTran[i].childCount; k++)
			{
				inputTran[i].GetChild(k).GetComponent<Text>().text = "";
			}
		}
	}
    
	Text userNameText;
	Text phoneText;
	Text yanzhengmaText;
	Text passwordText;

	/// <summary>
	/// 设置输入文本的功能
	/// </summary>
	void SetInputTextFunction(int id)
	{
		switch (id)
		{
			//登录界面时
			case 0:
				userNameText = inputTexts[0];
				passwordText = inputTexts[1];
				break;
				//注册界面时
			case 1:
				userNameText = inputTexts[0];
				phoneText = inputTexts[1];
				yanzhengmaText = inputTexts[2];
				passwordText = inputTexts[3];
				break;
				//找回密码界面时
			case 2:
				phoneText = inputTexts[0];
				yanzhengmaText = inputTexts[1];
				passwordText = inputTexts[2];
				break;
		}
	}

	/// <summary>
	/// 点击按钮,切换子界面
	/// </summary>
	void ClickBtn(int id)
	{
		if (id == curInputID)
			return;
		ResetInputText();

		curInputID = id;

		nextBtnBox.GetComponent<Image>().color = Color.white;
		nextBtnBox.enabled = true;
		isVerifyCodeRight = false;
		HideErrorTipText();

		dumiaoText.gameObject.SetActive(false);

		for (int i = 0; i < BtnSet.Length; i++)
		{
			if (id == i)
			{
				BtnSet[i].normalColor = ClickDownColor;
				BtnText[i].color = ClickDownColor;

				inputTran[i].gameObject.SetActive(true);

				inputTexts.Clear();
				//Debug.Log(inputTran[i].name);
				//Debug.Log(inputTran[i].childCount);
				for (int k = 0; k < inputTran[i].childCount; k++)
				{
					inputTexts.Add(inputTran[i].GetChild(k).GetComponent<Text>());
				}
				SetCurText(0, -1);
			}
			else
			{
				BtnSet[i].normalColor = NormalColor;
				BtnText[i].color = NormalColor;
				inputTran[i].gameObject.SetActive(false);
			}
		}

		SetInputTextFunction(id);

	}

	/// <summary>
	/// 确认输入的内容
	/// </summary>
	void ConfirmOK()
	{
		if (OnRegistPW(passwordText.text))
		{
			//Debug.Log("ConfirmOK");
			if (curInputID == 1)
				userManager.SendZhuceData(userNameText.text, phoneText.text, yanzhengmaText.text, passwordText.text,
					(ud, mg) =>
					{
						SetZhuceResult(ud, mg);
					});
			else if (curInputID == 0)
				userManager.RequestLogin(userNameText.text, passwordText.text,
					(ud, mg) =>
					{
						SetDengluResult(ud, mg);
					}
					);
			else
			{
				
				userManager.SendChangePasswordData(phoneText.text, yanzhengmaText.text, passwordText.text,
					(ud,mg)=>
					{
						
						SetDFindPasswordResult(ud,mg);
					}
					);
			}
		} 
		else
		{
			_errorTipText.text = errorTipStr[inputTexts.Count - 1];
		}
	}

	/// <summary>
	/// 确认输入并切换到下一个
	/// </summary>
	void ConfirmToNext()
	{
		SetCurText(curTextId+1,curTextId);
	}

	bool isVerifyCodeRight = false;

	void SetCurText(int id,int lastid)
	{
		dumiaoText.gameObject.SetActive(false);
		for (int i = 0; i < inputTexts.Count; i++)
		{
			//点击下一步时执行
			//上次的id比较小，先执行，输入有误就返回提示错误，无误再继续
			if (lastid<id && lastid == i)
			{
				//如果输入无误，显示下一个输入文本
				if (IsNotInputError(inputTexts[lastid]))
				{
					//如果是手机号点击下一步，先验证下
					if (inputTexts[lastid].Equals(phoneText) && !isVerifyCodeRight)
					{
						dumiaoText.gameObject.SetActive(true);
						nextBtnBox.enabled = false;
						nextBtnBox.GetComponent<Image>().color = Color.gray;
						ChooseURLToYanzheng();
					}
					_errorTipText.gameObject.SetActive(false);
					inputTexts[lastid].gameObject.SetActive(false);
					continue;
				}
				else
				{
					_errorTipText.text = errorTipStr[i];
					_errorTipText.gameObject.SetActive(true);
					return;
				} 
			}

			//需要显示的打开，更改当前对应的文本
			if (i == id)
			{
				curTextId = id;
				inputTexts[i].gameObject.SetActive(true);

				//传输回车键的回调函数，下一步或者最终确认
				Action action=null;
				if (id == inputTexts.Count - 1)
					action = ConfirmOK;
				else
					action = ConfirmToNext;
				_keyBoardManager.InitKeyboard(inputTexts[i],false, action);
			}
			else
			{
				inputTexts[i].gameObject.SetActive(false);
			}
		}

		ShowOrHideBtn(id);
	}

	/// <summary>
	/// 控制相关按钮的显示和隐藏
	/// </summary>
	void ShowOrHideBtn(int id)
	{
		if (id == inputTexts.Count - 1)
		{
			nextBtn.gameObject.SetActive(false);
			okBtn.gameObject.SetActive(true);
		}
		else
		{
			nextBtn.gameObject.SetActive(true);
			okBtn.gameObject.SetActive(false);
		}
		if (id > 0)
			backtolastBtn.gameObject.SetActive(true);
		else
			backtolastBtn.gameObject.SetActive(false);
	}

	/// <summary>
	/// 确认输入是否有误
	/// </summary>
	bool IsNotInputError(Text tt)
	{
		if (tt.Equals(userNameText))
			return OnUserName(userNameText.text);
		else if (tt.Equals(phoneText))
			return OnRegistPhone(phoneText.text);
		else if (tt.Equals(passwordText))
			return OnRegistPW(passwordText.text);
		
		return true;
	}

	/// <summary>
	/// 返回上级输入
	/// </summary>
	void BackToInput()
	{
		nextBtnBox.GetComponent<Image>().color = Color.white;
		nextBtnBox.enabled = true;
		SetCurText(curTextId - 1, curTextId);
	}

	/// <summary>
	/// 用户名输入框
	/// </summary>
	bool OnUserName(string value)
	{
		Regex regex = new Regex(@"^[a-zA-Z][a-zA-Z0-9_]{1,16}$");
		return regex.IsMatch(value);
	}

	/// <summary>
	/// 注册手机号码输入完成响应
	/// </summary>
	bool OnRegistPhone(string s)
	{
		Regex regex = new Regex("((13[0-9])|(14[5,7,9])|(15[^4])|(16[6])|(18[0-9])|(17[0,1,3,5,6,7,8])|(19[9]))\\d{8}");
		return regex.IsMatch(s);
	}
	/// <summary>
	/// 注册密码输入完成响应
	/// </summary>
	bool OnRegistPW(string s)
	{
		Regex regex = new Regex(@"^[\@A-Za-z0-9\!\#\$\%\^\&\*\.\~]{6,22}$");
		return regex.IsMatch(s);
	}
	
	/// <summary>
	/// 设置验证结果
	/// </summary>
	void SetYanzhengResult(YanzhengData yzd,string message)
	{
		isVerifyCodeRight = yzd.state;
		//碰撞框是否激活
		nextBtnBox.enabled = yzd.state;
		//颜色是否灰色
		nextBtnBox.GetComponent<Image>().color = yzd.state ? Color.white : Color.gray;
		//验证结果
		_errorTipText.text = message;
		//是否红色
		_errorTipText.color = yzd.state ? Color.yellow:Color.red;
		_errorTipText.gameObject.SetActive(true);
		if (yzd.state)
		{
			//发送成功时显示2秒隐藏
			Invoke("HideErrorTipText", 2f);
		}
	}

	void HideErrorTipText()
	{
		_errorTipText.color = Color.red;
		_errorTipText.gameObject.SetActive(false);
	}
	/// <summary>
	/// 发送验证码倒计时
	/// </summary>
	void StarVerifyTiming()
	{
		StopCoroutine("IEVerifyTiming");
		StartCoroutine("IEVerifyTiming");
	}
	/// <summary>
	/// 发送验证码倒计时
	/// </summary>
	IEnumerator IEVerifyTiming()
	{
		dumiaoCol.enabled = false;
		WaitForSeconds wfs = new WaitForSeconds(1);
		bool isRun = true;
		int i = 60;

		while (isRun)
		{
			dumiaoText.text = i.ToString();
			i--;
			if (i <= 0)
			{
				isRun = false;
			}
			yield return wfs;
		}
		dumiaoCol.enabled = true;
		dumiaoText.text = "重新发送";
		wfs = null;
	}

	/// <summary>
	/// 重新发送验证码
	/// </summary>
	void SendYanzhengmaAgain()
	{
		isVerifyCodeRight = false;
		nextBtnBox.enabled = false;
		ChooseURLToYanzheng();
	}

	void ChooseURLToYanzheng()
	{
		string username=userNameText.text;
		string phone=phoneText.text;
		InterfaceName interfaceName;
        //TODO
		////注册界面的发送验证码
		//if (curInputID == 1)
		//{
		//	interfaceName = InterfaceName.zhuceyanzheng;
			
		//}
		//else//找回密码界面的发送验证码
		//{
		//	interfaceName = InterfaceName.findpasswordyanzheng;
			
		//}
		//StarVerifyTiming();
		//userManager.YanzhengPhone(username, phone, interfaceName, 
		//	(yzd, mg) =>
		//{
		//	SetYanzhengResult(yzd, mg);
		//});
	}

	/// <summary>
	/// 设置注册的结果
	/// </summary>
	void SetZhuceResult(UserData ud, string message)
	{
		//验证结果
		if (!ud.state)
		{
			_errorTipText.text = message;
			HideErrorTipText();
		}
		else
		{
			SetLoginUI(ud);
		}
	}

	/// <summary>
	/// 设置登录结果
	/// </summary>
	void SetDengluResult(UserData ud, string message)
	{
        //TODO
		//验证结果
		//if (!ud.state)
		//{
		//	LogManager.Instance.ShowTipObj(message,2f);
		//}
		//else
		//{
		//	SetLoginUI(ud);
		//}
	}

	/// <summary>
	/// 设置找回密码结果，成功时自动登录
	/// </summary>
	void SetDFindPasswordResult(UserData ud, string message)
	{
        //验证结果
        if (!ud.state)
		{
            //TODO
            //LogManager.Instance.ShowTipObj(message, 2f);
		}
		else
		{
			SetLoginUI(ud,1.5f);
		}
	}
	
	/// <summary>
	/// 根据用户信息设置UI数据，提示内容，是否自动加载完图片自动隐藏
	/// </summary>
	/// <param name="ud"></param>
	/// <param name="showmg"></param>
	/// <param name="isAutoHide"></param>
	void SetLoginUI(UserData ud,float hideTime=0f)
	{
		yoopUserData = ud;
		//切换为已登录状态
        //TODO
		//LogManager.Instance.ShowLoadingObj();
        
		//拆分位置信息
		string[] tempAddress = ud.address.Split('/');
		string newAddress = "";
		for (int i=0;i< tempAddress.Length;i++)
		{
			newAddress += tempAddress[i];
			if (i < tempAddress.Length - 1)
				newAddress += " ";
		}
        
        //TODO
        ////开启新协程
        //IEnumerator _ieGetUserImage =YoopInterfaceSupport.GetImageFromURL(YoopInterfaceSupport.yoopInterfaceDic[InterfaceName.publicUserImageURL]+ud.img,512,512,
        //	//回调
        //	(_sprite) =>
        //	{
        //		LogManager.Instance.CloseLoadingObj(hideTime);

        //		curInputID = -1;

        //		UIManager.Instance.SetTanPanelActive(dengluzhuceTran.gameObject,false);
        //		_keyBoardManager.gameObject.SetActive(false);

        //		SwitchLoginUIState(true);

        //		for (int i = 0; i < _userImages.Length; i++)
        //		{
        //			_userImages[i].sprite = _sprite;
        //		}
        //	}
        //	,
        //          (str)=>
        //          {
        //              Debug.Log("LoginUIImage:"+str);
        //          });

        //ActionQueue.InitOneActionQueue().AddAction(_ieGetUserImage).StartQueue();
    }

    /// <summary>
    /// 修改头像和昵称后，重新赋值
    /// </summary>
    /// <param name="nickname"></param>
    /// <param name="sprite"></param>
    /// <param name="imageName"></param>
    public void SetUserData(string nickname,Sprite sprite,string imageName)
	{
		yoopUserData.nickname = nickname;
		yoopUserData.img = imageName;
	}

	#endregion
    
	private void Start()
	{
		InitDengluZhuce();
	}

	private void Awake()
	{
        //TODO
        //CheckLocalUserDataToSetLoginUI();
    }

    void CheckLocalUserDataToSetLoginUI()
	{
		//开场本地有用户数据时，自动登录
		if (userManager.CheckLocalUserData())
		{
			userManager.AutoLogin((ud, message) =>
			{
				SetDengluResult(ud, message);
				if (!ud.state)
				{
					userManager.Logout();
				}
				else
				{
                    //TODO
                    //edituserPanel.GetComponent<EditUserDataUI>().Init();
                }
            });
		}
		else
		{

        }
	}
	
}
