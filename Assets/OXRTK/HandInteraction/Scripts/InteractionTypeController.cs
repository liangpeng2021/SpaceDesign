using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The struct for available interaction of each hand. <br>
    /// 每只手可用交互类型的结构。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct InteractionSetting
    {
        public HandTrackingPlugin.HandType handType;
        public bool isHandDisplayOn;
        public bool useRayInteraction;
        public bool usePhysicalInteraction;
        public bool useUIInteraction;
       // [HideInInspector]
       // public bool isHandMenuOpen;
        public InteractionSetting(HandTrackingPlugin.HandType type, bool handDisplay, bool rayStatus, bool physicalStatus, bool uiStatus)
        {
            handType = type;
            isHandDisplayOn = handDisplay;
            useRayInteraction = rayStatus;
            usePhysicalInteraction = physicalStatus;
            useUIInteraction = uiStatus;
           // isHandMenuOpen = false;
        }
    }

    /// <summary>
    /// The class for interaction selection of current scene. <br>
    /// 选择当前场景交互的类。
    /// </summary>
    public class InteractionTypeController : MonoBehaviour
    {
        /// <summary>
        /// Selects available interaction for each hand in current scene. <br>
        /// 对左右手分别选择当前场景的可用交互。
        /// </summary>
        public InteractionSetting[] handSetting;

        /// <summary>
        /// OK visual and audio feedback for hand. <br>
        /// 当前手上OK的视觉和声音反馈。
        /// </summary>
        [Header("Gesture Feedback")]
        public bool okFeedback = true;

        private InteractionSetting[] m_PreviousHandSetting;
        HandMenuLocator[] m_HandMenus;
        int m_HandMenuOpened = 0;
        bool m_IsHandMenuOpen = false, m_IsLastHandMenuOpen = false;

        private void Update()
        {
            if (m_PreviousHandSetting == null)
                return;

            if (handSetting.Length != m_PreviousHandSetting.Length) {
                Debug.LogError("HandSetting length changed! Please verify.");
                return;
            }

            for (int i = 0; i < handSetting.Length; i++)
            {
                if (handSetting[i].handType != m_PreviousHandSetting[i].handType)
                {
                    Debug.LogError("HandTypeChanged, please make sure hand type doesn't change after running.");
                    return;
                }

                if ((handSetting[i].useRayInteraction || m_IsHandMenuOpen) != (m_PreviousHandSetting[i].useRayInteraction || m_IsLastHandMenuOpen))
                {
                    PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.RayInteraction, (handSetting[i].useRayInteraction || m_IsHandMenuOpen));
                    PointerManager.instance.UpdateCurrentInteraction(handSetting);
                }

                if ((handSetting[i].usePhysicalInteraction && !m_IsHandMenuOpen) != (m_PreviousHandSetting[i].usePhysicalInteraction && !m_IsLastHandMenuOpen))
                {
                    PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.PhysicalInteraction, handSetting[i].usePhysicalInteraction && !m_IsHandMenuOpen);
                    PointerManager.instance.UpdateCurrentInteraction(handSetting);
                }

                if ((handSetting[i].useUIInteraction && !m_IsHandMenuOpen) != (m_PreviousHandSetting[i].useUIInteraction && !m_IsLastHandMenuOpen))
                {
                    PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.UiIneraction, handSetting[i].useUIInteraction && !m_IsHandMenuOpen);
                    PointerManager.instance.UpdateCurrentInteraction(handSetting);
                }

                m_PreviousHandSetting[i].useRayInteraction = handSetting[i].useRayInteraction;
                m_PreviousHandSetting[i].usePhysicalInteraction = handSetting[i].usePhysicalInteraction;
                m_PreviousHandSetting[i].useUIInteraction = handSetting[i].useUIInteraction;
            }
            m_IsLastHandMenuOpen = m_IsHandMenuOpen;
        }

        void InteractionSetup()
        {
            bool physicalNeeded = false;
            
            if (PointerManager.instance == null)
                return;

            if (handSetting.Length > 2)
            {
                Debug.LogError("Wrong number in interaction type controller setting, fail to set");
                return;
            }
            for (int i = 0; i < handSetting.Length; i++)
            {
                PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.RayInteraction, handSetting[i].useRayInteraction || m_IsHandMenuOpen);

                PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.PhysicalInteraction, handSetting[i].usePhysicalInteraction && !m_IsHandMenuOpen);

                PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.UiIneraction, handSetting[i].useUIInteraction && !m_IsHandMenuOpen);

                if (handSetting[i].usePhysicalInteraction) { physicalNeeded = true; }
            }
            
            if (physicalNeeded) {TorchLight.isPhysical = true;}

            m_PreviousHandSetting = new InteractionSetting[handSetting.Length];
            for(int i = 0; i < handSetting.Length; i++)
            {
                m_PreviousHandSetting[i].handType = handSetting[i].handType;
                m_PreviousHandSetting[i].useRayInteraction = handSetting[i].useRayInteraction;
                m_PreviousHandSetting[i].usePhysicalInteraction = handSetting[i].usePhysicalInteraction;
                m_PreviousHandSetting[i].useUIInteraction = handSetting[i].useUIInteraction;
            }
            m_IsLastHandMenuOpen = m_IsHandMenuOpen;
        }

        IEnumerator Start()
        {
            InitHandMenuList();
            yield return new WaitUntil(() => PointerManager.instance != null);
            yield return new WaitUntil(() => PointerManager.instance.CheckActive());
            InteractionSetup();
            PointerManager.instance.UpdateCurrentInteraction(handSetting);

            InitHandDisplayMode();
        }

        public void InitHandDisplayMode()
        {
            /*
            if (handSetting[0].usePhysicalInteraction || handSetting[0].useUIInteraction ||
                handSetting[1].usePhysicalInteraction || handSetting[1].useUIInteraction)
            {
                HandDisplayModeSwtich.ForceHandDisplayOn(true);
            }
            else
            {
                HandDisplayModeSwtich.ForceHandDisplayOn(false);
            }
            */
            HandDisplayModeSwtich.ForceHandDisplayOn(handSetting[0].isHandDisplayOn, false);
            HandDisplayModeSwtich.ForceHandDisplayOn(handSetting[1].isHandDisplayOn, true);
        }

        //HandMenu switch
        void InitHandMenuList()
        {
            m_HandMenus = FindObjectsOfType<HandMenuLocator>();
            for (int i = 0; i < m_HandMenus.Length; i++)
            {
                m_HandMenus[i].onMenuStatusChange += SetHandMenuNumber;
            }
            m_HandMenuOpened = 0;
            m_IsHandMenuOpen = false;
        }

        void SetHandMenuNumber(bool status)
        {
            if (status)
            {
                if (m_HandMenuOpened == 0)
                {
                    m_IsHandMenuOpen = true;
                }
                m_HandMenuOpened += 1;
            }
            else if(m_HandMenuOpened > 0)
            {
                m_HandMenuOpened -= 1;
                if (m_HandMenuOpened == 0)
                {
                    m_IsHandMenuOpen = false;
                }
            }
        }
    }
}
