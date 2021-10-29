using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionPanelController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_InstructionBody;

    public bool isLaunhcer;
    
    void Start()
    {
        if (!isLaunhcer)
        {m_InstructionBody.text += "\n"+"若需要回到场景选择界面，请使用开花手势打开菜单，并通过手部射线点击Home按键"
                 + "或使用GTouch射线点击回退按钮。";
        }
    }
}
