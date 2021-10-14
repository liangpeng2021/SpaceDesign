/* Create by zh at 2021-09-17

    音乐的动画控制（封面图切换等）

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SpaceDesign.Music.MusicMaxMag;

namespace SpaceDesign.Music
{
    public class EachMusicAnim : MonoBehaviour
    {
        //当前播放的音乐（最中间的）
        public bool bNeedPlay;
        //类型
        public MusicAnimType musicPicType;

        //要运动的子节点的属性
        public EachMusicAttr emaCurChild;
        //要运动的子节点
        private Transform traChild;
        //子节点的图片颜色
        private Image imgChild;


        public bool bFadeHide = false;
        public bool bFadeShow = false;

        public void Init(EachMusicAnim empNext)
        {
            if (empNext == null)
            {
                Debug.LogError("前后值为空！");
                return;
            }

            Transform _tra = transform.GetChild(0);
            _tra.SetParent(empNext.transform);
            _tra.SetAsLastSibling();
            emaCurChild = null;
            traChild = null;
            imgChild = null;

        }

        public void SetValue()
        {
            if (emaCurChild == null)
            {
                emaCurChild = transform.GetChild(0).GetComponent<EachMusicAttr>();
                traChild = emaCurChild.transform;
                imgChild = emaCurChild.image;
            }

            bFadeHide = false;
            bFadeShow = false;

            if (MusicMaxMag.Inst.bClockWise)
            {
                //隐藏，控制的是最边上的（包括不显示的）
                bFadeHide = (musicPicType == MusicAnimType.AlLeft);
                //显示，控制的是显示中的两边（不包括不显示的）
                bFadeShow = (musicPicType == MusicAnimType.ShowRight);
            }
            else
            {
                //隐藏，控制的是最边上的（包括不显示的）
                bFadeHide = (musicPicType == MusicAnimType.AllRight);
                //显示，控制的是显示中的两边（不包括不显示的）
                bFadeShow = (musicPicType == MusicAnimType.ShowLeft);
            }

            Color _c = imgChild.color;


            if (musicPicType == MusicAnimType.Middle)
            {
                //中间的几块区域，颜色设置非半透
                //（否则：切换过快，渐隐渐显未完成，中间区域也是半透效果）
                _c.a = 1;
            }
            else if (musicPicType == MusicAnimType.AllRight || musicPicType == MusicAnimType.AlLeft)
            {
                //要显示的那个，位置需要瞬移过去
                //并且设置为透明
                if (bFadeHide == false)
                {
                    traChild.localPosition = Vector3.zero;
                    traChild.localEulerAngles = Vector3.zero;
                    _c.a = 0;
                }
            }

            imgChild.color = _c;
        }

        /// <summary>
        /// 刷新位置
        /// </summary>
        public bool RefreshMotion()
        {
            bool _bFinish = false;

            if (bFadeShow)
            {
                Color _c = imgChild.color;
                _c.a = Mathf.Lerp(_c.a, 1, MusicMaxMag.Inst.fFadeSpeed);
                imgChild.color = _c;

                if (_c.a >= 0.95f)
                {
                    _c.a = 1;
                    imgChild.color = _c;
                }

            }
            else if (bFadeHide)
            {
                Color _c = imgChild.color;
                _c.a = Mathf.Lerp(_c.a, 0, MusicMaxMag.Inst.fFadeSpeed);
                imgChild.color = _c;

                if (_c.a <= 0.05f)
                {
                    _c.a = 0;
                    imgChild.color = _c;
                }
            }

            //if (musicPicType != MusicPicType.AllRight && musicPicType != MusicPicType.AlLeft)
            {

                traChild.localPosition = Vector3.Lerp(traChild.localPosition, Vector3.zero, MusicMaxMag.Inst.fPosSpeed);
                traChild.localRotation = Quaternion.Lerp(traChild.localRotation, Quaternion.identity, MusicMaxMag.Inst.fRotSpeed);

                float _f1 = Vector3.Distance(traChild.localPosition, Vector3.zero);
                float _f2 = Vector3.Distance(traChild.localEulerAngles, Vector3.zero);
                if (_f1 < 0.01f && _f2 < 0.01f)
                {
                    traChild.localPosition = Vector3.zero;
                    traChild.localEulerAngles = Vector3.zero;
                    _bFinish = true;
                }
            }

            //print($"{transform.name}:+{_bFinish}");


            return _bFinish;
        }
    }
}