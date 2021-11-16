using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace AmazingAssets
{
    namespace DynamicRadialMasks
    {
        public class DRMEditorWindow : EditorWindow
        {
            public enum MASK_SHAPE { Torus, Tube, HeightField, Dot, Shockwave, Rings, Ripple };
            public enum MASK_ALGORITHM { Advanced, Simple }
            public enum MASK_BLEND_MODE { Additive, Normalized }
            public enum MASK_SCOPE { Local, Global }

            enum FILE_EXTENTION { cginc, shadersubgraph, asset };


            static MASK_SHAPE maskShape;
            static MASK_ALGORITHM maskAlgorithm;
            static MASK_BLEND_MODE maskBlendMode;
            static MASK_SCOPE maskScope;
            static int maskShapeCount;
            static int maskID;

            static string DRMEditorFolderPath;
            static string cgincFilePath;

            static GUIStyle headerStyle;


            [MenuItem("Assets/Create/Shader/Dynamic Radial Masks", false, 4301)]
            static public void ShowWindowFromAsset()
            {
                DRMEditorWindow window = (DRMEditorWindow)EditorWindow.GetWindow(typeof(DRMEditorWindow));
                window.titleContent = new GUIContent("Dynamic Radial Masks");

#if UNITY_2019_3_OR_NEWER
                window.minSize = new Vector2(400, 167);
                window.maxSize = new Vector2(400, 167);
#elif UNITY_2018_1_OR_NEWER
                window.minSize = new Vector2(400, 150);
                window.maxSize = new Vector2(400, 150);
#else
                window.minSize = new Vector2(400, 156);
                window.maxSize = new Vector2(400, 156);
#endif

                window.ShowUtility();
            }

            [MenuItem("Window/Amazing Assets/Dynamic Radial Masks", false, 4302)]
            static public void ShowWindowFromMainMenu()
            {
                ShowWindowFromAsset();
            }

            void OnFocus()
            {
                cgincFilePath = null;
            }

            void OnGUI()
            {
                EditorGUILayout.BeginVertical("Box");
                DrawMainWindow();
                EditorGUILayout.EndVertical();
            }


            void DrawMainWindow()
            {
                EditorGUI.BeginChangeCheck();

                maskShape = (MASK_SHAPE)EditorGUILayout.EnumPopup("Shape", maskShape);
                maskShapeCount = EditorGUILayout.IntSlider("Count", maskShapeCount, 1, 200);
                maskAlgorithm = (MASK_ALGORITHM)EditorGUILayout.EnumPopup("Algorithm", maskAlgorithm);
                maskBlendMode = (MASK_BLEND_MODE)EditorGUILayout.EnumPopup("Blend Mode", maskBlendMode);
                maskID = EditorGUILayout.IntSlider("ID", maskID, 1, 16);
                maskScope = (MASK_SCOPE)EditorGUILayout.EnumPopup("Scope", maskScope);

                if (EditorGUI.EndChangeCheck() || cgincFilePath == null)
                {
                    cgincFilePath = GetDRMFilePath(FILE_EXTENTION.cginc, maskShape, maskShapeCount, maskAlgorithm, maskBlendMode, maskID, maskScope);
                }



                GUILayout.Space(18);

                if (File.Exists(cgincFilePath))
                {
                    EditorGUILayout.BeginHorizontal("Toolbar");

                    if (GUILayout.Button("Copy Path", EditorStyles.toolbarButton))
                    {
                        TextEditor te = new TextEditor();
                        te.text = "\"" + cgincFilePath + "\"";
                        te.text = te.text.Replace(Path.DirectorySeparatorChar, '/');
                        te.SelectAll();
                        te.Copy();
                    }

                    if (GUILayout.Button("CGINC", EditorStyles.toolbarButton))
                    {
                        PingObject(cgincFilePath);
                    }

                    if (GUILayout.Button("Unity Sub Graph", EditorStyles.toolbarButton))
                    {
                        string filePath = GetDRMFilePath(FILE_EXTENTION.shadersubgraph, maskShape, maskShapeCount, maskAlgorithm, maskBlendMode, maskID, maskScope);

                        if (File.Exists(filePath))
                            PingObject(filePath);
                        else
                        {
                            CreateSubGraphFile(maskShape, maskShapeCount, maskAlgorithm, maskBlendMode, maskID, maskScope, FILE_EXTENTION.shadersubgraph);

                            AssetDatabase.Refresh();

                            PingObject(filePath);
                        }

                    }

                    if (GUILayout.Button("Amplify Shader Function", EditorStyles.toolbarButton))
                    {
                        string filePath = GetDRMFilePath(FILE_EXTENTION.asset, maskShape, maskShapeCount, maskAlgorithm, maskBlendMode, maskID, maskScope);

                        if (File.Exists(filePath))
                            PingObject(filePath);
                        else
                        {
                            CreateSubGraphFile(maskShape, maskShapeCount, maskAlgorithm, maskBlendMode, maskID, maskScope, FILE_EXTENTION.asset);

                            AssetDatabase.Refresh();

                            PingObject(filePath);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Create", EditorStyles.toolbarButton))
                    {
                        CreateCGINCFile(maskShape, maskShapeCount, maskAlgorithm, maskBlendMode, maskID, maskScope);

                        cgincFilePath = null;

                        AssetDatabase.Refresh();

                        Repaint();
                    }
                }
            }
                      
                        
            string GetDRMEditorFolderPath()
            {
                if (string.IsNullOrEmpty(DRMEditorFolderPath))
                {
                    DRMEditorFolderPath = Path.Combine(AmazingAssets.EditorUtility.GetAmazingAssetsFolderPath(), "Dynamic Radial Masks");
                }

                return DRMEditorFolderPath;
            }

            string GetTemplateFileLocation(FILE_EXTENTION _Extentnio, MASK_SHAPE _MaskShape, MASK_ALGORITHM _MaskAlgorithm, MASK_BLEND_MODE _MaskBlendMode)
            {
                string fileID = string.Empty;
                switch (_Extentnio)
                {
                    case FILE_EXTENTION.cginc: fileID = _MaskShape.ToString(); break;
                    case FILE_EXTENTION.shadersubgraph: fileID = "UnityShaderGraph"; break;
                    case FILE_EXTENTION.asset: fileID = "AmplifyShaderEditor"; break;
                }

                string fileName = string.Format("Template_{0}_{1}_{2}.txt", fileID, _MaskAlgorithm, _MaskBlendMode);

                string path = Path.Combine(GetDRMEditorFolderPath(), "Shaders");
                path = Path.Combine(path, "Templates");
                path = Path.Combine(path, fileName);

                return path;
            }
            
            string GetDRMFilePath(FILE_EXTENTION _Extention, MASK_SHAPE _MaskShape, int _ShapeCount, MASK_ALGORITHM _MaskAlgorithm, MASK_BLEND_MODE _MaskBlendMode, int _MaskID, MASK_SCOPE _MaskScope)
            {
                string fileName = string.Format("DynamicRadialMasks_{0}_{1}_{2}_{3}_ID{4}_{5}.{6}", _MaskShape, _ShapeCount, _MaskAlgorithm, _MaskBlendMode, _MaskID, _MaskScope, _Extention);

                string subFolderName = string.Empty;
                switch (_Extention)
                {
                    case FILE_EXTENTION.cginc: subFolderName = "CGINC"; break;
                    case FILE_EXTENTION.shadersubgraph: subFolderName = "Unity Shader Graph"; break;
                    case FILE_EXTENTION.asset: subFolderName = "Amplify Shader Editor"; break;
                }

                string path = Path.Combine(GetDRMEditorFolderPath(), "Shaders");
                path = Path.Combine(path, subFolderName);
                path = Path.Combine(path, _MaskShape.ToString());
                path = Path.Combine(path, fileName);

                return path;
            }


            void CreateCGINCFile(MASK_SHAPE _MaskShape, int _ShapeCount, MASK_ALGORITHM _MaskAlgorithm, MASK_BLEND_MODE _MaskBlendMode, int _MaskID, MASK_SCOPE _MaskScope)
            {
                string templateFileLocation = GetTemplateFileLocation(FILE_EXTENTION.cginc, _MaskShape, _MaskAlgorithm, _MaskBlendMode);

                string[] templateFileAllLines = ReadFileAllLines(templateFileLocation);
                if (templateFileAllLines == null || templateFileAllLines.Length == 0)
                    return;




                string[] cgincFile = new string[templateFileAllLines.Length];

                for (int i = 0; i < templateFileAllLines.Length; i++)
                {
                    if (templateFileAllLines[i].Contains("#FOR_LOOP#"))
                    {
                        if (_ShapeCount > 1)
                            templateFileAllLines[i] = templateFileAllLines[i].Replace("#FOR_LOOP#", string.Empty);
                        else
                            templateFileAllLines[i] = string.Empty;
                    }

                    cgincFile[i] = templateFileAllLines[i].Replace("#SHAPE_BIG#", _MaskShape.ToString().ToUpper()).
                                                            Replace("#SHAPE_SMALL#", _MaskShape.ToString()).
                                                            Replace("#ARRAY_LENGTH#", _ShapeCount.ToString()).
                                                            Replace("#ALGORITHM_BIG#", _MaskAlgorithm.ToString().ToUpper()).
                                                            Replace("#ALGORITHM_SMALL#", _MaskAlgorithm.ToString()).
                                                            Replace("#BLEND_MODE_BIG#", _MaskBlendMode.ToString().ToUpper()).
                                                            Replace("#BLEND_MODE_SMALL#", _MaskBlendMode.ToString()).
                                                            Replace("#ID#", _MaskID.ToString()).
                                                            Replace("#SCOPE_BIG#", _MaskScope == MASK_SCOPE.Local ? "LOCAL" : "GLOBAL").
                                                            Replace("#SCOPE_SMALL#", _MaskScope == MASK_SCOPE.Local ? "Local" : "Global").
                                                            Replace("#UNIFORM#", _MaskScope == MASK_SCOPE.Local ? string.Empty : "uniform ");
                }



                string saveFolder = Path.Combine(GetDRMEditorFolderPath(), "Shaders");
                saveFolder = Path.Combine(saveFolder, "CGINC");
                saveFolder = Path.Combine(saveFolder, _MaskShape.ToString());

                if (Directory.Exists(saveFolder) == false)
                    Directory.CreateDirectory(saveFolder);


                string saveLocalFileName = string.Format("DynamicRadialMasks_{0}_{1}_{2}_{3}_ID{4}_{5}.cginc", _MaskShape, _ShapeCount, _MaskAlgorithm, _MaskBlendMode, _MaskID, _MaskScope);
                saveLocalFileName = Path.Combine(saveFolder, saveLocalFileName);
                File.WriteAllLines(saveLocalFileName, cgincFile);
            }

            void CreateSubGraphFile(MASK_SHAPE _MaskShape, int _ShapeCount, MASK_ALGORITHM _MaskAlgorithm, MASK_BLEND_MODE _MaskBlendMode, int _MaskID, MASK_SCOPE _MaskScope, FILE_EXTENTION _Extention)
            {
                string templateFileLocation = GetTemplateFileLocation(_Extention, _MaskShape, _MaskAlgorithm, _MaskBlendMode);

                string[] templateFileAllLines = ReadFileAllLines(templateFileLocation);
                if (templateFileAllLines == null || templateFileAllLines.Length == 0)
                {
                    Debug.LogWarning("Template file not found: " + templateFileLocation);
                    return;
                }

                string cgincFilePath = GetDRMFilePath(FILE_EXTENTION.cginc, _MaskShape, _ShapeCount, _MaskAlgorithm, _MaskBlendMode, _MaskID, _MaskScope);
                string cgincFileGUID = AssetDatabase.AssetPathToGUID(cgincFilePath);
                if(string.IsNullOrEmpty(cgincFileGUID))
                {
                    Debug.LogWarning("CGINC file not found: " + cgincFilePath);
                    return;
                }


                string[] subGraphFile = new string[templateFileAllLines.Length];

                for (int i = 0; i < templateFileAllLines.Length; i++)
                {
                    subGraphFile[i] = templateFileAllLines[i].Replace("#SHAPE_BIG#", _MaskShape.ToString().ToUpper()).
                                                           Replace("#SHAPE_SMALL#", _MaskShape.ToString()).
                                                           Replace("#ARRAY_LENGTH#", _ShapeCount.ToString()).
                                                           Replace("#ALGORITHM_BIG#", _MaskAlgorithm.ToString().ToUpper()).
                                                           Replace("#ALGORITHM_SMALL#", _MaskAlgorithm.ToString()).
                                                           Replace("#BLEND_MODE_BIG#", _MaskBlendMode.ToString().ToUpper()).
                                                           Replace("#BLEND_MODE_SMALL#", _MaskBlendMode.ToString()).
                                                           Replace("#ID#", _MaskID.ToString()).
                                                           Replace("#SCOPE_BIG#", _MaskScope == MASK_SCOPE.Local ? "LOCAL" : "GLOBAL").
                                                           Replace("#SCOPE_SMALL#", _MaskScope == MASK_SCOPE.Local ? "Local" : "Global").
                                                           Replace("#CGINC_FILE_GUID#", cgincFileGUID);
                }



                string saveFolder = Path.Combine(GetDRMEditorFolderPath(), "Shaders");
                saveFolder = Path.Combine(saveFolder, _Extention == FILE_EXTENTION.asset ? "Amplify Shader Editor" : "Unity Shader Graph");
                saveFolder = Path.Combine(saveFolder, _MaskShape.ToString());

                if (Directory.Exists(saveFolder) == false)
                    Directory.CreateDirectory(saveFolder);
                

                string saveLocalFileName = string.Format("DynamicRadialMasks_{0}_{1}_{2}_{3}_ID{4}_{5}.{6}", _MaskShape, _ShapeCount, _MaskAlgorithm, _MaskBlendMode, _MaskID, _MaskScope, _Extention);
                saveLocalFileName = Path.Combine(saveFolder, saveLocalFileName);


                WriteFileAllLines(saveLocalFileName, subGraphFile);                
            }


            
            string[] ReadFileAllLines(string filePath)
            {
                if (string.IsNullOrEmpty(filePath) || File.Exists(filePath) == false)
                    return null;

                return File.ReadAllLines(filePath);
            }

            void WriteFileAllLines(string filePath, string[] fileData)
            {
                try
                {
                    File.WriteAllLines(filePath, fileData);
                }
                catch
                {
                    Debug.LogWarning("Can not create file: " + Path.GetFileName(filePath) + "\nReason: Absolute file path length exceeds 259 character limit.\nSolution: Move project closer to the system root directory, making the path shorter.\n");
                }

            }

            void PingObject(string path)
            {
                // Load object
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                if (obj == null)
                {
                    //Try folder
                    obj = AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(path), typeof(UnityEngine.Object));

                    if(obj == null)
                        return;
                }


                // Select the object in the project folder
                Selection.activeObject = obj;

                // Also flash the folder yellow to highlight it
                UnityEditor.EditorGUIUtility.PingObject(obj);
            }
        }
    }
}

