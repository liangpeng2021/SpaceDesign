using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandColliderHandle : MonoBehaviour
{
    public enum ColliderType
    {
        colliderOnly, colliderAndRigidbody, triggerOnly, triggerAndRigidbody
    }
    public enum JointNeedCollider
    {
        none, indexFingerTip, first3FingerTip, first4FingerTip, allFingerTip, allJoint
    }

    public static void AddColliderAndRigidbody(Transform[] joints, ColliderType type, JointNeedCollider need, float scaleFactor = 1f)
    {
        int[] toAdd = GenerateToDoArray(need);
        if (toAdd != null)
        {
            for (int i = 0; i < toAdd.Length; i++)
            {
                AddCollider(joints[toAdd[i]].gameObject, (type == ColliderType.triggerOnly || type == ColliderType.triggerAndRigidbody), scaleFactor);
                if (type == ColliderType.colliderAndRigidbody || type == ColliderType.triggerAndRigidbody)
                    AddRigidbody(joints[toAdd[i]].gameObject);
            }
        }
    }

    static void AddCollider(GameObject obj, bool isTrigger, float scaleFactor)
    {
        SphereCollider colliderTp = obj.AddComponent<SphereCollider>();
        colliderTp.radius *= scaleFactor;
        colliderTp.isTrigger = isTrigger;

        obj.layer = 2;
    }

    static void AddRigidbody(GameObject obj)
    {
        Rigidbody rigidTP = obj.AddComponent<Rigidbody>();
        rigidTP.isKinematic = true;
        rigidTP.useGravity = false;
        rigidTP.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    static int[] m_IndexOnly = new int[] { 16 };
    static int[] m_ThreeFingerTip = new int[] { 20, 16, 12 };
    static int[] m_FourFingerTip = new int[] { 20, 16, 12, 8 };
    static int[] m_AllFingerTip = new int[] { 20, 16, 12, 8, 4 };
    static int[] m_AllJoint = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

    static int[] GenerateToDoArray(JointNeedCollider need)
    {
        switch (need)
        {
            case JointNeedCollider.indexFingerTip:
                return m_IndexOnly;
            case JointNeedCollider.first3FingerTip:
                return m_ThreeFingerTip;
            case JointNeedCollider.first4FingerTip:
                return m_FourFingerTip;
            case JointNeedCollider.allFingerTip:
                return m_AllFingerTip;
            case JointNeedCollider.allJoint:
                return m_AllJoint;
        }
        return null;
    }
    // HandInfo.pred_3D: 21x3 keypoints are xyz coordinates in meters
    /*
     *
     *             *17 --- *18 --- *19 --- *20                      Thumb
     *            /
     *           /         
     *          *0 ------- *13 ---- *14 ---- *15 ---- *16        Index finger
     *           \\\          
     *            \\ ----- *9 ----- *10 ----- *11 ----- *12     Middle finger
     *             \\       
     *              \ ---- *5 ---- *6 ---- *7 ---- *8        Ring finger
     *               \ 
     *                *1 -- *2 -- *3 -- *4                   Pinky finger
     *                         
     */
}
