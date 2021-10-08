using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;

public class MarkTest : MonoBehaviour
{
    public GameObject imageTrack1;
    GameObject obj1;
    GameObject obj2;

    public GameObject imageTTrack2;
    public Transform parent;
    
    public void StartTrack1()
    {
        obj1 = Instantiate(imageTrack1, parent);
        obj1.transform.localPosition = Vector3.zero;
        obj1.transform.localRotation = Quaternion.identity;
        obj1.transform.localScale = Vector3.one;

        Image2DTrackingManager.Instance.TrackStart();
    }
    public void StartTrack2()
    {
        obj2 = Instantiate(imageTTrack2, parent);
        obj2.transform.localPosition = Vector3.zero;
        obj2.transform.localRotation = Quaternion.identity;
        obj2.transform.localScale = Vector3.one;

        Image2DTrackingManager.Instance.TrackStart();
    }
    public void StopTrack1()
    {
        Image2DTrackingManager.Instance.TrackStop();
        Destroy(obj1);
    }
    public void StopTrack2()
    {
        Image2DTrackingManager.Instance.TrackStop();
        Destroy(obj2);
    }
}
