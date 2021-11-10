using SpaceDesign;
using Museum;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using LitJson;

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
	public string username;
    public string scn_url;
    public string[] scn_data;

    public void Clear()
    {
        error = null;
        username = null;
        scn_url = null;
        if (scn_data != null)
        {
            for (int i = 0; i < scn_data.Length; i++)
            {
                scn_data[i] = null;
            }
            scn_data = null;
        }
    }
}

/// <summary>
///  create by liangpeng 2020-7-22 用户管理类
/// </summary>

public class UserManager : MonoBehaviour
{
    public class UserLocalData
    {
        /// <summary>
        /// 本地用户信息
        /// </summary>
        public string localuserName;
        public string localPassword;
    }

    UserLocalData userLocalData = new UserLocalData();

    string userpath;
    /// <summary>
    /// 检测用户是否登录过
    /// </summary>
    public bool CheckLocalUserData()
    {
        if (userpath == null)
        {
            userpath = SpaceDesign.PathConfig.GetPth();
            userpath = Path.Combine(userpath, "user.json");
        }
        
        userLocalData = JsonController.ParseJsonLitJson<UserLocalData>(userpath);
        if (userLocalData != null)
        {
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
        if (userpath == null)
        {
            userpath = SpaceDesign.PathConfig.GetPth();
            userpath = Path.Combine(userpath, "user.json");
        }
        if (userLocalData == null)
            userLocalData = new UserLocalData();
        userLocalData.localuserName = userName;
        userLocalData.localPassword = GameTools.Encrypt(passWord);

        JsonController.SaveJsonLitJson(userpath, userLocalData);
    }

    /// <summary>
	/// 自动登录
	/// </summary>
	public bool AutoLogin(System.Action<UserData, string> action,Action<string> errorAction)
    {
        try
        {
            RequestLogin(userLocalData.localuserName, GameTools.Decrypt(userLocalData.localPassword), action);
            return true;
        }
        catch (Exception e)
        {
            errorAction?.Invoke(e.ToString());
            return false;
        }
    }

    /// <summary>
	/// 注销用户
	/// </summary>
	public void Logout()
    {
        if (userpath == null)
        {
            userpath = SpaceDesign.PathConfig.GetPth();
            userpath = Path.Combine(userpath, "user.json");
        }
        if (userLocalData != null)
        {
            userLocalData.localuserName = "";
            userLocalData.localPassword = "";
        }

        if (File.Exists(userpath))
        {
            File.Delete(userpath);
        }
    }

    //   /// <summary>
    //   /// 注册界面提交注册信息
    //   /// </summary>
    //   IEnumerator TestRegist()
    //{
    //	WWWForm wwwForm = new WWWForm();
    //	//wwwForm.AddField("username", username);
    //	//wwwForm.AddField("password", password);
    //	//wwwForm.AddField("phone", phoneNum);
    //	//wwwForm.AddField("yzm", yanzhenma);
    //	UnityWebRequest www = UnityWebRequest.Post("http://pv.sohu.com/cityjson?ie=utf-8", wwwForm);
    //	www.SendWebRequest();
    //	while (www.isDone == false)
    //	{
    //		yield return 0;
    //	}
    //	bool isShow = false;
    //	string showMsg = null;
    //	if (!string.IsNullOrEmpty(www.error))
    //		Debug.Log(www.error);
    //	if (www.isDone && string.IsNullOrEmpty(www.error))
    //	{
    //		Debug.Log(www.downloadHandler.text);
    //	}
    //	else
    //	{
    //		isShow = true;
    //		showMsg = "服务器异常，请稍后再试";
    //	}
    //	//if (isShow)
    //	//	LogManager.Instance.AddLogMessage(showMsg);
    //	Debug.Log("showMsg:" + showMsg);
    //}
    
    /// <summary>
    /// 开启请求登录的协程
    /// </summary>
    public void RequestLogin(string username,string password,System.Action<UserData, string> action)
	{
        WWWForm wwwFrom = new WWWForm();
		wwwFrom.AddField("username", username);
		wwwFrom.AddField("password", password);

        EditorControl.Instance.ShowTipKeep("加载中...");
        Action<UserData> acstion = (ud) =>
         {
             string showMesg = "";
             bool isShow = false;
             if (ud.state)
             {
                 EditorControl.Instance.HideTip();
                 SaveUserData(username, password);
             }
             else
             {
                 switch (ud.error)
                 {
                     case "name_error":
                         showMesg = "用户名错误";
                         break;
                     case "user_disabled":
                         showMesg = "用户被禁用";
                         break;
                     case "pass_error":
                         showMesg = "密码错误";
                         break;
                     case "para_null":
                         showMesg = "用户名或密码不能为空";
                         break;
                     default:
                         isShow = false;
                         break;
                 }
             }
             if (isShow)
             {
                //LogManager.Instance.AddLogMessage(/*"登录失败：" +*/ showMesg);
                Debug.Log(/*"登录失败：" + */showMesg);
             }
             action?.Invoke(ud, showMesg);
         };

        IEnumerator enumerator = YoopInterfaceSupport.Instance.GetHttpData<UserData>(wwwFrom, InterfaceName.denglu,
            acstion);
        ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
    }
    //WWWForm wwwFrom;
    //Action<UserData, string> callback;
    //IEnumerator GetHttpData(/*WWWForm wwwForm, InterfaceName interfaceName*//*, Action<T> callback*/)
    //{
    //    if (GameTools.NetWorkEnv == NetState.NoNet)
    //    {
    //        Debug.Log("MyLog::网络连接失败，请检查网络设置");
    //        yield break;
    //    }
    //    Debug.Log("MyLog::GetHttpData");
    //    Debug.Log("MyLog::YoopInterfaceSupport.Instance:"+ YoopInterfaceSupport.Instance);
    //    Debug.Log("MyLog::yoopInterfaceDic:" + YoopInterfaceSupport.Instance.yoopInterfaceDic);
    //    Debug.Log("MyLog::yoopInterfaceDicCount:" + YoopInterfaceSupport.Instance.yoopInterfaceDic.Count);
    //    Debug.Log("MyLog::yoopInterfaceDic:denglu:" + YoopInterfaceSupport.Instance.yoopInterfaceDic.ContainsKey(InterfaceName.denglu));
    //    UnityWebRequest www = UnityWebRequest.Post(YoopInterfaceSupport.Instance.yoopInterfaceDic[InterfaceName.denglu], wwwFrom);
    //    Debug.Log("MyLog::SendWebRequest");
    //    www.timeout = 60;
    //    yield return www.SendWebRequest();
    //    UserData yyd;
    //    try
    //    {
    //        Debug.Log("MyLog::22222");
    //        if (www.error != null)
    //        {
    //            Debug.Log("MyLog::" + "|www.error:" + www.error);
    //        }

    //        if (www.isDone && string.IsNullOrEmpty(www.error))
    //        {
    //            //yyd = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);
    //            Debug.Log("MyLog::www.downloadHandler.text:" + www.downloadHandler.text);
    //            yyd = JsonMapper.ToObject<UserData>(www.downloadHandler.text);
    //            callback?.Invoke(yyd,"123");
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("MyLog::" + e);
    //        //LogManager.Instance.ShowTipObj("网络超时，请稍后再试", 2f);
    //    }
    //    yield return 0;
    //}

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

		IEnumerator enumerator = YoopInterfaceSupport.Instance.GetHttpData<YanzhengData>(wwwFrom, interfaceName,
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

		ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
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
        
        IEnumerator enumerator = YoopInterfaceSupport.Instance.GetHttpData<UserData>(wwwForm, InterfaceName.zhuce,
            (rd) =>
            {
                string showMsg = "";
                bool isShow = false;
                if (rd.state)
                {
                    isShow = true;
                    showMsg = "注册成功";
                }
                else
                {
                    switch (rd.error)
                    {
                        case "verify_error":
                            isShow = true;
                            showMsg = "验证码错误";
                            break;
                        case "verify_timeout":
                            isShow = true;
                            showMsg = "验证码超时";
                            break;
                        case "user_exists":
                            isShow = true;
                            showMsg = "用户名已被注册";
                            break;
                        case "phone_exists":
                            isShow = true;
                            showMsg = "手机号已被注册";
                            break;
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
                    Debug.Log(/*"注册失败：" + */showMsg);
                    //LogManager.Instance.AddLogMessage(/*"注册失败：" + */showMsg);
                }

                action?.Invoke(rd, showMsg);
            });
        ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
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
        
        IEnumerator enumerator = YoopInterfaceSupport.Instance.GetHttpData<UserData>(wwwForm, InterfaceName.findpassword,
            (rd) =>
            {
                string showMsg = "";
                bool isShow = false;
                if (rd.state)
                {
                    isShow = true;
                    showMsg = "修改密码成功";
                }
                else
                {
                    switch (rd.error)
                    {
                        case "verify_error":
                            isShow = true;
                            showMsg = "验证码错误";
                            break;
                        case "verify_timeout":
                            isShow = true;
                            showMsg = "验证码超时";
                            break;

                        case "phone_notexists":
                            isShow = true;
                            showMsg = "该手机号未被注册";
                            break;
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
                    Debug.Log(/*"找回密码失败：" + */showMsg);
                    //TODO
                    //LogManager.Instance.ShowTipObj(showMsg, 2f);
                }

                action?.Invoke(rd, showMsg);
            });
        ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
    }
}

