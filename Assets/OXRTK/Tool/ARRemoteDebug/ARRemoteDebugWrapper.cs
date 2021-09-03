using System;
using System.Collections;
using System.Collections.Generic;

namespace OXRTK.ARRemoteDebug
{
    public class ARRemoteDebugWrapper
    {
        /**
         *@brief Initialize the remote debugger.You should always call this function before others.<br>
         *初始化函数，在调用其他函数之前，请先调用此函数.<br>
         */

        public static void Init()
        {
#if ARRemoteDebug
            Initiator starter = new Initiator();
#endif
        }

        /**
         *@brief Add callbacks to android data listener.<br> 
         *The callbacks will be invoked when there is a data from androids.<br>
         *You should only add this type of callback on Unity editor side.<br>
         *添加回调函数，当手机端发送数据时，回调函数会被调用.<br>
         *仅限于在Unity编辑器段添加此类监听函数.<br>
         *@param callback: A delegate which will receive data from android devices.<br>
         *接收android设备数据的回调函数.<br>
         */

        public static void AddAndroidDataListener(Action<object> callback)
        {
#if ARRemoteDebug
            Conduit.instance?.AddAndroidDataListener(callback);
#endif
        }

        /**
         *@brief Add callbacks to Unity editor data listener.<br> 
         *The callbacks will be invoked when there is a data from Unity editor.<br>
         *You should only add this type of callback on android side.<br>
         *添加回调函数，当Unity编辑器发送数据时，回调函数会被调用.<br>
         *仅限于在android手机端添加此类监听函数.<br>
         *@param callback: A delegate which will receive data from Unity editor.<br>
         *接收Unity编辑器数据的回调函数.<br>
         */
        public static void AddEditorDataListener(Action<object> callback)
        {
#if ARRemoteDebug
            Conduit.instance?.AddEditorDataListener(callback);
#endif
        }

        /**
         *@brief Remove callbacks for android data.<br> 
         *删除用于监听android手机数据的回调函数.<br>
         *@param callback: A delegate function added before.<br>
         *先前添加的回调函数.<br>
         *@return
         */
        public static void RemoveAndroidDataListener(Action<object> callback)
        {
#if ARRemoteDebug
            Conduit.instance?.RemoveAndroidDataListener(callback);
#endif
        }

        /**
         *@brief Remove callbacks for Unity editor data.<br> 
         *删除用于监听Unity 编辑器数据的回调函数.<br>
         *@param callback: A delegate function added before.<br>
         *先前添加的回调函数.<br>
         */
        public static void RemoveEditorDataListener(Action<object> callback)
        {
#if ARRemoteDebug
            Conduit.instance?.RemoveEditorDataListener(callback);
#endif 
        }

        /**
         *@brief Send data from Unity editor to android devices.<br> 
         *Call this function only on Unity editor side.<br>
         *向android设备发送数据，此函数应该只在Unity编辑器端调用.<br>
         *@param data: <br>
         *Any C# classes or struct. <br>
         *The class it's self and all it's fields should be serializable.<br>
         *任何类和结构体都可以，前提是此类或者结构体本身和他下面的所有成员变量都需要可以序列化.<br>
         */
        public static void SendDataToAndroid(object data)
        {
#if ARRemoteDebug
            Conduit.instance?.SendDataToAndroid(data);
#endif
        }

        /**
         *@brief Send data from androids to Unity editor. <br>
         *Call this function only on android side.<br>
         *向Unity编辑器发送数据，此函数应该只在android手机端调用.<br>
         *@param data: <br>
         *Any C# classes or struct. <br>
         *The class it's self and all it's fields should be serializable.<br>
         *任何类和结构体都可以，前提是此类或者结构体本身和他下面的所有成员变量都需要可以序列化.<br>
         */
        public static void SendDataToEditor(object data)
        {
#if ARRemoteDebug
            Conduit.instance?.SendDataToEditor(data);
#endif
        }

        /**
         *@brief Indicate if there has a connection to android device/Unity editor.<br>
         *表明当前是否有一个有效网络链接.<br>
         *@return true:There is a connection.<br>
         *网络链路有效.<br>  
         *false:The connection is invalid.<br>
         *无网链路无效.<br>
         */
        public static bool IsConnected()
        {
            bool result = false;
#if ARRemoteDebug
            result = Conduit.instance == null ? false : Conduit.instance.IsConnected();
#endif
            return result;
        }
        /**
         *@brief Indicate if the remote debugger is inited.<br>
         *表明ARRemoteDebug是否初始化过.<br>
         *@return true:The debugger is inited.<br>
         *已经初始化.<br>  
         *false:The debugger hasn't been inited.<br>
         *还未初始化.<br>
         */
        public static bool IsInited()
        {
            bool result = false;
#if ARRemoteDebug
            result = Conduit.instance == null ? false : true;
#endif
            return result;
        }

        /**
         *@brief Indicate if current runtime is simulating android device.<br> 
         *表明当前是否在模拟android环境.<br>
         *@return true:Current runtime is an android simulator.<br>
         *当前运行环境正在模拟android设备.<br>
         *false:Current runtime is not an android simulator.<br>
         *当前运行环境没有模拟android设备.<br>
         */
        public static bool CheckAndroidSimulation()
        {
            bool result = false;
#if ARRemoteDebug
            result = Conduit.instance == null ? false : Conduit.instance.CheckAndroidSimulation();
#endif
            return result;
        }

        /**
         *@brief Get current plugin version.<br>
         *获取当前插件的版本信息.<br>
         *@return version string with X.Y.Z format.<br>
         *格式为X.Y.Z的版本字符串.<br>  
         */
        public static string Version()
        {
            string version = "";
#if ARRemoteDebug
            version = Conduit.Version();
#endif
            return version;
        }
    }

}
