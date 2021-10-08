using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using XR;
#if ARRemoteDebug
using OXRTK.ARRemoteDebug;
#endif
using OXRTK.Tool;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class to maintain all data output from algorithm.<br>
    /// 该类管理所有算法输出的数据。
    /// </summary>
    public class HandTrackingPlugin : MonoBehaviour
    {
        public static HandTrackingPlugin instance = null;
        public HandTrackingPlugin()
        {

        }

        readonly string m_OxrtkHandVersion = "0.2.4.2";

        /// <summary>
        /// The controller for right hand.<br>
        /// 右手控制器。
        /// </summary>
        public HandController rightHandController;

        /// <summary>
        /// The controller for left hand.<br>
        /// 左手控制器。
        /// </summary>
        public HandController leftHandController;

        /// <summary>
        /// Determines if prediction is enabled for right hand.<br>
        /// 决定是否为右手启用预测。
        /// </summary>
        public bool enableRightHandPrediction;

        /// <summary>
        /// Determines if prediction is enabled for left hand.<br>
        /// 决定是否为左手启用预测。
        /// </summary>
        public bool enableLeftHandPrediction;

        /// <summary>
        /// Determines if Remote Debug is enabled.<br>
        /// 决定是否启用远程调试。
        /// </summary>
        public bool enableRemoteDebug;

        /// <summary>
        /// Determines if Offline Test is enabled.<br>
        /// Offline testing is only for editor environment. It will be turned off on Android.<br>
        /// 决定是否启用离线调试。<br>
        /// 离线调试只是为editor环境设计。在Android这个模式会被关闭。
        /// </summary>
        public bool enableOfflineTest;

        /// <summary>
        /// The ID of enabled offline data. Notice that only 0~2 is accepted.<br>        
        /// 启用的离线数据的ID。注意只有0~2是可接受的。
        /// </summary>
        public int offlineDataId;

        bool m_IsHandTrackingStarted = false;

        /// <summary>
        /// The structure for all hand data output by algorithm.<br>
        /// 算法输出的所有手部数据的结构。
        /// </summary>
        /**          
         *  <br>                  
         *  
         *              The order of all joints.
         *              所有节点的顺序。
         *              
         *              *17 --- *18 --- *19 --- *20                     Thumb           拇指
         *            /
         *           /         
         *          *0 ------- *13 ---- *14 ---- *15 ---- *16           Index finger    食指
         *           \\\          
         *            \\ ----- *9 ----- *10 ----- *11 ----- *12         Middle finger   中指
         *             \\       
         *              \ ---- *5 ---- *6 ---- *7 ---- *8               Ring finger     无名指
         *               \ 
         *                *1 -- *2 -- *3 -- *4                          Pinky finger    小指
         *                         
         */

        /**         
         *
         *  <br>                  
         *  
         *              The order of all joints with DOF info. Finger tip does not have DOF.
         *              所有带有自由度信息的节点顺序。指尖没有自由度。
         *              
         *             *13 --- *14 --- *15 --- *                        Thumb           拇指
         *            /
         *           /         
         *          *0 ------- *1 ---- *2 ---- *3 ---- *                Index finger    食指
         *           \\\          
         *            \\ ----- *4 ----- *5 ----- *6 ----- *             Middle finger   中指
         *             \\       
         *              \ ---- *10 ---- *11 ---- *12 ---- *             Ring finger     无名指
         *               \ 
         *                *7 -- *8 -- *9 -- *                           Pinky finger    小指
         *                         
         */
        [StructLayout(LayoutKind.Sequential)]
        [System.Serializable]
        public struct HandInfo
        {
            /// <summary>
            /// The type of hand, left hand or right hand.<br>
            /// 手的类型，左手或者右手。
            /// </summary>
            public HandType handType;

            /// <summary>
            /// Whether a hand is detected.<br>
            /// 手是否被检测到。
            /// </summary>            
            public bool handDetected;

            /// <summary>
            /// The 3D coordinates of hand joints in meters. 63 float data in total. Data is in algorithm coordinates.<br>
            /// 手部节点的3D坐标，单位为米。总共63个浮点数据。数据是算法坐标系。
            /// </summary>            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
            public float[] pred3D;

            /// <summary>
            /// Static gesture (semantics).<br>
            /// 静态手势（语义）。
            /// </summary>            
            public StaticGesture staticGesture;

            /// <summary>
            /// Dynamic gesture (semantics).<br>
            /// 动态手势（语义）。
            /// </summary>
            public DynamicGesture dynamicGesture;

            /// <summary>
            /// The DOF data of joints in radian. 48 float data in total.<br>
            /// 手部节点的自由度信息，单位是弧度。总共48个浮点数据。
            /// </summary>            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public float[] dofData;
        }

        HandTracking.HandJointsAngles m_JointsAngles;

        /// <summary>
        /// The structure for all joints' local position and local rotation in Unity coordinate system.         
        /// 该结构储存所有Unity坐标系下的节点local position和local rotation信息。        
        /// </summary>
        struct JointData
        {
            /// <summary>
            /// The local position of joints. All local positions are relative to fisheye camera.<br>
            /// 节点的local position。所有local position数据都是相对于鱼眼相机而言。
            /// </summary>
            public Vector3[] localPositions;

            /// <summary>
            /// The local rotation of joints. Each local rotation is relative to its parent in architecture.<br>
            /// 节点的local rotation。每个local rotation都是相对于其结构中的母节点而言。
            /// </summary>
            public Quaternion[] localRotations;
        }
        JointData[] m_JointData = new JointData[2];
        JointData[] m_RawJointData = new JointData[2];
        JointData[] m_LastFrameRawJointData = new JointData[2];

        // The buffer to store right hand root joint's position.
        List<Vector3> m_RightHandBuffer;
        // The buffer to store left hand root joint's position.
        List<Vector3> m_LeftHandBuffer;

        /// <summary>
        /// The enum of hand types.<br>
        /// 手类型的枚举。
        /// </summary>
        public enum HandType
        {
            /// Right hand.<br>
            /// 右手。
            RightHand = 0,

            /// Left hand.<br>
            /// 左手。
            LeftHand = 1
        }

        /// <summary>
        /// The enum of static gestures(semantics).<br>
        /// 静态手势（语义）的枚举。
        /// </summary>
        public enum StaticGesture
        {
            /// No gesture detected.<br>
            /// 没有检测到手势。
            None = 0,

            /// Gesture One, extending index finger and keeping other fingers bent.<br>
            /// One手势，伸直食指，弯曲其他手指。
            One = 1,

            /*
            /// Gesture Five, extending all five fingers.<br>
            /// Five手势，伸直所有手指。
            Five = 2,
            */

            /// Gesture OK, doing pinch with thumb and index finger, extending other fingers.<br>
            /// OK手势，用拇指和食指捏住，伸直其他手指。
            OK = 3,

            /*
            /// Gesture Thumbs Up, extending thumb finger and keeping other fingers bent.<br>
            /// Thumbs Up手势，伸直拇指，弯曲其他手指。
            ThumbsUp = 4,
            /// Gesture Fist, bending all fingers.<br>
            /// Fist手势，弯曲所有手指。
            Fist = 5,            
            */
        }

        /// <summary>
        /// The enum of dynamic gestures(semantics).<br>
        /// 动态手势（语义）的枚举。
        /// </summary>
        public enum DynamicGesture
        {
            /// No gesture detected.<br>
            /// 没有检测到手势。
            None = 0,

            //Confirm = 1,
            /// Gesture LeftWave, extending all five fingers and keeping palm facing forward then move hand from right to left.<br>
            /// 左挥手势，张开五指，掌心朝前，手从右移到左。
            LeftWave = 2,

            /// Gesture RightWave, extending all five fingers and keeping palm facing backward then move hand from left to right.<br>
            /// 右挥手势，张开五指，掌心朝后，手从左移到右。
            RightWave = 3,

            //Exit = 4,
            //ZoomIn = 5,
            //ZoomOut = 6,

            /// Gesture Blossom, keeping palm facing backward and making a fist at first then extend all five fingers.<br>
            /// 开花手势，保持掌心向内并握拳，然后张开五根手指。
            Blossom = 7,

            //TouchClick = 8
        }

        /// <summary>
        /// The enum of orientations.<br>
        /// 方向的枚举。
        /// </summary>
        public enum Orientation
        {
            /// Left.<br>
            /// 左。
            Left = 0,
            /// Right.<br>
            /// 右。
            Right = 1,
            /// Forward.<br>
            /// 前。
            Forward = 2,
            /// Backward.<br>
            /// 后。
            Backward = 3,
            /// Up.<br>
            /// 上。
            Up = 4,
            /// Down.<br>
            /// 下。
            Down = 5,
            /// Unknown.<br>
            /// 未知。
            Unknown = -1
        }

        // 0 right hand, 1 left hand
        HandInfo[] m_HandInfo = new HandInfo[2];
        int[] m_HideHandCounter = new int[2] { 0, 0 };
        // Hand will be hid after not detected for some frames. This is only valid when hand be shown at least once.
        int m_HideHandThreshold = 3;
        bool[] m_HandBeShownOnce = new bool[] { false, false };

        /// <summary>
        /// Data of right hand.<br>
        /// 右手的数据。
        /// </summary>
        public HandInfo rightHandInfo
        {
            get
            {
                return m_HandInfo[0];
            }
        }

        /// <summary>
        /// Data of left hand.<br>
        /// 左手的数据。
        /// </summary>
        public HandInfo leftHandInfo
        {
            get
            {
                return m_HandInfo[1];
            }
        }

        // To receive data from OppoXR Unity SDK.
        HandTracking.HandInfo m_HandTrackingInfo;

        Thread m_HandThread;
        int m_HandThreadLock = 0;
        bool m_HandUpdating = false;
        bool m_IsHandTrackingCorrect = false;
        bool m_IsInitialized = false;
        bool m_IsPaused = false;

        /// <summary>
        /// The smooth filtering added to hand data. Filtering is stronger when the value is bigger. Default value is 0.65. Range is 0 ~ 0.9.<br>
        /// 添加到手部数据的平滑滤波。数值越大，滤波越强。默认数值是0.65。范围是0 ~ 0.9。
        /// </summary>
        [Range(0.0f, 0.9f)]
        private float m_SmoothFilter = 0.75f;
        bool m_LastFrameRightHandDetected = false;
        bool m_LastFrameLeftHandDetected = false;
        JointData[] m_LastFrameJointData = new JointData[2];

        /// <summary>
        /// Notices that hand data is updated.<br>
        /// 通知手部数据发生了更新。
        /// </summary>
        public Action onHandDataUpdated;

        // Offline data, only support right hand
        float[][] m_OfflinePred3DRight = new float[3][];
        float[][] m_OfflineDofRight = new float[3][];

        ReadGaborCalib m_CalibReader;

        /// <summary>
        /// The number to determine printing level.<br>
        /// On Android, set it with adb command like this "adb shell setprop oxrtk.hand.debug 0".<br>
        /// Default level is 0, only critical information will be printed. Set it to 1 will print more debugging information.<br>
        /// 决定打印级别的数字。<br>
        /// 在Android，使用adb指令来设置，例如"adb shell setprop oxrtk.hand.debug 0"。<br>
        /// 默认级别是0，仅打印重要信息。设置为1将会打印更多调试信息。
        /// </summary>
        public static int debugLevel = 0;

        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Debug.Log("OXRTK Hand version: " + m_OxrtkHandVersion);
                instance = this;
                if (rightHandController.handType != HandType.RightHand || leftHandController.handType != HandType.LeftHand)
                {
                    Debug.LogError("LeftHandController or rightHandController is not set correctly!!!");
                }

                if (Application.platform == RuntimePlatform.Android)
                {
                    debugLevel = ToolLibraryDll.GetOxrtkDebugLevel(0);
                    Debug.Log("OXRTK Hand Debug Level: " + debugLevel);

                    m_CalibReader = new ReadGaborCalib();
                    ReadCalibAndSetHand();
                }

                m_HandInfo[0].handType = HandType.RightHand;
                m_HandInfo[1].handType = HandType.LeftHand;
                DontDestroyOnLoad(gameObject);

                m_IsInitialized = false;

                // Add a AudioListener in Unity Editor
                if (Application.platform != RuntimePlatform.Android && !GetComponent<AudioListener>())
                    gameObject.AddComponent<AudioListener>();
            }

        }

        void ReadCalibAndSetHand()
        {
            m_CalibReader.SetRightFisheyeTransform(rightHandController.transform);
            m_CalibReader.SetLeftFisheyeTransform(leftHandController.transform);
            m_CalibReader.ApplyCalibrationDataForFisheye();
            if (debugLevel > 0)
            {
                Debug.Log("Calibration data applied to hand tracking.");
            }
        }

        void Start()
        {
            Init();

            if (enableRemoteDebug)
            {
                InitRemoteDebug();
            }

            if (enableOfflineTest)
            {
                m_OfflinePred3DRight[0] = new float[] { 0.03738001f, 0.199273f, 0.3518906f, 0.01910841f, 0.153428f, 0.4172456f, 0.001942474f, 0.1502346f, 0.4290337f, -0.01565555f, 0.1563107f, 0.4256646f, -0.02761797f, 0.1650215f, 0.4110732f, 0.01788059f, 0.1348879f, 0.4057102f, -0.00659981f, 0.1242006f, 0.416451f, -0.02354817f, 0.1405539f, 0.4088766f, -0.02910886f, 0.1577754f, 0.3915311f, 0.01194448f, 0.1153758f, 0.3882376f, -0.00974049f, 0.1012655f, 0.4065422f, -0.02850949f, 0.1145329f, 0.4031393f, -0.0440511f, 0.1312464f, 0.3919648f, 0.005048651f, 0.1151606f, 0.3639294f, -0.007226113f, 0.08487177f, 0.3686217f, -0.02246674f, 0.06989331f, 0.3746228f, -0.04134275f, 0.05445133f, 0.381866f, 0.01094059f, 0.1746752f, 0.3379139f, -0.01367943f, 0.1560669f, 0.3371065f, -0.03076088f, 0.1355484f, 0.3418638f, -0.05480645f, 0.1229205f, 0.3626241f };
                m_OfflinePred3DRight[1] = new float[] { 0.05331478f, 0.1591405f, 0.3865468f, -0.02170831f, 0.1551379f, 0.4191414f, -0.0310388f, 0.1428545f, 0.4047927f, -0.01968475f, 0.1386132f, 0.390265f, -0.0006959736f, 0.1426039f, 0.3900101f, -0.01683941f, 0.1380438f, 0.4318846f, -0.03108168f, 0.1205674f, 0.4139793f, -0.01654957f, 0.120114f, 0.3939629f, 0.006256066f, 0.1307421f, 0.3937511f, -0.0093509f, 0.1142947f, 0.4419423f, -0.02702898f, 0.1034446f, 0.4179808f, -0.01644155f, 0.1050344f, 0.397359f, 0.006566703f, 0.1114586f, 0.390164f, 0.008063056f, 0.0972114f, 0.4353545f, -0.01709542f, 0.07842787f, 0.4251402f, -0.01072016f, 0.08592609f, 0.4052461f, 0.005304489f, 0.09360301f, 0.3886327f, 0.04702151f, 0.1209976f, 0.3887718f, 0.04566145f, 0.09238198f, 0.4002766f, 0.04209643f, 0.06620774f, 0.4064098f, 0.02994924f, 0.03397945f, 0.4163462f };
                m_OfflinePred3DRight[2] = new float[] { 0.03108577f, 0.1490634f, 0.2951075f, 0.006160118f, 0.08963519f, 0.3456437f, -0.002493672f, 0.07331035f, 0.3557649f, -0.01545774f, 0.06561357f, 0.3671952f, -0.03170396f, 0.05970369f, 0.3751075f, -0.005735252f, 0.07957676f, 0.3302933f, -0.02363072f, 0.05798788f, 0.3368161f, -0.04306363f, 0.04943796f, 0.3495169f, -0.06551018f, 0.04912791f, 0.3614212f, -0.02326826f, 0.07292105f, 0.3110696f, -0.04876877f, 0.05822065f, 0.3228179f, -0.06878173f, 0.05656101f, 0.334506f, -0.0919231f, 0.05789749f, 0.3459381f, -0.03389546f, 0.08556087f, 0.2919441f, -0.06529784f, 0.08263804f, 0.301714f, -0.06598903f, 0.1020008f, 0.3125421f, -0.06554668f, 0.1253559f, 0.319255f, -0.005878162f, 0.1440122f, 0.2847347f, -0.03631672f, 0.1412356f, 0.2890764f, -0.06027254f, 0.1298344f, 0.2946932f, -0.07610588f, 0.1263498f, 0.323638f };

                m_OfflineDofRight[0] = new float[] { 2.396988f, 1.761166f, -0.5338452f, 0.1091887f, 0.1540372f, 0.183939f, -0.04332268f, -0.007739488f, 0.233902f, 0.04353679f, 0.003360846f, 0.1914348f, -0.2298538f, -0.1561632f, 0.6249911f, -0.1669132f, -0.03725532f, 1.233339f, -0.02852678f, 0.116727f, 0.5250631f, -0.5070258f, 0.1027572f, 0.7436334f, -0.386479f, 0.1882098f, 0.7610379f, -0.5514225f, 0.1370375f, 0.3372689f, -0.2390147f, 0.04068819f, 0.8565266f, -0.4054487f, 0.1109012f, 1.144582f, -0.2058551f, 0.2052835f, 0.5856497f, 0.8864787f, -0.1985791f, 0.05968224f, -0.5079826f, -0.04234691f, 0.05253753f, 0.6278334f, -0.2553582f, 0.3116001f };
                m_OfflineDofRight[1] = new float[] { 1.234967f, 0.9514265f, 0.02619695f, -0.09411046f, -0.1563383f, 1.07538f, 0.3726141f, 0.004483572f, 1.52662f, -0.1301612f, 0.2933269f, 0.4084629f, -0.3178003f, -0.1479801f, 1.503801f, -0.1559918f, -0.006312136f, 1.043675f, -0.08118708f, 0.1146128f, 0.9625377f, -0.7373257f, 0.5072528f, 1.270244f, -0.7193689f, 0.2558152f, 0.8314595f, -0.5701577f, 0.2132573f, 0.6902053f, -0.2084363f, 0.3023883f, 1.381251f, -0.6642267f, 0.3139013f, 1.077954f, -0.3485374f, 0.01941379f, 0.9965396f, 0.4175389f, -0.08707554f, -0.3057607f, -0.5886133f, 0.236184f, 0.4677662f, 0.5012727f, -0.2035993f, -0.2214756f };
                m_OfflineDofRight[2] = new float[] { 3.161619f, 1.541642f, -0.6668231f, 0.04007807f, 0.1685098f, 0.9350526f, 0.3406281f, 0.1190794f, 1.292031f, -0.30247f, 0.2239193f, 0.1774009f, 0.0297053f, 0.1763048f, 0.5408337f, -0.1783253f, -9.96697E-05f, 0.2830801f, -0.04618171f, 0.07440365f, 0.2987705f, 0.1001395f, 0.2571967f, 0.1620641f, -0.5044183f, 0.007840077f, 0.1860926f, 0.0133178f, 0.1844217f, 0.1212714f, 0.1286108f, 0.1328091f, 0.1931707f, -0.2875761f, 0.07580268f, 0.3272955f, -0.1995749f, 0.08869359f, 0.341292f, 0.871666f, -0.176671f, 0.2360372f, -0.3171333f, -0.05901188f, -0.1473105f, 0.6527475f, -0.2672208f, 0.6574141f };
            }
        }

        void Init()
        {
            InitDataStructure();

            if (Application.platform != RuntimePlatform.Android) return;

            enableOfflineTest = false;

            // Start hand tracking and get algorithm data in multi-threading.
            if (m_HandThread == null)
            {
                m_HandUpdating = true;
                m_HandThread = new Thread(new ThreadStart(ThreadUpdateHandData));
                m_HandThread.Start();
            }

            /*
            // Start hand tracking.
            HandTracking.SetGestureClassificationMode(0);
            if (!m_IsHandTrackingStarted)
            {
                HandTracking.Start();                
                m_IsHandTrackingStarted = true;
            }
            m_IsInitialized = true;
            */

            XRCameraManager.OnRenderPause += OnPause;
            XRManager.OnQuit += Release;
        }

        void InitDataStructure()
        {
            m_HandTrackingInfo = new HandTracking.HandInfo();
            m_JointsAngles = new HandTracking.HandJointsAngles();

            for (int i = 0; i < m_HandInfo.Length; i++)
            {
                m_HandInfo[i].pred3D = new float[63];
                m_HandInfo[i].dofData = new float[48];
            }

            for (int i = 0; i < m_JointData.Length; i++)
            {
                m_JointData[i].localPositions = new Vector3[21];
                m_JointData[i].localRotations = new Quaternion[21];

                m_LastFrameJointData[i].localPositions = new Vector3[21];
                m_LastFrameJointData[i].localRotations = new Quaternion[21];

                m_RawJointData[i].localPositions = new Vector3[21];
                m_RawJointData[i].localRotations = new Quaternion[21];

                m_LastFrameRawJointData[i].localPositions = new Vector3[21];
                m_LastFrameRawJointData[i].localRotations = new Quaternion[21];
            }
        }

        void InitRemoteDebug()
        {
#if ARRemoteDebug
            ARRemoteDebugWrapper.Init();

            GameObject arRemoteDebugWrapperObj = GameObject.Find("ARRemoteDebug.Conduit");
            arRemoteDebugWrapperObj.transform.SetParent(transform);

            if (Application.platform == RuntimePlatform.Android)
            {
                ARRemoteDebugWrapper.AddEditorDataListener(OnEditorDataReceived);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.LinuxEditor)
            {
                ARRemoteDebugWrapper.AddAndroidDataListener(OnAndroidDataReceived);
            }
#endif
        }

        void OnAndroidDataReceived(object data)
        {
            if (data is HandInfo[])
            {
                HandInfo[] info = (HandInfo[])data;

                m_HandInfo[0] = info[0];
                if (m_HandInfo[0].handDetected)
                {
                    for (int i = 0; i < m_RawJointData[0].localPositions.Length; i++)
                    {
                        m_RawJointData[0].localPositions[i] = new Vector3(m_HandInfo[0].pred3D[i * 3],
                                    -m_HandInfo[0].pred3D[i * 3 + 1], m_HandInfo[0].pred3D[i * 3 + 2]);
                    }
                    ApplyDofData(m_HandInfo[0].dofData, m_RawJointData[0].localRotations);
                }

                m_HandInfo[1] = info[1];
                if (m_HandInfo[1].handDetected)
                {
                    for (int i = 0; i < m_RawJointData[1].localPositions.Length; i++)
                    {
                        m_RawJointData[1].localPositions[i] = new Vector3(m_HandInfo[1].pred3D[i * 3],
                                    -m_HandInfo[1].pred3D[i * 3 + 1], m_HandInfo[1].pred3D[i * 3 + 2]);
                    }
                    ApplyDofData(m_HandInfo[1].dofData, m_RawJointData[1].localRotations);
                }
            }
        }

        void OnEditorDataReceived(object data)
        {

        }

        void Update()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (!m_IsHandTrackingStarted || m_IsPaused)
                {
                    return;
                }

                /*
                var resultOfGetHandInfo = HandTracking.GetHandInfo(ref m_HandTrackingInfo);
                var resultOfGetHandJointAngles = HandTracking.GetHandJointAngles(ref m_JointsAngles);
                */
                //if (resultOfGetHandInfo == XRError.XR_ERROR_SUCCESS && resultOfGetHandJointAngles == XRError.XR_ERROR_SUCCESS)
                if (debugLevel > 0)
                {
                    Debug.Log("updating, m_HandThreadLock " + m_HandThreadLock + ", m_IsHandTrackingCorrect " + m_IsHandTrackingCorrect);
                }

                if (m_HandThreadLock == 1)
                {
                    m_HandInfo[0].handDetected = m_HandTrackingInfo.right.handDetected;
                    if (m_IsHandTrackingCorrect && m_HandInfo[0].handDetected)
                    {
                        m_HandBeShownOnce[0] = true;
                        m_HideHandCounter[0] = 0;
                        for (int i = 0; i < m_HandTrackingInfo.right.pred3D.Length; i++)
                        {
                            m_HandInfo[0].pred3D[i * 3] = m_HandTrackingInfo.right.pred3D[i].x;
                            m_HandInfo[0].pred3D[i * 3 + 1] = m_HandTrackingInfo.right.pred3D[i].y;
                            m_HandInfo[0].pred3D[i * 3 + 2] = m_HandTrackingInfo.right.pred3D[i].z;

                            m_RawJointData[0].localPositions[i] = new Vector3(m_HandInfo[0].pred3D[i * 3],
                                -m_HandInfo[0].pred3D[i * 3 + 1], m_HandInfo[0].pred3D[i * 3 + 2]);
                        }
                        m_HandInfo[0].staticGesture = (StaticGesture)((int)m_HandTrackingInfo.right.staticGesture);
                        m_HandInfo[0].dynamicGesture = (DynamicGesture)((int)m_HandTrackingInfo.right.dynamicGesture);

                        for (int i = 0; i < m_JointsAngles.right.angles.Length; i++)
                        {
                            m_HandInfo[0].dofData[i * 3] = m_JointsAngles.right.angles[i].x;
                            m_HandInfo[0].dofData[i * 3 + 1] = m_JointsAngles.right.angles[i].y;
                            m_HandInfo[0].dofData[i * 3 + 2] = m_JointsAngles.right.angles[i].z;
                        }
                        ApplyDofData(m_HandInfo[0].dofData, m_RawJointData[0].localRotations);
                    }
                    else
                    {
                        if (m_HideHandCounter[0] < m_HideHandThreshold - 1 && m_HandBeShownOnce[0])
                        {
                            m_HideHandCounter[0]++;
                            m_HandInfo[0].handDetected = true;
                        }
                        else
                        {
                            m_HandInfo[0].staticGesture = StaticGesture.None;
                            m_HandInfo[0].dynamicGesture = DynamicGesture.None;
                        }
                    }

                    m_HandInfo[1].handDetected = m_HandTrackingInfo.left.handDetected;
                    if (m_IsHandTrackingCorrect && m_HandInfo[1].handDetected)
                    {
                        m_HandBeShownOnce[1] = true;
                        m_HideHandCounter[1] = 0;
                        for (int i = 0; i < m_HandTrackingInfo.left.pred3D.Length; i++)
                        {
                            m_HandInfo[1].pred3D[i * 3] = m_HandTrackingInfo.left.pred3D[i].x;
                            m_HandInfo[1].pred3D[i * 3 + 1] = m_HandTrackingInfo.left.pred3D[i].y;
                            m_HandInfo[1].pred3D[i * 3 + 2] = m_HandTrackingInfo.left.pred3D[i].z;

                            m_RawJointData[1].localPositions[i] = new Vector3(m_HandInfo[1].pred3D[i * 3],
                                -m_HandInfo[1].pred3D[i * 3 + 1], m_HandInfo[1].pred3D[i * 3 + 2]);
                        }
                        m_HandInfo[1].staticGesture = (StaticGesture)((int)m_HandTrackingInfo.left.staticGesture);
                        m_HandInfo[1].dynamicGesture = (DynamicGesture)((int)m_HandTrackingInfo.left.dynamicGesture);

                        for (int i = 0; i < m_JointsAngles.left.angles.Length; i++)
                        {
                            m_HandInfo[1].dofData[i * 3] = m_JointsAngles.left.angles[i].x;
                            m_HandInfo[1].dofData[i * 3 + 1] = m_JointsAngles.left.angles[i].y;
                            m_HandInfo[1].dofData[i * 3 + 2] = m_JointsAngles.left.angles[i].z;
                        }
                        ApplyDofData(m_HandInfo[1].dofData, m_RawJointData[1].localRotations);
                    }
                    else
                    {
                        if (m_HideHandCounter[1] < m_HideHandThreshold - 1 && m_HandBeShownOnce[1])
                        {
                            m_HideHandCounter[1]++;
                            m_HandInfo[1].handDetected = true;
                        }
                        else
                        {
                            m_HandInfo[1].staticGesture = StaticGesture.None;
                            m_HandInfo[1].dynamicGesture = DynamicGesture.None;
                        }
                    }

#if ARRemoteDebug
                    ARRemoteDebugWrapper.SendDataToEditor(m_HandInfo);
#endif                                                       
                    m_HandThreadLock = 0;

                }
            }
            else if (enableOfflineTest)
            {
                m_HandInfo[0].handDetected = true;
                offlineDataId = offlineDataId < 0 ? 0 : offlineDataId;
                offlineDataId = offlineDataId >= m_OfflineDofRight.Length ? m_OfflinePred3DRight.Length - 1 : offlineDataId;
                m_HandInfo[0].pred3D = m_OfflinePred3DRight[offlineDataId];
                m_HandInfo[0].dofData = m_OfflineDofRight[offlineDataId];

                for (int i = 0; i < m_RawJointData[0].localPositions.Length; i++)
                {
                    m_RawJointData[0].localPositions[i] = new Vector3(m_HandInfo[0].pred3D[i * 3],
                                -m_HandInfo[0].pred3D[i * 3 + 1], m_HandInfo[0].pred3D[i * 3 + 2]);
                }

                ApplyDofData(m_HandInfo[0].dofData, m_RawJointData[0].localRotations);
            }

            PostProcessHandData();
            onHandDataUpdated?.Invoke();
        }

        // Post processing contains filtering (smoothing), interpolation (from 30 fps to 60) and prediction.
        void PostProcessHandData()
        {
            // right hand filtering
            if (m_HandInfo[0].handDetected)
            {
                if (!m_LastFrameRightHandDetected)
                {
                    Array.Copy(m_RawJointData[0].localPositions, m_JointData[0].localPositions, m_RawJointData[0].localPositions.Length);
                    Array.Copy(m_RawJointData[0].localRotations, m_JointData[0].localRotations, m_RawJointData[0].localRotations.Length);
                }
                else
                {
                    for (int i = 0; i < m_JointData[0].localPositions.Length; i++)
                    {
                        m_JointData[0].localPositions[i] = Vector3.Lerp(m_LastFrameJointData[0].localPositions[i],
                            m_RawJointData[0].localPositions[i], 1 - m_SmoothFilter);
                        m_JointData[0].localRotations[i] = m_RawJointData[0].localRotations[i];
                    }
                }

                if (enableRightHandPrediction)// && rightHandController.activeHand.handVisualizationType == BaseHand.HandVisualizationType.Model)
                {
                    if (m_RightHandBuffer == null)
                    {
                        m_RightHandBuffer = new List<Vector3>();
                    }
                    m_RightHandBuffer.Add(m_RawJointData[0].localPositions[0]);
                    if (m_RightHandBuffer.Count == 20)
                    {
                        Vector3 deltaAvg = Vector3.zero;
                        for (int i = 10; i < m_RightHandBuffer.Count - 1; i++)
                        {
                            deltaAvg += (m_RightHandBuffer[i + 1] - m_RightHandBuffer[i]);
                        }
                        deltaAvg = deltaAvg / 9;
                        Vector3 predictedOffset = deltaAvg * 2.5f;
                        for (int i = 0; i < m_JointData[0].localPositions.Length; i++)
                        {
                            m_JointData[0].localPositions[i] += predictedOffset;
                        }
                        //m_JointData[0].localPositions[0] += deltaAvg * 2.5f;
                        m_RightHandBuffer.RemoveAt(0);
                    }
                }

                Array.Copy(m_RawJointData[0].localPositions, m_LastFrameRawJointData[0].localPositions, m_RawJointData[0].localPositions.Length);
                Array.Copy(m_RawJointData[0].localRotations, m_LastFrameRawJointData[0].localRotations, m_RawJointData[0].localRotations.Length);

                Array.Copy(m_JointData[0].localPositions, m_LastFrameJointData[0].localPositions, m_JointData[0].localPositions.Length);
                Array.Copy(m_JointData[0].localRotations, m_LastFrameJointData[0].localRotations, m_JointData[0].localRotations.Length);

                m_LastFrameRightHandDetected = true;
            }
            else
            {
                if (m_RightHandBuffer != null)
                {
                    m_RightHandBuffer.Clear();
                }
                m_LastFrameRightHandDetected = false;
            }

            // left hand filtering
            if (m_HandInfo[1].handDetected)
            {
                if (!m_LastFrameLeftHandDetected)
                {
                    Array.Copy(m_RawJointData[1].localPositions, m_JointData[1].localPositions, m_RawJointData[1].localPositions.Length);
                    Array.Copy(m_RawJointData[1].localRotations, m_JointData[1].localRotations, m_RawJointData[1].localRotations.Length);
                }
                else
                {
                    for (int i = 0; i < m_JointData[1].localPositions.Length; i++)
                    {
                        m_JointData[1].localPositions[i] = Vector3.Lerp(m_LastFrameJointData[1].localPositions[i],
                            m_RawJointData[1].localPositions[i], 1 - m_SmoothFilter);
                        m_JointData[1].localRotations[i] = m_RawJointData[1].localRotations[i];
                    }
                }

                if (enableLeftHandPrediction)// && leftHandController.activeHand.handVisualizationType == BaseHand.HandVisualizationType.Model)
                {
                    if (m_LeftHandBuffer == null)
                    {
                        m_LeftHandBuffer = new List<Vector3>();
                    }
                    m_LeftHandBuffer.Add(m_RawJointData[1].localPositions[0]);
                    if (m_LeftHandBuffer.Count == 20)
                    {
                        Vector3 deltaAvg = Vector3.zero;
                        for (int i = 10; i < m_LeftHandBuffer.Count - 1; i++)
                        {
                            deltaAvg += (m_LeftHandBuffer[i + 1] - m_LeftHandBuffer[i]);
                        }
                        deltaAvg = deltaAvg / 9;
                        Vector3 predictedOffset = deltaAvg * 2.5f;
                        for (int i = 0; i < m_JointData[1].localPositions.Length; i++)
                        {
                            m_JointData[1].localPositions[i] += predictedOffset;
                        }
                        //m_JointData[1].localPositions[0] += deltaAvg * 2.5f;
                        m_LeftHandBuffer.RemoveAt(0);
                    }
                }

                Array.Copy(m_RawJointData[1].localPositions, m_LastFrameRawJointData[1].localPositions, m_RawJointData[1].localPositions.Length);
                Array.Copy(m_RawJointData[1].localRotations, m_LastFrameRawJointData[1].localRotations, m_RawJointData[1].localRotations.Length);

                Array.Copy(m_JointData[1].localPositions, m_LastFrameJointData[1].localPositions, m_JointData[1].localPositions.Length);
                Array.Copy(m_JointData[1].localRotations, m_LastFrameJointData[1].localRotations, m_JointData[1].localRotations.Length);
                m_LastFrameLeftHandDetected = true;
            }
            else
            {
                if (m_LeftHandBuffer != null)
                {
                    m_LeftHandBuffer.Clear();
                }
                m_LastFrameLeftHandDetected = false;
            }
        }

        void ApplyDofData(float[] dofData, Quaternion[] localRots)
        {
            // rotation
            // root
            // This index is dof data's index.
            int index = 0;
            localRots[0] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2], true);

            // thumb
            index = 13;
            localRots[17] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 14;
            localRots[18] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 15;
            localRots[19] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            localRots[20] = Quaternion.identity;

            // index
            index = 1;
            localRots[13] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 2;
            localRots[14] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 3;
            localRots[15] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            localRots[16] = Quaternion.identity;

            // middle
            index = 4;
            localRots[9] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 5;
            localRots[10] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 6;
            localRots[11] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            localRots[12] = Quaternion.identity;

            // ring
            index = 10;
            localRots[5] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 11;
            localRots[6] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 12;
            localRots[7] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            localRots[8] = Quaternion.identity;

            // pinky
            index = 7;
            localRots[1] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 8;
            localRots[2] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            index = 9;
            localRots[3] = ConvertRotation(dofData[index * 3], dofData[index * 3 + 1], dofData[index * 3 + 2]);
            localRots[4] = Quaternion.identity;
        }

        // Converts from algorithm data (x, y, z) to Unity coordinate system data (rot)
        Quaternion ConvertRotation(float x, float y, float z, bool isRoot = false)
        {
            // Algorithm output DOF is radian.
            x *= Mathf.Rad2Deg;
            y *= Mathf.Rad2Deg;
            z *= Mathf.Rad2Deg;

            Vector3 vec = new Vector3(-x, -y, z);
            Quaternion rot = Quaternion.AngleAxis(vec.magnitude, vec.normalized);
            if (isRoot)
            {
                rot = Quaternion.AngleAxis(vec.magnitude, vec.normalized);
                rot = Quaternion.Euler(0, 180, 180) * rot;
            }
            return rot;
        }

        /// <summary>
        /// Gets local position of selected joint. Data is relative to fisheye camera.<br>
        /// 获取选定节点的local position。数据相对于鱼眼相机。
        /// </summary>
        /// <param name="type">Right hand or left hand.<br>右手或左手。</param>
        /// <param name="id">ID of selected joint.<br>选定节点的ID。</param>
        /// <returns>Local position of selected joint.<br>选定节点的local position。</returns>
        public Vector3 GetJointLocalPosition(HandType type, int id)
        {
            int index = type == HandType.RightHand ? 0 : 1;
            return m_JointData[index].localPositions[id];
        }

        /// <summary>
        /// Gets local position of selected joint from raw data. Data is relative to fisheye camera and not processed by filtering.<br>
        /// 从原始数据获取选定节点的local position。数据相对于鱼眼相机，并且没有经过滤波处理。
        /// </summary>
        /// <param name="type">Right hand or left hand.<br>右手或左手。</param>
        /// <param name="id">ID of selected joint.<br>选定节点的ID。</param>
        /// <returns>Local position of selected joint.<br>选定节点的local position。</returns>
        public Vector3 GetRawJointLocalPosition(HandType type, int id)
        {
            int index = type == HandType.RightHand ? 0 : 1;
            return m_RawJointData[index].localPositions[id];
        }

        /// <summary>
        /// Gets world position of selected joint.<br>
        /// 获取选定节点的world position。
        /// </summary>
        /// <param name="type">Right hand or left hand.<br>右手或左手。</param>
        /// <param name="id">ID of selected joint.<br>选定节点的ID。</param>
        /// <returns>World position of selected joint.<br>选定节点的world position。</returns>
        public Vector3 GetJointWorldPosition(HandType type, int id)
        {
            Vector3 pos = GetJointLocalPosition(type, id);
            if (type == HandType.RightHand)
            {
                pos = rightHandController.transform.position + rightHandController.transform.rotation * pos;
            }
            else
            {
                pos = leftHandController.transform.position + leftHandController.transform.rotation * pos;
            }
            return pos;
        }

        /// <summary>
        /// Gets local rotation of selected joint. Each local rotation is relative to its parent in architecture.<br>
        /// 获取选定节点的local rotation。每个local rotation都是相对于其结构中的母节点而言。
        /// </summary>
        /// <param name="type">Right hand or left hand.<br>右手或左手。</param>
        /// <param name="id">ID of selected joint.<br>选定节点的ID。</param>
        /// <returns>Local rotation of selected joint.<br>选定节点的local rotation。</returns>
        public Quaternion GetJointLocalRotation(HandType type, int id)
        {
            int index = type == HandType.RightHand ? 0 : 1;
            return m_JointData[index].localRotations[id];
        }

        /// <summary>
        /// Gets local rotation of selected joint from raw data. Data is not processed by filtering. Each local rotation is relative to its parent in architecture.<br>
        /// 从原始数据获取选定节点的local rotation。数据未经滤波处理。每个local rotation都是相对于其结构中的母节点而言。
        /// </summary>
        /// <param name="type">Right hand or left hand.<br>右手或左手。</param>
        /// <param name="id">ID of selected joint.<br>选定节点的ID。</param>
        /// <returns>Local rotation of selected joint.<br>选定节点的local rotation。</returns>
        public Quaternion GetRawJointLocalRotation(HandType type, int id)
        {
            int index = type == HandType.RightHand ? 0 : 1;
            return m_RawJointData[index].localRotations[id];
        }

        /// <summary>
        /// Gets world rotation of selected joint.<br>
        /// 获取选定节点的world rotation。
        /// </summary>
        /// <param name="type">Right hand or left hand.<br>右手或左手。</param>
        /// <param name="id">ID of selected joint.<br>选定节点的ID。</param>
        /// <returns>World rotation of selected joint.<br>选定节点的world rotation。</returns>
        public Quaternion GetJointWorldRotation(HandType type, int id)
        {
            Quaternion rot = GetJointLocalRotation(type, id);
            if (id == 0)
            {
                if (type == HandType.RightHand)
                {
                    rot = rightHandController.transform.rotation * rot;
                }
                else
                {
                    rot = leftHandController.transform.rotation * rot;
                }
            }
            else if (id == 1 || id == 5 || id == 9 || id == 13 || id == 17)
            {
                rot = GetJointWorldRotation(type, 0) * rot;
            }
            else
            {
                rot = GetJointWorldRotation(type, id - 1) * rot;
            }

            return rot;
        }

        float m_TimeStart, m_TimeEnd;
        float m_AverageTime;
        void ThreadUpdateHandData()
        {
            HandTracking.SetGestureClassificationMode(0);
            if (!m_IsHandTrackingStarted)
            {
                HandTracking.Start();
                m_IsHandTrackingStarted = true;
            }
            m_IsInitialized = true;
            while (m_HandUpdating)
            {
                m_TimeStart = Time.realtimeSinceStartup;
                if (debugLevel > 1)
                {
                    Debug.Log("Thread updating, m_HandThreadLock " + m_HandThreadLock + ", m_IsPaused " + m_IsPaused);
                }
                if (m_HandThreadLock == 0 && !m_IsPaused)
                {
                    float timeReadDataStart = Time.realtimeSinceStartup;
                    var resultOfGetHandInfo = HandTracking.GetHandInfo(ref m_HandTrackingInfo);
                    float timeGetHandInfoDone = Time.realtimeSinceStartup;
                    var resultOfGetHandJointAngles = HandTracking.GetHandJointAngles(ref m_JointsAngles);
                    float timeGetHandJointAnglesDone = Time.realtimeSinceStartup;
                    if (debugLevel > 1)
                    {
                        Debug.Log("Get algorithm data in " + (timeGetHandJointAnglesDone - timeReadDataStart) * 1000 + "ms, " +
                            "GetHandInfo: " + (timeGetHandInfoDone - timeReadDataStart) * 1000 + "ms, " +
                            "GetHandJointAngles: " + (timeGetHandJointAnglesDone - timeGetHandInfoDone) * 1000 + "ms");
                    }

                    if (resultOfGetHandInfo == XRError.XR_ERROR_SUCCESS && resultOfGetHandJointAngles == XRError.XR_ERROR_SUCCESS)
                    {
                        m_IsHandTrackingCorrect = true;
                    }
                    else
                    {
                        if (debugLevel > 1)
                        {
                            Debug.Log("Thread updating result!!! resultOfGetHandInfo: " + resultOfGetHandInfo + ", resultOfGetHandJointAngles: " + resultOfGetHandJointAngles);
                        }
                        m_IsHandTrackingCorrect = false;
                    }

                    m_HandThreadLock = 1;
                }

                m_TimeEnd = Time.realtimeSinceStartup;
                int sleepTime = 33 - (int)((m_TimeEnd - m_TimeStart) * 1000);
                if (sleepTime < 0)
                {
                    sleepTime = 0;
                }
                Thread.Sleep(sleepTime);
            }
        }

        /// <summary>
        /// Stops hand tracking. This should be called when exiting application.<br>
        /// 停止手部追踪。在退出程序时应当调用这个函数。
        /// </summary>
        void Release()
        {
            Debug.Log("HandTrackingPlugin Release ");
            if (Application.platform == RuntimePlatform.Android)
            {
                // Stop hand tracking.
                m_HandUpdating = false;
                m_HandThread = null;
                HandTracking.Stop();
            }
        }

        /// <summary>
        /// Pauses or resumes hand tracking.<br>
        /// 暂停或者继续手部追踪。
        /// </summary>
        /// <param name="pause"> Application is paused or not currently.
        /// 应用当前是否处于暂停状态</param>
        void OnPause(bool pause)
        {
            Debug.Log("HandTrackingPlugin OnPause: " + pause);
            if (!m_IsInitialized) return;

            m_IsPaused = pause;
            if (pause)
            {
                HandTracking.Pause();
            }
            else
            {
                HandTracking.Resume();
            }
        }
    }
}

