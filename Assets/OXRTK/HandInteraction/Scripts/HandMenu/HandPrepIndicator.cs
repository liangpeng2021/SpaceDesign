using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The hand menu indicator.<br>
    /// 随手菜单指示器。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public class HandPrepIndicator : MonoBehaviour
    {
        /// <summary>
        /// Indicator image for normal status.<br>
        /// 指示器默认状态下图片。
        /// </summary>
        public Image baseImage;

        /// <summary>
        /// Indicator image for highlight status.<br>
        /// 指示器高亮状态下图片。
        /// </summary>
        public Image highlightImage;

        /// <summary>
        /// Indicator showing status.<br>
        /// 指示器显示状态。
        /// </summary>
        public ShowingStatus isShow = ShowingStatus.Off;

        [Header("Sound Effect")]
        /// <summary>
        /// Indicator showing audio.<br>
        /// 指示器开始显示提示音。
        /// </summary>
        public AudioClip showSFX;

        /// <summary>
        /// Indicator hiding audio.<br>
        /// 指示器失败提示音。
        /// </summary>
        public AudioClip hideSFX;

        /// <summary>
        /// Indicator success audio.<br>
        /// 指示器成功提示音。
        /// </summary>
        public AudioClip succeedSFX;

        private Animator m_Anim;
        private AudioSource m_As;
        // Start is called before the first frame update
        void Start()
        {
            if (m_Anim == null)
                m_Anim = GetComponent<Animator>();

            if (m_As == null)
                m_As = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Indicator start showing.<br>
        /// 指示器开始指示。
        /// </summary>
        public void Show()
        {
            m_Anim.ResetTrigger("Normal");
            m_Anim.ResetTrigger("Open");
            m_Anim.ResetTrigger("Off");
            m_Anim.SetTrigger("Highlighted");
            m_As.PlayOneShot(showSFX);
            isShow = ShowingStatus.Showing;
        }

        /// <summary>
        /// Indicator fail.<br>
        /// 指示器指示失败。
        /// </summary>
        public void Fail()
        {
            m_Anim.ResetTrigger("Open");
            m_Anim.ResetTrigger("Off");
            m_Anim.ResetTrigger("Highlighted");
            m_Anim.SetTrigger("Normal");
            m_As.PlayOneShot(hideSFX);
            isShow = ShowingStatus.Fail;
        }

        /// <summary>
        /// Indicator success.<br>
        /// 指示器指示成功。
        /// </summary>
        public void Success()
        {
            m_Anim.ResetTrigger("Normal");
            m_Anim.ResetTrigger("Off");
            m_Anim.ResetTrigger("Highlighted");
            m_Anim.SetTrigger("Open");
            m_As.PlayOneShot(succeedSFX);
            isShow = ShowingStatus.Success;
        }

        /// <summary>
        /// Indicator disappear and off.<br>
        /// 指示器消失关闭。
        /// </summary>
        public void Off()
        {
            m_Anim.ResetTrigger("Normal");
            m_Anim.ResetTrigger("Open");
            m_Anim.ResetTrigger("Highlighted");
            m_Anim.SetTrigger("Off");
            m_As.PlayOneShot(hideSFX);
            isShow = ShowingStatus.Off;
        }

        /*
        public bool IsToOffAvailable()
        {
            if (m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Highlighted") ||
                ((m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Normal") || m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Open")) && m_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f))
            {
                return true;
            }
            return false;
        }*/

    }
}