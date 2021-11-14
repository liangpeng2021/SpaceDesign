using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using prometheus;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class Demo : MonoBehaviour
{
    public float delayDuration;
    public MeshPlayerPRM meshPlayerPRM;
    public string file;
    public bool isPlaying;
    public int startFrame;
    public Animator animator;
    public ParticleSystem particleSystem;
    public PlayableDirector playableDirector;
    // Start is called before the first frame update
    void Start()
    {
        PlayAni();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            meshPlayerPRM.JumpFrame(118);
        }
    }
    public void PlayAni()
    {
        animator.Play("Take 001");
        OpenSource(() =>
        {
            StartCoroutine(DoGo());
        });
    }

    public IEnumerator DoGo()
    {
        yield return new WaitForSeconds(delayDuration);
        particleSystem.Play();
        yield return new WaitForSeconds(0.2f);
        animator.gameObject.SetActive(false);
        playableDirector.Play();
        meshPlayerPRM.Play();
        isPlaying = true;
    }

    public void OpenSource(System.Action callback)
    {
        meshPlayerPRM.OpenSourceAsync(file, false, startFrame, callback);
    }
}
