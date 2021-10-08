using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 用于编辑器中实例化预设
/// </summary>

public class CreatePrefabEditor : Editor
{
	[MenuItem("Tools/CreatePrefab")]
	static void CreatePrefab()
	{
		if (Selection.activeGameObject != null)
		{
			KeyBoardManager keyBoardManager = Selection.activeGameObject.GetComponent<KeyBoardManager>();
			keyBoardManager.InitGameObject();
		}
	}

	[MenuItem("Tools/RemovePrefab")]
	static void RemovePrefab()
	{
		if (Selection.activeGameObject != null)
		{
			KeyBoardManager keyBoardManager = Selection.activeGameObject.GetComponent<KeyBoardManager>();
			keyBoardManager.DestroyGameObject();
		}
	}

    [MenuItem("Tools/AddButtonRay")]
    static void AddButtonRay()
    {
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<UserManager>())
        {
            AddChildButtonRay(Selection.activeGameObject.transform);
        }
    }

    static void AddChildButtonRay(Transform tran)
    {
        if (tran.GetComponent<OnTagetButton>() && tran.GetComponent<ButtonRayReceiver>()==null)
        {
            tran.gameObject.AddComponent<ButtonRayReceiver>();
        }
        for (int i = 0; i < tran.childCount; i++)
        {
            AddChildButtonRay(tran.GetChild(i));
        }
    }

    [MenuItem("Tools/RemoveButtonRay")]
    static void RemoveButtonRay()
    {
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<UserManager>())
        {
            RemoveChildButtonRay(Selection.activeGameObject.transform);
        }
    }

    static void RemoveChildButtonRay(Transform tran)
    {
        if (tran.GetComponent<OnTagetButton>() && tran.GetComponent<ButtonRayReceiver>())
        {
            DestroyImmediate(tran.gameObject.GetComponent<ButtonRayReceiver>());
        }
        for (int i = 0; i < tran.childCount; i++)
        {
            RemoveChildButtonRay(tran.GetChild(i));
        }
    }

    static Color focusColor;
    static Color pressColor;

    [MenuItem("Tools/ChangeImageColor")]
    static void ChangeImageColor()
    {
        ColorUtility.TryParseHtmlString("#2EC76BFF", out focusColor);
        ColorUtility.TryParseHtmlString("#6070FFFF", out pressColor);

        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<UserManager>())
        {
            ChangeChildImageColor(Selection.activeGameObject.transform);
        }
    }

    static void ChangeChildImageColor(Transform tran)
    {
        ImageColorUIButtonSet imageColorUIButtonSet = tran.GetComponent<ImageColorUIButtonSet>();
        TextColorUIButtonSet textColorUIButtonSet = tran.GetComponent<TextColorUIButtonSet>();
        if (imageColorUIButtonSet)
        {
            imageColorUIButtonSet._focusColor = focusColor;
            imageColorUIButtonSet._pressColor = pressColor;
        }
        if (textColorUIButtonSet)
        {
            textColorUIButtonSet._focusColor = focusColor;
            textColorUIButtonSet._pressColor = pressColor;
        }

        for (int i = 0; i < tran.childCount; i++)
        {
            ChangeChildImageColor(tran.GetChild(i));
        }
    }
}
