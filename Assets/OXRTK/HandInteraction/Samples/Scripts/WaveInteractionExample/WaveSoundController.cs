using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaveSoundController : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_AudioClipList;
    private AudioSource m_AS;
    
    // Start is called before the first frame update
    void Start()
    {
        m_AS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAudio(int index)
    {
        m_AS.PlayOneShot(m_AudioClipList[index]);
    }
}
