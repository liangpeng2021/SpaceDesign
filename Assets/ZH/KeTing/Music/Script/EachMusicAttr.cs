/* Create by zh at 2021-09-22

    每首音乐的属性信息等

 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDesign.Music
{
    public class EachMusicAttr : MonoBehaviour
    {
        public string strName;
        public AudioClip audioClip;
        [HideInInspector]
        public Image image;

        void Start()
        {
            image = GetComponent<Image>();
        }

    }
}