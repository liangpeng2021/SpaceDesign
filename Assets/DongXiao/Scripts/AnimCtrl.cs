using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimCtrl : MonoBehaviour
{
    Animator animator;
    public string parameterName;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetEffect()
    {
        animator.speed = 0;
        animator.SetTrigger(parameterName);
        animator.speed = 1;
    }
}