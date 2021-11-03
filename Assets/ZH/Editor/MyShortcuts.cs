/* Create by zh at 2021-1-14

    MyShortcuts
    自定义快捷键脚本

 */
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class MyShortcuts
{
    /// <summary>
    /// 按下Shift + Q，给当前选中的所有对象重置位置（如果是RectTransform，设置为填充父节点）
    /// </summary>
    [ClutchShortcut("RestTra", KeyCode.Q, ShortcutModifiers.Shift)]
    static void RestTra()
    {
        Transform[] ts = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);
        foreach (var v in ts)
        {
            RectTransform rct = v.GetComponent<RectTransform>();
            if (rct == null)
            {
                v.localPosition = Vector3.zero;
                v.localEulerAngles = Vector3.zero;
                v.localScale = Vector3.one;
            }
            else
            {
                rct.anchorMin = Vector2.zero;
                rct.anchorMax = Vector2.one;
                rct.pivot = new Vector2(0.5f, 0.5f);

                rct.offsetMax = rct.offsetMin = Vector2.zero;
                rct.localPosition = Vector3.zero;
                rct.localEulerAngles = Vector3.zero;
                rct.localScale = Vector3.one;
                rct.offsetMax = rct.offsetMin = Vector2.zero;
            }
        }
    }
}
