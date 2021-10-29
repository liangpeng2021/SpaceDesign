using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveInteractionExampleDisplay : MonoBehaviour
{
    /// <summary>
    /// Image move type. FreeMove - move with hand; SnapPage - move with hand, snap to nearest page when hand leave; WaveOnly - only move page by page when wave received.
    /// <br>图片移动方式. FreeMove - 跟随手动; SnapPage - 跟随手动，手离开后自动移动到最近的页面; WaveOnly - 仅在收到左右挥后整页移动.</br>
    /// </summary>
    enum MoveStyle
    {
        FreeMove, SnapPage, WaveOnly
    }
    [SerializeField] QuadScrollRect m_QuadScrollRect;
    [SerializeField] GameObject m_WaveCDIcon;
    [SerializeField] TMPro.TMP_Text m_WaveCDText;
    [SerializeField] MoveStyle m_MoveStyle = MoveStyle.FreeMove;
    [SerializeField] bool m_LoopPage;

    bool m_ManualMoveEnable = false;
    float m_CDTimer = 0f;

    public bool needPress;
    private bool isPressed;
    
    #region Manual Move

    /// <summary>
    /// Lock/Unlock for manualmove. No action when waveonly mode.<br>锁定/解锁，以备手动移动. 【仅挥动】模式不做动作.</br>
    /// </summary>
    /// <param name="isLocked">Is Lock.<br>是否锁定.</br></param>
    public void LockForManualMove(bool isLocked)
    {
        if (m_MoveStyle == MoveStyle.WaveOnly)
            return;
        if (isLocked)
        {
            m_QuadScrollRect.g_IsMoving = false;
            m_ManualMoveEnable = true;
            CancelInvoke("SnapToNearest");
        }
        else
        {
            m_ManualMoveEnable = false;
            if (m_MoveStyle == MoveStyle.SnapPage)
                Invoke("SnapToNearest", 0.5f);
        }
    }

    /// <summary>
    /// ManualMove the image.<br>手动移动画面.</br>
    /// </summary>
    /// <param name="delta">Move delta.<br>移动距离.</param>
    public void ManualMove(Vector2 delta)
    {
        if (!needPress || (needPress && isPressed))
        {
            if (m_ManualMoveEnable && !m_QuadScrollRect.g_IsMoving)
            {
                m_QuadScrollRect.ManualMove(delta);
            }
        }
    }

    void SnapToNearest()
    {
        m_QuadScrollRect.SnapToNearest();
    }

    #endregion Manual Move



    #region Wave

    /// <summary>
    /// For receive Wave interaction.<br>用来接收挥动交互.</br>
    /// </summary>
    /// <param name="dir">Hand wave direction, 1 wave right, -1 wave left.<br>手挥动方向, 1右挥，-1左挥.</param>
    public void WaveHandle(Vector2Int dir)
    {
        if (!needPress || (needPress && isPressed))
        {
            m_QuadScrollRect.MovePage(dir);
        }
    }

    public void UpdateWaveCDDisplay(float time)
    {
        m_CDTimer = time;
        m_WaveCDIcon.SetActive(m_CDTimer > 0);
        m_WaveCDText.text = m_CDTimer.ToString("0.00s");
    }

    void Update()
    {
        if (m_CDTimer > 0)
        {
            m_CDTimer -= Time.deltaTime;
            if (m_CDTimer <= 0)
            {
                m_CDTimer = 0;
                m_WaveCDIcon.SetActive(false);
            }
            m_WaveCDText.text = m_CDTimer.ToString("0.00s");
        }
        DebugUpdate();
    }

    #endregion Wave

    public void ChangePressStatus(bool pressStatus)
    {
        isPressed = pressStatus;
    }

    void DebugUpdate()
    {

    }
}
