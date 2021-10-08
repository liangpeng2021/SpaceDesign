using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Camera gaze module, send gaze events to WaveGazeHandle.cs.<br>相机凝视组件,用于发射凝视信号到WaveGazeHandle.cs.</br>
    /// </summary>
    public class WaveInteractionGazeCaster : MonoBehaviour
    {
        public static WaveInteractionGazeCaster instance;
        private void Awake()
        {
            if (WaveInteractionGazeCaster.instance != null)
            {
                if (WaveInteractionGazeCaster.instance.gameObject.activeInHierarchy)
                {
                    Destroy(this);
                    return;
                }
                else
                {
                    Destroy(WaveInteractionGazeCaster.instance);
                }
            }
            instance = this;
        }

        WaveInteractionGazeHandle m_ActiveGazeHandle = null;
        void Update()
        {
            GazeCast();
        }

        void GazeCast()
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
            {
                WaveInteractionGazeHandle handleTp = hit.transform.GetComponent<WaveInteractionGazeHandle>();
                if (handleTp != null)
                {
                    if (m_ActiveGazeHandle != handleTp)
                    {
                        if (m_ActiveGazeHandle != null)
                            m_ActiveGazeHandle.OnGazeExit();
                        m_ActiveGazeHandle = handleTp;
                        m_ActiveGazeHandle.OnGazeEnter();
                    }
                    m_ActiveGazeHandle.OnGaze(hit.point);
                    return;
                }
            }
            if (m_ActiveGazeHandle != null)
            {
                m_ActiveGazeHandle.OnGazeExit();
                m_ActiveGazeHandle = null;
            }
        }
    }
}
