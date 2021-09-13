/* Create by zh at 2019-4-19

   游戏的功能接口类
   
    比如：网络判断、文件存读等、

 */
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using System;

namespace Museum
{
    /// <summary>
    /// 网络类型
    /// </summary>
    public enum NetState
    {
        NoNet,
        Wifi,
        PhoneData,
    }

    /// <summary>
    /// 游戏的功能接口类
    /// </summary>
    public class GameTools
    {
        #region 【不要调用，不起作用】 屏幕设置,横屏锁定[SetScreenLock]

        /// <summary>
        /// 锁定横屏，不要调用，不起作用！
        /// </summary>
        static void SetScreenLock(bool isLock)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = !isLock;
            Screen.autorotateToPortraitUpsideDown = !isLock;
        }
        #endregion

        #region 网络判断[NetWorkEnv]
        //NetworkReachability.NotReachable => NetState.NoNet;
        //NetworkReachability.ReachableViaLocalAreaNetwork => NetState.Wifi;
        //NetworkReachability.ReachableViaCarrierDataNetwork => NetState.PhoneData;
        /// <summary>
        /// 用户网络判断
        /// </summary>
        public static NetState NetWorkEnv
        {
            get
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                    return NetState.NoNet;
                else
                    return (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) ? NetState.Wifi : NetState.PhoneData;
            }
        }
        #endregion

        #region 音频转字节流[ConvertClipToBytes]
        /// <summary>
        /// 音频转字节流
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static byte[] ConvertClipToBytes(AudioClip clip)
        {
            //clip.length;
            float[] samples = new float[clip.samples];

            clip.GetData(samples, 0);

            short[] intData = new short[samples.Length];
            //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

            byte[] bytesData = new byte[samples.Length * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            int rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = new byte[2];
                byteArr = System.BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }
        #endregion

        #region 获得属性面板数值
        /// <summary>
        /// 获取属性面板的Rotation的值
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Vector3 GetInspectorRot(Transform transform)
        {
            // 获取原生值
            System.Type transformType = transform.GetType();
            System.Reflection.PropertyInfo m_propertyInfo_rotationOrder = transformType.GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
            object m_OldRotationOrder = m_propertyInfo_rotationOrder.GetValue(transform, null);
            System.Reflection.MethodInfo m_methodInfo_GetLocalEulerAngles = transformType.GetMethod("GetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
            object value = m_methodInfo_GetLocalEulerAngles.Invoke(transform, new object[] { m_OldRotationOrder });
            //Debug.Log("反射调用GetLocalEulerAngles方法获得的值：" + value.ToString());
            string temp = value.ToString();
            //将字符串第一个和最后一个去掉
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            //用‘，’号分割
            string[] tempVector3;
            tempVector3 = temp.Split(',');
            //将分割好的数据传给Vector3
            Vector3 vector3 = new Vector3(float.Parse(tempVector3[0]), float.Parse(tempVector3[1]), float.Parse(tempVector3[2]));
            return vector3;
        }
        #endregion

        #region 本地文件,存[WriteFile]、读[ReadFile]、文件夹排序读取[GetDireBySort]
        /// <summary>
        /// 向本地写（保存）内容
        /// </summary>
        public static void WriteFile(string pth, string fileName, byte[] content)
        {
            string archivePth = Path.Combine(pth, fileName);
            Directory.CreateDirectory(archivePth);
            MyTools.MyFile.SetFileBytes(archivePth, content);
        }
        /// <summary>
        /// 从本地读（加载）内容
        /// </summary>
        public static byte[] ReadFile(string filePth, string fileName)
        {
            string archivePth = Path.Combine(filePth, fileName);
            string pth = Path.Combine(archivePth, "context.timeline");
            return MyTools.MyFile.GetFileBytes(pth);
        }

        /// <summary>
        /// 把文件夹中的内容排序并且读取
        /// </summary>
        public static DirectoryInfo[] GetDireBySort(string pth)
        {
            if (false == Directory.Exists(pth))
            {
                Directory.CreateDirectory(pth);
            }
            DirectoryInfo di = new DirectoryInfo(pth);
            DirectoryInfo[] dis = di.GetDirectories();
            MyTools.DirectoryComparer dc = new MyTools.DirectoryComparer();
            System.Array.Sort(dis, dc);
            return dis;
        }

        #endregion

        #region 退出程序[QuitApp]
        /// <summary>
        /// 退出APP
        /// </summary>
        public static void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region 跳转网页反馈界面[TurnInfoFeedback]
        /// <summary>
        /// 跳转网页反馈界面
        /// </summary>
        public static void TurnInfoFeedback()
        {
            Application.OpenURL("http://www.yoop.com.cn/feedback.php");
        }
        #endregion

        #region MD5加密[Encrypt]、解密[Decrypt]
        const string _key = "aNjUUMTxYZ520Zaz4Gz66B5rqbeUtMtT";
        /// <summary>
        /// 加密
        /// </summary>
        public static string Encrypt(string toE)
        {
            RijndaelManaged rjm = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(_key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
            };
            ICryptoTransform ict = rjm.CreateEncryptor();

            byte[] encryptBts = Encoding.UTF8.GetBytes(toE);
            byte[] resultBts = ict.TransformFinalBlock(encryptBts, 0, encryptBts.Length);

            return Convert.ToBase64String(resultBts, 0, resultBts.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static string Decrypt(string toD)
        {
            RijndaelManaged rjm = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(_key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
            };
            ICryptoTransform ict = rjm.CreateDecryptor();

            byte[] decryptBts = Convert.FromBase64String(toD);
            byte[] resultBts = ict.TransformFinalBlock(decryptBts, 0, decryptBts.Length);

            return Encoding.UTF8.GetString(resultBts);
        }
        #endregion

    }
}
