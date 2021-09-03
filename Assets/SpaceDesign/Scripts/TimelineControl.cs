using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
/// <summary>
/// 控制播放timeline不同时段动画，/*create by 梁鹏 2021-8-6 */
/// </summary>
[System.Serializable]
public struct TimelineData
{
    /// <summary>
    /// 片段名称
    /// </summary>
    public string fragmentName;
    /// <summary>
    /// 起始时间
    /// </summary>
    public float startTime;
    /// <summary>
    /// 结束时间
    /// </summary>
    public float endTime;
}
public class TimelineControl : MonoBehaviour
{
    public static TimelineControl Instance;
    public PlayableDirector playableDirector;
    
    public TimelineData[] timelineDatas;
    /// <summary>
    /// 开始播放
    /// </summary>
    bool startPlay;
    [HideInInspector]
    public TimelineData curTimeData;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 是否自动继续
    /// </summary>
    public bool isAutoContinue;
    
    public void StartPause()
    {
		playableDirector.time = 0;
        playableDirector.Pause();
    }

    // Update is called once per frame
    void Update()
    {
		if (startPlay)
        {
            if (playableDirector.time > curTimeData.endTime && !isAutoContinue)
            {
                playableDirector.Pause();
                startPlay = false;
            }
        }
    }
    /// <summary>
    /// 根据时段数据播放
    /// </summary>
    /// <param name="timelineData"></param>
    public void SetCurTimelineData(TimelineData timelineData)
    {
        startPlay = true;
        curTimeData = timelineData;
		playableDirector.time = curTimeData.startTime;
        playableDirector.Play();
    }
    /// <summary>
    /// 根据时段名称播放
    /// </summary>
    /// <param name="name"></param>
    public void SetCurTimelineData(string name)
    {
        for (int i = 0; i < timelineDatas.Length; i++)
        {
            if (name.Equals(timelineDatas[i].fragmentName))
            {
                startPlay = true;
                curTimeData = timelineDatas[i];

                playableDirector.time = curTimeData.startTime;

				playableDirector.Play();

                break;
            }
        }
    }
    /// <summary>
    /// 自动播放
    /// </summary>
    public void AutoPlay(TimelineData timelineData)
    {
        startPlay = true;
        curTimeData = timelineData;
        playableDirector.time = curTimeData.startTime;
        playableDirector.Play();
        isAutoContinue = true;
    }
}
