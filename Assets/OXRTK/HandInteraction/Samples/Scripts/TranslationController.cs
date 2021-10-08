using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationController : MonoBehaviour
{
    float m_Position = 0.5f, m_RotateSpeed = 0.5f, m_Size = 0.5f;
    Vector3 m_originalLocalSize;
    public void SetPosition(float xSpeed)
    {
        m_Position = xSpeed;
    }
    
    public void SetYRotation(float ySpeed)
    {
        m_RotateSpeed = ySpeed;
    }

    public void SetSize(float zSpeed)
    {
        m_Size = zSpeed;
    }
    // Start is called before the first frame update
    void Start()
    {
        m_originalLocalSize = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, (m_RotateSpeed - 0.5f) * Time.deltaTime * 200, 0);
        transform.localScale = m_originalLocalSize * (1 + (m_Size - 0.5f)* 0.5f); 
    }
}
