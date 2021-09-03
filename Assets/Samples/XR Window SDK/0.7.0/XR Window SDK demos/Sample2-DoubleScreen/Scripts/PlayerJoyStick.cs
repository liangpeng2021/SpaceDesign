/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Samples
{
    public class PlayerJoyStick : MonoBehaviour
    {
        [Header("是否是在XZ平面：true-XZ flase-XY")]
        [SerializeField]
        private bool IsXZPlane = true;

        private Transform mPlayer;
        private float moveSpeed = 5f;
        private float mPlayerPx = 0;
        private float mPlayerPz = 0;
        private Vector3 moveDir = Vector3.zero;
        private void Start()
        {
            mPlayer = transform;
        }
        private void Update()
        {
            if (mPlayer == null) return;
            mPlayerPx = EasyJoystick.Instance.JoystickTouch.x;
            mPlayerPz = EasyJoystick.Instance.JoystickTouch.y;
            if (mPlayerPx != 0 || mPlayerPz != 0)
            {
                Debug.Log(EasyJoystick.Instance.JoystickTouch);
                if (IsXZPlane)
                {
                    if (Mathf.Abs(mPlayerPz) > Mathf.Abs(mPlayerPx))
                    {
                        moveDir = mPlayerPz > 0 ? mPlayer.forward : -mPlayer.forward;
                    }
                    else
                    {
                        moveDir = mPlayerPx > 0 ? mPlayer.right : -mPlayer.right;
                    }
                    moveDir = moveDir.normalized;
                    moveDir.y = 0;
                }
                else
                {
                    moveDir.x = mPlayerPx;
                    moveDir.y = mPlayerPz;
                    moveDir = moveDir.normalized;
                }
                mPlayer.position += moveDir * moveSpeed * Time.deltaTime;
            }
        }
    }
}