using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XR;

public class ReadyMessage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start in apps");
        SendLauncherReady();
    }
    /// <summary>
    /// 通用信息发送
    /// </summary>
    /// <param name="msg"></param>
    private void SendMessage(Message msg)
    {

        AppManager.BroadcastMessage(msg);

        StringBuilder strMessage = new StringBuilder(String.Format("SendMessage:{0}", msg.GetChannel()));

        for (int i = 0; i < msg.GetKeyCount(); ++i)
        {
            strMessage.Append(string.Format(",{0}={1}", msg.GetKeyName(i), msg.GetValueString(i)));
        }


        Debug.Log(strMessage.ToString());
    }

    /// <summary>
    /// Launcher启动时信息（开机动画结束）
    /// </summary>
    public void SendLauncherReady()
    {

        if (Application.platform != RuntimePlatform.Android)

            return;

        Message msg = new Message();
        msg.SetChannel("LauncherInfo");
        msg.SetValue("msg", "LauncherReady");


        SendMessage(msg);
    }
}
