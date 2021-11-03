using prometheus;
using UnityEngine;
using UnityEngine.Video;

[ExecuteInEditMode]
public class MeshTimelinePRM : MonoBehaviour
{
    public float speed;
    public int  currentFrame;
    private MeshPlayerPRM meshPlayerPRM;
    private VideoPlayer videoPlayer;
    public MeshFilter meshFilter;
    public bool hideModelInPause;
    [HideInInspector]
    public bool isPause;
    public bool hadInit;
    private void Awake()
    {

    }

    void Start()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
        if (hideModelInPause) {
         
            meshFilter.mesh = null;
        }

        if (!Application.isPlaying)
        {
            GetPlayerComp();
            hadInit = false;
        }
    }

    [ExecuteInEditMode]
    public void Update()
    {

    }

    public void Pause() {
        meshPlayerPRM.Pause();
    }
    public void Resume() {
        meshPlayerPRM.Play();
    }

    public void StartPlay(int frame) {
        GetMeshPlayComp();
        if (Application.isPlaying)
        {
            currentFrame = frame;

            if (meshPlayerPRM.isOpened)
            {
                if (!meshPlayerPRM.isPlaying) {
                    meshPlayerPRM.JumpFrame(frame);
                }             
            }
            else
            {
                Debug.LogWarning("OpenSource");
                meshPlayerPRM.OpenSource(meshPlayerPRM.sourceUrl, frame, true);
            }
        }
        else {

        }  
    }

    public void SetSpeed(float speed) {
        this.speed = speed;
        GetMeshPlayComp();
        meshPlayerPRM.SetSpeed(speed);
    }

    public void TimelinePreview(double timeLineTime)
    {
        currentFrame = (int)(timeLineTime * meshPlayerPRM.sourceFPS);
        PreviewFrame(currentFrame);
    }

    public void PreviewFrame(int frm)
    {
        if (!Application.isPlaying)
        {
            GetPlayerComp();

            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }

            if (!hadInit)
            {
                meshPlayerPRM.OpenSource(meshPlayerPRM.sourceUrl, 0, false);
                hadInit = true;
            }

            videoPlayer.Play();
            videoPlayer.Pause();
            meshPlayerPRM.PreparePreviewFrame(frm);
        }
    }

    public void GetPlayerComp()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (meshPlayerPRM == null)
        {
            meshPlayerPRM = GetComponent<MeshPlayerPRM>();
        }
    }

    public void ResetTime() {
        currentFrame = 0;
    }

    public void StopPlaying() {
        if (!Application.isPlaying)
        {
            meshFilter.mesh = null;
            return;
        }

        if (hideModelInPause)
        {
            meshFilter.mesh = null;
        }

        ResetTime();
        GetMeshPlayComp();

        if (meshPlayerPRM.isPlaying) {
            meshPlayerPRM.Pause();
        }     
    }
    public MeshPlayerPRM GetMeshPlayComp()
    {
        if (meshPlayerPRM == null)
        {
            meshPlayerPRM = GetComponent<MeshPlayerPRM>();
        }
        return meshPlayerPRM;
    }

    public void OnDisable()
    {
        if (!Application.isPlaying)
        {
            meshPlayerPRM.SwitchFrameReadyType(-1);
            if (meshPlayerPRM.isInitialized == true)
            {
                meshPlayerPRM.SwitchFrameReadyType(-1);
                meshPlayerPRM.Stop();
                meshPlayerPRM.Uninitialize();
            }
        }
    }
}
