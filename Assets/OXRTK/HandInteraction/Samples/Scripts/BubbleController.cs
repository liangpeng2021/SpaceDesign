using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    [SerializeField] GameObject m_Target;
    private ParticleSystem m_Beacon;
    private Animator m_Anim;
    private bool hidden;

    private void OnEnable()
    {
        if (hidden)
            gameObject.SetActive(false);
    } 

    // Start is called before the first frame update
    void Start()
    {
        m_Beacon = GetComponentInChildren<ParticleSystem>();
        m_Anim = GetComponent<Animator>();
    }
    
    public void StopBeacon()
    {
        if (!hidden)
        {
            m_Beacon.Stop();
            hidden = true;
            m_Anim.SetTrigger("TurnOff");
        }
    }

    void OnTriggerExit(Collider col)
    { 
        if (m_Target != null && col.gameObject == m_Target)
        {
            m_Anim.SetTrigger("TurnOff");
        }
    }
}
