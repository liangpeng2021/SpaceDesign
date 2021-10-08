
using Museum;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 验证功能与Yoop交互，数据类
/// </summary>
public struct YanzhengData
{
	public bool state;
	public string error;
}
/// <summary>
/// 账号信息
/// </summary>
public struct UserData
{
	public bool state;
	public string error;
	public string nickname;
	public string address;
	public string name;
	public string img;
}

/// <summary>
///  create by liangpeng 2020-7-22 用户管理类
/// </summary>

public class UserManager : MonoBehaviour
{
	//用户本地帐号密码Key
	private const string idKey = "UserID";
	private const string pwKey = "UserPW";

	/// <summary>
	/// 本地用户信息
	/// </summary>
	public static string localuserName;
	public static string localPassword;

	/// <summary>
	/// 检测用户是否登录过
	/// </summary>
	public bool CheckLocalUserData()
	{
		if (PlayerPrefs.HasKey(idKey) && PlayerPrefs.HasKey(pwKey))
		{
			localuserName = PlayerPrefs.GetString(idKey);
			localPassword = GameTools.Decrypt(PlayerPrefs.GetString(pwKey));
			return true;
		}
		else
		{
			return false;
		}
	}
	
	/// <summary>
	/// 保存用户名和信息到本地
	/// </summary>
	public void SaveUserData(string userName, string passWord)
	{
		localuserName = userName;
		localPassword = passWord;
		PlayerPrefs.SetString(idKey, userName);
		PlayerPrefs.SetString(pwKey, GameTools.Encrypt(passWord));
	}

	/// <summary>
	/// 注销用户
	/// </summary>
	public void Logout()
	{
		//Debug.LogError("Logout");
		localuserName = "";
		localPassword = "";
		PlayerPrefs.DeleteKey(idKey);
		PlayerPrefs.DeleteKey(pwKey);
	}
	
	//void Start()
	//{
	//	StartCoroutine(TestRegist());
	//}

	/// <summary>
	/// 注册界面提交注册信息
	/// </summary>
	IEnumerator TestRegist()
	{
		WWWForm wwwForm = new WWWForm();
		//wwwForm.AddField("username", username);
		//wwwForm.AddField("password", password);
		//wwwForm.AddField("phone", phoneNum);
		//wwwForm.AddField("yzm", yanzhenma);
		UnityWebRequest www = UnityWebRequest.Post("http://pv.sohu.com/cityjson?ie=utf-8", wwwForm);
		www.SendWebRequest();
		while (www.isDone == false)
		{
			yield return 0;
		}
		bool isShow = false;
		string showMsg = null;
		if (!string.IsNullOrEmpty(www.error))
			Debug.Log(www.error);
		if (www.isDone && string.IsNullOrEmpty(www.error))
		{
			Debug.Log(www.downloadHandler.text);
		}
		else
		{
			isShow = true;
			showMsg = "服务器异常，请稍后再试";
		}
		//if (isShow)
		//	LogManager.Instance.AddLogMessage(showMsg);
		Debug.Log("showMsg:" + showMsg);
	}
	
	/// <summary>
	/// 自动登录
	/// </summary>
	public void AutoLogin(System.Action<UserData, string> action)
	{
		RequestLogin(localuserName,localPassword, action);
	}

	/// <summary>
	/// 开启请求登录的协程
	/// </summary>
	public void RequestLogin(string username,string password,System.Action<UserData, string> action)
	{
		WWWForm wwwFrom = new WWWForm();
		wwwFrom.AddField("username", username);
		wwwFrom.AddField("password", password);

        //TODO
		//IEnumerator enumerator = YoopInterfaceSupport.GetHttpVideoData<UserData>(wwwFrom, InterfaceName.denglu,
		//	(ud) =>
		//	{
		//		string showMesg = "";
		//		bool isShow = false;
		//		if (ud.state)
		//		{
		//			SaveUserData(username, password);
		//		}
		//		else
		//		{
		//			switch (ud.error)
		//			{
		//				case "name_error":
		//					showMesg = "用户名错误";
		//					break;
		//				case "user_disabled":
		//					showMesg = "用户被禁用";
		//					break;
		//				case "pass_error":
		//					showMesg = "密码错误";
		//					break;
		//				case "para_null":
		//					showMesg = "用户名或密码不能为空";
		//					break;
		//				default:
		//					isShow = false;
		//					break;
		//			}
		//		}
		//		if (isShow)
		//		{
		//			//LogManager.Instance.AddLogMessage(/*"登录失败：" +*/ showMesg);
		//			Debug.Log(/*"登录失败：" + */showMesg);
		//		}
		//		action?.Invoke(ud, showMesg);
		//	});
		//ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
	}

	/// <summary>
	/// 验证手机号
	/// </summary>
	public void YanzhengPhone(string username, string phone,InterfaceName interfaceName, System.Action<YanzhengData,string> callback)
	{
		WWWForm wwwFrom = new WWWForm();
		//Debug.Log(username);
		if (username != null && username != "")
			wwwFrom.AddField("username", username);
		wwwFrom.AddField("phone", phone);
		//Debug.Log(phone);

		IEnumerator enumerator = YoopInterfaceSupport.GetHttpVideoData<YanzhengData>(wwwFrom, interfaceName,
			(rd) =>
			{
				string showMsg = "";
				bool isShow = false;
				if (rd.state)
				{
					isShow = true;
					showMsg = "验证码已发送";
				}
				else
				{
					switch (rd.error)
					{
						case "para_null":
							isShow = true;
							showMsg = "请填写正确的注册信息";
							break;
						case "user_exists":
							isShow = true;
							showMsg = "用户名已被注册";
							break;
						case "phone_exists":
							isShow = true;
							showMsg = "手机号已被注册";
							break;
						case "sms_server_error":
						case "database_error":
							isShow = true;
							showMsg = "服务器异常，请稍后再试";
							break;
						default:
							isShow = false;
							break;
					}
				}
				if (isShow)
				{
					Debug.Log(showMsg);
				}

				callback?.Invoke(rd, showMsg);
			});
        //TODO
		//ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
	}

	/// <summary>
	/// 向后台发送注册的数据
	/// </summary>
	/// <param name="phoneNum"></param>
	/// <param name="password"></param>
	public void SendZhuceData(string username, string phoneNum, string yanzhenma, string password,System.Action<UserData ,string> action)
	{
		WWWForm wwwForm = new WWWForm();
		wwwForm.AddField("username", username);
		wwwForm.AddField("password", password);
		wwwForm.AddField("phone", phoneNum);
		wwwForm.AddField("yzm", yanzhenma);

        //TODO
		//IEnumerator enumerator = YoopInterfaceSupport.GetHttpVideoData<UserData>(wwwForm, InterfaceName.zhuce,
		//	(rd) =>
		//	{
		//		string showMsg = "";
		//		bool isShow = false;
		//		if (rd.state)
		//		{
		//			isShow = true;
		//			showMsg = "注册成功";

		//				//保存到本地
		//				SaveUserData(username, password);
		//		}
		//		else
		//		{
		//			switch (rd.error)
		//			{
		//				case "verify_error":
		//					isShow = true;
		//					showMsg = "验证码错误";
		//					break;
		//				case "verify_timeout":
		//					isShow = true;
		//					showMsg = "验证码超时";
		//					break;
		//				case "user_exists":
		//					isShow = true;
		//					showMsg = "用户名已被注册";
		//					break;
		//				case "phone_exists":
		//					isShow = true;
		//					showMsg = "手机号已被注册";
		//					break;
		//				case "database_error":
		//					isShow = true;
		//					showMsg = "服务器异常，请稍后再试";
		//					break;
		//				default:
		//					isShow = false;
		//					break;
		//			}
		//		}
		//		if (isShow)
		//		{
		//			Debug.Log(/*"注册失败：" + */showMsg);
		//			//LogManager.Instance.AddLogMessage(/*"注册失败：" + */showMsg);
		//		}

		//		action?.Invoke(rd, showMsg);
		//	});
		//ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
	}

	/// <summary>
	/// 发送找回密码的信息到后台
	/// </summary>
	/// <param name="phone"></param>
	/// <param name="yanzhengma"></param>
	/// <param name="password"></param>
	/// <param name="action"></param>
	public void SendChangePasswordData(string phone,string yanzhengma,string password, System.Action<UserData, string> action)
	{
		WWWForm wwwForm = new WWWForm();
		wwwForm.AddField("phone", phone);
		wwwForm.AddField("yzm", yanzhengma);
		wwwForm.AddField("password", password);

        //TODO
		//IEnumerator enumerator = YoopInterfaceSupport.GetHttpVideoData<UserData>(wwwForm, InterfaceName.findpassword,
		//	(rd) =>
		//	{
		//		string showMsg = "";
		//		bool isShow = false;
		//		if (rd.state)
		//		{
		//			isShow = true;
		//			showMsg = "修改密码成功";

		//			SaveUserData(rd.name, password);
		//		}
		//		else
		//		{
		//			switch (rd.error)
		//			{
		//				case "verify_error":
		//					isShow = true;
		//					showMsg = "验证码错误";
		//					break;
		//				case "verify_timeout":
		//					isShow = true;
		//					showMsg = "验证码超时";
		//					break;

		//				case "phone_notexists":
		//					isShow = true;
		//					showMsg = "该手机号未被注册";
		//					break;
		//				case "database_error":
		//					isShow = true;
		//					showMsg = "服务器异常，请稍后再试";
		//					break;
		//				default:
		//					isShow = false;
		//					break;
		//			}
		//		}
		//		if (isShow)
		//		{
		//			Debug.Log(/*"找回密码失败：" + */showMsg);
  //                  LogManager.Instance.ShowTipObj(showMsg,2f);
  //                  //LogManager.Instance.AddLogMessage(/*"找回密码失败：" +*/ showMsg);
  //              }

		//		action?.Invoke(rd, showMsg);
		//	});
		//ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
	}
}

