/*
** Copyright(C) OPPO Limited
**
** Licensed under the Apache License, Version 2.0 (the "License");
** you may not use this file except in compliance with the License.
** You may obtain a copy of the License at
**
**     http://www.apache.org/licenses/LICENSE-2.0
**
** Unless required by applicable law or agreed to in writing, software
** distributed under the License is distributed on an "AS IS" BASIS,
** WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
** See the License for the specific language governing permissions and
** limitations under the License.
*/
using UnityEditor;
using UnityEngine;

namespace XR.Samples.Editor
{
    public class PackageWindow : EditorWindow
    {
        static PackageWindow m_packageWindow;
        public static void ShowWindow()
        {
            if (m_packageWindow == null)
            {
                m_packageWindow = GetWindow<PackageWindow>();
                m_packageWindow.titleContent = new GUIContent(Constants.SDKName + " Samples");
            }
            m_packageWindow.Focus();
        }

        void OnGUI()
        {
            PackageUtilities.OnGUI();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}