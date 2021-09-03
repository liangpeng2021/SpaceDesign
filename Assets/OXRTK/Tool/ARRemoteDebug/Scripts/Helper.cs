using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using System;
using System.Security.Cryptography;

namespace OXRTK.ARRemoteDebug
{
    public class Helper
    {
        static public bool showLoginError = false;
        static public byte[] ObjectToByteArray(System.Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        static public System.Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            System.Object obj = (System.Object)binForm.Deserialize(memStream);


            return obj;
        }

        public static int GetHashNumberFromString(string text)
        {
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(text));
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }

        public static string ObjectToBase64(System.Object obj)
        {
            return Convert.ToBase64String(ObjectToByteArray(obj));
        }

        public static System.Object Base64ToObject(string base64string)
        {
            return ByteArrayToObject(Convert.FromBase64String(base64string));
        }

    }

}
