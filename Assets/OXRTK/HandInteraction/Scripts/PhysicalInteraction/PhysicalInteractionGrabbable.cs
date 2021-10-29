using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Attach to object to enable finger tip to grab. <br>添加到物体上，提供和指尖的抓取交互.</br>
    /// </summary>
    public class PhysicalInteractionGrabbable : PhysicalInteractionInteractable
    {
        /// <summary>
        /// Data class for hand that interacts with the objects. Record hand status and filter data.<br>和物体相交的手的数据结构, 用于记录手的状态和滤波数据.</br>
        /// </summary>
        public class FingerTipsStruct
        {
            public PhysicalInteractionFingerTip thumb = null;
            public List<PhysicalInteractionFingerTip> fingers = new List<PhysicalInteractionFingerTip>();
            public int grabFrameCount = 0;
            public int releaseFrameCount = 0;
            public int removeFrameCount = 0;
        }

        /// <summary>
        /// Distance between index/middle/ring finger and thumb, if any distance smaller than this number, and two or more finger are on the object, the object will be grabbed. <br>指尖距离阀值，当母指和(食指/中指/无名指)指尖的距离小于阀值,并且有两个以上的指尖接触物体则判断为抓取.</br>
        /// </summary>
        [Range(0.015f, 0.1f)]
        [SerializeField] float m_FingerDisGrabThreadhold = 0.05f;
        [SerializeField] int m_FilterFrameDelay = 3;

        Transform m_OriginalParent = null;
        Transform m_MoveParent = null;
        PhysicalInteractionHand m_GrabbedHand = null;

        Dictionary<PhysicalInteractionHand, FingerTipsStruct> m_fingerTipHands = new Dictionary<PhysicalInteractionHand, FingerTipsStruct>();

        /// <summary>
        /// When finger tip enter, for PhysicalInteractionFingerTip use only. <br>当指尖进入，仅供PhysicalInteractionFingerTip使用.</br>
        /// </summary>
        /// <param name="hand">The releative PhysicalInteractionHand<br>对应的PhysicalInteractionHand.</br></param>
        /// <param name="fingerTip">The releative PhysicalInteractionFingerTip<br>对应的PhysicalInteractionFingerTip.</br></param>
        public override void OnFingerTipEnter(PhysicalInteractionHand hand, PhysicalInteractionFingerTip fingerTip)
        {
            if (!m_fingerTipHands.ContainsKey(hand))
            {//This hand not exist, create hand in dictionary
                m_fingerTipHands.Add(hand, new FingerTipsStruct());
            }
            //Alread have hand, add data
            if (fingerTip.IsThumb)
                m_fingerTipHands[hand].thumb = fingerTip;
            else
                m_fingerTipHands[hand].fingers.Add(fingerTip);

        }

        /// <summary>
        /// When finger tip exit, for PhysicalInteractionFingerTip use only. <br>当指尖退出，仅供PhysicalInteractionFingerTip使用.</br>
        /// </summary>
        /// <param name="hand">The releative PhysicalInteractionHand<br>对应的PhysicalInteractionHand.</br></param>
        /// <param name="fingerTip">The releative PhysicalInteractionFingerTip<br>对应的PhysicalInteractionFingerTip.</br></param>
        public override void OnFingerTipExit(PhysicalInteractionHand hand, PhysicalInteractionFingerTip fingerTip)
        {
            if (m_fingerTipHands.ContainsKey(hand))
            {//Alread have hand, remove data
                if (fingerTip.IsThumb)
                    m_fingerTipHands[hand].thumb = null;
                else
                {
                    if (m_fingerTipHands[hand].fingers.Contains(fingerTip))//Already have finger, remove
                        m_fingerTipHands[hand].fingers.Remove(fingerTip);
                    else//Finger not exist, error case
                        Debug.LogError("Receive fingertip exit from no exist finger [" + hand.name + "][" + fingerTip.name + "] - ObjName[" + name + "]");
                }
            }
            else
            {// Hand not exist, error case
                Debug.LogError("Receive fingertip exit from no exist hand [" + hand.name + "] - ObjName[" + name + "]");
            }
        }

        /// <summary>
        /// Reset everything when hand end change. <br>当手终端发生重置，相应重置所有可交互物体.</br>
        /// </summary>
        public override void FTHReset()
        {
            m_fingerTipHands.Clear();
            UnGrab();
        }

        void Update()
        {
            if (m_GrabbedHand != null)
            {
                SyncMove();
            }
            if (m_fingerTipHands.Count > 0)
            {
                CheckGrabCase();
            }
        }

        void CheckGrabCase()
        {
            List<PhysicalInteractionHand> toDelete = new List<PhysicalInteractionHand>();
            List<PhysicalInteractionHand> toGrab = new List<PhysicalInteractionHand>();
            foreach (KeyValuePair<PhysicalInteractionHand, FingerTipsStruct> handData in m_fingerTipHands)
            {
                if (handData.Value.thumb == null && handData.Value.fingers.Count == 0)
                {
                    if(m_fingerTipHands[handData.Key].removeFrameCount < m_FilterFrameDelay)
                    m_fingerTipHands[handData.Key].removeFrameCount++;
                    if (m_fingerTipHands[handData.Key].removeFrameCount == m_FilterFrameDelay)
                    {
                        toDelete.Add(handData.Key);
                        continue;
                    }
                }
                else
                {
                    m_fingerTipHands[handData.Key].removeFrameCount = 0;
                }
                if (ShouldGrab(handData.Key, handData.Value))
                {
                    if (m_fingerTipHands[handData.Key].grabFrameCount < m_FilterFrameDelay)
                    {
                        m_fingerTipHands[handData.Key].grabFrameCount++;
                        m_fingerTipHands[handData.Key].releaseFrameCount = 0;
                    }
                    if (m_fingerTipHands[handData.Key].grabFrameCount == m_FilterFrameDelay)
                        toGrab.Add(handData.Key);
                }
                else
                {
                    m_fingerTipHands[handData.Key].grabFrameCount = 0;
                    m_fingerTipHands[handData.Key].releaseFrameCount++;
                }
            }

            for (int i = 0; i < toDelete.Count; i++)
            {
                RemoveHand(toDelete[i]);
            }

            if (m_GrabbedHand != null)
            {//Already grabbed, try should release
                if (m_fingerTipHands[m_GrabbedHand].releaseFrameCount > m_FilterFrameDelay)
                {
                    UnGrab();
                }
            }

            if (m_GrabbedHand == null)
            {//Nothing grabbed, try should grab
                for (int i = 0; i < toGrab.Count; i++)
                {
                    if (m_fingerTipHands.ContainsKey(toGrab[i]))
                    {
                        Grab(toGrab[i]);
                        break;
                    }
                }
            }
        }

        bool ShouldGrab(PhysicalInteractionHand hand, FingerTipsStruct handData)
        {
            if(m_GrabbedHand == hand)
            {
                if (hand.IsFist())
                    return true;
            }
            if (handData.thumb == null || handData.fingers.Count == 0)
                return false;
            for (int i = 0; i < handData.fingers.Count; i++)
            {
                if (IsTwoFingerDisLessThanThreadhold(handData.fingers[i].transform, handData.thumb.transform))
                {
                    return true;
                }
            }
            return false;
        }

        void Grab(PhysicalInteractionHand hand)
        {
            if (HandTrackingPlugin.debugLevel > 0) Debug.Log(hand.name + " - Grab start");
            if (hand.TryGrabMe(transform))
            {
                m_GrabbedHand = hand;
                if (m_OriginalParent == null && transform.parent != null)
                    m_OriginalParent = transform.parent;
                if (m_MoveParent == null)
                    m_MoveParent = new GameObject(name + "-MoveRoot").transform;
                m_MoveParent.localScale = Vector3.one;
                SyncMove();
                transform.SetParent(m_MoveParent);
                if (HandTrackingPlugin.debugLevel > 0) Debug.Log(hand.name + " - Grab obj - " + name);
            }
            else
            {
                if (HandTrackingPlugin.debugLevel > 0) Debug.Log(hand.name + " - Grab fail");
            }
        }

        void SyncMove()
        {
            if (m_MoveParent)
                m_GrabbedHand.SyncMove(m_MoveParent);
        }

        void UnGrab()
        {
            transform.SetParent(m_OriginalParent);
            m_GrabbedHand.ReleaseMe(transform);
            m_GrabbedHand = null;
        }

        void RemoveHand(PhysicalInteractionHand hand)
        {
            if (m_GrabbedHand == hand)
            {
                UnGrab();
            }
            m_fingerTipHands.Remove(hand);
        }

        bool IsTwoFingerDisLessThanThreadhold(Transform fingerA, Transform fingerB)
        {
            return Vector3.Distance(fingerA.position, fingerB.position) < m_FingerDisGrabThreadhold;
        }
    }
}
