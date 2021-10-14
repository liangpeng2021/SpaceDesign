/* This wizard will replace a selection with an object or prefab.
* Scene objects will be cloned (destroying their prefab links).
* original coding by 'yesfish', nabbed from Unity Forums
* 'keep parent' added by Dave A (also removed 'rotation' option, using localRotation
*/

//------------ Modify by zh ------------
/*
    把场景中的多个对象统一替换为某一个预制体，保留原对象的Component等属性

    用法：
    1.在场景中选中（一个或多个）对象，然后点击【"MyTools/-Replace Selection"】路径的按钮
    2.为【Replacement Object】变量选择希望替换的预制体
    3.【KeepOriginals】变量可以保留原场景中的对象
    4.单机【Replace】按钮即可
*/
//------------------End------------------

using UnityEngine;
using UnityEditor;
using System.Collections;

public class RebackPrefab : ScriptableWizard
{
    static GameObject replacement = null;
    static bool keep = false;

    public GameObject ReplacementObject = null;
    public bool KeepOriginals = false;

    [MenuItem("MyTools/Replace Selection")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
        "Replace Selection", typeof(RebackPrefab), "Replace");
    }

    public RebackPrefab()
    {
        ReplacementObject = replacement;
        KeepOriginals = keep;
    }

    void OnWizardUpdate()
    {
        replacement = ReplacementObject;
        keep = KeepOriginals;
    }

    void OnWizardCreate()
    {
        if (replacement == null)
            return;

        Undo.RegisterSceneUndo("Replace Selection");

        Transform[] transforms = Selection.GetTransforms(
        SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform t in transforms)
        {
            GameObject g;
            PrefabType pref = EditorUtility.GetPrefabType(replacement);

            if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
            {
                g = (GameObject)EditorUtility.InstantiatePrefab(replacement);
            }
            else
            {
                g = (GameObject)Editor.Instantiate(replacement);
            }
            g.transform.parent = t.parent;
            g.name = replacement.name;
            g.transform.localPosition = t.localPosition;
            g.transform.localScale = t.localScale;
            g.transform.localRotation = t.localRotation;
        }

        if (!keep)
        {
            foreach (GameObject g in Selection.gameObjects)
            {
                GameObject.DestroyImmediate(g);
            }
        }
    }
}