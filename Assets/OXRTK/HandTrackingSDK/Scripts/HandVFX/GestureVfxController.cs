using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OXRTK.ARHandTracking;
using UnityEngine;
using UnityEngine.Serialization;

namespace OXRTK.ARHandTracking
{
    public enum FingerType
    {
        index,
        palm,
        pinky,
        ring,
        mid,
        thumb
    }
    
    /// <summary>
    /// The class for finger material control.<br>
    /// 用于手指材料控制的类。
    /// </summary>
    public class FingerVfx
    {
        /// <summary>
        /// Finger's material <br>
        /// 手指对应材质。
        /// </summary>
        public Material matInstance;
        
        /// <summary>
        /// Control point of the finger's vfx. <br>
        /// 手指视觉效果的控制点。
        /// </summary>
        public Transform attractor;
        
        /// <summary>
        /// IEnumerator for the code-driven animation. <br>
        /// 用以代码控制动画的集合访问器。
        /// </summary>
        public IEnumerator fingerFX;

        /// <summary>
        /// Function for the declaration of the FingerVfx Class. <br>
        /// 用以FingerVfx声明的函数。
        /// </summary>
        public FingerVfx(Material refMat, Transform attr)
        {
            attractor = attr;
            matInstance = refMat;
        }
    }

    /// <summary>
    /// The class for shader control of gesture interaction vfx.<br>
    /// Demo中控制手势交互视觉效果的shader的类。
    /// </summary>
    public class GestureVfxController : MonoBehaviour
    {
        #region Private Field
        
        RayInteractionPointer m_ParentRayPointer;
        HandController m_HandController;
        
        SkinnedMeshRenderer m_HandSmr;
        Dictionary<FingerType, FingerVfx> m_FingerMat = new Dictionary<FingerType, FingerVfx>();
        FingerType[] m_TranslateTrigger = {FingerType.index, FingerType.thumb};
        bool m_IsPinched;
        
        AudioSource m_Audio;

        [Header("Interaction audio")] 
        [SerializeField] AudioClip m_PinchDownAudio;
        [SerializeField] AudioClip m_PinchUpAudio;
        
        IEnumerator m_InitProcess;
        bool m_Initialized = false;
        
        #endregion

        void OnEnable()
        {
            if (!m_Initialized)
            {
                if (m_InitProcess != null)
                {
                    StopCoroutine(m_InitProcess);
                }

                m_InitProcess = Init();
                StartCoroutine(m_InitProcess);
            }
        }

        IEnumerator Init()
        {
            if(gameObject.GetComponent<AudioSource>() != null)
                m_Audio = GetComponent<AudioSource>();
            
            m_ParentRayPointer = GetComponentInParent<RayInteractionPointer>();
            m_ParentRayPointer.onActionEvent.AddListener(OnEvent);

            m_HandController = GetComponentInParent<HandController>();
            
            yield return new WaitUntil(() => m_HandController.activeHand != null);
                    
            yield return new WaitUntil(() => m_HandController.activeHand.joints[20] != null);

            if (m_HandSmr == null)
            {
                m_HandSmr = GetComponent<SkinnedMeshRenderer>();
            }
            
            var allHandMat = m_HandSmr.materials;

            m_FingerMat.Add(FingerType.index, new FingerVfx(allHandMat[0], m_HandController.activeHand.joints[16]));
            m_FingerMat.Add(FingerType.palm, new FingerVfx(allHandMat[1], m_HandController.activeHand.joints[0]));
            m_FingerMat.Add(FingerType.pinky, new FingerVfx(allHandMat[2], m_HandController.activeHand.joints[4]));
            m_FingerMat.Add(FingerType.ring, new FingerVfx(allHandMat[3], m_HandController.activeHand.joints[8]));
            m_FingerMat.Add(FingerType.mid, new FingerVfx(allHandMat[4], m_HandController.activeHand.joints[12]));
            m_FingerMat.Add(FingerType.thumb, new FingerVfx(allHandMat[5], m_HandController.activeHand.joints[20]));

            foreach (var fv in m_FingerMat)
            {
                fv.Value.matInstance.SetVector("_TriggerPos", new Vector4(fv.Value.attractor.position.x,
                    fv.Value.attractor.position.y,
                    fv.Value.attractor.position.z,
                    0));
            }

            m_Initialized = true;
        }

        void Update()
        {
            if (m_IsPinched)
            {
                UpdateFingerAttr(m_TranslateTrigger);
            }
        }

        public void OnEvent(PointerEventType eventType, GameObject obj = null)
        {
            switch (eventType)
            {
                case PointerEventType.OnPinchUp:
                    TriggerFingerFx(m_TranslateTrigger, m_PinchUpAudio, false);
                    m_IsPinched = false;
                    break;

                case PointerEventType.OnPinchDown:
                    TriggerFingerFx(m_TranslateTrigger, m_PinchDownAudio, true);
                    m_IsPinched = true;
                    break;
            }
        }

        private void TriggerFingerFx(FingerType[] fingers, AudioClip sound, bool isStart)
        {
            foreach (FingerType ft in fingers)
            {
                StartGestureFx(m_FingerMat[ft], isStart);
            }
            
            if (PointerManager.instance)
            {
                if (!PointerManager.instance.GetHandInteraction(m_HandController.handType).usePhysicalInteraction && sound != null)
                {
                    m_Audio.PlayOneShot(sound);
                }
                
            }
        }

        void StartGestureFx(FingerVfx fv, bool isStart)
        {
            if (fv.fingerFX != null)
            {
                StopCoroutine(fv.fingerFX);
            }

            fv.fingerFX = GestureEffectProcess(fv, isStart);
            StartCoroutine(fv.fingerFX);
        }

        void UpdateFingerAttr(FingerType[] fingers)
        {
            foreach (FingerType ft in fingers)
            {

                m_FingerMat[ft].matInstance.SetVector("_TriggerPos", new Vector4(m_FingerMat[ft].attractor.position.x,
                    m_FingerMat[ft].attractor.position.y,
                    m_FingerMat[ft].attractor.position.z,
                    0));
            }
        }

        IEnumerator GestureEffectProcess(FingerVfx fv, bool isStart)
        {
            float waitTime = .12f;
            float elapsedTime = 0f;

            float d = isStart ? 1 : 5;

            while (elapsedTime < waitTime)
            {
                d += (isStart ? 1f : -1f);
                fv.matInstance.SetFloat("_TriggerHighlightPower", d);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            fv.matInstance.SetFloat("_TriggerHighlightPower", isStart ? 10 : 0);
            yield return null;
        }
    }
}
