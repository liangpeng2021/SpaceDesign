using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace OXRTK.ARRemoteDebug
{
    [System.Serializable]
    class ConduitPackage
    {
        public string channel;
        public byte[] data;
    }
    public class Conduit : MonoBehaviour
    {
        
        public static Conduit instance { get; private set; }
        public int networkManSize = 1024 * 1024;
        public int tcpPort = 7777;
        
        [HideInInspector]
        public bool editorOnDevice = false;

        Dictionary<string, Action<object>> m_OnReceivedAndroidData = new Dictionary<string, Action<object>>();
        Dictionary<string, Action<object>> m_OnReceivedEditorData = new Dictionary<string, Action<object>>();
        readonly HashSet<int> m_ClientIds = new HashSet<int>();
        bool m_ServerConnected = false;
        byte[] heartBeatData;

        Telepathy.Server m_Server = null;
        Telepathy.Client m_Client = null;

        static Conduit()
        {
       
        }

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            Telepathy.Log.Info = UnityEngine.Debug.Log;
            Telepathy.Log.Warning = UnityEngine.Debug.LogWarning;
            Telepathy.Log.Error = UnityEngine.Debug.LogError;


            
            if(Application.platform == RuntimePlatform.Android ||
                    CheckAndroidSimulation())
            {
 
                //open TCP server            
                if (m_Server == null)
                {
                    m_Server = new Telepathy.Server(networkManSize);
                    m_Server.OnConnected = OnServerConnected;
                    m_Server.OnData = OnServerDataRecevied;
                    m_Server.OnDisconnected = OnServerDisconnected;

                }

                //start server
                m_Server.Start(tcpPort);
                   
            }
            else if (editorOnDevice ||
                        Application.platform == RuntimePlatform.OSXEditor ||
                        Application.platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.LinuxEditor)
            {


            }

            string message = "**HeartBeat**";
            ConduitPackage package = new ConduitPackage();
            package.channel = message;
            package.data = Helper.ObjectToByteArray(message);
            heartBeatData = Helper.ObjectToByteArray(package);
        }

        // Update is called once per frame
        void Update()
        {
            if (m_Server != null)
                m_Server.Tick(100);

            if (m_Client != null)
                m_Client.Tick(100);

            SendHeartBeat();
        }

        
        string GetNamespace()
        {
            return "**DefaultChannel**";

            /*
            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(3).GetMethod();
            var Class = methodBase.ReflectedType;
            var Namespace = Class.Namespace;

            return Namespace;
            */
        }
        

        public bool CheckAndroidSimulation()
        {
            bool simulation = false;
            Type type = Type.GetType("OXRTK.ARRemoteDebug.SimulateAndroidSide, Assembly-CSharp-Editor");
            if (type == null)
                return false;

            FieldInfo finfo = type.GetField("enabled", BindingFlags.Static | BindingFlags.Public);

            if (finfo == null)
                return false;

            simulation = (bool)finfo.GetValue(null);
            
            return simulation;
        }
        
        public void AddAndroidDataListener(Action<object> callback)
        {

            string Namespace = GetNamespace();
        
            if (!m_OnReceivedAndroidData.ContainsKey(Namespace))
                m_OnReceivedAndroidData.Add(Namespace, callback);
            else
                m_OnReceivedAndroidData[Namespace] += callback;
        }

        public void AddEditorDataListener(Action<object> callback)
        {
            string Namespace = GetNamespace();
            
            if (!m_OnReceivedEditorData.ContainsKey(Namespace))
                m_OnReceivedEditorData.Add(Namespace, callback);
            else
                m_OnReceivedEditorData[Namespace] += callback;
        }

        public void RemoveAndroidDataListener(Action<object> callback)
        {
            string Namespace = GetNamespace();
            
            if (m_OnReceivedAndroidData.ContainsKey(Namespace))
                m_OnReceivedAndroidData[Namespace] -= callback;
        }

        public void RemoveEditorDataListener(Action<object> callback)
        {
            string Namespace = GetNamespace();
            
            if (m_OnReceivedEditorData.ContainsKey(Namespace))
                m_OnReceivedEditorData[Namespace] -= callback;
        }

        public void ConnectedToAndroid(string address)
        {
            if(m_Client == null)
            {
                m_Client = new Telepathy.Client(networkManSize);
                m_Client.OnConnected = OnClientConnected;
                m_Client.OnData = OnClientDataRecevied;
                m_Client.OnDisconnected = OnClientDisconnected;
            }

            m_Client.Connect(address, tcpPort);
            //m_NetworkDiscovery?.transport?.ClientConnect(address);
        }

        public void SendDataToAndroid(object data)
        {
            ConduitPackage package = new ConduitPackage();
            package.channel = GetNamespace();
            package.data = Helper.ObjectToByteArray(data);
            if(m_Client != null && m_Client.Connected)
                m_Client.Send(new ArraySegment<byte>(Helper.ObjectToByteArray(package)));
        }

        public void SendDataToEditor(object data)
        {
            ConduitPackage package = new ConduitPackage();
            package.channel = GetNamespace();
            package.data = Helper.ObjectToByteArray(data);
            byte[] bytes = Helper.ObjectToByteArray(package);

            foreach (int id in m_ClientIds)
            {
                if(m_Server != null && m_Server.Active)
                    m_Server.Send(id,new ArraySegment<byte>(bytes));
            }
            
        }

        public bool IsConnected()
        {
            return m_ClientIds.Count > 0 || m_ServerConnected == true;
        }

        public static string Version()
        {
            return Resources.Load<TextAsset>("version").text;
        }

        void OnServerConnected(int connId)
        {
            if (!m_ClientIds.Contains(connId))
                m_ClientIds.Add(connId);
        }

        void OnServerDataRecevied(int connId, ArraySegment<byte> data)
        {
            object o = Helper.ByteArrayToObject(data.Array);
            if(o is ConduitPackage)
            {
                ConduitPackage package = (ConduitPackage)o;

                if (m_OnReceivedEditorData.ContainsKey(package.channel))
                {
                    m_OnReceivedEditorData[package.channel]?.Invoke(Helper.ByteArrayToObject(package.data));
                }

            }

        }

        void OnServerDisconnected(int connId)
        {
            if (m_ClientIds.Contains(connId))
                m_ClientIds.Remove(connId);
        }

        void OnClientConnected()
        {
            UnityEngine.Debug.Log("Client connected to server!");
            InvokeRepeating("SendHeartBeat", 1.0f, 1.0f);
            m_ServerConnected = true;
        }

        void OnClientDataRecevied(ArraySegment<byte> data)
        {
            object o = Helper.ByteArrayToObject(data.Array);
            if (o is ConduitPackage)
            {
                ConduitPackage package = (ConduitPackage)o;

                if (m_OnReceivedAndroidData.ContainsKey(package.channel))
                {
                    m_OnReceivedAndroidData[package.channel]?.Invoke(Helper.ByteArrayToObject(package.data));
                }
            }

        }

        void OnClientDisconnected()
        {
            UnityEngine.Debug.Log("Client disconnected from server!");
            m_ServerConnected = false;
        }

        void SendHeartBeat()
        {

            if (m_Client != null && m_Client.Connected)
                m_Client.Send(new ArraySegment<byte>(Helper.ObjectToByteArray(heartBeatData)));
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void OnApplicationQuit()
        {
            Stop();
        }

        void Stop()
        {
            if(m_Server != null)
            {
                m_Server.Stop();
                m_Server = null;
            }

            if(m_Client != null)
            {
                m_Client.Disconnect();
                m_Client = null;
            }
        }
    }
}

