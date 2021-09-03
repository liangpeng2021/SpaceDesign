using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace OXRTK.ARRemoteDebug
{
    public class RemoteDebugWindow : EditorWindow
    {
        //[MenuItem("OXRTK/ARRemoteDebug/Uninstall")]
        static void Uninstall()
        {
            if (Directory.Exists(Application.dataPath + "/ARRemoteDebugDevelop"))
                return;
            
            if (Directory.Exists(Application.dataPath + "/ARRemoteDebug"))
                Directory.Delete(Application.dataPath + "/ARRemoteDebug", true);
                
                
            DefineSymbols.RemoveSymbols();
            
        }
        Vector2 m_ScrollPos;
        string m_ServerInfo = "";
        string m_ManualAddress = "";

        [MenuItem("OXRTK/ARRemoteDebug/Android List", false, 90)]
        static void GetAllAndroidList()
        {
            // VoltageWindow.GetWindow<T> will retrieve an existing window of type T (in this case MyFirstWindow) or create a new one if none exist.
            // Pass the name you want your window to display as a parameter.
            RemoteDebugWindow window = EditorWindow.GetWindow<RemoteDebugWindow>("Android List");

            // Once a window is retrieved, show it.
            window.Show();

        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ARRemoteDebug Version : " + Conduit.Version());
            GUILayout.Label("Android IP Address :", GUILayout.Width(120));
            m_ManualAddress = GUILayout.TextField(m_ManualAddress, GUILayout.Width(120));
            if(GUILayout.Button("Connect"))
            {
                if (Conduit.instance == null)
                {
                    EditorUtility.DisplayDialog("Attention", "Make sure you are in the playing mode", "OK");
                }
                else
                {
                    if (!string.IsNullOrEmpty(m_ManualAddress))
                        Conduit.instance.ConnectedToAndroid(m_ManualAddress);
                }
                    


 
            }
            
            EditorGUILayout.EndHorizontal();
            m_ScrollPos =
                EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 100));
            EditorGUILayout.BeginVertical();
                        
            EditorGUILayout.EndVertical();
            GUILayout.Label(m_ServerInfo);
            EditorGUILayout.EndScrollView();
            
        }

        private void Update()
        {
            Repaint();
        }
    }
}

