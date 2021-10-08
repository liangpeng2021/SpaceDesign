using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Module to handle collider touch wave interaction.<br>碰撞接触挥动交互组件.</br>
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class WaveInteractionTouchHandle : WaveInteraction
    {
        Dictionary<BaseHand, List<GameObject>> m_Hands = new Dictionary<BaseHand, List<GameObject>>();

        protected override void Start()
        {
            base.Start();
            m_InteractiveAreaSize = m_Collider.size;
            //m_MinMoveDisBeforeWave = m_Collider.size * m_WaveMinMoveDisPer;
        }

        // Update is called once per frame
        void Update()
        {
            if (!g_IsPause)
            {
                if (m_ActiveHand != null)
                {
                    if (m_ActiveHand.handGameObject.activeInHierarchy)
                    {
                        WaveMainLogic();
                    }
                    else
                    {
                        RemoveHand(m_ActiveHand);
                    }
                }
            }
        }

        //手节点进入
        void OnTriggerEnter(Collider other)
        {
            BaseHand hand = other.gameObject.GetComponentInParent<BaseHand>();
            if (hand != null)
            {
                AddNode(hand, other.gameObject);
            }
        }

        //记录手节点到字典
        void AddNode(BaseHand hand, GameObject other)
        {
            if (!m_Hands.ContainsKey(hand))
            {
                AddNewHand(hand);
            }
            if (!m_Hands[hand].Contains(other))
                m_Hands[hand].Add(other.gameObject);
            if (HandTrackingPlugin.debugLevel > 1) Debug.LogWarning("[WIT-AddNode] - " + hand.GetInstanceID() + "_" + other.name);
        }

        //字典创建新手
        void AddNewHand(BaseHand hand)
        {
            m_Hands.Add(hand, new List<GameObject>());
            if (HandTrackingPlugin.debugLevel > 1) Debug.LogWarning("[WIT-AddNewHand] - " + hand.GetInstanceID());
            if (m_ActiveHand == null) TryGetNextActiveHand();
        }

        //手节点退出
        void OnTriggerExit(Collider other)
        {
            BaseHand hand = other.gameObject.GetComponentInParent<BaseHand>();
            if (hand != null)
            {
                RemoveNode(hand, other.gameObject);
            }
        }

        //移除手节点
        void RemoveNode(BaseHand hand, GameObject other)
        {
            if (m_Hands.ContainsKey(hand))
            {
                if (m_Hands[hand].Contains(other.gameObject))
                {
                    m_Hands[hand].Remove(other.gameObject);
                    if (HandTrackingPlugin.debugLevel > 1) Debug.LogWarning("[WIT-RemoveNode] - " + hand.GetInstanceID() + "_" + other.name);
                }
                else
                {
                    Debug.LogError("WIT-RemoveNode Not exist node - " + hand.GetInstanceID() + ":" + other.name);
                }
                if (m_Hands[hand].Count == 0)
                {
                    RemoveHand(hand);
                }
            }
            else
            {
                Debug.LogError("WIT-RemoveNode Not exist hand - " + hand.GetInstanceID());
            }
        }

        //字典中移除手
        void RemoveHand(BaseHand hand)
        {
            m_Hands.Remove(hand);
            if (m_ActiveHand == hand)
            {
                m_ActiveHand = null;
                g_OnHandExit?.Invoke();
                ResetWave();
                if (HandTrackingPlugin.debugLevel > 0) Debug.LogWarning("[WIT-RemoveHand] - Remove current hand, TryGetNextActiveHand");
                TryGetNextActiveHand();
            }
            else
            {
                if (m_ActiveHand != null)
                {
                    if (HandTrackingPlugin.debugLevel > 0) Debug.LogWarning("[WIT-RemoveHand] - Remain m_ActiveHand = " + m_ActiveHand.GetInstanceID() + "  RemovedHand = " + hand.GetInstanceID());
                }
                else
                {
                    if (HandTrackingPlugin.debugLevel > 0) Debug.LogWarning("[WIT-RemoveHand] - No Active Hand.");
                }
            }
        }

        //尝试获取下一个可用的hand
        void TryGetNextActiveHand()
        {
            if (m_Hands.Count > 0)
            {
                foreach (BaseHand k in m_Hands.Keys)
                {
                    m_ActiveHand = k;
                    InitHandPos();
                    g_OnHandEnter?.Invoke((int)m_ActiveHand.handType);
                    if (HandTrackingPlugin.debugLevel > 0) Debug.LogWarning("[WIT-TryGetNextActiveHand] - Got:" + m_ActiveHand.GetInstanceID());
                    return;
                }
            }
            //After set active hand, record and cal pos for enable left or right
        }
    }
}