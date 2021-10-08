using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OXRTK.ARHandTracking;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        /// Finger's after image material <br>
        /// 手指残影指对应材质。
        /// </summary>
        public Material aiMatInstance;
        
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
        public FingerVfx(Material refMat, Transform attr, Material aiMat)
        {
            attractor = attr;
            matInstance = refMat;
            aiMatInstance = aiMat;
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
        FingerType[] m_TranslateTrigger = { FingerType.index, FingerType.thumb };
        FingerType[] m_AllFingers = { FingerType.index, FingerType.thumb, FingerType.mid, FingerType.pinky, FingerType.ring };
        bool m_IsPinched;

        AudioSource m_Audio;

        [Header("Interaction audio")]
        [SerializeField] AudioClip m_PinchDownAudio;
        [SerializeField] AudioClip m_PinchUpAudio;
        [SerializeField] private MeshFilter m_AfterImage;
        
        IEnumerator m_InitProcess;
        bool m_Initialized = false;
        private bool m_OkActivated = true;

        private Vector3 m_AiPos;
        private Quaternion m_AiRot;

        private bool m_isHandHidden;
        #endregion

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check InteractionSetting and set effects for hands.
            if (HandTrackingPlugin.debugLevel > 0)
            {
                Debug.Log("OnSceneLoaded: scene " + scene.name + ", mode " + mode);
            }

            if (m_FingerMat.Count != 0)
            {
                ResetFingerAttr();
                ResetAfterImage();
            }
            
            InteractionTypeController itc = FindObjectOfType<InteractionTypeController>();

            if (itc)
            {
                m_OkActivated = itc.okFeedback;
            }
            else
            {
                m_OkActivated = true;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

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

        void OnDisable()
        {
            ResetAfterImage();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        IEnumerator Init()
        {
            if (gameObject.GetComponent<AudioSource>() != null)
            {
                m_Audio = GetComponent<AudioSource>();
            }

            m_ParentRayPointer = GetComponentInParent<RayInteractionPointer>();
            m_ParentRayPointer.onActionEvent.AddListener(OnEvent);

            m_HandController = GetComponentInParent<HandController>();

            yield return new WaitUntil(() => m_HandController.activeHand != null);

            yield return new WaitUntil(() => m_HandController.activeHand.joints[20] != null);

            if (m_HandSmr == null)
            {
                //m_HandSmr = GetComponent<SkinnedMeshRenderer>();
                m_HandSmr = GetComponent<ModelHand>().skinnedMeshRenderer;
            }

            var allAiMat = m_AfterImage.GetComponent<MeshRenderer>().materials;

            var allHandMat = m_HandSmr.materials;

            m_FingerMat.Add(FingerType.index, new FingerVfx(allHandMat[0], m_HandController.activeHand.joints[16], allAiMat[0]));
            m_FingerMat.Add(FingerType.palm, new FingerVfx(allHandMat[1], m_HandController.activeHand.joints[0], allAiMat[1]));
            m_FingerMat.Add(FingerType.pinky, new FingerVfx(allHandMat[2], m_HandController.activeHand.joints[4], allAiMat[2]));
            m_FingerMat.Add(FingerType.ring, new FingerVfx(allHandMat[3], m_HandController.activeHand.joints[8], allAiMat[3]));
            m_FingerMat.Add(FingerType.mid, new FingerVfx(allHandMat[4], m_HandController.activeHand.joints[12], allAiMat[4]));
            m_FingerMat.Add(FingerType.thumb, new FingerVfx(allHandMat[5], m_HandController.activeHand.joints[20], allAiMat[5]));

            foreach (var fv in m_FingerMat)
            {
                fv.Value.matInstance.SetVector("_TriggerPos", new Vector4(
                    fv.Value.attractor.position.x,
                    fv.Value.attractor.position.y,
                    fv.Value.attractor.position.z,
                    0));
            }

            m_Initialized = true;

            m_isHandHidden = m_HandController.hidHandMode;
        }
        
        void Update()
        {
            if (!(m_HandController.hands[m_HandController.activeHandId] is ModelHand))
                return;
            
            if (m_IsPinched && m_OkActivated)
            {
                if (!m_HandController.hidHandMode)
                {
                    UpdateFingerAttr(m_TranslateTrigger);
                }
                else if (m_HandController.hidHandMode)
                {
                    m_AfterImage.transform.position = m_AiPos;
                    m_AfterImage.transform.rotation = m_AiRot;
                }
            }

            if (m_isHandHidden != m_HandController.hidHandMode)
            {
                m_isHandHidden = m_HandController.hidHandMode;
                
                ResetAfterImage();
            }
        }

        public void OnEvent(PointerEventType eventType, GameObject obj = null)
        {
            if (!m_OkActivated || !(m_HandController.hands[m_HandController.activeHandId] is ModelHand))
                return;

            switch (eventType)
            {
                case PointerEventType.OnPinchUp:
                    if (!m_HandController.hidHandMode)
                    {
                        TriggerFingerFx(m_TranslateTrigger, m_PinchUpAudio, false);
                    }
                    m_IsPinched = false;
                    break;

                case PointerEventType.OnPinchDown:
                    TriggerFingerFx(m_TranslateTrigger, m_PinchDownAudio, true);

                    if (m_HandController.hidHandMode)
                    {
                        // Update the mesh of the After Image
                        m_AiPos = m_HandSmr.transform.position;
                        m_AiRot = m_HandSmr.transform.rotation;
                        
                        m_HandSmr.BakeMesh(m_AfterImage.mesh);
                    }

                    m_IsPinched = true;
                    break;
            }
        }

        private void TriggerFingerFx(FingerType[] fingers, AudioClip sound, bool isStart)
        {
            if (m_HandController.hidHandMode)
            {
                UpdateFingerAttr(m_TranslateTrigger);
            }

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

            if (m_HandController.hidHandMode)
            {
                fv.fingerFX = AfterImageEffectProcess(fv);
            }
            else
            {
                fv.fingerFX = GestureEffectProcess(fv, isStart);
            }
            
            StartCoroutine(fv.fingerFX);
        }

        void UpdateFingerAttr(FingerType[] fingers)
        {
            if (m_FingerMat.Count != 0)
            {
                foreach (FingerType ft in fingers)
                {
                    m_FingerMat[ft].matInstance.SetVector("_TriggerPos", new Vector4(
                        m_FingerMat[ft].attractor.position.x,
                        m_FingerMat[ft].attractor.position.y,
                        m_FingerMat[ft].attractor.position.z,
                        0));

                    m_FingerMat[ft].aiMatInstance.SetVector("_TriggerPos", new Vector4(
                        m_FingerMat[ft].attractor.position.x,
                        m_FingerMat[ft].attractor.position.y,
                        m_FingerMat[ft].attractor.position.z,
                        0));
                }
            }
        }
        
        void ResetFingerAttr()
        {
            m_IsPinched = false;
            
            if (m_FingerMat.Count != 0)
            {
                foreach (FingerType ft in m_AllFingers)
                {
                    if (m_FingerMat[ft].fingerFX != null)
                    {
                        StopCoroutine(m_FingerMat[ft].fingerFX);
                    }

                    m_FingerMat[ft].matInstance.SetVector("_TriggerPos", new Vector4(
                        999, 999f, 999f, 0));
                }
            }
        }

        void ResetAfterImage()
        {
            m_AfterImage.gameObject.SetActive(false);
            
            if (m_FingerMat.Count != 0)
            {
                foreach (FingerType ft in m_AllFingers)
                {
                    if (m_FingerMat[ft].fingerFX != null)
                    {
                        StopCoroutine(m_FingerMat[ft].fingerFX);
                    }

                    m_FingerMat[ft].aiMatInstance.SetFloat("_TriggerHighlightPower", 0);
                }
            }
        }

        IEnumerator AfterImageEffectProcess(FingerVfx fv)
        {
            m_AfterImage.gameObject.SetActive(true);
            
            float waitTime = .35f;
            float elapsedTime = 0f;

            float d = 10;
            fv.aiMatInstance.SetFloat("_TriggerHighlightPower", d);
            
            while (elapsedTime < waitTime)
            {
                d = Mathf.Lerp(d, 0, 0.2f);
                fv.aiMatInstance.SetFloat("_TriggerHighlightPower", d);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            fv.aiMatInstance.SetFloat("_TriggerHighlightPower", 0);
            m_AfterImage.gameObject.SetActive(false);
            yield return null;
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
