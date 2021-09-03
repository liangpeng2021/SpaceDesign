using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for skeleton hand.<br>
    /// 用于骨骼手的类。
    /// </summary>
    public class SkeletonHand : BaseHand
    {
        /// <summary>
        /// The scale factor applied to skeleton. This will keep wrist at algorithm position and scale other parts.<br>
        /// 用于骨骼的缩放参数。这会将手腕保持在算法位置并缩放其他部分。
        /// </summary>
        public float scaleFactor = 1;

        /// <summary>
        /// The prefab used for joint.<br>
        /// 用于节点的prefab。
        /// </summary>
        public GameObject jointPrefab;

        /// <summary>
        /// The prefab used for bone connecting joints.<br>
        /// 用于连接节点的骨头的prefab。
        /// </summary>
        public GameObject bonePrefab;

        /// <summary>
        /// The 20 bones in a skeleton hand.<br>
        /// 骨骼手中的所有20根骨头。
        /// </summary>
        Transform[] m_Bones;

        protected override void UpdateHand()
        {
            UpdateHandData();
            UpdateSkeletonHand();
        }

        protected override void Init()
        {
            handGameObject.transform.SetParent(transform, false);
            handGameObject.transform.localRotation = Quaternion.identity;
            
            joints = new Transform[21];
            m_Bones = new Transform[20];

            for (int i = 0; i < joints.Length; i++)
            {
                joints[i] = Instantiate(jointPrefab).transform;
                joints[i].name = "Joint_" + i;
                joints[i].transform.SetParent(handGameObject.transform);                
            }
            for (int i = 0; i < m_Bones.Length; i++)
            {
                m_Bones[i] = Instantiate(bonePrefab).transform;
                m_Bones[i].name = "Bone_" + i;
                m_Bones[i].transform.SetParent(handGameObject.transform);                
            }

            joints[1].SetParent(joints[0]);
            joints[5].SetParent(joints[0]);
            joints[9].SetParent(joints[0]);
            joints[13].SetParent(joints[0]);
            joints[17].SetParent(joints[0]);

            joints[2].SetParent(joints[1]);
            joints[3].SetParent(joints[2]);
            joints[4].SetParent(joints[3]);

            joints[6].SetParent(joints[5]);
            joints[7].SetParent(joints[6]);
            joints[8].SetParent(joints[7]);

            joints[10].SetParent(joints[9]);
            joints[11].SetParent(joints[10]);
            joints[12].SetParent(joints[11]);

            joints[14].SetParent(joints[13]);
            joints[15].SetParent(joints[14]);
            joints[16].SetParent(joints[15]);

            joints[18].SetParent(joints[17]);
            joints[19].SetParent(joints[18]);
            joints[20].SetParent(joints[19]);

            HandColliderHandle.AddColliderAndRigidbody(joints, m_ColliderType, m_JointCollider);
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

        void UpdateSkeletonHand()
        {
            if (!m_HandInfo.handDetected)
            {
                handGameObject.SetActive(false);
            }
            else
            {                
                for (int i = 0; i < joints.Length; i++)
                {
                    if (i == 0)
                    {                        
                        joints[i].transform.position = HandTrackingPlugin.instance.GetJointWorldPosition(handType, i);
                    }
                    else
                    {                             
                        joints[i].transform.position = joints[0].transform.position +
                            (HandTrackingPlugin.instance.GetJointWorldPosition(handType, i) - joints[0].transform.position) * scaleFactor;
                    }
                    
                    joints[i].transform.localRotation = HandTrackingPlugin.instance.GetJointLocalRotation(handType, i);
                }

                for (int i = 0; i < m_Bones.Length; i++)
                {
                    Vector3 BoneStart;
                    if (i % 4 == 0)
                    {
                        BoneStart = joints[0].transform.position;                        
                    }
                    else
                    {
                        BoneStart = joints[i].transform.position;                        
                    }
                    Vector3 BoneEnd = joints[i + 1].transform.position;
                    m_Bones[i].transform.position = (BoneStart + BoneEnd) / 2;
                    float distanceBetweenTwoJoints = Vector3.Distance(BoneStart, BoneEnd);
                    m_Bones[i].transform.localScale = new Vector3(bonePrefab.transform.localScale.x,
                        distanceBetweenTwoJoints * 0.5f, bonePrefab.transform.localScale.z);
                    m_Bones[i].transform.rotation = Quaternion.LookRotation((BoneEnd - BoneStart).normalized) * Quaternion.Euler(90, 0, 0);
                }

                handGameObject.SetActive(true);
            }
        }
    }
}
