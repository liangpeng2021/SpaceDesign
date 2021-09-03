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
        public bool useRayInteraction;
        public bool usePhysicalInteraction;
        public bool useUIInteraction;

        public InteractionSetting(HandTrackingPlugin.HandType type, bool rayStatus, bool physicalStatus, bool uiStatus)
        {
            handType = type;
            useRayInteraction = rayStatus;
            usePhysicalInteraction = physicalStatus;
            useUIInteraction = uiStatus;
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

        private InteractionSetting[] m_PreviousHandSetting;

        private void Update()
        {
            if (m_PreviousHandSetting == null)
                return;

            if (handSetting.Length != m_PreviousHandSetting.Length) {
                Debug.LogError("HandSetting length changed! Please verify.");
                return;
            }

            for(int i = 0; i < handSetting.Length; i++)
            {
                if (handSetting[i].handType != m_PreviousHandSetting[i].handType)
                {
                    Debug.LogError("HandTypeChanged, please make sure hand type doesn't change after running.");
                    return;
                }

                if (handSetting[i].useRayInteraction != m_PreviousHandSetting[i].useRayInteraction)
                {
                    PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.RayInteraction, handSetting[i].useRayInteraction);
                    m_PreviousHandSetting[i].useRayInteraction = handSetting[i].useRayInteraction;
                    PointerManager.instance.UpdateCurrentInteraction(handSetting);
                }

                if (handSetting[i].usePhysicalInteraction != m_PreviousHandSetting[i].usePhysicalInteraction)
                {
                    PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.PhysicalInteraction, handSetting[i].usePhysicalInteraction);
                    m_PreviousHandSetting[i].usePhysicalInteraction = handSetting[i].usePhysicalInteraction;
                    PointerManager.instance.UpdateCurrentInteraction(handSetting);
                }

                if (handSetting[i].useUIInteraction != m_PreviousHandSetting[i].useUIInteraction)
                {
                    PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.UiIneraction, handSetting[i].useUIInteraction);
                    m_PreviousHandSetting[i].useUIInteraction = handSetting[i].useUIInteraction;
                    PointerManager.instance.UpdateCurrentInteraction(handSetting);
                }
            }    
        }

        void InteractionSetup()
        {
            if (PointerManager.instance == null)
                return;

            if (handSetting.Length > 2)
            {
                Debug.LogError("Wrong number in interaction type controller setting, fail to set");
                return;
            }
            for (int i = 0; i < handSetting.Length; i++)
            {
                PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.RayInteraction, handSetting[i].useRayInteraction);

                PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.PhysicalInteraction, handSetting[i].usePhysicalInteraction);

                PointerManager.instance.SetHandInteraction(handSetting[i].handType, HandInteractionType.UiIneraction, handSetting[i].useUIInteraction);
            }

            m_PreviousHandSetting = new InteractionSetting[handSetting.Length];
            for(int i = 0; i < handSetting.Length; i++)
            {
                m_PreviousHandSetting[i].handType = handSetting[i].handType;
                m_PreviousHandSetting[i].useRayInteraction = handSetting[i].useRayInteraction;
                m_PreviousHandSetting[i].usePhysicalInteraction = handSetting[i].usePhysicalInteraction;
                m_PreviousHandSetting[i].useUIInteraction = handSetting[i].useUIInteraction;
            }
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => PointerManager.instance != null);
            yield return new WaitUntil(() => PointerManager.instance.CheckActive());
            InteractionSetup();
            PointerManager.instance.UpdateCurrentInteraction(handSetting);
        }

    }
}
