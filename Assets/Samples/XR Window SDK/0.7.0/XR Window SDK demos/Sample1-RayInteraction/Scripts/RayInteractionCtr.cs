/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XR.Samples
{
    public class RayInteractionCtr : MonoBehaviour
    {
        [SerializeField]
        private Text errorText;
        [SerializeField]
        private Transform targetTrans;
        [SerializeField]
        private RawImage preview;

        private static string BasePath = "/storage/emulated/0/DCIM/";
        private Quaternion initRotation;
        private Vector3 initScale;
        private Vector3 scaleOffset = Vector3.one * 0.05f;

        private void Start()
        {
            if (errorText) errorText.text = "";
            if (targetTrans != null)
            {
                initScale = targetTrans.localScale;
                initRotation = targetTrans.rotation;
            }

            ScreenCaptureManager.Instance.OnXRCaptureResult += SetPreview;
        }

        private void SetPreview(Texture texture)
        {
            preview.texture = texture;
        }

        public void Rotate(int value)
        {
            if (targetTrans == null) return;
            if (value == 0)
            {
                targetTrans.rotation = initRotation;
            }
            else
            {
                targetTrans.Rotate(Vector3.up * value * (-1), Space.World);
            }
        }

        public void Scale(int scale)
        {
            if (targetTrans == null) return;
            if (scale == 0)
            {
                targetTrans.localScale = initScale;
            }
            else
            {
                targetTrans.localScale += scaleOffset * scale;
            }
        }

        public void TakePhoto()
        {
            XRError result = ScreenCaptureManager.Instance.TakeCameraShot(Time2File.GetDataPath(BasePath, false));

            string info = result == XRError.XR_ERROR_SUCCESS ? "拍照成功" : "拍照失败";

            SetErrorText(info);
        }

        public void StartVideo()
        {
            XRError result = ScreenCaptureManager.Instance.StartVideoCapture(Time2File.GetDataPath(BasePath, true));

            string info = result == XRError.XR_ERROR_SUCCESS ? "开始录制" : "录制失败";

            SetErrorText(info);
        }

        public void StopVideo()
        {
            //SetErrorText(TakePVManager.GetInstance.StopVideo());
            XRError result = ScreenCaptureManager.Instance.StopVideoCapture();

            string info = result == XRError.XR_ERROR_SUCCESS ? "结束录制" : "结束失败";

            SetErrorText(info);
        }

        private void SetErrorText(string text)
        {
            errorText.text = text;
            //if (errorText == null) return;
            //if (errorText.text == text) return;
            //StartCoroutine(ErrorText(text));
        }

        IEnumerator ErrorText(string text)
        {
            if (errorText)
            {
                errorText.text = text;
                yield return new WaitForSeconds(5.0f);
                errorText.text = "";
            }
            yield break;
        }
    }
}