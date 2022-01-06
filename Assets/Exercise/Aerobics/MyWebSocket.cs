/* Create by zh at 2021-10-15

   大屏跳操，收发指令（WebSocket功能）

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using BestHTTP.WebSocket;
using System;
using BestHTTP.Examples;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpaceDesign
{
    public class MyWebSocket : MonoBehaviour
    {
        //[SerializeField]
        //private string url = "ws://192.168.1.100:7777";
        //public InputField msg;
        //public Text console;
        private WebSocket webSocket;
        //是否初始化
        private bool bInit = false;
        //是否连接成功
        private bool bConnected = false;

        void OnApplicationQuit() { Close(); }
        void OnDestroy() { Close(); }

        //void Update()
        //{
        //    print(webSocket.IsOpen);
        //    if (Input.GetKeyDown(KeyCode.A))
        //    {
        //        StartGame();
        //    }
        //    if (Input.GetKeyDown(KeyCode.B))
        //    {
        //        EndGame();
        //    }
        //}

        //连接地址（仅Ip地址，端口号从7777，8888，9999循环尝试）
        string connectIpUrl;
        //连接成功回调函数
        Action connectCallback;

        //端口3中状态【0：7777】【1：8888】【2：9999】
        int portType = 0;

        void _InitAndConnect()
        {
            if (string.IsNullOrEmpty(connectIpUrl))
                return;
            if (connectCallback == null)
                return;
            InitAndConnect(connectIpUrl, connectCallback);
        }

        /// <summary>
        /// 初始化并连接
        /// </summary>
        public void InitAndConnect(string ipUrl, Action act)
        {
            if (string.IsNullOrEmpty(ipUrl))
                return;
            //这里的url地址不带端口号，直接赋值不带端口号的
            //!!!（后面也不可再赋值，循环端口的时候用）!!!
            connectIpUrl = ipUrl;
            connectCallback = act;

            string port = ":7777";
            switch (portType)
            {
                default:
                case 0: port = ":7777"; break;
                case 1: port = ":8888"; break;
                case 2: port = ":9999"; break;
            }

            portType++;
            //循环尝试端口
            if (portType >= 3)
                portType = 0;

            ipUrl += port;
            //print("连接：" + ipUrl);

            webSocket = new WebSocket(new Uri(ipUrl));
            webSocket.OnOpen = OnOpen;
            webSocket.OnMessage = OnMessageReceived;
            webSocket.OnError = OnError;
            webSocket.OnClosed = OnClosed;
            bInit = true;

            bConnected = false;

            webSocket.Open();
        }

        public void StartGame()
        {
            if (bConnected == false)
            {
                if (string.IsNullOrEmpty(connectIpUrl))
                    return;

                Invoke("_InitAndConnect", 2);
                return;
            }

            //print("发送开始");
            byte[] objs = new byte[6];
            objs[0] = 0x10;
            objs[1] = 0;
            objs[2] = 0;
            objs[3] = 0;
            objs[4] = 1;
            objs[5] = 0x11;
            webSocket.Send(objs);
        }
        public void EndGame()
        {
            if (bConnected == false)
            {
                if (string.IsNullOrEmpty(connectIpUrl))
                    return;

                Invoke("_InitAndConnect", 2);
                return;
            }

            //print("发送结束");
            byte[] objs = new byte[6];
            objs[0] = 0x10;
            objs[1] = 0;
            objs[2] = 0;
            objs[3] = 0;
            objs[4] = 1;
            objs[5] = 0x07;
            webSocket.Send(objs);
            //延迟一秒调用一下确认退出
            Invoke("InvokDelayConfirmEnd", 2);
        }

        void InvokDelayConfirmEnd()
        {
            //print("发送结束");
            byte[] objs = new byte[6];
            objs[0] = 0x10;
            objs[1] = 0;
            objs[2] = 0;
            objs[3] = 0;
            objs[4] = 1;
            objs[5] = 0x07;
            webSocket.Send(objs);
        }


        void Close()
        {
            if (webSocket != null && webSocket.IsOpen)
            {
                webSocket.Close();

                //webSocket.OnOpen = null;
                //webSocket.OnMessage = null;
                //webSocket.OnError = null;
                //webSocket.OnClosed = null;
                //webSocket = null;
            }
        }

        #region WebSocket Event Handlers

        /// <summary>
        /// Called when the web socket is open, and we are ready to send and receive data
        /// </summary>
        void OnOpen(WebSocket ws)
        {
            bConnected = (ws.State == WebSocketStates.Open) || (ws.IsOpen) || (webSocket.State == WebSocketStates.Open) || (webSocket.IsOpen);
            if (bConnected)
            {
                connectCallback?.Invoke();
            }
        }

        /// <summary>
        /// Called when we received a text message from the server
        /// </summary>
        void OnMessageReceived(WebSocket ws, string message)
        {
            Debug.Log("返回消息：" + message);
        }

        /// <summary>
        /// Called when the web socket closed
        /// </summary>
        void OnClosed(WebSocket ws, UInt16 code, string message)
        {
            bConnected = false;
        }

        /// <summary>
        /// Called when an error occured on client side
        /// </summary>
        void OnError(WebSocket ws, string ex)
        {
            bConnected = (ws.State == WebSocketStates.Open) || (ws.IsOpen) || (webSocket.State == WebSocketStates.Open) || (webSocket.IsOpen);
            if (bConnected == false)
            {
                if (string.IsNullOrEmpty(connectIpUrl))
                    return;

                Invoke("_InitAndConnect", 1);
                return;
            }

            //            Debug.Log("ex:" + ex);
            //            string errorMsg = string.Empty;
            //#if !UNITY_WEBGL || UNITY_EDITOR
            //            if (ws.InternalRequest.Response != null)
            //                errorMsg = string.Format("Status Code from Server: {0} and Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message);
            //#endif
            //            Debug.Log(errorMsg);
        }

        #endregion
    }
}