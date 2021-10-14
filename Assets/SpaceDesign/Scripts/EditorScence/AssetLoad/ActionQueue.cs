using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 协程管理类，协程太多，统一管理 create by liangpeng 2020-8-21
/// </summary>

public class ActionQueue : MonoBehaviour
{
	List<IEnumerator> actions = new List<IEnumerator>();

	static ActionQueue actionQueue=null;

	public static ActionQueue InitOneActionQueue()
	{
		if (actionQueue == null)
		{
			actionQueue = new GameObject().AddComponent<ActionQueue>();
		}
		return actionQueue;
	}
	
	/// <summary>
	/// 添加一个协程方法到队列
	/// </summary>
	/// <param name="enumerator">一个协程</param>
	/// <returns></returns>
	public ActionQueue AddAction(IEnumerator enumerator)
	{
        actions.Add(enumerator);
        return this;
	}
    
    /// <summary>
    /// 开始执行队列
    /// </summary>
    /// <returns></returns>
    public void StartQueue()
	{
        if (!isRunning)
        {
            startQueueAsync = StartQueueAsync();
            StartCoroutine(startQueueAsync);
        }
    }

    IEnumerator startQueueAsync;

	bool isRunning = false;

	IEnumerator StartQueueAsync()
	{
		isRunning = true;
        
        while (actions.Count > 0)
		{
			yield return actions[0];
			
			actions.RemoveAt(0);
			
			yield return new WaitForEndOfFrame();
		}
		
		isRunning = false;
	}
}

