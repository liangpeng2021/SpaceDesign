using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class to maintain different visualizations of hand.<br>
    /// 管理手的不同显示类型的类。
    /// </summary>
    public class HandController : MonoBehaviour
    {
        /// <summary>
        /// The hand type to tell if it is left hand or right hand.<br>
        /// 表示手类型是右手或者左手。<br>
        /// </summary>        
        public HandTrackingPlugin.HandType handType;

        /// <summary>
        /// The id of active hand.<br>
        /// 当前激活的手的ID。<br>
        /// </summary>
        public int activeHandId;

        /// <summary>
        /// The active hand.<br>
        /// 当前激活的手。<br>
        /// </summary>
        [HideInInspector]
        public BaseHand activeHand;

        /// <summary>
        /// The collection of all possible active hands.<br>
        /// 所有可激活的手的集合。<br>
        /// </summary>
        /// <remarks>
        /// Only one hand can be activated at the same time.<br>
        /// 同一时刻只有一只手可以被激活。<br>
        /// </remarks>
        public BaseHand[] hands;

        /// <summary>
        /// Notices that active hand is changed.<br>
        /// 通知激活的手发生了改变。<br>
        /// </summary>
        /// <param name="HandController">The controller of changed active hand.<br>
        /// 发生了改变的手对应的控制器。</param>
        public static event Action<HandController> onActiveHandChanged;

        int m_LastActiveHandId;

        void Awake()
        {
            if (hands.Length <= 0)
            {
                Debug.LogError("There is no available hand. OXRTK Hand requires to have at least one available hand !!!");
                return;
            }
            
            activeHandId = activeHandId < 0 ? 0 : activeHandId;
            activeHandId = activeHandId >= hands.Length ? hands.Length - 1 : activeHandId;

            activeHand = hands[activeHandId];
        }

        // Start is called before the first frame update
        void Start()
        {
            m_LastActiveHandId = activeHandId;
            updateHandVisualization(true);
        }

        // Update is called once per frame
        void Update()
        {
            updateHandVisualization();
        }

        void updateHandVisualization(bool isInit = false)
        {
            if (hands.Length <= 0)
            {                
                Debug.LogError("There is no available hand. OXRTK Hand requires to have at least one available hand !!!");
                return;
            }
            activeHandId = activeHandId < 0 ? 0 : activeHandId;
            activeHandId = activeHandId >= hands.Length ? hands.Length - 1 : activeHandId;

            for (int i = 0; i < hands.Length; i++)
            {
                if (i == activeHandId)
                {
                    if (isInit)
                    {                        
                        hands[i].enabled = true;
                        hands[i].gameObject.SetActive(true);
                        activeHand = hands[i];

                        onActiveHandChanged?.Invoke(this);
                    }
                    else
                    {
                        if (!hands[i].enabled)
                        {
                            hands[i].enabled = true;
                            hands[i].gameObject.SetActive(true);
                            activeHand = hands[i];

                            onActiveHandChanged?.Invoke(this);
                        }
                    }                    
                }
                else
                {
                    if (isInit)
                    {
                        hands[i].enabled = false;
                        hands[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        if (hands[i].enabled)
                        {
                            hands[i].enabled = false;
                            hands[i].gameObject.SetActive(false);
                        }
                    }                    
                }
            }
        }
    }
}


