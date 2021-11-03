using UnityEngine;
using prometheus;
using UnityEngine.Playables;

public class TimelineDemo : MonoBehaviour
{
    public MeshPlayerPRM playerPRM;
    public PlayableDirector playableDirector;

    // Start is called before the first frame update
    void Start()
    {
        StartDemo();
        playableDirector.paused += Paused;
    }

    public void Paused(PlayableDirector playableDirector) {
        //Todo
    }

    public void StartDemo() {
        playerPRM.PrepareVideo(() =>
        {
            playerPRM.OpenSource(playerPRM.sourceUrl,0,false, successCallback:()=> {
                playableDirector.Play();
            });
        });
    }

    public void OnClickPause() {
        playableDirector.Pause();
    }

    public void OnClickPlay()
    {
        playableDirector.Play();
    }
}
