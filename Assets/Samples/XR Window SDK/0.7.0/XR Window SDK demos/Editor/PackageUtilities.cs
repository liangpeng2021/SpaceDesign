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
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace XR.Samples.Editor
{
    [InitializeOnLoad]
    public class PackageUtilities
    {
        class PackageDependency
        {
            public readonly string packageName;
            public readonly string displayName;
            public bool installed;
            public PackageDependency(string packageName, string displayName)
            {
                this.packageName = packageName;
                this.displayName = displayName;
                installed = false;
            }
        }
        static ListRequest Request;
        static List<PackageDependency> s_dependencies = new List<PackageDependency>() {
            new PackageDependency("com.unity.textmeshpro","TextMeshPro")
        };
        static int s_toBeInstalled = 0;

        static PackageUtilities()
        {
            ListDependencies();
        }

        static void ListDependencies()
        {
            Request = Client.List(true, true);
            EditorApplication.update += Progress;
        }

        static void CheckDependencies(PackageCollection collection)
        {
            HashSet<string> installed = new HashSet<string>();
            foreach (var package in collection)
            {
                installed.Add(package.name);
            }
            s_toBeInstalled = 0;
            foreach (var dependency in s_dependencies)
            {
                if (installed.Contains(dependency.packageName))
                {
                    dependency.installed = true;
                }
                else
                {
                    dependency.installed = false;
                    ++s_toBeInstalled;
                }
            }



            if (s_toBeInstalled > 0)
            {
                PackageWindow.ShowWindow();
            }
        }

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                {
                    CheckDependencies(Request.Result);
                }
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }
        }

        public static void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("XR Window SDK Samples Dependencies", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                if (s_toBeInstalled > 0)
                {
                    foreach (var dependency in s_dependencies)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(dependency.displayName);
                        if (dependency.installed)
                        {
                            GUILayout.Label("Installed");
                        }
                        else if (GUILayout.Button("Install"))
                        {
                            Client.Add(dependency.packageName);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("All dependencies are already installed");
                }
            }
            GUILayout.EndVertical();
        }
    }
}