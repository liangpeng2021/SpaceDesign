using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Example display scrip that receive and use wave interaction.<br>示例显示，接收和使用挥动交互.</br>
/// </summary>
public class QuadScrollRect : MonoBehaviour
{
    /// <summary>
    /// Image move type. FreeMove - move with hand; SnapPage - move with hand, snap to nearest page when hand leave; WaveOnly - only move page by page when wave received.
    /// <br>图片移动方式. FreeMove - 跟随手动; SnapPage - 跟随手动，手离开后自动移动到最近的页面; WaveOnly - 仅在收到左右挥后整页移动.</br>
    /// </summary>
    [SerializeField] Renderer m_Render;
    [SerializeField] Vector2Int m_TotalPages = Vector2Int.zero;
    [SerializeField] bool m_LoopPage;

    [HideInInspector]public bool g_IsMoving = false;

    Vector2 m_PageSize = Vector2.one;
    Vector2Int m_PageIndexs = Vector2Int.zero;

    Vector2 m_MoveToPos = Vector2.zero;
    Vector2 m_MoveDirs = Vector2.one;
    Vector2 m_MoveSpeeds = Vector2.one;
    Vector2 m_MaxRange = Vector2.one;
    Vector2 m_TextureOffset = Vector2.zero;

    void Start()
    {
        m_TextureOffset = m_Render.material.mainTextureOffset;
        if (m_TotalPages.x > 1)
        {
            m_PageSize.x = 1f / m_TotalPages.x;
            m_MaxRange.x = m_PageSize.x * (m_TotalPages.x - 1);
        }
        else
        {
            m_PageSize.x = m_Render.material.mainTextureScale.x;
            m_MaxRange.x = 1f - m_Render.material.mainTextureScale.x;
        }
        if (m_TotalPages.y > 1)
        {
            m_PageSize.y = 1f / m_TotalPages.y;
            m_MaxRange.y = m_PageSize.y * (m_TotalPages.y - 1);
        }
        else
        {
            m_PageSize.y = m_Render.material.mainTextureScale.y;
            m_MaxRange.y = 1f - m_Render.material.mainTextureScale.y;
        }
    }

    void Update()
    {
        if (g_IsMoving)
        {
            MoveOffset(Vector2.Scale(m_MoveDirs, m_MoveSpeeds) * Time.deltaTime);
            CheckArrive();
        }
    }

    public void MovePage(Vector2Int dir)
    {
        m_PageIndexs -= dir;

        if (!m_LoopPage)
        {
            if (m_PageIndexs.x < 0) m_PageIndexs.x = 0;
            if (m_PageIndexs.x >= m_TotalPages.x) m_PageIndexs.x = m_TotalPages.x - 1;
            if (m_PageIndexs.y < 0) m_PageIndexs.y = 0;
            if (m_PageIndexs.y >= m_TotalPages.y) m_PageIndexs.y = m_TotalPages.y - 1;
        }

        m_MoveToPos.x = m_PageIndexs.x * m_PageSize.x;
        m_MoveToPos.y = m_PageIndexs.y * m_PageSize.y;

        m_MoveDirs.x = -dir.x;
        m_MoveDirs.y = -dir.y;

        g_IsMoving = true;
    }

    void CheckArrive()
    {
        bool moveXDone = false;
        bool moveYDone = false;

        if (m_MoveDirs.x > 0)
        {
            if (m_TextureOffset.x >= m_MoveToPos.x) moveXDone = true;
        }
        else if (m_MoveDirs.x < 0)
        {
            if (m_TextureOffset.x <= m_MoveToPos.x) moveXDone = true;
        }
        else
        {
            moveXDone = true;
        }

        if (m_MoveDirs.y > 0)
        {
            if (m_TextureOffset.y >= m_MoveToPos.y) moveYDone = true;
        }
        else if (m_MoveDirs.y < 0)
        {
            if (m_TextureOffset.y <= m_MoveToPos.y) moveYDone = true;
        }
        else
        {
            moveYDone = true;
        }

        if (moveXDone && moveYDone)
        {
            SetOffset(m_MoveToPos);
            g_IsMoving = false;
            m_MoveDirs = Vector2.zero;
        }
    }

    public void ManualMove(Vector2 delta)
    {
        MoveOffset(Vector2.Scale(-delta, m_PageSize));
    }
    void MoveOffset(Vector2 delta)
    {
        m_TextureOffset += delta;
        if (!m_LoopPage)
        {
            if (m_TextureOffset.x < 0) m_TextureOffset.x = 0;
            if (m_TextureOffset.x > m_MaxRange.x) m_TextureOffset.x = m_MaxRange.x;
            if (m_TextureOffset.y < 0) m_TextureOffset.y = 0;
            if (m_TextureOffset.y > m_MaxRange.y) m_TextureOffset.y = m_MaxRange.y;
        }
        m_Render.material.mainTextureOffset = m_TextureOffset;
    }

    void SetOffset(Vector2 toPos)
    {
        m_TextureOffset = toPos;
        m_Render.material.mainTextureOffset = m_TextureOffset;
    }

    public void SnapToNearest()
    {
        int pageIndexTpX = (int)(m_TextureOffset.x / m_PageSize.x);
        float remainX = m_TextureOffset.x % m_PageSize.x;

        int pageIndexTpY = (int)(m_TextureOffset.y / m_PageSize.y);
        float remainY = m_TextureOffset.y % m_PageSize.y;

        if (remainX * 2f > m_PageSize.x)
        {
            pageIndexTpX++;
        }
        if (remainY * 2f > m_PageSize.y)
        {
            pageIndexTpX++;
        }

        if (!m_LoopPage)
        {
            if (pageIndexTpX < 0) pageIndexTpX = 0;
            if (pageIndexTpX >= m_PageSize.x) pageIndexTpX = m_TotalPages.x - 1;
            if (pageIndexTpY < 0) pageIndexTpY = 0;
            if (pageIndexTpY >= m_PageSize.y) pageIndexTpY = m_TotalPages.y - 1;
        }

        m_PageIndexs.x = pageIndexTpX;
        m_PageIndexs.y = pageIndexTpY;

        m_MoveToPos = Vector2.Scale(m_PageSize, m_PageIndexs);

        if (m_TextureOffset.x > m_MoveToPos.x) m_MoveDirs.x = -1;
        else if (m_TextureOffset.x < m_MoveToPos.x) m_MoveDirs.x = 1;
        else m_MoveDirs.x = 0;

        if (m_TextureOffset.y > m_MoveToPos.y) m_MoveDirs.y = -1;
        else if (m_TextureOffset.y < m_MoveToPos.y) m_MoveDirs.y = 1;
        else m_MoveDirs.y = 0;

        g_IsMoving = true;
    }
}
