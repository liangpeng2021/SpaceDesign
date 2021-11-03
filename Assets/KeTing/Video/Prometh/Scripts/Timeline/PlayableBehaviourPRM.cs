using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class PlayableBehaviourPRM : PlayableBehaviour
{
    public float firstFrame=0;
    public float lastFrame=0;
    [HideInInspector]
    public MeshTimelinePRM meshTimelinePRM;

    //depends your source
    private float speed;
    private bool isEditorMode;
    private bool RealPause;

    private PlayableDirector director;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable)
    {
        //Debug.Log("OnGraphStart");
    }

    // Called when the owning graph stops playing
    public override void OnGraphStop(Playable playable)
    {
        //Debug.LogError("OnGraphStop");
    }

    // Called when the state of the playable is set to Play
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        //Debug.LogError("OnBehaviourPlay");

        if (RealPause)
        {
            meshTimelinePRM.Resume();
        }
        else {
            speed = CalculateSpeed(playable.GetDuration());
            meshTimelinePRM.SetSpeed(speed);
            meshTimelinePRM.StartPlay((int)firstFrame + (int)(playable.GetTime() * meshTimelinePRM.GetMeshPlayComp().sourceFPS));
        }

        speed = CalculateSpeed(playable.GetDuration());
        meshTimelinePRM.SetSpeed(speed);
        meshTimelinePRM.StartPlay((int)firstFrame+ (int)(playable.GetTime()* meshTimelinePRM.GetMeshPlayComp().sourceFPS));
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
     
        //Debug.LogError("OnBehaviourPause");
        if (meshTimelinePRM)
        {
            if (info.effectivePlayState== PlayState.Playing) {
                RealPause = true;
                meshTimelinePRM.Pause();
            }

            if (info.effectivePlayState == PlayState.Paused)
            {
                RealPause = false;
                meshTimelinePRM.StopPlaying();
            }          
        }
    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info)
    {
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        CheckIsEditorMode(playable);
        if (!meshTimelinePRM)
        {
            meshTimelinePRM = playerData as MeshTimelinePRM;
        }

        if (isEditorMode)
        {
            meshTimelinePRM.SetSpeed(speed);
            meshTimelinePRM.TimelinePreview((firstFrame/15) + playable.GetTime()* speed);
        }
        else {
        }
   
        base.ProcessFrame(playable, info, playerData);
    }

    public void CheckIsEditorMode(Playable playable) {
        if (director == null)
        {
            director = playable.GetGraph().GetResolver() as PlayableDirector;
        }

        if (!Application.isPlaying && director.state == PlayState.Paused)
        {
            isEditorMode = true;
        }
        else
        {
            isEditorMode = false;
        }
    }

    public float CalculateSpeed(double duration) {
        float frames = (float)duration * meshTimelinePRM.GetMeshPlayComp().sourceFPS;
        float speed = 1;

        if (lastFrame == -1)
        {
            return 1;
        }

        if (firstFrame < lastFrame && firstFrame >= 0)
        {
            speed = (lastFrame - firstFrame) / frames;
        }
        return speed;
    }
}
