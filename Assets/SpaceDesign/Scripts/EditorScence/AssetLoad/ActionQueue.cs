using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 协程管理类，协程太多，统一管理 create by liangpeng 2020-8-21
/// </summary>

public class ActionQueue : MonoBehaviour
{
	//event Action onComplete;
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
	///// <summary>
	///// 添加一个任务到队列
	///// </summary>
	///// <param name="startAction">开始时执行的方法</param>
	///// <param name="IsCompleted">判断该节点是否完成</param>
	///// <returns></returns>
	//public ActionQueue AddAction(Action startAction, Func<bool> IsCompleted)
	//{
	//	actions.Add(new OneAction(startAction, IsCompleted));
	//	return this;
	//}
	/// <summary>
	/// 添加一个协程方法到队列
	/// </summary>
	/// <param name="enumerator">一个协程</param>
	/// <returns></returns>
	public ActionQueue AddAction(IEnumerator enumerator)
	{
        if (!actions.Contains(enumerator))
        {
            actions.Add(enumerator);
            StartCoroutine(enumerator);
        }
        else
        {
            StopCoroutine(enumerator);
            StartCoroutine(enumerator);
            Debug.Log("协程链表已包含");
        } 
		return this;
	}

    //public ActionQueue RemoveAction(IEnumerator enumerator, string name)
    //{
    //    if (actions.Contains(enumerator))
    //    {
    //        Debug.Log(name);
    //        actions.Remove(enumerator);
    //        StopCoroutine(enumerator);
    //        StopCoroutine(startQueueAsync);
    //        isRunning = false;
    //    }

    //    return this;
    //}

    ///// <summary>
    ///// 添加一个任务到队列
    ///// </summary>
    ///// <param name="action">一个方法</param>
    ///// <returns></returns>
    //public ActionQueue AddAction(Action action)
    //{
    //	actions.Add(new OneAction(action));
    //	return this;
    //}

    ///// <summary>
    ///// 绑定执行完毕回调
    ///// </summary>
    ///// <param name="callback"></param>
    ///// <returns></returns>
    //public ActionQueue BindCallback(Action callback)
    //{
    //	onComplete += callback;
    //	return this;
    //}
    /// <summary>
    /// 开始执行队列
    /// </summary>
    /// <returns></returns>
    public void StartQueue()
	{
		//if (!isRunning)
		//{
		//	startQueueAsync = StartQueueAsync();
		//	StartCoroutine(startQueueAsync);
		//}
	}

	IEnumerator startQueueAsync;

	bool isRunning = false;

	IEnumerator StartQueueAsync()
	{
		isRunning = true;
		//if (actions.Count > 0)
		//{
		//	if (actions[0].startAction != null)
		//	{
		//		actions[0].startAction();
		//	}
		//}
		while (actions.Count > 0)
		{
			
			yield return actions[0];
			
			actions.RemoveAt(0);
			//if (actions.Count > 0)
			//{
			//	if (actions[0].startAction != null)
			//	{
			//		actions[0].startAction();
			//	}
			//}
			//else
			//{
			//	break;
			//}
			yield return new WaitForEndOfFrame();
		}
		//if (onComplete != null)
		//{
		//	onComplete();
		//}
		//Destroy(gameObject);
		isRunning = false;
	}

	class OneAction
	{
		public Action startAction;
		public IEnumerator enumerator;
		public OneAction(Action startAction, Func<bool> IsCompleted)
		{
			this.startAction = startAction;
			//如果没用协程，自己创建一个协程
			enumerator = new CustomEnumerator(IsCompleted);
		}

		public OneAction(IEnumerator enumerator, Action action = null)
		{
			this.startAction = action;
			this.enumerator = enumerator;
		}

		public OneAction(Action action)
		{
			this.startAction = action;
			this.enumerator = null;
		}

		/// <summary>
		/// 自定义的协程
		/// </summary>
		class CustomEnumerator : IEnumerator
		{
			public object Current => null;
			Func<bool> IsCompleted;
			public CustomEnumerator(Func<bool> IsCompleted)
			{
				this.IsCompleted = IsCompleted;
			}
			public bool MoveNext()
			{
				return !IsCompleted();
			}

			public void Reset()
			{
			}
		}
	}
}

