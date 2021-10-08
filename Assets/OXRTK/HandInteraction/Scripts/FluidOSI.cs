using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR;
using OXRTK.ARHandTracking;


namespace OXRTK
{

    [RequireComponent(typeof(MeshRenderer))]
    public class FluidOSI : MonoBehaviour
    {
        public Renderer frame;
        private Camera m_MainCamera;
         public Vector2 attrPos;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            frame = GetComponent<MeshRenderer>();

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                m_MainCamera = XRCameraManager.Instance.eventCamera;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                yield return new WaitUntil(() => CenterCamera.centerCamera != null);
                m_MainCamera = CenterCamera.centerCamera;
            }
        }

        // Update is called once per frame
        void Update()
        {    
            float eval = 0;

            if (attrPos.x < 0f)
            {
                if (attrPos.y < 0f)
                {
                    if (Mathf.Abs(attrPos.x) > Mathf.Abs(attrPos.y))
                    {
                        // The indicator on the left-bottom edge; Bae val is 0.82
                       // Debug.Log("Left-bottom!!");

                        eval = .91f - (Mathf.Abs(attrPos.y) / Mathf.Abs(attrPos.x) * 0.16f);
                        SetShaderParam(eval);
                    }
                    else
                    {
                        // The indicator is on the bottom-left edge; Base val is 0.66
                       // Debug.Log("Bottom-left!!");

                        eval = .66f + (Mathf.Abs(attrPos.x) / Mathf.Abs(attrPos.y) * 0.09f);
                        SetShaderParam(eval);
                    }
                }
                else
                {
                    if (Mathf.Abs(attrPos.x) > Mathf.Abs(attrPos.y))
                    {
                        // The indicator on the left-top edge; Base val is 0.91
                       // Debug.Log("Left-top!!");

                        eval = .91f + (Mathf.Abs(attrPos.y) / Mathf.Abs(attrPos.x) * 0.16f);
                        SetShaderParam(eval);
                    }
                    else
                    {
                        // The indicator on the top-left edge; Base val is 0
                       // Debug.Log("Top-left!!");

                        eval = 0.16f - (Mathf.Abs(attrPos.x) / Mathf.Abs(attrPos.y) * 0.09f);
                        SetShaderParam(eval);
                    }
                }
            }
            else
            {
                if (attrPos.y < 0f)
                {
                    if (Mathf.Abs(attrPos.x) > Mathf.Abs(attrPos.y))
                    {
                        // The indicator on the right-top edge; Base val is 0.41
                       // Debug.Log("Right-bottom!!");

                        eval = .41f + (Mathf.Abs(attrPos.y) / Mathf.Abs(attrPos.x) * 0.16f);
                        SetShaderParam(eval);
                    }
                    else
                    {
                        // The indicator is on the bottom-right edge; Base val is 0.50
                       // Debug.Log("Bottom-right!!");

                        eval = .66f - (Mathf.Abs(attrPos.x) / Mathf.Abs(attrPos.y) * 0.09f);
                        SetShaderParam(eval);
                    }
                }
                else
                {
                    if (Mathf.Abs(attrPos.x) > Mathf.Abs(attrPos.y))
                    {
                        // The indicator on the right edge; Base val is 0.32
                      //  Debug.Log("Right-top!!");

                        eval = .41f - (Mathf.Abs(attrPos.y) / Mathf.Abs(attrPos.x) * 0.16f);
                        SetShaderParam(eval);
                    }
                    else
                    {
                        // The indicator on the top edge; Base val is 0.16
                      //  Debug.Log("Top-right!!");

                        eval = 0.16f + (Mathf.Abs(attrPos.x) / Mathf.Abs(attrPos.y) * 0.09f);
                        SetShaderParam(eval);
                    }
                }
            }
        }

        public void SetAttractor(Vector3 handPos)
        {
            attrPos = handPos;
            attrPos -= new Vector2(0.5f, 0.5f);
        }

        void SetShaderParam(float eval)
        {
            frame.material.SetFloat("_XMove", eval + 0.34f);
        }
    }
}
