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
        /// <summary>
        /// The skinned mesh renderer controlled by this component.<br>
        /// 被该组件控制的skinned mesh renderer。
        /// </summary>
        public SkinnedMeshRenderer skinnedMeshRenderer;

        protected override void Init()
        {
            joints = new Transform[21];
            for (int i = 0; i < joints.Length; i++)
            {
                string jointName = "joint" + i;
                joints[i] = findChildRecursively(handGameObject.transform, jointName);
            }
            HandColliderHandle.AddColliderAndRigidbody(joints, m_ColliderType, m_JointCollider, colliderScaleFactor);
            
            handRenderers.Add(skinnedMeshRenderer);
            base.Init();
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

        protected override void UpdateHandTransform()
        {
            if (m_HandInfo.handDetected)
            {
                handGameObject.transform.localPosition = HandTrackingPlugin.instance.GetJointLocalPosition(handType, 0);

                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i].localRotation = HandTrackingPlugin.instance.GetJointLocalRotation(handType, i);
                }
            }
        }
    }
}

