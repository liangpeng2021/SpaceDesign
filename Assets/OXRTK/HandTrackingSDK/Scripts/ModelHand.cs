using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for model hand.<br>
    /// 用于模型手的类。
    /// </summary>
    public class ModelHand : BaseHand
    {
        protected override void Init()
        {                                                
            joints = new Transform[21];
            for (int i = 0; i < joints.Length; i++)
            {
                string jointName = "joint" + i;                
                joints[i] = findChildRecursively(handGameObject.transform, jointName);
            }
            HandColliderHandle.AddColliderAndRigidbody(joints,m_ColliderType, m_JointCollider, colliderScaleFactor);
        }

        Transform findChildRecursively(Transform parent, string target)
        {
            Transform t = parent.Find(target);
            if (t != null)
            {
                return t;
            }

            if (parent.childCount != 0)
            {
                foreach (Transform child in parent.GetComponentInChildren<Transform>())
                {
                    t = findChildRecursively(child, target);
                    if (t != null)
                    {
                        return t;
                    }
                }
            }
            return t;
        }

        protected override void UpdateHand()
        {
            UpdateHandData();
            UpdateModelHand();
        }

        void UpdateHandData()
        {            
            switch (handType)
            {
                default:
                case HandTrackingPlugin.HandType.RightHand:                    
                    m_HandInfo = HandTrackingPlugin.instance.rightHandInfo;
                    break;
                case HandTrackingPlugin.HandType.LeftHand:                    
                    m_HandInfo = HandTrackingPlugin.instance.leftHandInfo;
                    break;
            }
        }

        void UpdateModelHand()
        {
            if (!m_HandInfo.handDetected)
            {
                handGameObject.SetActive(false);
            }
            else
            {                
                handGameObject.transform.localPosition = HandTrackingPlugin.instance.GetJointLocalPosition(handType, 0);

                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i].localRotation = HandTrackingPlugin.instance.GetJointLocalRotation(handType, i);
                }
                
                handGameObject.SetActive(true);
            }
        }
    }
}

