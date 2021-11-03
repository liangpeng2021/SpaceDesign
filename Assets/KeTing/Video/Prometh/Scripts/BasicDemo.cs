using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using prometheus;

public class BasicDemo : MonoBehaviour
{
    public MeshPlayerPRM meshPlayerPRM;
    public VideoPlayer videoPlayer;
    public Slider slider;
    public int currentFrame;
    public bool frameChanged;
    public bool isRealPause;
    public bool isSelect;
    private int maxFrame;
    public Text frameText;

    void Start()
    {
        slider.onValueChanged.AddListener((value) => {
            if (isSelect) {
                frameChanged = true;
                if (videoPlayer.isPlaying)
                {
                    Pause();
                }
                currentFrame = (int)value;
                meshPlayerPRM.PreparePreviewFrame(currentFrame);
            }      
        });
    }

    public void OpenAndPlay(string str) {
        meshPlayerPRM.sourceUrl = str;
        meshPlayerPRM.PrepareVideo(() =>
        {
            meshPlayerPRM.OpenSource(meshPlayerPRM.sourceUrl, meshPlayerPRM.awakeStartFrame, true);
            //meshPlayerPRM.OpenSourceAsync(meshPlayerPRM.sourceUrl, meshPlayerPRM.awakeStartFrame, true);
        });
    }

    public void OnReleaseSlider() {
        isSelect = false;
        if (isRealPause)
        {

        }
        else {
            Debug.Log("OnReleaseSlider() + " + currentFrame);
            Play();
        }
    }

    public void OnClickSlider()
    {
        isSelect = true;
        frameChanged = true;
        if (videoPlayer.isPlaying)
        {
            Pause();
        }
        currentFrame = (int)slider.value;
        Debug.Log("OnClickSlider:" + (int)slider.value);
        meshPlayerPRM.PreparePreviewFrame(currentFrame);
    }

    void Update()
    {
        if (maxFrame==0) {
            maxFrame = meshPlayerPRM.sourceFrameCount;
            slider.maxValue = maxFrame-1;
        }
        if (videoPlayer.isPlaying&& !isSelect) {
            
            slider.value = videoPlayer.frame;
            currentFrame = (int)videoPlayer.frame;
        }
       
        frameText.text = $"Frame:{currentFrame}";
    }

    private void FixedUpdate()
    {
    }

    public void JumpFrame(int frame)
    {
        meshPlayerPRM.JumpFrame(frame);     
    }

    public void Play()
    {
        if (frameChanged)
        {
            Debug.Log("Play frameChanged currentFrame=" + currentFrame);
            meshPlayerPRM.OpenSource(meshPlayerPRM.sourceUrl, currentFrame, true);
        }
        else {

            meshPlayerPRM.Play();
        }
        isRealPause = false;
        frameChanged = false;
    }

    public void OnPause() {
        isRealPause = true;
        Pause();
    }

    public void Pause()
    {
        meshPlayerPRM.Pause();
        currentFrame = (int)videoPlayer.frame;
    }

    public void LastFrame()
    {
        if (videoPlayer.isPlaying)
        {
            OnPause();
        }
        var last = currentFrame - 1;
        
        if (last >= 0)
        {
            frameChanged = true;
            currentFrame = last;
        }
        else {
            frameChanged = true;
            currentFrame = (int)(videoPlayer.frameCount - 1);
        }

        meshPlayerPRM.PreparePreviewFrame(currentFrame);
        slider.value = currentFrame;
    }

    public void NextFrame()
    {
        if (videoPlayer.isPlaying)
        {
            OnPause();
        }

        var next = currentFrame + 1;
        if (next <= (int)(videoPlayer.frameCount - 1))
        {
            frameChanged = true;
            currentFrame = next;
        }
        else {
            frameChanged = true;
            currentFrame = 0;
        }

        slider.value = currentFrame;
        meshPlayerPRM.PreparePreviewFrame(currentFrame);
    }

    private void OnApplicationFocus(bool focus)
    {
        //if (focus == false)
        //{
        //    Pause();
        //}
        //else
        //{
        //    Play();
        //}
    }
}
