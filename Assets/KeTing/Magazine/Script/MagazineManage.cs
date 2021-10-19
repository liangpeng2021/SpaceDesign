/* Create by zh at 2021-10-11

    杂志控制脚本

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceDesign.Magazine
{
    public class MagazineManage : MonoBehaviour
    {
        static MagazineManage inst;
        public static MagazineManage Inst
        {
            get
            {
                if (inst == null)
                    inst = FindObjectOfType<MagazineManage>();
                return inst;
            }
        }
        //人物和Icon的距离状态
        public PlayerPosState curPlayerPosState = PlayerPosState.Far;
        //播放模型
        public Transform traModel;
        //Icon、UI等正在切换中
        bool bUIChanging = false;
        //运动阈值
        float fThreshold = 0.1f;

        void OnEnable()
        {
            PlayerManage.refreshPlayerPosEvt += RefreshPos;
        }

        void OnDisable()
        {
            PlayerManage.refreshPlayerPosEvt -= RefreshPos;
        }

        void Start()
        {
            btnCheckDetail.onClick.AddListener(OnCheckDetail);
            btnQuit.onClick.AddListener(OnQuit);
        }

        /// <summary>
        /// 刷新位置消息
        /// </summary>
        public void RefreshPos(Vector3 pos)
        {
            if (bUIChanging == true)
                return;

            Vector3 _v3 = traModel.position;
            _v3.y = pos.y;
            float _dis = Vector3.Distance(_v3, pos);
            //print($"目标的距离:{_dis}");

            PlayerPosState lastPPS = curPlayerPosState;

            if (_dis > 5f)
            {
                if (lastPPS == PlayerPosState.Far)
                    return;
                curPlayerPosState = PlayerPosState.Far;
            }
            else if (_dis <= 5f && _dis > 1.5f)
            {
                if (lastPPS == PlayerPosState.Middle)
                    return;
                curPlayerPosState = PlayerPosState.Middle;
            }
            else if (_dis <= 1.5f)
            {
                if (lastPPS == PlayerPosState.Close)
                    return;
                curPlayerPosState = PlayerPosState.Close;
            }

            StartCoroutine("IERefreshPos", lastPPS);
        }


        /// <summary>
        /// UI等刷新位置消息
        /// </summary>
        IEnumerator IERefreshPos(PlayerPosState lastPPS)
        {
            print($"刷新位置，上一状态：{lastPPS}，目标状态:{curPlayerPosState}");

            //WaitForSeconds _wfs = new WaitForSeconds(0.1f);

            if (lastPPS == PlayerPosState.Far && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 远距离=>中距离
                yield return IEFarToMiddle();
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Close)
            {
                /// 中距离=>近距离
                yield return IEMiddleToClose();
            }
            else if (lastPPS == PlayerPosState.Close && curPlayerPosState == PlayerPosState.Middle)
            {
                /// 近距离=>中距离
                yield return IECloseToMiddle(false);
            }
            else if (lastPPS == PlayerPosState.Middle && curPlayerPosState == PlayerPosState.Far)
            {
                /// 中距离=>远距离
                yield return IEMiddleToFar();
            }

            yield return 0;
        }

        /// <summary>
        /// 远距离=>中距离
        /// </summary>
        IEnumerator IEFarToMiddle()
        {
            //UI开始变化
            bUIChanging = true;

            //远距离=>中距离
            //Icon从静态变成动态
            //Icon的自旋转动画开启
            foreach (var v in animIconMiddle)
                v.enabled = true;
            //Icon自身上下浮动开启
            animIconFar.enabled = true;
            traIcon.gameObject.SetActive(true);

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 中距离=>远距离
        /// </summary>
        IEnumerator IEMiddleToFar()
        {
            //UI开始变化
            bUIChanging = true;

            //中距离=>远距离
            //Icon从动态变成静态
            //Icon的自旋转动画关闭
            foreach (var v in animIconMiddle)
                v.enabled = false;
            //Icon自身上下浮动关闭
            animIconFar.enabled = false;
            traIcon.gameObject.SetActive(true);

            yield return 0;
            //UI变化结束
            bUIChanging = false;
        }
        /// <summary>
        /// 中距离=>近距离
        /// </summary>
        IEnumerator IEMiddleToClose()
        {

            //UI开始变化
            bUIChanging = true;

            //中距离=>近距离

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.zero, fIconSpeed * 2f * Time.deltaTime);
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, Vector3.one, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traTotalUI.localScale, Vector3.one);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.zero;
                    traTotalUI.localScale = Vector3.one;
                    break;
                }
                yield return 0;
            }

            if (bDetailing == false)
            {
                //===========================================================================



                //这里应该从Oppo的Marker调用
                StartCoroutine(IEMarker());



                //===========================================================================
            }

            //UI变化结束
            bUIChanging = false;
        }

        /// <summary>
        /// 近距离=>中距离
        /// </summary>
        IEnumerator IECloseToMiddle(bool bTalkOver)
        {
            //UI开始变化
            bUIChanging = true;

            //近距离=>中距离

            while (true)
            {
                traIcon.localScale = Vector3.Lerp(traIcon.localScale, Vector3.one, fIconSpeed * 2f * Time.deltaTime);
                traTotalUI.localScale = Vector3.Lerp(traTotalUI.localScale, Vector3.zero, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(traTotalUI.localScale, Vector3.zero);
                if (_fDis < fThreshold)
                {
                    traIcon.localScale = Vector3.one;
                    traTotalUI.localScale = Vector3.zero;
                    break;
                }
                yield return 0;
            }


            //UI变化结束
            bUIChanging = false;
        }

        #region Icon变化，远距离（大于5米，静态）（小于5米，大于1.5米，动态）
        [Header("===Icon变化，原距离（大于5米，或者小于5米，大于1.5米）")]
        //Icon的对象
        public Transform traIcon;
        //吸引态，上下移动动画
        public Animator animIconFar;
        //轻交互，半球动画+音符动画
        public Animator[] animIconMiddle;
        //Icon的移动速度
        public float fIconSpeed = 1;
        #endregion

        #region 重交互，大UI，近距离（小于1.5米）
        [Header("===重交互，大UI，近距离（小于1.5米）")]
        //UI的变化速度
        public float fUISpeed = 5;
        //小UI
        public Transform traTotalUI;

        //定位动画
        public Animator animMarker;
        //扫描查看动画
        public Animator animCheck;
        //查看详情按钮
        public Button btnCheckDetail;
        //退出详情按钮
        public Button btnQuit;
        //定位到的图片
        public Image imgPicture;
        //定位到的图片的详情
        public Image imgDetail;
        //正在显示详情界面中
        public bool bDetailing;

        IEnumerator IEMarker()
        {
            //Marker动画开启
            animMarker.gameObject.SetActive(true);
            animMarker.enabled = true;

            //===========================================================================
            //这里应该从Marker获取位置信息

            yield return new WaitForSeconds(3);

            //===========================================================================
            Transform _traBtnCheckDetail = btnCheckDetail.transform;
            Vector3 _v3 = Vector3.one;
            while (true)
            {
                _traBtnCheckDetail.localScale = Vector3.Lerp(_traBtnCheckDetail.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(_traBtnCheckDetail.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    _traBtnCheckDetail.localScale = _v3;
                    break;
                }
                yield return 0;
            }

            //Marker动画关闭
            animMarker.enabled = false;
            animMarker.gameObject.SetActive(false);
        }

        /// <summary>
        /// 查看详情按钮响应
        /// </summary>
        void OnCheckDetail()
        {
            StopAllCoroutines();
            StartCoroutine("IECheckDetail");
        }

        IEnumerator IECheckDetail()
        {
            Transform _traBtnCheckDetail = btnCheckDetail.transform;
            Vector3 _v3 = Vector3.zero;
            while (true)
            {
                _traBtnCheckDetail.localScale = Vector3.Lerp(_traBtnCheckDetail.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(_traBtnCheckDetail.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    _traBtnCheckDetail.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            //扫描动画开启
            animCheck.gameObject.SetActive(true);
            animCheck.enabled = true;
            yield return new WaitForSeconds(3);
            //扫描动画关闭
            animCheck.enabled = false;
            animCheck.gameObject.SetActive(false);

            Transform _traImgDetail = imgDetail.transform;
            _v3 = Vector3.one;
            while (true)
            {
                _traImgDetail.localScale = Vector3.Lerp(_traImgDetail.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(_traImgDetail.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    _traImgDetail.localScale = _v3;
                    break;
                }
                yield return 0;
            }
            btnQuit.gameObject.SetActive(true);

            bDetailing = true;
        }

        /// <summary>
        /// 关闭详情界面响应
        /// </summary>
        void OnQuit()
        {
            StopAllCoroutines();
            StartCoroutine("IEQuit");
        }

        IEnumerator IEQuit()
        {
            bDetailing = false;

            btnQuit.gameObject.SetActive(false);

            //Marker动画开启
            animMarker.gameObject.SetActive(false);
            animMarker.enabled = false;
            animCheck.gameObject.SetActive(false);
            animCheck.enabled = false;

            Transform _traImgDetail = imgDetail.transform;
            Vector3 _v3 = Vector3.zero;
            while (true)
            {
                _traImgDetail.localScale = Vector3.Lerp(_traImgDetail.localScale, _v3, fUISpeed * Time.deltaTime);
                float _fDis = Vector3.Distance(_traImgDetail.localScale, _v3);
                if (_fDis < fThreshold)
                {
                    _traImgDetail.localScale = _v3;
                    break;
                }
                yield return 0;
            }
        }

        #endregion

    }
}