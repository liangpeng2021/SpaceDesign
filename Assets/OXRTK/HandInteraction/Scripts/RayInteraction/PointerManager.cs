using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for pointer interaction management. <br>
    /// 管理近场远场交互的类。
    /// </summary>
    public class PointerManager : MonoBehaviour
    {
        struct InteractionInfo
        {
            public HandInteractionType interactionType;
            public InteractionPriority priority;
            public InteractionInfo(HandInteractionType t, InteractionPriority p)
            {
                interactionType = t;
                priority = p;
            }
        }

        public static PointerManager instance = null;

        public Action<bool> onHandMenuChanged;
        GameObject m_LeftHand, m_RightHand;
        
        List<InteractionInfo> m_LeftAvailableTypes = new List<InteractionInfo>();
        List<InteractionInfo> m_RightAvailableTypes = new List<InteractionInfo>();

        Dictionary<HandTrackingPlugin.HandType, InteractionSetting> m_HandSettings = new Dictionary<HandTrackingPlugin.HandType, InteractionSetting>();

        bool m_IsActive = false;

        HandMenuLocator[] m_HandMenus;
        int m_HandMenuOpened = 0;
        bool m_IsHandMenuOpened = false;

        #region IKControl
        [SerializeField]
        GameObject m_PosePrefab;
        [SerializeField]
        GameObject m_AvatarPrefab;

        GameObject m_PoseManager;
        GameObject m_Avatar;
        #endregion
        
        /// <summary>
        /// The parameter used to set the smoothen level of the ray interaction. <br>
        /// 用以设置射线交互平滑程度。
        /// </summary>
        public SmoothLevel smoothLevel = SmoothLevel.high;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                // Initiate IK control
                if (m_PosePrefab && m_AvatarPrefab)
                {
                    m_PoseManager = Instantiate(m_PosePrefab, transform);
                    m_Avatar = Instantiate(m_AvatarPrefab, transform);
                }
            }

            StartCoroutine(CheckInteractionSetting());
        }

        IEnumerator CheckInteractionSetting()
        {
            if (GameObject.FindObjectOfType<InteractionTypeController>() == null)
            {
                if (PointerManager.instance != null && PointerManager.instance.CheckActive())
                {
                    PointerManager.instance.ResetInteractionTypeList();
                    yield break;
                }
                else
                {
                    yield return new WaitUntil(() => PointerManager.instance != null);
                    yield return new WaitUntil(() => PointerManager.instance.CheckActive());
                    PointerManager.instance.ResetInteractionTypeList();
                }
            }
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => HandTrackingPlugin.instance != null);
            yield return new WaitUntil(() => HandTrackingPlugin.instance.leftHandController != null);
            m_LeftHand = HandTrackingPlugin.instance.leftHandController.gameObject;
            yield return new WaitUntil(() => HandTrackingPlugin.instance.rightHandController != null);
            m_RightHand = HandTrackingPlugin.instance.rightHandController.gameObject;
            //ResetInteractionTypeList();

            m_IsActive = true;
            onHandMenuChanged += WavePauseControl;
        }

        void SwitchIK(bool onOff)
        {
            m_PoseManager.SetActive(onOff);
            m_Avatar.SetActive(onOff);
        }

        /// <summary>
        /// Checks whether the script is intialized (script initialization is finished). <br>
        /// 检查当前脚本是否初始化完成。
        /// </summary>
        public bool CheckActive()
        {
            return m_IsActive;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitHandMenuList();
        }

        void InitHandMenuList()
        {
            m_HandMenus = FindObjectsOfType<HandMenuLocator>();
            for (int i = 0; i < m_HandMenus.Length; i++)
            {
                m_HandMenus[i].onMenuStatusChange += SetHandMenuNumber;
            }
            m_HandMenuOpened = 0;
            if (m_IsHandMenuOpened)
            {
                m_IsHandMenuOpened = false;
                onHandMenuChanged?.Invoke(m_IsHandMenuOpened);
            }
        }

        void SetHandMenuNumber(bool status)
        {
            if (status)
            {
                if (m_HandMenuOpened == 0)
                {
                    m_IsHandMenuOpened = true;
                    onHandMenuChanged?.Invoke(m_IsHandMenuOpened);
                }
                m_HandMenuOpened += 1;
            }
            else if (m_HandMenuOpened > 0)
            {
                m_HandMenuOpened -= 1;
                if (m_HandMenuOpened == 0)
                {
                    m_IsHandMenuOpened = false;
                    onHandMenuChanged?.Invoke(m_IsHandMenuOpened);
                }
            }
        }

        /// <summary>
        /// Checks pointer's priority level and Update higher priority interaction. <br>
        /// 检查当前交互优先级以及更新高优先级的交互。<br>
        /// Only used for hand ray interaction and hand pressable interaction for now. <br>
        /// 当前只用于手部远端射线交互与近端按压交互。
        /// </summary>
        /// <param name="hand">The hand in current interaction(left hand/right hand) <br>对应操作手（左手/右手）.</param>
        /// <param name="type">Current interaction type<br>对应交互类型.</param>
        /// <param name="priority">Current interaction priority<br>对应交互优先级.</param>
        /// <param name="isExitInteraction">Exit or start the interaction. <br>退出或进入当前交互.</param>
        /// <returns>
        /// -1: lower priority than current interaction. <br>-1: 待判断交互优先级低于当前有效最高优先级的交互<br>
        /// 0: same priority with current interaction. <br>0: 待判断交互优先级等于当前有效最高优先级的交互<br>
        /// 1: higher priority than current interaction. <br>1: 待判断交互优先级高于当前有效最高优先级的交互
        /// </returns>
        public int UpdatePriorityInteraction(HandTrackingPlugin.HandType hand, HandInteractionType type, InteractionPriority priority, bool isExitInteraction = false)
        {
            if (!m_IsActive)
                return -1;

            InteractionInfo info = new InteractionInfo(type, priority);
            int res = -100;

            switch (hand)
            {
                case HandTrackingPlugin.HandType.LeftHand:
                    if (isExitInteraction)
                    {
                        if (m_LeftAvailableTypes.Count > 0 && m_LeftAvailableTypes.Contains(info))
                        {
                            if (m_LeftAvailableTypes[0].Equals(info) && m_LeftAvailableTypes.Count > 1)
                            {
                                UpdatePriority(hand, m_LeftAvailableTypes[1].interactionType, true);
                            }
                            UpdatePriority(hand, type, false);
                            m_LeftAvailableTypes.Remove(info);
                        }
                    }
                    else
                    {
                        if (m_LeftAvailableTypes.Count > 0)
                        {
                            if (!m_LeftAvailableTypes.Contains(info))
                            {
                                int listNum = m_LeftAvailableTypes.Count;
                                for (int i = 0; i < m_LeftAvailableTypes.Count; i++)
                                {
                                    if (priority < m_LeftAvailableTypes[i].priority)
                                    {
                                        if (i == 0)
                                        {
                                            UpdatePriority(hand, m_LeftAvailableTypes[0].interactionType, false);
                                            UpdatePriority(hand, type, true);
                                            res = (int)Mathf.Max(1, res);
                                        }
                                        else
                                        {
                                            UpdatePriority(hand, type, false);
                                            res = (int)Mathf.Max(-1, res);
                                        }
                                        m_LeftAvailableTypes.Insert(i, info);
                                        break;
                                    } 
                                } 
                                if(priority == m_LeftAvailableTypes[0].priority)
                                {
                                    res = (int)Mathf.Max(0, res);
                                }
                                if (listNum == m_LeftAvailableTypes.Count)
                                {
                                    m_LeftAvailableTypes.Add(info);
                                    UpdatePriority(hand, type, true);
                                    res = (int)Mathf.Max(-1, res);
                                }
                            }
                            else
                            {
                                if (priority == m_LeftAvailableTypes[0].priority)
                                    res = 0;
                                else
                                    res = -1;
                            }
                        }
                        else
                        {
                            m_LeftAvailableTypes.Add(info);
                            UpdatePriority(hand, type, true);
                            res = (int)Mathf.Max(1, res);
                        }
                    }
                    break;
                case HandTrackingPlugin.HandType.RightHand:
                    if (isExitInteraction)
                    {
                        if (m_RightAvailableTypes.Count > 0 && m_RightAvailableTypes.Contains(info))
                        {
                            if (m_RightAvailableTypes[0].Equals(info) && m_RightAvailableTypes.Count > 1)
                            {
                                UpdatePriority(hand, m_RightAvailableTypes[1].interactionType, true);
                            }
                            UpdatePriority(hand, type, false);
                            m_RightAvailableTypes.Remove(info);
                        }
                    }
                    else
                    {
                        if (m_RightAvailableTypes.Count > 0)
                        {
                            if (!m_RightAvailableTypes.Contains(info))
                            {
                                int listNum = m_RightAvailableTypes.Count;
                                for (int i = 0; i < m_RightAvailableTypes.Count; i++)
                                {
                                    if (priority < m_RightAvailableTypes[i].priority)
                                    {
                                        if (i == 0)
                                        {
                                            UpdatePriority(hand, m_RightAvailableTypes[0].interactionType, false);
                                            UpdatePriority(hand, type, true);
                                            res = (int)Mathf.Max(1, res);
                                        }
                                        else
                                        {
                                            UpdatePriority(hand, type, false);
                                            res = (int)Mathf.Max(-1, res);
                                        }
                                        m_RightAvailableTypes.Insert(i, info);
                                        break;
                                    }
                                }
                                if (priority == m_RightAvailableTypes[0].priority)
                                {
                                    res = (int)Mathf.Max(0, res);
                                }
                                if (listNum == m_RightAvailableTypes.Count)
                                {
                                    m_RightAvailableTypes.Add(info);
                                    UpdatePriority(hand, type, true);
                                    res = (int)Mathf.Max(-1, res);
                                }
                            }
                            else
                            {
                                if (priority == m_RightAvailableTypes[0].priority)
                                    res = 0;
                                else
                                    res = -1;
                            }
                        }
                        else
                        {
                            m_RightAvailableTypes.Add(info);
                            UpdatePriority(hand, type, true);
                            res = (int)Mathf.Max(1, res);
                        }
                    }
                    break;
            }
            return res;
        }

        void UpdatePriority(HandTrackingPlugin.HandType hand, HandInteractionType type, bool newStatus)
        {
            switch (hand)
            {
                case HandTrackingPlugin.HandType.LeftHand:
                    switch (type)
                    {
                        case HandInteractionType.RayInteraction:
                            if (m_LeftHand.GetComponent<RayInteractionPointer>() != null)
                                m_LeftHand.GetComponent<RayInteractionPointer>().UpdatePriority(newStatus);
                            break;
                        case HandInteractionType.UiIneraction:
                            if (m_LeftHand.GetComponent<UiInteractionPointer>() != null)
                                m_LeftHand.GetComponent<UiInteractionPointer>().UpdatePriority(newStatus);
                            break;
                    }
                    break;
                case HandTrackingPlugin.HandType.RightHand:
                    switch (type)
                    {
                        case HandInteractionType.RayInteraction:
                            if (m_RightHand.GetComponent<RayInteractionPointer>() != null)
                                m_RightHand.GetComponent<RayInteractionPointer>().UpdatePriority(newStatus);
                            break;
                        case HandInteractionType.UiIneraction:
                            if (m_RightHand.GetComponent<UiInteractionPointer>() != null)
                                m_RightHand.GetComponent<UiInteractionPointer>().UpdatePriority(newStatus);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Force reset and disable all interaction. <br>
        /// 用来重置并关闭所有交互.
        /// </summary>
        public void ResetInteractionTypeList()
        {
            if (!m_IsActive)
            {
                return;
            }

            if (m_LeftHand.GetComponent<RayInteractionPointer>() != null)
            {
                m_LeftHand.GetComponent<RayInteractionPointer>().isEnabled = false;
            }
            if (m_RightHand.GetComponent<RayInteractionPointer>() != null)
            {
                m_RightHand.GetComponent<RayInteractionPointer>().isEnabled = false;
            }

            if ((m_LeftHand.GetComponent<RayInteractionPointer>() != null && !m_LeftHand.GetComponent<RayInteractionPointer>().enabled && m_RightHand.GetComponent<RayInteractionPointer>() != null && !m_RightHand.GetComponent<RayInteractionPointer>().enabled)
                || (m_LeftHand.GetComponent<RayInteractionPointer>() != null && !m_LeftHand.GetComponent<RayInteractionPointer>().enabled && m_RightHand.GetComponent<RayInteractionPointer>() == null)
                || (m_RightHand.GetComponent<RayInteractionPointer>() != null && !m_RightHand.GetComponent<RayInteractionPointer>().enabled && m_LeftHand.GetComponent<RayInteractionPointer>() == null)
                || (m_LeftHand.GetComponent<RayInteractionPointer>() == null && m_RightHand.GetComponent<RayInteractionPointer>() == null))
                SwitchIK(false);

            if (m_LeftHand.GetComponent<PhysicalInteractionHand>() != null)
            {
                m_LeftHand.GetComponent<PhysicalInteractionHand>().isEnabled = false;
            }
            if (m_RightHand.GetComponent<PhysicalInteractionHand>() != null)
            {
                m_RightHand.GetComponent<PhysicalInteractionHand>().isEnabled = false;
            }
                       
            if (m_LeftHand.GetComponent<UiInteractionPointer>() != null)
            {
                m_LeftHand.GetComponent<UiInteractionPointer>().isEnabled = false;
            }
            if (m_RightHand.GetComponent<UiInteractionPointer>() != null)
            {
                m_RightHand.GetComponent<UiInteractionPointer>().isEnabled = false;
            }

            if (LaserPointer.instance != null)
                LaserPointer.instance.Reset();

            m_LeftAvailableTypes.Clear();
            m_RightAvailableTypes.Clear();
        }

        /// <summary>
        /// Enables available interaction. <br>
        /// 用来打开可用交互.
        /// </summary>
        public void SetHandInteraction(HandTrackingPlugin.HandType handType, HandInteractionType InteractionType, bool onoff)
        {
            if (!m_IsActive)
                return;

            if ((handType == HandTrackingPlugin.HandType.LeftHand && m_LeftHand == null) ||
                (handType == HandTrackingPlugin.HandType.RightHand && m_RightHand == null))
            {
                Debug.LogError("Fail to set " + handType.ToString() + " interaction due to hand unavailable");
                return;
            }
            switch (InteractionType)
            {
                case HandInteractionType.RayInteraction:
                    if (onoff)
                    {
                        if (handType == HandTrackingPlugin.HandType.LeftHand &&
                            m_LeftHand.GetComponent<RayInteractionPointer>() != null)
                        {
                            m_LeftHand.GetComponent<RayInteractionPointer>().isEnabled = true;
                            SwitchIK(true);
                        }

                        if (handType == HandTrackingPlugin.HandType.RightHand &&
                            m_RightHand.GetComponent<RayInteractionPointer>() != null)
                        {
                            m_RightHand.GetComponent<RayInteractionPointer>().isEnabled = true;
                            SwitchIK(true);
                        }
                    }
                    else
                    {
                        if (handType == HandTrackingPlugin.HandType.LeftHand &&
                            m_LeftHand.GetComponent<RayInteractionPointer>() != null)
                        {
                            m_LeftHand.GetComponent<RayInteractionPointer>().isEnabled = false;
                        }

                        if (handType == HandTrackingPlugin.HandType.RightHand &&
                            m_RightHand.GetComponent<RayInteractionPointer>() != null)
                        {
                            m_RightHand.GetComponent<RayInteractionPointer>().isEnabled = false;
                        }
                    }
                    break;
                case HandInteractionType.PhysicalInteraction:
                    if (onoff)
                    {
                        if (handType == HandTrackingPlugin.HandType.LeftHand && m_LeftHand.GetComponent<PhysicalInteractionHand>() != null)
                            m_LeftHand.GetComponent<PhysicalInteractionHand>().isEnabled = true;
                        if (handType == HandTrackingPlugin.HandType.RightHand && m_RightHand.GetComponent<PhysicalInteractionHand>() != null)
                            m_RightHand.GetComponent<PhysicalInteractionHand>().isEnabled = true;
                    }
                    else
                    {
                        if (handType == HandTrackingPlugin.HandType.LeftHand && m_LeftHand.GetComponent<PhysicalInteractionHand>() != null)
                            m_LeftHand.GetComponent<PhysicalInteractionHand>().isEnabled = false;
                        if (handType == HandTrackingPlugin.HandType.RightHand && m_RightHand.GetComponent<PhysicalInteractionHand>() != null)
                            m_RightHand.GetComponent<PhysicalInteractionHand>().isEnabled = false;
                    }
                    break;
                case HandInteractionType.UiIneraction:
                    if (onoff)
                    {
                        if (handType == HandTrackingPlugin.HandType.LeftHand && m_LeftHand.GetComponent<UiInteractionPointer>() != null)
                        {
                            m_LeftHand.GetComponent<UiInteractionPointer>().isEnabled = true;
                        }
                        if (handType == HandTrackingPlugin.HandType.RightHand && m_RightHand.GetComponent<UiInteractionPointer>() != null)
                        {
                            m_RightHand.GetComponent<UiInteractionPointer>().isEnabled = true;
                        }
                    }
                    else
                    {
                        if (handType == HandTrackingPlugin.HandType.LeftHand && m_LeftHand.GetComponent<UiInteractionPointer>() != null)
                        {
                            m_LeftHand.GetComponent<UiInteractionPointer>().isEnabled = false;
                        }
                        if (handType == HandTrackingPlugin.HandType.RightHand && m_RightHand.GetComponent<UiInteractionPointer>() != null)
                        {
                            m_RightHand.GetComponent<UiInteractionPointer>().isEnabled = false;
                        }
                    }
                    break;
            }
            
            if ((m_LeftHand.GetComponent<RayInteractionPointer>() != null && !m_LeftHand.GetComponent<RayInteractionPointer>().enabled && m_RightHand.GetComponent<RayInteractionPointer>() != null && !m_RightHand.GetComponent<RayInteractionPointer>().enabled)
               || (m_LeftHand.GetComponent<RayInteractionPointer>() != null && !m_LeftHand.GetComponent<RayInteractionPointer>().enabled && m_RightHand.GetComponent<RayInteractionPointer>() == null)
               || (m_RightHand.GetComponent<RayInteractionPointer>() != null && !m_RightHand.GetComponent<RayInteractionPointer>().enabled && m_LeftHand.GetComponent<RayInteractionPointer>() == null)
               || (m_LeftHand.GetComponent<RayInteractionPointer>() == null && m_RightHand.GetComponent<RayInteractionPointer>() == null))
                    SwitchIK(false);
        }

        /// <summary>
        /// Update current scene interaction settings. <br>
        /// 更新当前场景设置.
        /// </summary>
        public void UpdateCurrentInteraction(InteractionSetting[] settings)
        {
            m_HandSettings.Clear();
            for(int i = 0; i < settings.Length; i++)
            {
                if (m_HandSettings.ContainsKey(settings[i].handType))
                {
                    m_HandSettings[settings[i].handType] = settings[i];
                }
                else
                {
                    m_HandSettings.Add(settings[i].handType, settings[i]);
                }
            }
        }

        /// <summary>
        /// Get current scene interaction settings. <br>
        /// 获取当前场景设置.
        /// </summary>
        public InteractionSetting GetHandInteraction(HandTrackingPlugin.HandType hand)
        {
            if (m_HandSettings.ContainsKey(hand))
            {
                return m_HandSettings[hand];
            }
            else
            {
                InteractionSetting emptySetting = new InteractionSetting(hand, true, false, false, false);
                return emptySetting;
            }
        }

        void WavePauseControl(bool status)
        {
            WaveInteraction.g_IsPause = status;
        }
    } 
}
