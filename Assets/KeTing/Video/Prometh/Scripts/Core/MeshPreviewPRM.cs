using UnityEngine.Video;
using UnityEngine;
using prometheus;

[ExecuteInEditMode]
public class MeshPreviewPRM : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private MeshPlayerPRM meshPlayerPRM;
    public int sourceFrameCount;
    public int previewFrame;
    public bool hadInit;

    void Start()
    {
        if (!Application.isPlaying)
        {      
            GetPlayerComp();
            hadInit = false;
            if (!hadInit)
            {
                meshPlayerPRM.Preview(0);
                sourceFrameCount = meshPlayerPRM.sourceFrameCount;
                hadInit = true;
            }
        }
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
                meshPlayerPRM.Preview(0);
                hadInit = true;
            }

            videoPlayer.Play();
            videoPlayer.Pause();
            previewFrame = frm;
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

    public void OnDisable()
    {
        if (!Application.isPlaying&&(videoPlayer!=null && meshPlayerPRM!=null)) {
            if (meshPlayerPRM.isInitialized == true)
            {
                meshPlayerPRM.SwitchFrameReadyType(-1);
                meshPlayerPRM.Stop();
                meshPlayerPRM.Uninitialize();
            }
        }   
    }

    public void OnDestroy()
    {
        hadInit = false;
    }
}
