using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OXRTK.ARHandTracking
{
    [RequireComponent(typeof(ToggleController))]
    public class HandDisplayModeSwtich : MonoBehaviour
    {
        public static List<HandDisplayModeSwtich> handDisplayControls = new List<HandDisplayModeSwtich>();
        private HandController m_ConnectedController;
        public bool isLeft;
        [HideInInspector]public ToggleController toggle;
       // [HideInInspector] public static bool rightHandControllerLocated = false;
       // [HideInInspector] public static bool leftHandControllerLocated = false;

        //void Awake()
        /*IEnumerator Start()
        {
            if (isLeft)
            {
                //yield return new WaitUntil(() => HandTrackingPlugin.instance.leftHandController != null);
                m_ConnectedController = HandTrackingPlugin.instance.leftHandController;
                leftHandControllerLocated = true;
            }
            else
            {
                yield return new WaitUntil(() => HandTrackingPlugin.instance.rightHandController != null);
                m_ConnectedController = HandTrackingPlugin.instance.rightHandController;
                rightHandControllerLocated = true;
            }
            
            handDisplayControls.Add(this);
            toggle = GetComponent<ToggleController>();
        }*/

        void Awake()
        {
            handDisplayControls.Add(this);
            toggle = GetComponent<ToggleController>();
        }

        public void ToggleHandDisplay()
        {
            if(m_ConnectedController == null)
            {
                if(isLeft && HandTrackingPlugin.instance.leftHandController != null)
                {
                    m_ConnectedController = HandTrackingPlugin.instance.leftHandController;
                } else if(!isLeft && HandTrackingPlugin.instance.rightHandController != null)
                {
                    m_ConnectedController = HandTrackingPlugin.instance.rightHandController;
                }
            }
            if (m_ConnectedController != null)
            {
                // Toggle the hand view in the HandController.cs script
                m_ConnectedController.hidHandMode = !m_ConnectedController.hidHandMode;
                m_ConnectedController.hands[m_ConnectedController.activeHandId].SwitchHandDisplay(m_ConnectedController.hidHandMode);
            }
        }

        public void ToggleHandDisplay(bool isOn)
        {
            if (m_ConnectedController == null)
            {
                if (isLeft && HandTrackingPlugin.instance.leftHandController != null)
                {
                    m_ConnectedController = HandTrackingPlugin.instance.leftHandController;
                }
                else if (!isLeft && HandTrackingPlugin.instance.rightHandController != null)
                {
                    m_ConnectedController = HandTrackingPlugin.instance.rightHandController;
                }
            }
            if (m_ConnectedController != null)
            {
                m_ConnectedController.hidHandMode = !isOn;
                m_ConnectedController.hands[m_ConnectedController.activeHandId].SwitchHandDisplay(m_ConnectedController.hidHandMode);
            }
        }

        public static void ForceBothHandsDisplayOn(bool isOn)
        {
            foreach (HandDisplayModeSwtich hdm in handDisplayControls)
            {
                hdm.ToggleHandDisplay(isOn);
                hdm.toggle.SetToggle(isOn);
            }
        }

        public static void ForceHandDisplayOn(bool isOn, bool isLeft)
        {
            foreach (HandDisplayModeSwtich hdm in handDisplayControls)
            {
                if (hdm.isLeft == isLeft)
                {
                    hdm.ToggleHandDisplay(isOn);
                    hdm.toggle.SetToggle(isOn);
                }                
            }
        }
    }
}