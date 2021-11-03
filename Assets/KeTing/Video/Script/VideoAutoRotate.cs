/* Create by zh at 2021-09-26

   跟随用户的相机自旋转

 */

using UnityEngine;
using System.Collections;
namespace SpaceDesign.Video
{
    public class VideoAutoRotate : MonoBehaviour
    {
        //public Transform traVideoExpand;
        private Transform traFollow;
        private Transform traEye;

        float fTime = 0;
        bool bMove;
        //Vector3 v3Forward;
        //Vector3 v3Pos;
        //Vector3 v3Eur;
        //目标物的偏移速度
        public float fSpeed = 0.05f;
        //目标物up轴的偏移（高度）
        public float fHight = 0f;
        //目标物forward轴的偏移（距离）
        public float fDis = 1;
        //目标物right轴的偏移（左右）
        public float fOffset = 0;

        //移动中的时长，如果大于3秒，就直接赋值到目标位置，（防止一直卡住）
        private float fMoveTime;

        void OnEnable()
        {
            StartCoroutine("IEResetPos");
        }
        void OnDisable()
        {
            StopAllCoroutines();
        }

        //void Awake()
        //{
        //    traFollow = this.transform;
        //    traEye = XR.XRCameraManager.Instance.stereoCamera.transform;
        //    if (traEye == null)
        //        Debug.LogError("Video跟随，眼睛对象为空");
        //}

        IEnumerator IEResetPos()
        {
            traFollow = this.transform;
            traEye = XR.XRCameraManager.Instance.stereoCamera.transform;

            bool bPause = false;
            float fTime = 0;

            Vector3 v3Forward = traEye.forward;
            Vector3 v3Pos = traEye.position + traEye.forward * fDis + traEye.up * fHight + traEye.right * fOffset;
            //Vector3 v3Eur = new Vector3(0, traEye.eulerAngles.y, 0);

            while (true)
            {
                if (bPause == false)
                {
                    fTime += Time.deltaTime;
                    if (fTime > 15)
                    {
                        fTime = 0;
                        bPause = true;

                        traFollow.position = v3Pos;
                        traFollow.forward = v3Forward;
                        //traFollow.eulerAngles = v3Eur;

                    }

                    traFollow.position = Vector3.Lerp(traFollow.position, v3Pos, fSpeed * Time.deltaTime);
                    traFollow.forward = Vector3.Lerp(traFollow.forward, v3Forward, fSpeed * Time.deltaTime);
                    //traFollow.eulerAngles = Vector3.Lerp(traFollow.eulerAngles, v3Eur, fSpeed * Time.deltaTime);

                }
                else
                {
                    v3Forward = traEye.forward;
                    v3Pos = traEye.position + traEye.forward * fDis + traEye.up * fHight + traEye.right * fOffset;
                    //v3Eur = new Vector3(0, traEye.eulerAngles.y, 0);

                    bPause = false;
                }

                yield return 0;
            }
        }

        //void Update()
        //{

        //    traFollow.position = traEye.position;// traEye.position + traEye.forward * fDis + traEye.up * fHight + traEye.right * fOffset;
        //    traFollow.forward = traEye.forward;
        //    traFollow.eulerAngles = new Vector3(0, traEye.eulerAngles.y, 0);

        //    traVideoExpand.localPosition = traVideoExpand.localEulerAngles = Vector3.zero;
        //    traVideoExpand.localScale = Vector3.one;

        //    fTime += Time.deltaTime;
        //    if (fTime > 3f)
        //    {
        //        traFollow.position = Vector3.Lerp(traFollow.position, traEye.position, fSpeed * Time.deltaTime);// traEye.position + traEye.forward * fDis + traEye.up * fHight + traEye.right * fOffset;
        //        traFollow.forward = Vector3.Lerp(traFollow.forward, traEye.position, fSpeed * Time.deltaTime);// traEye.position + traEye.forward * fDis + traEye.up * fHight + traEye.right * fOffset;


        //        if (fTime > 6f)
        //        {
        //            fTime = 0;
        //        }
        //    }
        //    else
        //    {

        //    }


        //    //fTime += Time.deltaTime;
        //    ////print(bMove);
        //    //if (fTime > 3f)
        //    //{
        //    //    fTime = 0;
        //    //    if (bMove == false)
        //    //    {
        //    //        //===================跟随东南西北四个方向======================================
        //    //        //forward = Vector3.forward;
        //    //        //pos = new Vector3(0, hight, dis);

        //    //        //if (DotToAngle(eyeTra.forward, Vector3.forward) <= 45f)
        //    //        //{
        //    //        //    forward = Vector3.forward;
        //    //        //    pos = new Vector3(0, hight, dis);
        //    //        //}
        //    //        //else if (DotToAngle(eyeTra.forward, -Vector3.forward) <= 45f)
        //    //        //{
        //    //        //    forward = -Vector3.forward;
        //    //        //    pos = new Vector3(0, hight, -dis);
        //    //        //}
        //    //        //else if (DotToAngle(eyeTra.forward, Vector3.right) <= 45f)
        //    //        //{
        //    //        //    forward = Vector3.right;
        //    //        //    pos = new Vector3(dis, hight, 0);
        //    //        //}
        //    //        //else if (DotToAngle(eyeTra.forward, -Vector3.right) <= 45f)
        //    //        //{
        //    //        //    forward = -Vector3.right;
        //    //        //    pos = new Vector3(-dis, hight, 0);
        //    //        //}
        //    //        //位置加上目标相机的位置
        //    //        //pos = pos + eyeTra.position;
        //    //        //===========================================================================

        //    //        //====================跟随当前视觉方向========================================
        //    //        v3Forward = traEye.forward;
        //    //        v3Pos = traEye.position;// traEye.position + traEye.forward * fDis + traEye.up * fHight + traEye.right * fOffset;
        //    //        v3Eur = new Vector3(0, traEye.eulerAngles.y, 0);

        //    //        traVideoExpand.localPosition = traVideoExpand.localEulerAngles = Vector3.zero;
        //    //        traVideoExpand.localScale = Vector3.one;
        //    //        //===========================================================================

        //    //        bMove = true;
        //    //        fMoveTime = 0;
        //    //    }
        //    //}

        //    //if (bMove)
        //    //{
        //    //    //fMoveTime += Time.deltaTime;
        //    //    //if (fMoveTime > 3)
        //    //    //{
        //    //    //    fMoveTime = 0;
        //    //    //    targetTra.forward = v3Forward;
        //    //    //    targetTra.position = v3Pos;
        //    //    //    bMove = false;
        //    //    //    return;
        //    //    //}

        //    //    //if (DotToAngle(traFollow.forward, v3Forward) >= 180)
        //    //    //    traFollow.forward = v3Forward;
        //    //    //else
        //    //    //    traFollow.forward = Vector3.Lerp(traFollow.forward, v3Forward, fSpeed * Time.deltaTime);

        //    //    traFollow.eulerAngles = Vector3.Lerp(traFollow.eulerAngles, v3Eur, fSpeed * 2 * Time.deltaTime);
        //    //    traFollow.position = Vector3.Lerp(traFollow.position, v3Pos, fSpeed * Time.deltaTime);

        //    //    //traVideoExpand.localPosition = Vector3.Lerp(traVideoExpand.localPosition, Vector3.zero, fSpeed * 2 * Time.deltaTime);

        //    //    //if (Vector3.Distance(targetTra.forward, forward) < 0.1f && Vector3.Distance(targetTra.position, pos) < 0.1f)
        //    //    //if (DotToAngle(targetTra.forward, v3Forward) < 1f && Vector3.Distance(traFollow.position, v3Pos) < 0.01f)
        //    //    if (Vector3.Distance(traFollow.position, v3Pos) < 0.01f)
        //    //    {
        //    //        traFollow.forward = v3Forward;
        //    //        traFollow.position = v3Pos;
        //    //        bMove = false;
        //    //    }
        //    //}
        //}

        ///// <summary>
        ///// 计算两向量之间的夹角
        ///// </summary>
        ///// <param name="_from"></param>
        ///// <param name="_to"></param>
        ///// <returns></returns>
        //public float DotToAngle(Vector3 _from, Vector3 _to)
        //{
        //    float rad = 0;
        //    rad = Mathf.Acos(Vector3.Dot(_from.normalized, _to.normalized));
        //    return rad * Mathf.Rad2Deg;
        //}

    }
}