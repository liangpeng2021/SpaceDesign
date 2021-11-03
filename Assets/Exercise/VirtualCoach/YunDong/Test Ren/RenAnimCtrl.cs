using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RenAnimCtrl : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        //animator.SetBool("Idle", true);
        animator.SetTrigger("Idle");

    }


    void Update()
    {

    }

    public void SetCurState(string parameters)
    {
        animator.speed = 0;

        //animator.SetBool(parameters.ToString()+" 0", true);
        animator.SetTrigger(parameters.ToString());

        animator.speed = 1;
    }
}