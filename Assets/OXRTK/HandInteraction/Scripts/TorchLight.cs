using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArmIK;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for visual cues management of ray interaction, UI interaction and physical interaction. <br>
    /// 射线，UI与物理交互的视觉线索管理。
    [RequireComponent(typeof(UiInteractionPointer))]
    [RequireComponent(typeof(PhysicalInteractionHand))]
    public class TorchLight : MonoBehaviour
    {
        private UiInteractionPointer m_UIP;
        private PhysicalInteractionHand m_PIH;
        private HandController m_ConnectedHand;

        private Collider[] m_TouchableObj;

        private Transform m_TDirStart;
        private Transform m_TDirEnd;
        private Transform m_Thumb;
        
        private int probeDirID;
        private int probePosID;

        private int m_ProbeDirID_G;
        private int m_ProbePosID_G;
        
        private bool initialized = false;
        
        // Local hand propertities
        private Vector3 m_TDir;
        private Vector3 m_PosProbe;

        public static bool isPhysical;

        // Global left hand for indirect mode
        public static Vector4 probe1Ind;
        public static Vector4 probe2Ind;
        
        // Global right hand for indirect mode
        public static Vector4 inputDir1Ind;
        public static Vector4 inputDir2Ind;

        // Public static vector for Global hands directions
        public static Vector4 leftHandDir;
        public static Vector4 righttHandDir;

        private bool m_ConnectedHandDetected = false;

        private void OnEnable()
        {
        }

        void OnHandDetectionChanged(HandTrackingPlugin.HandType handType, bool detected)
        {
            if (!detected)
            {
                if (handType == HandTrackingPlugin.HandType.LeftHand &&
                    m_ConnectedHand.handType == HandTrackingPlugin.HandType.LeftHand)
                {
                    probe1Ind = Vector4.one * 999f;
                    Shader.SetGlobalVector(m_ProbePosID_G, probe1Ind);
                }
                else if (handType == HandTrackingPlugin.HandType.RightHand &&
                         m_ConnectedHand.handType == HandTrackingPlugin.HandType.RightHand)
                {
                    probe2Ind = Vector4.one * 999f;
                    Shader.SetGlobalVector(m_ProbePosID_G, probe2Ind);

                }
                m_PosProbe = Vector3.one * 999f;
                Shader.SetGlobalVector(probePosID, new Vector4(m_PosProbe.x, m_PosProbe.y, m_PosProbe.z, 0));
                
                if (handType == m_ConnectedHand.handType) 
                {m_ConnectedHandDetected = false;}
            }
            else
            {
                if (handType == m_ConnectedHand.handType)
                {m_ConnectedHandDetected = true;}
            }
        }

        IEnumerator Start()
        {
            m_UIP = GetComponent<UiInteractionPointer>();
            m_PIH = GetComponent<PhysicalInteractionHand>();
            
            m_ConnectedHand = GetComponent<HandController>();

            yield return new WaitUntil(() => m_ConnectedHand.activeHand != null);
            yield return new WaitUntil(() => m_ConnectedHand.activeHand.joints[(int)HandJointID.Wrist] != null);

            m_TDirEnd = m_ConnectedHand.activeHand.joints[16];
            m_Thumb = m_ConnectedHand.activeHand.joints[20];

            yield return new WaitUntil(() => PoseManager.Instance.m_LShoulder != null);
            yield return new WaitUntil(() => PoseManager.Instance.m_RShoulder != null);

            
            if (m_ConnectedHand.handType == HandTrackingPlugin.HandType.LeftHand)
            {
                probeDirID = Shader.PropertyToID("_inputDir");
                probePosID = Shader.PropertyToID("_probe");

                m_ProbeDirID_G = Shader.PropertyToID("_inputDir_ind");
                m_ProbePosID_G = Shader.PropertyToID("_probe_ind");

                m_TDirStart = PoseManager.Instance.m_LShoulder;
            }
            else
            {
                probeDirID = Shader.PropertyToID("_inputDir2");
                probePosID = Shader.PropertyToID("_probe2");
                
                m_ProbeDirID_G = Shader.PropertyToID("_inputDir2_ind");
                m_ProbePosID_G = Shader.PropertyToID("_probe2_ind");

                m_TDirStart = PoseManager.Instance.m_RShoulder;
            }
            
            if ( m_ConnectedHand.handType == HandTrackingPlugin.HandType.LeftHand)
            {
                
                Shader.SetGlobalVector(m_ProbePosID_G, new Vector4(9999, 9999, 9999, 9999));
            }
            else if (m_ConnectedHand.handType == HandTrackingPlugin.HandType.RightHand)
            {
                
                Shader.SetGlobalVector(m_ProbePosID_G, new Vector4(9999, 9999, 9999, 9999));
            }
            
            m_PosProbe = Vector3.one * 999f;
            Shader.SetGlobalVector(probePosID, new Vector4(m_PosProbe.x, m_PosProbe.y, m_PosProbe.z, 0));
            
            CustomizedGestureController.instance.onHandDisplayChanged += OnHandDetectionChanged;
            initialized = true;
        }

        void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (!m_ConnectedHandDetected)
            {
                return;
            }

            if (!isPhysical)
            {
                m_TDir = Vector3.Lerp(m_TDir, (m_TDirEnd.position - m_TDirStart.position).normalized, 0.4f);
                // posProbe = Vector3.Lerp(posProbe, m_TDirEnd.position, 0.35f);
                m_PosProbe = m_TDirEnd.position;
            }
            else
            {
                m_TDir = Vector3.Lerp(m_TDir, ((m_TDirEnd.position+m_Thumb.position)/2 - m_TDirStart.position).normalized, 0.4f);
                // posProbe = Vector3.Lerp(posProbe, (m_TDirEnd.position+m_Thumb.position)/2, 0.35f);
                m_PosProbe = (m_TDirEnd.position+m_Thumb.position)/2;
            }

            Shader.SetGlobalVector(probePosID, new Vector4(m_PosProbe.x, m_PosProbe.y, m_PosProbe.z, 0));
            Shader.SetGlobalVector(probeDirID, new Vector4(m_TDir.x, m_TDir.y, m_TDir.z, 0));
                
            if (m_ConnectedHand.handType == HandTrackingPlugin.HandType.LeftHand)
            {
                Shader.SetGlobalVector(m_ProbePosID_G, probe1Ind);
                Shader.SetGlobalVector(m_ProbeDirID_G, inputDir1Ind);
                
                leftHandDir = m_TDir;
            }
            else if (m_ConnectedHand.handType == HandTrackingPlugin.HandType.RightHand)
            {
                Shader.SetGlobalVector(m_ProbePosID_G, probe2Ind);
                Shader.SetGlobalVector(m_ProbeDirID_G, inputDir2Ind);
                
                righttHandDir = m_TDir;
            }
        }
    }
}