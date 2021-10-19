/* Create by zh at 2021-

   音乐特效的事件监听 

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceDesign.Music
{
    public class EffectTriggerListener : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            MusicManage.Inst.EffectToMinUI();
        }
    }
}