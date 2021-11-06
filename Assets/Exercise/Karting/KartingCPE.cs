/* Create by zh at 2021-10-14

   卡丁车CPE控制 

 */

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SpaceDesign.Karting
{
    public class CPEBikeData
    {
        public int ErrorCode;//"":0,
        public float Direction;//"": 60， //方向，-100...100 0表示直行， -100表示最左， 100表示最右
        public float Speed;//"": 11.38， //速度，公里/小时
        public int BMP;//"": 60,//心跳，次数/分钟
        public int Weight;//"": 70, //体重，公斤
        public int Power;//"": 79 //功率，瓦
    }

    public class KartingCPE : MonoBehaviour
    {
        static KartingCPE inst;
        public static KartingCPE Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<KartingCPE>();
                return inst;
            }
        }

        string TurnInputName = "Horizontal";
        string AccelerateButtonName = "Accelerate";
        string BrakeButtonName = "Brake";

        public Text textLog;
        public CPEBikeData cpeBikeData;

        public bool Accelerate;//= Input.GetButton(AccelerateButtonName),
        public bool Brake;//= Input.GetButton(BrakeButtonName),
        public float TurnInput;//= Input.GetAxis("Horizontal")
        //速度范围：0-1
        public float Speed;

        public int ErrorCode;//"":0,
        public float Direction;//"": 60， //方向，-100...100 0表示直行， -100表示最左， 100表示最右
        public int BMP;//"": 60,//心跳，次数/分钟
        public int Weight;//"": 70, //体重，公斤
        public int Power;//"": 79 //功率，瓦
        public float fKcal;//消耗卡路里（速度大于0.2，每秒增加0.09卡路里）


        public Image imgKcal;
        public Text textKcal;
        public Text textBMP;
        public Text textTime;
        //#if UNITY_EDITOR
        //        private void Update()
        //        {
        //            if (Input.GetButton(AccelerateButtonName))
        //            {
        //                Accelerate = true;
        //                Speed = Mathf.Lerp(Speed, fSpeedSpeed, Time.deltaTime);
        //            }
        //            else
        //            {
        //                Brake = false;
        //                if (Speed > 0.01f)
        //                {
        //                    Speed = Mathf.Lerp(Speed, 0, Time.deltaTime);
        //                    Speed = 0;
        //                }
        //            }

        //            float _f = Input.GetAxis("Horizontal");
        //            TurnInput = _f;
        //        }
        //#else
        //#endif
        public float fRate = 0.5f;
        void Update()
        {
            if (Speed > 0.2f)
                imgKcal.fillAmount = (fKcal / 450);

            TurnInput = Mathf.Lerp(TurnInput, 0, fRate * Time.deltaTime);
        }

        public void StartGame()
        {
            StopCoroutine("IEBike");
            StartCoroutine("IEBike");
        }
        public void EndGame()
        {
            StopCoroutine("IEBike");
        }

        //是否玩游戏中
        public bool bPlaying = false;
        public float fDirSpeed = 0.0035f;
        public float fSpeedSpeed = 30f;
        public IEnumerator IEBike()
        {
            //yield break;
            print("开始游戏");


            WaitForSeconds wfs05 = new WaitForSeconds(0.1f);

            Speed = 0;
            Direction = 0;
            BMP = 0;
            Weight = 0;
            Power = 0;
            fKcal = 0;
            float _fTotalTime = 0;

            while (true)
            {
                bool bTest = false;
                if (bTest)
                {
                    UnityWebRequest www = UnityWebRequest.Get("http://192.168.31.123:8020/iot/bike/deviceInfo");
                    //yield return www.SendWebRequest();
                    www.SendWebRequest();
                    float _time = 0;
                    while (www.isDone == false)
                    {
                        _time += Time.deltaTime;
                        if (_time > 3)
                        {
                            _time = 0;
                            ShowLog("KartingCPE-----网络错误，请重试！");
                            //yield break;
                        }
                        yield return 0;
                    }
                    //ShowLog(www.downloadHandler.text, true);
                    //#if UNITY_EDITOR
                    //#endif
                    //print(www.downloadHandler.text);


                    if (www.isDone && string.IsNullOrEmpty(www.error))
                    {
                        cpeBikeData = JsonConvert.DeserializeObject<CPEBikeData>(www.downloadHandler.text);
                        //ShowLog(cpeBikeData.ErrorCode.ToString());
                        if (cpeBikeData.ErrorCode == 0)
                        {
                            TurnInput = cpeBikeData.Direction * fDirSpeed;
                            //速度有个上限
                            Speed = cpeBikeData.Speed / fSpeedSpeed;
                            Accelerate = (Speed > 0);
                            Brake = (Speed < 0);
                            //Accelerate = true;

                            Direction = cpeBikeData.Direction;
                            BMP = cpeBikeData.BMP;
                            ErrorCode = cpeBikeData.ErrorCode;
                            Power = cpeBikeData.Power;
                            Weight = cpeBikeData.Weight;

                            //ShowLog(cpeBikeData.Direction.ToString());
                            //ShowLog(cpeBikeData.Speed.ToString());
                            //ShowLog(cpeBikeData.BMP.ToString());
                            //ShowLog(cpeBikeData.Weight.ToString());
                            //ShowLog(cpeBikeData.Power.ToString());
                            //print(cpeBikeData.Direction.ToString());
                            //print(cpeBikeData.Speed.ToString());
                            //print(cpeBikeData.BMP.ToString());
                            //print(cpeBikeData.Weight.ToString());
                            //print(cpeBikeData.Power.ToString());
                        }
                    }
                }

                //0.1秒请求一次
                _fTotalTime += 0.1f;

                //速度大于0.2（cpeBikeData.Speed / fSpeedSpeed），每秒加0.09卡路里，这里是0.5秒一次
                if (Speed > 0.2)
                {
                    fKcal += 0.05f;
                    textKcal.text = (Mathf.FloorToInt(fKcal)).ToString();
                    textBMP.text = BMP.ToString();

                    float s = (_fTotalTime % 60);
                    float m = (((_fTotalTime - s) / 60) % 60);
                    float h = ((_fTotalTime - s) / 3600);
                    textTime.text = $"{((int)h).ToString("D2")}:{((int)m).ToString("D2")}:{((int)s).ToString("D2")}";
                }
                yield return wfs05;
            }
        }

        public void Clear()
        {
            Accelerate = false;
            TurnInput = 0;
            Speed = 0;
        }

        public void Fowrard()
        {
            Accelerate = true;
            TurnInput = 0;
            Speed = 1;
        }

        public void RightFowrard()
        {
            Accelerate = true;
            TurnInput = -0.8f;
            Speed = 1;
        }
        public void LeftFowrard()
        {
            Accelerate = true;
            TurnInput = 0.8f;
            Speed = 1;
        }

        void ShowLog(string str, bool bClear = false)
        {
            //if (bClear)
            //    textLog.text = null;
            textLog.text = $"{str}\n";
        }
    }

}
