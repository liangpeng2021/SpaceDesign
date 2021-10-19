///* Create by zh at 2021-09-17

//    音乐控制脚本（大界面：扩展态，有图片列表动画）

// */

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace SpaceDesign.Music
//{
//    public class MusicMaxMag : MonoBehaviour
//    {
//        //当前音乐播放状态
//        MusicPlayState musicPlayState
//        {
//            get { return MusicManage.Inst.curMusicPlayState; }
//            set { MusicManage.Inst.curMusicPlayState = value; }
//        }

//        //是否正在播放中
//        bool bPlaying
//        {
//            get { return MusicManage.Inst.bPlaying; }
//            set { MusicManage.Inst.bPlaying = value; }
//        }
//        //当前播放的时间进度
//        float fCurPlayTime
//        {
//            get { return MusicManage.Inst.fCurPlayTime; }
//            set { MusicManage.Inst.fCurPlayTime = value; }
//        }
//        //音乐播放
//        AudioSource audioSource
//        {
//            get { return MusicManage.Inst.audioSource; }
//        }

//        //当前音乐的序号
//        private int iCurMusicNum
//        {
//            get { return MusicManage.Inst.iCurMusicNum; }
//            set { MusicManage.Inst.iCurMusicNum = value; }
//        }
//        //音乐数据
//        private EachMusicAnim[] aryEachMusicAnim
//        {
//            get { return MusicManage.Inst.aryEachMusicAnim; }
//        }

//        //播放状态按钮
//        public Button btnState;
//        //左切换按钮
//        public Button btnLeft;
//        //右切换按钮
//        public Button btnRight;
//        //播放按钮
//        public Button btnPlay;
//        //暂停按钮
//        public Button btnPause;

//        //音乐播放的进度条
//        public Slider slidMusic;
//        //进度条手动拖拽中
//        public bool bSlideDragging;

//        //显示当前播放时间
//        public Text textCurPlayTime;
//        //当前播放音乐的总时长
//        public float fTotalPlayTime = 0.1f;
//        //显示当前播放音乐的总时长
//        public Text textTotalPlayTime;

//        //总的音乐个数
//        public int iTotalMusicNum = 7;
//        //显示当前音乐的序号
//        public Text textCurMusicNum;
//        //当前音乐的名称
//        public Text textCurMusicName;


//        ////渐隐渐显速度
//        //public float fFadeSpeed = 0.05f;
//        ////移动速度
//        //public float fPosSpeed = 0.04f;
//        ////旋转速度
//        //public float fRotSpeed = 0.08f;
//        //是否顺时针
//        public bool bClockWise = true;
//        //是否运行动画
//        private bool bGoAnim = false;

//        void Start()
//        {
//            //播放的循环，不用该值控制
//            audioSource.loop = false;

//            btnLeft.onClick.AddListener(OnLeft);
//            btnRight.onClick.AddListener(OnRight);
//            btnPlay.onClick.AddListener(OnPlay);
//            btnPause.onClick.AddListener(OnPause);
//            btnState.onClick.AddListener(OnState);

//            EventTrigger _trigger = slidMusic.GetComponent<EventTrigger>();
//            if (_trigger == null)
//                _trigger = slidMusic.gameObject.AddComponent<EventTrigger>();

//            EventTrigger.Entry _entry = new EventTrigger.Entry
//            {
//                eventID = EventTriggerType.PointerDown,
//                callback = new EventTrigger.TriggerEvent(),
//            };
//            _entry.callback.AddListener(x => { bSlideDragging = true; });
//            _trigger.triggers.Add(_entry);

//            _entry = new EventTrigger.Entry
//            {
//                eventID = EventTriggerType.PointerUp,
//                callback = new EventTrigger.TriggerEvent(),
//            };
//            _entry.callback.AddListener(x => { bSlideDragging = false; audioSource.time = slidMusic.value * fTotalPlayTime; });

//            _trigger.triggers.Add(_entry);

//            //开始的时候刷新一下数据
//            _SetCurMusicNum(1);
//            OnLeft();
//            OnRight();
//        }

//        void Update()
//        {
//            if (bGoAnim)
//            {
//                bGoAnim = false;
//                foreach (var v in aryEachMusicAnim)
//                {
//                    //有一个未完成，这里都要继续
//                    if (v.RefreshMotion() == false)
//                    {
//                        bGoAnim = true;
//                    }
//                }
//            }

//            if (bSlideDragging == false)
//            {
//                if (audioSource.isPlaying)
//                {
//                    _SetCurPlayTime(true);
//                }
//                else
//                {
//                    //播完了，判断状态，操作下一步
//                    if (audioSource.time >= fTotalPlayTime)
//                    {
//                        //print($"播完了，切换:{musicPlayState}");

//                        switch (musicPlayState)
//                        {
//                            case MusicPlayState.AllLoop:
//                                OnRight();
//                                break;
//                            case MusicPlayState.OneLoop:
//                                audioSource.Stop();
//                                audioSource.time = 0;
//                                audioSource.Play();
//                                break;
//                            case MusicPlayState.Order:
//                                if (iCurMusicNum < iTotalMusicNum)
//                                    OnRight();
//                                else
//                                {
//                                    audioSource.time = 0;
//                                    OnPause();
//                                }
//                                break;
//                        }
//                    }
//                }
//            }
//            else
//            {
//                _SetCurPlayTime(false);
//            }
//        }


//        /// <summary>
//        /// 刷新位置消息
//        /// </summary>
//        public void RefreshPos(PlayerPosState ips)
//        {

//            switch (ips)
//            {
//                case PlayerPosState.Far:

//                    break;
//                case PlayerPosState.Middle:

//                    break;
//                case PlayerPosState.Close:

//                    break;
//            }
//        }

//        /// <summary>
//        /// 播放状态按钮
//        /// </summary>
//        public void OnState()
//        {
//            musicPlayState = ((int)(musicPlayState)) == 2 ? 0 : musicPlayState + 1;
//            string _str = null;
//            switch (musicPlayState)
//            {
//                case MusicPlayState.AllLoop: _str = "全部循环"; break;
//                case MusicPlayState.OneLoop: _str = "单首循环"; break;
//                case MusicPlayState.Order: _str = "顺序播放"; break;
//            }

//            btnState.GetComponentInChildren<Text>().text = _str;
//        }

//        /// <summary>
//        /// 播放
//        /// </summary>
//        public void OnPlay()
//        {
//            bPlaying = true;
//            btnPause.gameObject.SetActive(bPlaying);
//            btnPlay.gameObject.SetActive(!bPlaying);
//            if (audioSource.time >= iTotalMusicNum)
//                audioSource.time = 0;
//            audioSource.Play();
//        }

//        /// <summary>
//        /// 暂停
//        /// </summary>
//        public void OnPause()
//        {
//            bPlaying = false;
//            btnPause.gameObject.SetActive(bPlaying);
//            btnPlay.gameObject.SetActive(!bPlaying);
//            audioSource.Pause();
//        }

//        /// <summary>
//        /// 向左切换按钮
//        /// </summary>
//        public void OnLeft()
//        {
//            _SetCurMusicNum(iCurMusicNum - 1);
//            bClockWise = true;
//            _InitEachMusicAnim();
//        }
//        /// <summary>
//        /// 向右切换按钮
//        /// </summary>
//        public void OnRight()
//        {
//            _SetCurMusicNum(iCurMusicNum + 1);
//            bClockWise = false;
//            _InitEachMusicAnim();
//        }

//        /// <summary>
//        /// 切换完后，初始化音乐（动画、音频等）
//        /// </summary>
//        void _InitEachMusicAnim()
//        {
//            if (audioSource.isPlaying)
//            {
//                audioSource.time = 0;
//                _SetCurPlayTime(true);
//                audioSource.Stop();
//            }

//            int _iLen = aryEachMusicAnim.Length;

//            if (bClockWise)
//            {
//                for (int i = 0; i < _iLen; i++)
//                {
//                    EachMusicAnim emp = null;
//                    if (i < _iLen - 1)
//                        emp = aryEachMusicAnim[i + 1];
//                    else if (i == (_iLen - 1))
//                        emp = aryEachMusicAnim[0];
//                    else
//                        break;
//                    aryEachMusicAnim[i].Init(emp);
//                }
//            }
//            else
//            {
//                for (int i = _iLen - 1; i >= 0; i--)
//                {
//                    EachMusicAnim emp = null;
//                    if (i > 0)
//                        emp = aryEachMusicAnim[i - 1];
//                    else if (i == 0)
//                        emp = aryEachMusicAnim[_iLen - 1];
//                    else
//                        break;
//                    aryEachMusicAnim[i].Init(emp);
//                }
//            }

//            foreach (var v in aryEachMusicAnim)
//            {
//                v.SetValue();
//                //最中间的音乐播放
//                if (v.musicPicType == MusicAnimType.Center)
//                {
//                    //bool _bAutoPlay = audioSource.isPlaying;
//                    //if (_bAutoPlay)
//                    //    audioSource.Stop();
//                    audioSource.time = 0;
//                    _SetCurPlayTime(true);
//                    textCurMusicName.text = v.emaCurChild.strName;
//                    audioSource.clip = v.emaCurChild.audioClip;
//                    fTotalPlayTime = audioSource.clip.length;
//                    textTotalPlayTime.text = ((int)(fTotalPlayTime / 60)).ToString("D2") + ":" + ((int)(fTotalPlayTime % 60)).ToString("D2");
//                    if (bPlaying)
//                        audioSource.Play();
//                }
//            }

//            bGoAnim = true;
//        }

//        //设置当前播放的时间
//        void _SetCurPlayTime(bool bSetSlider)
//        {
//            float _f = audioSource.time;
//            if (bSetSlider)
//                slidMusic.SetValueWithoutNotify(_f / fTotalPlayTime);
//            //fCurPlayTime = fTime;
//            //print($"{((int)(_f / 60))}----{((int)(_f % 60))}");
//            textCurPlayTime.text = ((int)(_f / 60)).ToString("D2") + ":" + ((int)(_f % 60)).ToString("D2");
//        }


//        /// <summary>
//        /// 设置并显示当前的音乐序号
//        /// </summary>
//        void _SetCurMusicNum(int iNum)
//        {
//            iCurMusicNum = iNum;
//            if (iNum <= 0)
//                iCurMusicNum = iTotalMusicNum;
//            else if (iNum > iTotalMusicNum)
//                iCurMusicNum = 1;

//            textCurMusicNum.text = iCurMusicNum.ToString();
//        }

//    }
//}