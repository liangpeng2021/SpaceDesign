using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDesign
{
    public class VideoTV2DCtr : MonoBehaviour
    {
        //3D总长（这里是帧数，不是秒数，秒数乘以帧率）容积视频的总帧数还要减1，因为最后一帧不播放
        float fTotalFrame3D = 2613;
        //3D的音乐总长，（这里是秒数，跟容积视频的长度是不同的）
        float fTotalTime3DMusic = 115.271f;

        void Start()
        {

        }

        void Update()
        {

        }
    }
}