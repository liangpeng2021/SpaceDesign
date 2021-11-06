///* Create by zh at 2021-09-17

//    音乐的动画控制（封面图切换等）

// */

//using UnityEngine;

//namespace SpaceDesign.Music
//{
//    public class EachMusicAnim : MonoBehaviour
//    {
//        //类型
//        public MusicAnimType musicPicType;

//        //要运动的子节点的属性
//        public EachMusicAttr emaCurChild;
//        //要运动的子节点
//        private Transform traChild;
//        //子节点的材质球
//        [SerializeField]
//        private MeshRenderer renderChild;

//        public bool bFadeHide = false;
//        public bool bFadeShow = false;

//        private float _fFadeSpeed { get { return MusicManage.Inst.fFadeSpeed; } }
//        private float _fPosSpeed { get { return MusicManage.Inst.fPosSpeed; } }
//        private float _fRotSpeed { get { return MusicManage.Inst.fRotSpeed; } }

//        public void Init(EachMusicAnim empNext)
//        {
//            if (empNext == null)
//            {
//                Debug.LogError("前后值为空！");
//                return;
//            }

//            Transform _tra = transform.GetChild(0);
//            _tra.SetParent(empNext.transform);
//            _tra.SetAsLastSibling();
//            emaCurChild = null;
//            traChild = null;
//            renderChild = null;
//        }

//        public void SetValue()
//        {
//            if (emaCurChild == null)
//            {
//                emaCurChild = transform.GetChild(0).GetComponent<EachMusicAttr>();
//                traChild = emaCurChild.transform;
//                renderChild = traChild.GetComponent<MeshRenderer>();
//            }

//            //===========================================================================
//            //椭圆形的动画，两边的不隐藏了
//            //bFadeHide = false;
//            //bFadeShow = false;

//            //if (MusicManage.Inst.bClockWise)
//            //{
//            //    //隐藏，控制的是最边上的（包括不显示的）
//            //    bFadeHide = (musicPicType == MusicAnimType.AlLeft);
//            //    //显示，控制的是显示中的两边（不包括不显示的）
//            //    bFadeShow = (musicPicType == MusicAnimType.ShowRight);
//            //}
//            //else
//            //{
//            //    //隐藏，控制的是最边上的（包括不显示的）
//            //    bFadeHide = (musicPicType == MusicAnimType.AllRight);
//            //    //显示，控制的是显示中的两边（不包括不显示的）
//            //    bFadeShow = (musicPicType == MusicAnimType.ShowLeft);
//            //}

//            //Color _c = renderChild.sharedMaterial.GetColor("_MainColor");

//            //if (musicPicType == MusicAnimType.Center || musicPicType == MusicAnimType.Other)
//            //{
//            //    //中间的几块区域，颜色设置非半透
//            //    //（否则：切换过快，渐隐渐显未完成，中间区域也是半透效果）
//            //    _c.a = 1;
//            //}
//            //else if (musicPicType == MusicAnimType.AllRight || musicPicType == MusicAnimType.AlLeft)
//            //{
//            //    //要显示的那个，位置需要瞬移过去
//            //    //并且设置为透明
//            //    if (bFadeHide == false)
//            //    {
//            //        traChild.localPosition = Vector3.zero;
//            //        traChild.localEulerAngles = Vector3.zero;
//            //        _c.a = 0;
//            //    }
//            //}
//            //SetPropBlock(_c);
//            //===========================================================================
//        }
//        /// <summary>
//        /// 刷新位置
//        /// </summary>
//        public bool RefreshMotion()
//        {
//            bool _bFinish = false;

//            //===========================================================================
//            //椭圆形的动画，两边的不隐藏了
//            //Color _c = GetPropBlockColor();
//            //if (bFadeShow)
//            //{
//            //    _c.a = Mathf.Lerp(_c.a, 1, _fFadeSpeed);
//            //    SetPropBlock(_c);

//            //    if (_c.a >= 0.95f)
//            //    {
//            //        _c.a = 1;
//            //        SetPropBlock(_c);

//            //    }

//            //}
//            //else if (bFadeHide)
//            //{
//            //    _c.a = Mathf.Lerp(_c.a, 0, _fFadeSpeed);
//            //    SetPropBlock(_c);

//            //    if (_c.a <= 0.05f)
//            //    {
//            //        _c.a = 0;
//            //        SetPropBlock(_c);
//            //    }
//            //}
//            //===========================================================================


//            traChild.localPosition = Vector3.Lerp(traChild.localPosition, Vector3.zero, _fPosSpeed);
//            traChild.localRotation = Quaternion.Lerp(traChild.localRotation, Quaternion.identity, _fRotSpeed);
//            traChild.localScale = Vector3.Lerp(traChild.localScale, v3Scale, _fPosSpeed);

//            float _f1 = Vector3.Distance(traChild.localPosition, Vector3.zero);
//            float _f2 = Vector3.Distance(traChild.localEulerAngles, Vector3.zero);
//            if (_f1 < 0.01f && _f2 < 0.01f)
//            {
//                traChild.localPosition = Vector3.zero;
//                traChild.localEulerAngles = Vector3.zero;
//                traChild.localScale = v3Scale;
//                _bFinish = true;
//            }

//            //print($"{transform.name}:+{_bFinish}");

//            return _bFinish;
//        }
//        Vector3 v3Scale = new Vector3(320f, 320f, 1f);

//        ////优化render
//        //public MaterialPropertyBlock matPropBlock;

//        ///// <summary>
//        ///// 设置材质属性，自定义颜色
//        ///// </summary>
//        //public void SetPropBlock(Color c)
//        //{
//        //    if (renderChild == null)
//        //        return;
//        //    matPropBlock = new MaterialPropertyBlock();
//        //    renderChild.GetPropertyBlock(matPropBlock);
//        //    matPropBlock.SetColor("_MainColor", c);
//        //    renderChild.SetPropertyBlock(matPropBlock);
//        //}
//        //public Color GetPropBlockColor()
//        //{
//        //    if (renderChild == null)
//        //        return Color.white;
//        //    matPropBlock = new MaterialPropertyBlock();
//        //    renderChild.GetPropertyBlock(matPropBlock);
//        //    return matPropBlock.GetColor("_MainColor");
//        //}
//    }
//}