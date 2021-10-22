using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDesign
{
    public class ButtonAnimCtrl : MonoBehaviour
    {
        Animator animator;
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void SetEnterPress()
        {
            animator.SetBool("IsPress", true);
        }

        public void SetExitPress()
        {
            animator.SetBool("IsPress", false);
        }
    }
}