/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Samples
{
    public class PlayerControl : MonoBehaviour
    {
        private Transform mPlayer;
        private float moveSpeed = 2f;
        private float mPlayerPx = 0;
        private float mPlayerPz = 0;
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
                Quaternion wantRotation = Quaternion.LookRotation(new Vector3(mPlayerPx, 0, mPlayerPz));
                Quaternion dampingRotation = Quaternion.Lerp(mPlayer.rotation, wantRotation, 10f * Time.deltaTime);
                mPlayer.rotation = dampingRotation;
                mPlayer.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            }
        }
    }
}