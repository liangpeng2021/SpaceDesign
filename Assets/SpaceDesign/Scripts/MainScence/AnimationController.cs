using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 动效控制，/*create by 梁鹏 2021-8-31 */
/// </summary>
[System.Serializable]
public struct ButtonRayData
{
    public ButtonRayReceiver buttonRayReceiver;
    public TimelineData timelineData;
}
public class AnimationController : MonoBehaviour
{
    public ButtonRayData[] buttonRayData;
    // Start is called before the first frame update
    void Start()
    {
        TimelineControl.Instance.StartPause();
        for (int i = 0; i < buttonRayData.Length; i++)
        {
            if (buttonRayData[i].buttonRayReceiver)
            {
                TimelineData timelineData = buttonRayData[i].timelineData;
                buttonRayData[i].buttonRayReceiver.onPinchDown.AddListener(() => { PlayTimeLineFragement(timelineData); });
            }
        }
    }
    /// <summary>
    /// 播放timeLine片段
    /// </summary>
    void PlayTimeLineFragement(TimelineData timelineData)
    {
        TimelineControl.Instance.SetCurTimelineData(timelineData);
        TimelineControl.Instance.isAutoContinue = false;
    }
    //public 
}
