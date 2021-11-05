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
    public PlayableDirector playableDirector;
    [Header("此处的fragmentName必须为唯一名称")]
    public TimelineData[] timelineDatas;
    /// <summary>
    /// 开始播放
    /// </summary>
    bool startPlay;
    /// <summary>
    /// 当前播放的时段
    /// </summary>
    [HideInInspector]
    public TimelineData curTimeData;

    Dictionary<string, TimelineData> timeDataDic = new Dictionary<string, TimelineData>();

    /// <summary>
    /// 是否自动继续
    /// </summary>
    public bool isAutoContinue;
    
    /// <summary>
    /// 结束时候的回调
    /// </summary>
    System.Action endAction;

    public BoxCollider[] boxColliders;
    
    public void StartPause()
    {
		playableDirector.time = 0;
        playableDirector.Pause();

        for (int i = 0; i < timelineDatas.Length; i++)
        {
            timeDataDic.Add(timelineDatas[i].fragmentName, timelineDatas[i]);
        }
    }

    void SetCollidersEnable(bool isEnable)
    {
        //for (int i = 0; i < boxColliders.Length; i++)
        //{
        //    boxColliders[i].enabled = isEnable;
        //}
    }

    // Update is called once per frame
    void Update()
    {
		if (startPlay)
        {
            if (playableDirector.time >= curTimeData.endTime && !isAutoContinue)
            {
                playableDirector.Pause();
                startPlay = false;

                SetCollidersEnable(true);

                endAction?.Invoke();
                endAction = null;
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
        if (timeDataDic.ContainsKey(name))
        {
            startPlay = true;
            curTimeData = timeDataDic[name];

            playableDirector.time = curTimeData.startTime;

            playableDirector.Play();

            SetCollidersEnable(false);
        }
    }

    /// <summary>
    /// 根据时段名称播放,加结束回调
    /// </summary>
    /// <param name="name"></param>
    public void SetCurTimelineData(string name,System.Action action)
    {
        if (timeDataDic.ContainsKey(name))
        {
            startPlay = true;
            curTimeData = timeDataDic[name];

            playableDirector.time = curTimeData.startTime;

            playableDirector.Play();
            endAction = action;
            SetCollidersEnable(false);
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
