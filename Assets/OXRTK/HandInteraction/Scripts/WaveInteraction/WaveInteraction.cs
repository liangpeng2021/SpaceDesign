using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Base class for wave handle.<br>挥动控制的基本class.</br>
    /// </summary>
    public class WaveInteraction : MonoBehaviour
    {
        #region Enum, Class and Struct
        /// <summary>
        /// Event call back with int.<br>返回int的回调事件</br>
        /// </summary>
        [System.Serializable]        
        public class UnityEventInt : UnityEvent<int> { }

        /// <summary>
        /// Event call back with float.<br>返回float的回调事件</br>
        /// </summary>
        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float> { }

        /// <summary>
        /// Event call back with Vector2.<br>返回Vector2的回调事件</br>
        /// </summary>
        [System.Serializable]
        public class UnityEventVector2 : UnityEvent<Vector2> { }

        /// <summary>
        /// Event call back with Vector2Int.<br>返回Vector2Int的回调事件</br>
        /// </summary>
        [System.Serializable]
        public class UnityEventVector2Int : UnityEvent<Vector2Int> { }

        /// <summary>
        /// Event call back with Vector3.<br>返回Vector3的回调事件</br>
        /// </summary>
        [System.Serializable]
        public class UnityEventVector3 : UnityEvent<Vector3> { }

        /// General bool structure for 4 dir.
        [System.Serializable]
        protected struct DirectionBool
        {
            public bool Left;
            public bool Right;
            public bool Up;
            public bool Down;
            public DirectionBool(bool left, bool right, bool up, bool down) { this.Left = left; this.Right = right; this.Up = up; this.Down = down; }
        }

        /// Wave Direction
        protected enum Dir
        {
            Right, Left, Up, Down
        }

        #endregion Enum, Class and Struct



        #region Configuration

        [Tooltip("Which direction hand can wave.\n手可以触发挥动的方向.")]
        [SerializeField] DirectionBool m_WaveEnabled = new DirectionBool(true, true, false, false);

        [Tooltip("Minimal distance hand have to travel before wave can trigger, \nvalue is percentage of the interactive area size in 0-1." +
            "\n在触发挥动前手需要移动的最小距离,\n数值是用0-1表示的可交互区域的百分比.")]
        [SerializeField] protected Vector2 m_WaveDistance = new Vector2(0.3f, 0.4f);

        [Tooltip("Hand move velocity threshold that trigger wave, Normalized distance/second.\n手移动速度触发挥动触发阈值, 统一化的距离每秒.")]
        [SerializeField] Vector2 m_WaveVelocity = new Vector2(1.5f, 1f);

        [Tooltip("Cooldown time between wave.\n挥动间隔冷却时间.")]
        [SerializeField] float m_WaveCD = 1f;

        /// <summary>
        /// Set wave direction on the fly.<br>运行中设置可以挥动的方向.</br>
        /// </summary>
        /// <param name="left">Can wave left?<br>是否可以左挥.</br></param>
        /// <param name="right">Can wave right?<br>是否可以右挥.</br></param>
        /// <param name="up">Can wave up?<br>是否可以上挥.</br></param>
        /// <param name="down">Can wave down?<br>是否可以下挥.</br></param>
        public void SetEnabledWaveDirection(bool left, bool right, bool up, bool down)
        {
            m_WaveEnabled.Left = left;
            m_WaveEnabled.Right = right;
            m_WaveEnabled.Up = up;
            m_WaveEnabled.Down = down;
        }

        #endregion Configuration

        [Space(10)]

        #region Public Events
        [Space(5)]
        /// <summary>
        /// Event: When hand enter collider.<br>事件:当手进入collider.</br>
        /// </summary>
        public UnityEventInt g_OnHandEnter;

        [Space(5)]
        /// <summary>
        /// Event: When hand exit collider.<br>事件:当手离开collider.</br>
        /// </summary>
        public UnityEvent g_OnHandExit;

        [Space(5)]
        /// <summary>
        /// Event: Hand move delta, value is normalized value to the collider size.<br>事件:手移动的增量值,返回值是相对于collider大小归一化的.</br>
        /// </summary>
        public UnityEventVector2 g_OnHandMove;

        [Space(5)]
        /// <summary>
        /// Event: Hand relative position, value is normalized value to the collider size.<br>事件:手的位置,返回值是相对于collider大小归一化的.</br>
        /// </summary>
        public UnityEventVector2 g_OnHandPos;

        [Space(5)]
        /// <summary>
        /// Event: When hand wave while in the collider.<br>事件:当手在collider内挥动.</br>
        /// </summary>
        public UnityEventVector2Int g_OnHandWave;

        [Space(5)]
        /// <summary>
        /// Event: When hand wave in cooldown.<br>事件:当手挥动冷却.</br>
        /// </summary>
        public UnityEventFloat g_OnHandWaveCD;

        #endregion Public Events


        /// <summary>
        /// Pause wave handles.<br>暂停挥动接收.</br>
        /// </summary>
        public static bool g_IsPause = false;

        #region Internal Var

        //Only one hande can be active and interacte at same time. First come first active. Others ignore.
        protected BaseHand m_ActiveHand = null;
        protected BoxCollider m_Collider;
        protected bool m_WaveLock = false;

        #endregion Internal Var



        protected virtual void Start()
        {
            m_Collider = GetComponent<BoxCollider>();
            if (m_Collider == null)
            {
                Debug.LogError("WaveTouchHandle BoxCollider Missing, self destroy.");
                Destroy(this);
                return;
            }
        }



        #region Wave Logic

        protected Vector2 m_InteractiveAreaSize = Vector2.zero;
        Vector2 m_PrevPos = Vector2.zero;
        Vector2 m_CenterPos = Vector2.zero;
        Vector2 m_TraveledDistance = Vector2.zero;
        Vector2 m_TraveledTime = Vector2.zero;
        DirectionBool m_WaveReady = new DirectionBool(false, false, false, false);
        DirectionBool m_WaveVelocityReach = new DirectionBool(false, false, false, false);
        bool m_IsWaveCD = false;
        float m_WaveActiveTime = 0f;

        protected virtual void WaveMainLogic()
        {
            Vector2 currPos = GetHandPos();
            //Current frame move total
            Vector2 delta = currPos - m_PrevPos;
            m_PrevPos = currPos;

            //Calculate delta relative to area size, normalized delta， 0-1 of the relative interative area size.
            delta.x /= m_InteractiveAreaSize.x;
            delta.y /= m_InteractiveAreaSize.y;
            g_OnHandMove?.Invoke(delta);

            currPos.x /= m_InteractiveAreaSize.x;
            currPos.y /= m_InteractiveAreaSize.y;
            g_OnHandPos?.Invoke(currPos);

            if (m_IsWaveCD)
            {
                if (Time.time >= m_WaveActiveTime)
                    m_IsWaveCD = false;
            }
            else
            {
                if (!m_WaveLock)
                {
                    CheckWaveBaseOnDistanceAndVelocity(delta);
                }
            }
            CheckWaveReadyDirection(delta);
        }

        void CheckWaveBaseOnDistanceAndVelocity(Vector2 delta)
        {
            m_TraveledDistance += delta;
            m_TraveledTime.x += Time.deltaTime;
            m_TraveledTime.y += Time.deltaTime;

            //LogDebug(delta);

            if (m_WaveEnabled.Right && m_TraveledDistance.x > m_WaveDistance.x)
            {
                if (Mathf.Abs(m_TraveledDistance.x) / m_TraveledTime.x > m_WaveVelocity.x)
                    Wave(Dir.Right);
            }
            else if (m_WaveEnabled.Left && m_TraveledDistance.x < -m_WaveDistance.x)
            {
                if (Mathf.Abs(m_TraveledDistance.x) / m_TraveledTime.x > m_WaveVelocity.x)
                    Wave(Dir.Left);
            }

            if (m_WaveEnabled.Up && m_TraveledDistance.y > m_WaveDistance.y)
            {
                if (Mathf.Abs(m_TraveledDistance.y) / m_TraveledTime.y > m_WaveVelocity.y)
                    Wave(Dir.Up);
            }
            else if (m_WaveEnabled.Down && m_TraveledDistance.y < -m_WaveDistance.y)
            {
                if (Mathf.Abs(m_TraveledDistance.y) / m_TraveledTime.y > m_WaveVelocity.y)
                    Wave(Dir.Down);
            }
        }

        void Wave(Dir dir)
        {
            ResetWave();
            switch (dir)
            {
                case Dir.Left:
                    WaveAction(new Vector2Int(-1, 0));
                    break;
                case Dir.Right:
                    WaveAction(new Vector2Int(1, 0));
                    break;
                case Dir.Up:
                    WaveAction(new Vector2Int(0, 1));
                    break;
                case Dir.Down:
                    WaveAction(new Vector2Int(0, -1));
                    break;
            }
        }

        void WaveAction(Vector2Int dir)
        {
            m_IsWaveCD = true;
            m_WaveActiveTime = Time.time + m_WaveCD;
            g_OnHandWave?.Invoke(dir);
            g_OnHandWaveCD?.Invoke(m_WaveCD);
        }

        protected virtual void InitHandPos()
        {
            m_PrevPos = GetHandPos();
        }
        protected virtual Vector2 GetHandPos()
        {
            Vector3 result = transform.InverseTransformPoint(m_ActiveHand.joints[12].transform.position);
            return result;
        }

        void CheckWaveReadyDirection(Vector2 delta)
        {
            if (m_PrevPos.x < m_CenterPos.x)
            {
                if (delta.x > 0) ResetWaveReadyDirection(Dir.Left);
                m_WaveReady.Right = true;
            }
            else if (m_PrevPos.x > m_CenterPos.x)
            {
                if (delta.x < 0) ResetWaveReadyDirection(Dir.Right);
                m_WaveReady.Left = true;
            }

            if (m_PrevPos.y < m_CenterPos.y)
            {
                if (delta.y > 0) ResetWaveReadyDirection(Dir.Down);
                m_WaveReady.Up = true;
            }
            else if (m_PrevPos.y > m_CenterPos.y)
            {
                if (delta.y < 0) ResetWaveReadyDirection(Dir.Up);
                m_WaveReady.Down = true;
            }
        }

        protected void ResetWave()
        {
            m_TraveledDistance = Vector2.zero;
            m_TraveledTime = Vector2.zero;
            m_WaveReady.Left = false;
            m_WaveReady.Right = false;
            m_WaveReady.Up = false;
            m_WaveReady.Down = false;
            m_WaveVelocityReach.Left = false;
            m_WaveVelocityReach.Right = false;
            m_WaveVelocityReach.Up = false;
            m_WaveVelocityReach.Down = false;
        }

        void ResetWaveReadyDirection(Dir dir)
        {
            switch (dir)
            {
                case Dir.Left:
                    m_WaveReady.Left = false;
                    m_WaveVelocityReach.Left = false;
                    m_TraveledDistance.x = 0;
                    m_TraveledTime.x = 0;
                    break;
                case Dir.Right:
                    m_WaveReady.Right = false;
                    m_WaveVelocityReach.Right = false;
                    m_TraveledDistance.x = 0;
                    m_TraveledTime.x = 0;
                    break;
                case Dir.Up:
                    m_WaveReady.Up = false;
                    m_WaveVelocityReach.Up = false;
                    m_TraveledDistance.y = 0;
                    m_TraveledTime.y = 0;
                    break;
                case Dir.Down:
                    m_WaveReady.Down = false;
                    m_WaveVelocityReach.Down = false;
                    m_TraveledDistance.y = 0;
                    m_TraveledTime.y = 0;
                    break;
            }
        }
        #endregion Wave Logic


        #region Debug
        //public TMPro.TMP_Text u_LogText;
        //void LogDebug(Vector2 delta)
        //{
        //    if (u_LogText)
        //        u_LogText.text = "Delta=" + delta.ToString("0.00") 
        //            + "\nDis: D=" + m_TraveledDistance.x.ToString("0.00") +" T="+m_TraveledTime.x.ToString("0.00")
        //            + "\nVel: Left=" + m_WaveVelocityReach.Left + " Right=" + m_WaveVelocityReach.Right
        //            + "\nCan: Left=" + m_WaveReady.Left + " Right=" + m_WaveReady.Right;


        //}
        #endregion Debug
    }
}
