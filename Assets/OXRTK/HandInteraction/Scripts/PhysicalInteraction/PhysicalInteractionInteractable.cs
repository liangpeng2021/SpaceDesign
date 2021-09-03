using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// Base class for Physical Interaction's finger tip to interact with object, not for direct use. <br>基本父类，用于和物理交互组件中的指尖进行交互，请勿直接使用.</br>
    /// </summary>
    public class PhysicalInteractionInteractable : MonoBehaviour
    {
        /// <summary>
        /// When finger tip enter, for PhysicalInteractionFingerTip use only. <br>当指尖进入，仅供PhysicalInteractionFingerTip使用.</br>
        /// </summary>
        /// <param name="hand">The releative PhysicalInteractionHand<br>对应的PhysicalInteractionHand.</br></param>
        /// <param name="fingerTip">The releative PhysicalInteractionFingerTip<br>对应的PhysicalInteractionFingerTip.</br></param>
        public virtual void OnFingerTipEnter(PhysicalInteractionHand hand, PhysicalInteractionFingerTip fingerTip) { }

        /// <summary>
        /// When finger tip exit, for PhysicalInteractionFingerTip use only. <br>当指尖退出，仅供PhysicalInteractionFingerTip使用.</br>
        /// </summary>
        /// <param name="hand">The releative PhysicalInteractionHand<br>对应的PhysicalInteractionFingerTip.</br></param>
        /// <param name="fingerTip">The releative PhysicalInteractionFingerTip<br>对应的PhysicalInteractionFingerTip.</br></param>
        public virtual void OnFingerTipExit(PhysicalInteractionHand hand, PhysicalInteractionFingerTip fingerTip) { }

        /// <summary>
        /// Reset everything when hand end change. <br>当手终端发生重置，相应重置所有可交互物体.</br>
        /// </summary>
        public virtual void FTHReset() { }
    }
}