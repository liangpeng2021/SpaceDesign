using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAppearance : MonoBehaviour
{
    [SerializeField] Renderer m_Render;
    [SerializeField] Transform m_RelativeSpace;
    [SerializeField] Material m_MatUnPressed;
    [SerializeField] Material m_MatPressed;
    [SerializeField] Texture2D m_LeftPressed;
    [SerializeField] Texture2D m_LeftRelease;
    [SerializeField] Texture2D m_RigthPressed;
    [SerializeField] Texture2D m_RigthRelease;

    Vector3 defaultScale = new Vector3(0.1f, 0.1f);
    Vector3 currScale = new Vector3(0.05f, 0.05f);
    Vector2 rangeLimit = new Vector2(0.5f, 0.5f);
    Vector3 currPos = new Vector3(0, 0, -0.001f);

    void Start()
    {
        rangeLimit.x = m_RelativeSpace.localScale.x * 0.5f;
        rangeLimit.y = m_RelativeSpace.localScale.y * 0.5f;
    }
    public void UpdateHandZ(float zPos)
    {
        if (zPos > 1) zPos = 1;
        if (zPos < 0.5f) zPos = 0.5f;

        zPos = 1.5f - zPos;

        currScale = defaultScale * zPos;
        currScale.z = 1f;
        transform.localScale = currScale;
    }

    public void UpdateHandPos(Vector2 pos)
    {

        currPos.x = Mathf.Clamp(pos.x * m_RelativeSpace.localScale.x, -rangeLimit.x, rangeLimit.x);
        currPos.y = Mathf.Clamp(pos.y * m_RelativeSpace.localScale.y, -rangeLimit.y, rangeLimit.y);
        transform.localPosition = currPos;
    }
    public void UpdatePress(bool isPress)
    {
        m_Render.material = isPress ? m_MatPressed : m_MatUnPressed;
    }
    public void UpdateLeftRightHand(int handIndex)
    {//0-right  1-left
        m_MatUnPressed.mainTexture = handIndex == 0 ? m_RigthRelease : m_LeftRelease;
        m_MatPressed.mainTexture = handIndex == 0 ? m_RigthPressed : m_LeftPressed;
    }
}
