using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using OXRTK.ARHandTracking;

/// <summary>
/// 命名，/*create by 梁鹏 2021-10-11 */
/// </summary>
public class SetName : MonoBehaviour
{
    public Text inputtext;
    public ButtonRayReceiver backtoBtn;

    Action backAction;
    private void OnDestroy()
    {
        inputtext = null;
        backtoBtn = null;
        backAction = null;
    }
    
    private void OnEnable()
    {
        backtoBtn.onPinchDown.AddListener(BackTo);
    }

    private void OnDisable()
    {
        backtoBtn.onPinchDown.RemoveListener(BackTo);
    }

    void BackTo()
    {
        backAction?.Invoke();
    }

    public void StartSetName(Action action,Action<string> confirmAction)
    {
        backAction = action;
        EditorControl.Instance.keyBoardManager.InitKeyboard(inputtext, true, confirmAction);
    }
}
