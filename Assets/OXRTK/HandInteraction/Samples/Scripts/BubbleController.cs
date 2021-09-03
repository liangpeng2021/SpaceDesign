using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    [SerializeField] GameObject m_Target;
    private ParticleSystem m_Beacon;
    private Animator m_Anim;
    
    // Start is called before the first frame update
    void Start()
    {
        m_Beacon = GetComponentInChildren<ParticleSystem>();
        m_Anim = GetComponent<Animator>();
    }
    
    public void StopBeacon()
    {
        m_Beacon.Stop();
    }

    void OnTriggerExit(Collider col)
    { 
        if (m_Target != null && col.gameObject == m_Target)
        {
            m_Anim.SetTrigger("TurnOff");
        }
    }
}
