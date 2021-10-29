using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OXRTK.ARHandTracking;
using UnityEngine.Events;
using TMPro;

public class WaveInteractionExampleManager : MonoBehaviour
{
    [System.Serializable]
    public class UnityEventString : UnityEvent<string> { }

    [SerializeField] Canvas m_MenuCanvas;

    [SerializeField] GameObject m_GazeWaveCanvas;
    [SerializeField] GameObject m_GazeDragCanvas;
    [SerializeField] GameObject m_TouchWaveCanvas;
    [SerializeField] GameObject m_TouchDragCanvas;

    [SerializeField] Button m_TouchLessModeBtn;
    [SerializeField] Button m_TouchModeBtn;

    [SerializeField] Button m_WaveModeBtn;
    [SerializeField] Button m_DragModeBtn;

    [SerializeField] Button m_LeftRightModeBtn;
    [SerializeField] Button m_UpDownModeBtn;

    [SerializeField] GameObject m_ButtonManager;

    public UnityEventString g_OnInstructionChange;

    GameObject m_ActveCanvas = null;

    Vector3 m_GazeSetupPos = new Vector3(0, -0.42f, 0);
    Vector3 m_GazeSetupScale = new Vector3(0.01f, 0.01f, .01f);

    Vector3 m_TouchSetupPos = new Vector3(0, -0.21f, -0.8f);
    Vector3 m_TouchSetupScale = new Vector3(0.005f, 0.005f, .005f);

    bool m_IsGaze = true;
    bool m_IsWave = true;
    bool m_IsLeftRight = true;
    
    private bool m_NeedResetPos = false;   

    void Start()
    {
        BtnClickTouchLessMode();
        BtnClickWaveMode();
        BtnClickLeftRightMode();
        m_MenuCanvas.worldCamera = XR.XRCameraManager.Instance.eventCamera;

        PointerManager.instance.onHandMenuChanged += ChangeMenuStatus;
    }

    private void OnDestroy()
    {
        PointerManager.instance.onHandMenuChanged -= ChangeMenuStatus;
    }

    public void BtnClickTouchLessMode()
    {
        m_IsGaze = true;
        m_TouchModeBtn.interactable = true;
        m_TouchLessModeBtn.interactable = false;
        SetTouchOrGaze();
    }
    public void BtnClickTouchMode()
    {
        m_IsGaze = false;
        m_TouchModeBtn.interactable = false;
        m_TouchLessModeBtn.interactable = true;
        SetTouchOrGaze();
    }

    public void BtnClickWaveMode()
    {
        m_IsWave = true;
        m_DragModeBtn.interactable = true;
        m_WaveModeBtn.interactable = false;
        SetWaveorDrag();
    }
    public void BtnClickDragMode()
    {
        m_IsWave = false;
        m_DragModeBtn.interactable = false;
        m_WaveModeBtn.interactable = true;
        SetWaveorDrag();
    }

    public void BtnClickLeftRightMode()
    {
        m_IsLeftRight = true;
        m_LeftRightModeBtn.interactable = false;
        m_UpDownModeBtn.interactable = true;
        SetXorY();
    }
    public void BtnClickUpDownMode()
    {
        m_IsLeftRight = false;
        m_LeftRightModeBtn.interactable = true;
        m_UpDownModeBtn.interactable = false;
        SetXorY();
    }

    void SetTouchOrGaze()
    {
        m_MenuCanvas.transform.localPosition = m_IsGaze ? m_GazeSetupPos : m_TouchSetupPos;
        m_MenuCanvas.transform.localScale = m_IsGaze ? m_GazeSetupScale : m_TouchSetupScale;
        SetWaveorDrag();
    }

    void SetWaveorDrag()
    {
        m_ActveCanvas?.SetActive(false);

        if (m_IsGaze)
            m_ActveCanvas = m_IsWave ? m_GazeWaveCanvas : m_GazeDragCanvas;
        else
            m_ActveCanvas = m_IsWave ? m_TouchWaveCanvas : m_TouchDragCanvas;

        m_ActveCanvas.SetActive(true);

        if (m_IsWave)
        {
            m_LeftRightModeBtn.gameObject.SetActive(true);
            m_UpDownModeBtn.gameObject.SetActive(true);
            SetXorY();
        }
        else
        {
            m_LeftRightModeBtn.gameObject.SetActive(false);
            m_UpDownModeBtn.gameObject.SetActive(false);
            InstructionTextChange();
        }
    }

    void SetXorY()
    {
        m_ActveCanvas.GetComponentInChildren<WaveInteraction>().SetEnabledWaveDirection(m_IsLeftRight, m_IsLeftRight, !m_IsLeftRight, !m_IsLeftRight);
        TextMeshProUGUI tpText = m_ActveCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (m_IsLeftRight)
            tpText.text = tpText.text.Replace("上下", "左右");
        else
            tpText.text = tpText.text.Replace("左右", "上下");
        InstructionTextChange();
    }

    void InstructionTextChange()
    {
        string str = m_ActveCanvas.GetComponentInChildren<TextMeshProUGUI>().text;
        g_OnInstructionChange?.Invoke(str);
        //Debug.Log(str);
    }

    void ChangeMenuStatus(bool handMenuStatus)
    {
        if(m_ButtonManager != null)
        {
            m_ButtonManager.SetActive(!handMenuStatus);
        }
    }
}
