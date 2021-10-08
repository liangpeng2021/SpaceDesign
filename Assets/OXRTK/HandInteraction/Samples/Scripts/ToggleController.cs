using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    public class ToggleController : MonoBehaviour
    {
        public bool toggleStatus;
        public GameObject notToggled;
        public GameObject toggled;

        private void Start()
        {
            SetToggleVisualization(toggleStatus);
        }

        public void SetToggleVisualization(bool status)
        {
            if (notToggled != null)
                notToggled.SetActive(!status);
            if (toggled != null)
                toggled.SetActive(status);
        }

        public void SetToggle(bool status)
        {
            toggleStatus = status;
            SetToggleVisualization(toggleStatus);
        }

        public void SwitchToggle()
        {
            toggleStatus = !toggleStatus;
            SetToggleVisualization(toggleStatus);
        }
    }
}