using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// This is attaches to each finger tip joint by PhysicalInteractionHand for physical interaction. <br>由PhysicalInteractionHand添加到每个需要的指尖节点用于交互.</br>
    /// </summary>
    public class PhysicalInteractionFingerTip : MonoBehaviour
    {
        PhysicalInteractionHand m_FingerTipControl;
        FingerTipJointID m_FingerID;
        bool m_IsEnabled = true;
        bool m_HadCollider = false;
        bool m_WasColliderEnabled = false;
        bool m_WasTrigger = false;
        bool m_HadRigid = false;
        float m_ColliderScaleFactor = 1f;
        SphereCollider m_collider;
        Rigidbody m_Rigidbody;

        List<PhysicalInteractionInteractable> m_CollidedObjs = new List<PhysicalInteractionInteractable>();

        /// <summary>
        /// Enable/Disable this finger tip. <br>开启/禁用这个指尖.</br>
        /// </summary>
        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set { ToggleOnOff(value); }
        }

        /// <summary>
        /// Initialize, for PhysicalInteractionHand to use only.  <br>初始化指尖,仅供PhysicalInteractionHand使用.</br>
        /// </summary>
        public void Init(PhysicalInteractionHand fingerTipControl, FingerTipJointID fingerID, float colliderScaleFactor)
        {
            m_ColliderScaleFactor = colliderScaleFactor;

            m_FingerTipControl = fingerTipControl;
            m_FingerID = fingerID;

            //Get or Create SphereCollider
            m_collider = GetComponent<SphereCollider>();
            if (m_collider == null)
                AddCollider();
            else
            {
                m_HadCollider = true;
                m_WasColliderEnabled = m_collider.enabled;
                m_WasTrigger = m_collider.isTrigger;
            }

            //Get or Create Rigidbody
            m_Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (m_Rigidbody == null)
                AddRigidbody();
            else
                m_HadRigid = true;
        }
        void AddCollider()
        {
            m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.radius *= m_ColliderScaleFactor;
        }
        void AddRigidbody()
        {
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        void ToggleOnOff(bool isEnableTp)
        {
            if (m_IsEnabled != isEnableTp)
            {
                m_IsEnabled = isEnableTp;
                if (m_IsEnabled)
                    EnableFingerTip();
                else
                    DisableFingerTip();
            }
        }
        void EnableFingerTip()
        {
            if (!m_HadCollider)
            {
                AddCollider();
            }
            else
            {
                m_collider.enabled = true;
                m_collider.isTrigger = false;
            }
            if (!m_HadRigid)
            {
                AddRigidbody();
            }
        }
        void DisableFingerTip()
        {
            if (!m_HadCollider)
            {
                Destroy(m_collider);
            }
            else
            {
                m_collider.enabled = m_WasColliderEnabled;
                m_collider.isTrigger = m_WasTrigger;
            }
            if (!m_HadRigid)
            {
                Destroy(m_Rigidbody);
            }
        }

        /// <summary>
        /// When finger disable, send exit data to all obj
        /// </summary>
        void OnDisable()
        {
            m_CollidedObjs.RemoveAll(x => x == null);
            for (int i = 0; i < m_CollidedObjs.Count; i++)
            {
                m_CollidedObjs[i].OnFingerTipExit(m_FingerTipControl, this);
            }
            m_CollidedObjs.Clear();
        }

        /// <summary>
        /// When finger collide with obj, send collide data to obj 
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            if (m_IsEnabled)
            {
                PhysicalInteractionInteractable interactbleTp = other.GetComponent<PhysicalInteractionInteractable>();
                if (interactbleTp != null)
                {
                    interactbleTp.OnFingerTipEnter(m_FingerTipControl, this);
                    m_CollidedObjs.Add(interactbleTp);
                }
            }
        }

        /// <summary>
        /// When finger exit collide with obj, send exit data to obj
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerExit(Collider other)
        {
            if (m_IsEnabled)
            {
                PhysicalInteractionInteractable interactbleTp = other.GetComponent<PhysicalInteractionInteractable>();
                if (interactbleTp != null)
                {
                    interactbleTp.OnFingerTipExit(m_FingerTipControl, this);
                    if (m_CollidedObjs.Contains(interactbleTp))
                        m_CollidedObjs.Remove(interactbleTp);
                }
            }
        }

        /// <summary>
        /// Is this a Thumb. <br>是不是母指指尖.</br>
        /// </summary>
        public bool IsThumb
        {
            get { return m_FingerID == FingerTipJointID.Thumb; }
        }
    }
}
