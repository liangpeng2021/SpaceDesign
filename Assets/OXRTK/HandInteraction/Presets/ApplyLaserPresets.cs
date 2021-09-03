using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Presets;
#endif

namespace OXRTK.ARHandTracking
{
    public class ApplyLaserPresets : MonoBehaviour
    {
#if UNITY_EDITOR
        public Preset preset;
        public void ApplyLaserVis(LineRenderer line)
        {
            preset.ApplyTo(line);
        }
#endif
    }
}