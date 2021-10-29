using System;
using System.Collections;
using System.Collections.Generic;
using OXRTK.ARHandTracking;
using UnityEngine;
using UnityEngine.SceneManagement;
using XR;
using ArmIK;

namespace OXRTK
{
    public class HudManager : MonoBehaviour
    {
        private HandController m_LeftHand;
        private HandController m_RightHand;

        private RectTransform m_LeftHandIndicator;
        private RectTransform m_RightHandIndicator;
        
        private RectTransform m_LeftPing;
        private RectTransform m_RightPing;
        
        [SerializeField ]private Camera m_MainCamera;
        
        private int m_ScreenHeight;
        private int m_ScreenWidth;
        private Vector3 m_ScreenOffset;

        private HandTrackingPlugin.HandInfo infoL;
        private HandTrackingPlugin.HandInfo infoR;

        private IEnumerator indicatorProcess;
        private float m_Offset = 0.1f;

        public bool m_UsePalm;

        [SerializeField] private FluidOSI m_LeftOsi;
        [SerializeField] private FluidOSI m_RightOsi;
        
        public GameObject overlayMask;

        [SerializeField] private GameObject m_WavePlane;

        public static HudManager instance { get; set; }

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InteractionTypeController interactionTypeController = FindObjectOfType<InteractionTypeController>();
            if (interactionTypeController == null)
            {
                if (HandTrackingPlugin.debugLevel > 0)
                {
                    Debug.Log("No InteractionTypeController in current scene, force turning on HandDisplay.");
                }
                HandDisplayModeSwtich.ForceBothHandsDisplayOn(true);
            }
        }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            instance = this;

            m_ScreenHeight = 1080;
            m_ScreenWidth = 1920;
            m_ScreenOffset = new Vector3(m_ScreenWidth, m_ScreenHeight, 0) / 2;

            /*m_LeftPing.gameObject.SetActive(false);
            m_RightPing.gameObject.SetActive(false);*/
            m_LeftOsi.gameObject.SetActive(false);
            m_RightOsi.gameObject.SetActive(false);
            
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                m_MainCamera = XRCameraManager.Instance.eventCamera;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                yield return new WaitUntil(() => CenterCamera.instance.centerCamera != null);
                m_MainCamera = CenterCamera.instance.centerCamera;
            }

            yield return new WaitUntil(() => HandTrackingPlugin.instance != null);
            m_LeftHand = HandTrackingPlugin.instance.leftHandController;
            m_RightHand = HandTrackingPlugin.instance.rightHandController;

            yield return null;
        }

        void SetIndicator()
        {
            if (indicatorProcess != null)
            {
                StopCoroutine(indicatorProcess);
            }
        }

        IEnumerator SettingIndicatorProcess(bool onOff)
        {
            yield return null;
        }

        Vector3 V3Clamp(Vector3 v, float bld)
        {
            Vector3 result = new Vector3();

            v = new Vector3(v.x * 1920f, v.y * 1080f, v.z);

            if (Mathf.Abs(v.y) > Mathf.Abs(v.x))
            {
                result.x = v.x > 0 ? ((m_ScreenHeight / 2) * v.x) / v.y : -((m_ScreenHeight / 2) * v.x) / v.y;
                result.y = v.y > 0 ? (m_ScreenHeight / 2) : (-m_ScreenHeight / 2);
            }
            else
            {
                result.y = v.y > 0 ? ((m_ScreenWidth / 2) * (v.y / v.x)) : ((m_ScreenWidth / 2) * (v.y / v.x));
                result.x = v.x > 0 ? (m_ScreenWidth / 2) : (-m_ScreenWidth / 2);
            }

            return result;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (m_LeftHand == null || m_RightHand == null)
            {
                return;
            }
            // Update Hand Info
            infoL = HandTrackingPlugin.instance.leftHandInfo;
            infoR = HandTrackingPlugin.instance.rightHandInfo;
            
            UpdateOsiIndicator(m_RightHand, m_RightOsi, infoR);
            UpdateOsiIndicator(m_LeftHand, m_LeftOsi, infoL);
            
            // Control the indirect wave plane torch light

            TorchLight.probe1Ind = m_WavePlane.transform.InverseTransformPoint(m_LeftHand.activeHand.joints[16].position);
            TorchLight.probe2Ind = m_WavePlane.transform.InverseTransformPoint(m_RightHand.activeHand.joints[16].position);

            TorchLight.inputDir1Ind = m_WavePlane.transform.InverseTransformDirection(TorchLight.leftHandDir);
            TorchLight.inputDir2Ind = m_WavePlane.transform.InverseTransformDirection(TorchLight.righttHandDir);
        }
        
        
        void UpdateOsiIndicator(HandController hc, FluidOSI osi, HandTrackingPlugin.HandInfo hInfo)
        {
            if (hc.activeHand)
            {
                Vector3 screenPoint;
                
                if (m_UsePalm)
                {
                    Vector3 palmPos = (hc.activeHand.joints[0].position +
                                        hc.activeHand.joints[5].position +
                                        hc.activeHand.joints[13].position) / 3f;
                    
                    screenPoint = m_MainCamera.WorldToViewportPoint(palmPos);
                }
                else
                {
                    screenPoint = m_MainCamera.WorldToViewportPoint(hc.activeHand.joints[16].position);
                }
                
                Vector3 hudIndicatorPos = screenPoint;
                
                osi.SetAttractor(hudIndicatorPos);

                if (screenPoint.x > 1 - m_Offset || screenPoint.y > 1 - m_Offset || screenPoint.x < m_Offset || screenPoint.y < m_Offset)
                {                    
                    osi.gameObject.SetActive(true);
                }
                else
                {
                    osi.gameObject.SetActive(false);
                }
            }
            
            if (!hInfo.handDetected)
            {
                osi.gameObject.SetActive(false);
            }
        }

        public void ShowOverlayMask()
        {
            overlayMask.SetActive(true);
        }

        public void HideOverlayMask()
        {
            overlayMask.SetActive(false);
        }
    }
}
