using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public MeshPlayerPRM meshPlayerPRM;
    // Update is called once per frame
    string s = "Video3D.mp4";
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            meshPlayerPRM.autoPlay = false;
            meshPlayerPRM.OpenSource(s);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            meshPlayerPRM.autoPlay = true;
            meshPlayerPRM.OpenSource(s);

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.JumpFrame(0, false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.JumpFrame(0, true);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.JumpFrame(1000, false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.JumpFrame(1000, true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.Pause();
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.Stop();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            meshPlayerPRM.OpenSource(s);
            meshPlayerPRM.JumpFrame(0, true);
            meshPlayerPRM.Pause();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            meshPlayerPRM.Play();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            meshPlayerPRM.Pause();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            meshPlayerPRM.Stop();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            meshPlayerPRM.frameIndex = 0;
        }
    }
}
