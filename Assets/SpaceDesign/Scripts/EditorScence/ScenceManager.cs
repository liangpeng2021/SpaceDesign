using OXRTK.ARHandTracking;
using SpaceDesign;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 场景管理类，/*create by 梁鹏 2021-10-10 */
/// </summary>
namespace SpaceDesign
{
    public class ScenceManager : MonoBehaviour
    {
        #region 翻页
        /// <summary>
        /// 每页数量
        /// </summary>
        int pageCount;
        /// <summary>
        /// 场景列表界面
        /// </summary>
        public ButtonRayReceiver[] scenceBtns;
        Text[] texts;
        Material[] materials;

        /// <summary>
        /// 总页数
        /// </summary>
        int totalPageScenceNum = -1;
        /// <summary>
        /// 当前在编辑的场景所在页，每页3个
        /// </summary>
        int curPageScenceId = 0;
        /// <summary>
        /// 当前在编辑的场景在当前页的下标
        /// </summary>
        int curScenceBtnIndex = -1;

        /// <summary>
        /// 页数
        /// </summary>
        public Text pageNumText;
        /// <summary>
        /// 翻到上页
        /// </summary>
        public ButtonRayReceiver turnPageLastBtn;
        /// <summary>
        /// 翻到下页
        /// </summary>
        public ButtonRayReceiver turnPageNextBtn;
        /// <summary>
        /// 选中后出现对应按钮
        /// </summary>
        public Transform seletTran;
        /// <summary>
        /// 翻到上一页
        /// </summary>
        void TurnLastPage()
        {
            //选中状态去掉
            if (curScenceBtnIndex > -1 && curScenceBtnIndex < materials.Length)
                materials[curScenceBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
            seletTran.gameObject.SetActive(false);
            //下一页按钮打开
            turnPageNextBtn.gameObject.SetActive(true);
            //当前页数修改
            curPageScenceId--;

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                //显示按钮
                scenceBtns[i].gameObject.SetActive(true);
                //更新名字
                texts[i].text = scenceDataList[curPageScenceId * pageCount + i].Label;
            }
            //如果是第一页，关掉上一页按钮
            if (curPageScenceId == 0)
                turnPageLastBtn.gameObject.SetActive(false);
            ///更新页数显示
            pageNumText.text = (curPageScenceId + 1).ToString() + "/" + (totalPageScenceNum + 1).ToString();
        }
        /// <summary>
        /// 翻到下一页
        /// </summary>
        void TurnNextPage()
        {
            //选中状态去掉
            if (curScenceBtnIndex > -1 && curScenceBtnIndex < materials.Length)
                materials[curScenceBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
            seletTran.gameObject.SetActive(false);
            //上一页按钮打开
            turnPageLastBtn.gameObject.SetActive(true);
            //当前页数修改
            curPageScenceId++;
            //当前页还剩多少个
            int index = scenceDataList.Count - 1 - curPageScenceId * pageCount;

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                //没满的关掉
                if (i > index)
                {
                    scenceBtns[i].gameObject.SetActive(false);
                }
                else
                {
                    //满的更新
                    scenceBtns[i].gameObject.SetActive(true);
                    texts[i].text = scenceDataList[curPageScenceId * pageCount + i].Label;
                }
            }
            //如果是最后一页，下一页按钮关掉
            if (curPageScenceId == totalPageScenceNum)
                turnPageNextBtn.gameObject.SetActive(false);
            //更新页数显示
            pageNumText.text = (curPageScenceId + 1).ToString() + "/" + (totalPageScenceNum + 1).ToString();
        }

        /// <summary>
        /// 设置页面内的按钮
        /// </summary>
        void SetScenceBtnState()
        {
            //控制按钮显示和隐藏
            int index = (scenceDataList.Count - 1) % pageCount;

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                if (i > (scenceDataList.Count - 1 - curPageScenceId * pageCount))
                    scenceBtns[i].gameObject.SetActive(false);
                else
                {
                    scenceBtns[i].gameObject.SetActive(true);

                    //名称赋值
                    texts[i].text = scenceDataList[curPageScenceId * pageCount + i].Label;
                }
            }

            //翻页和页码更新
            if (index >= 0)
            {
                if (curPageScenceId == 0)
                {
                    turnPageLastBtn.gameObject.SetActive(false);
                }
                else
                    turnPageLastBtn.gameObject.SetActive(true);

                if (curPageScenceId == totalPageScenceNum)
                    turnPageNextBtn.gameObject.SetActive(false);
                else
                    turnPageNextBtn.gameObject.SetActive(true);

                pageNumText.gameObject.SetActive(true);
                pageNumText.text = (curPageScenceId + 1).ToString() + "/" + (totalPageScenceNum + 1).ToString();
            }
            else
            {
                pageNumText.gameObject.SetActive(false);
                for (int i = 0; i < scenceBtns.Length; i++)
                {
                    scenceBtns[i].gameObject.SetActive(false);
                }

                turnPageLastBtn.gameObject.SetActive(false);
                turnPageNextBtn.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 赋值总页数
        /// </summary>
        /// <param name="count"></param>
        void SetTotalPageNum(int count)
        {
            if (count == 0)
            {
                totalPageScenceNum = -1;
                return;
            }

            if (count % pageCount == 0)
            {
                totalPageScenceNum = count / pageCount - 1;
            }
            else
                totalPageScenceNum = count / pageCount;
        }

        void InitPage()
        {
            pageCount = scenceBtns.Length;
            texts = new Text[scenceBtns.Length];
            materials = new Material[scenceBtns.Length];

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                texts[i] = scenceBtns[i].transform.Find("Text (TMP)").GetComponent<Text>();
                materials[i] = scenceBtns[i].transform.Find("Cube/BackBoard").GetComponent<MeshRenderer>().material;
            }

            seletTran.gameObject.SetActive(false);
            pageNumText.gameObject.SetActive(false);
            turnPageLastBtn.gameObject.SetActive(false);
            turnPageNextBtn.gameObject.SetActive(false);

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                scenceBtns[i].gameObject.SetActive(false);
            }

            isInitPage = true;
        }
        #endregion

        /// <summary>
        /// 创建场景
        /// </summary>
        public ButtonRayReceiver createScenceBtn;
        /// <summary>
        /// 加载场景
        /// </summary>
        public ButtonRayReceiver downloadScenceBtn;
        /// <summary>
        /// 上传场景
        /// </summary>
        public ButtonRayReceiver uploadScenceBtn;
        /// <summary>
        /// 配置场景数据到本地
        /// </summary>
        public ButtonRayReceiver configScenceBtn;

        /// <summary>
        /// 删除场景数据
        /// </summary>
        public ButtonRayReceiver deleteScenceBtn;

        [HideInInspector]
        public List<ScenceData> scenceDataList = new List<ScenceData>();
        /// <summary>
        /// 跳转到主场景
        /// </summary>
        public ButtonRayReceiver mainloadBtn;
        /// <summary>
        /// 公共的场景文件URL地址
        /// </summary>
        string publicScenceUrl;

        UserData userData;
        bool isInitPage = false;
        private void Start()
        {
            if (!isInitPage)
                InitPage();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i] = null;
            }
            texts = null;

            createScenceBtn = null;
            downloadScenceBtn = null;
            uploadScenceBtn = null;
            configScenceBtn = null;
            deleteScenceBtn = null;

            publicScenceUrl = null;

            for (int i = 0; i < scenceDataList.Count; i++)
            {
                scenceDataList[i].Clear();
                scenceDataList[i] = null;
            }
            scenceDataList = null;

            userData.Clear();

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                scenceBtns[i] = null;
            }
            scenceBtns = null;
        }

        private void OnEnable()
        {
            AddScenceEvent();
        }

        private void OnDisable()
        {
            RemoveScenceEvent();
        }

        void AddScenceEvent()
        {
            createScenceBtn.onPinchDown.AddListener(CreateScence);
            downloadScenceBtn.onPinchDown.AddListener(DownLoadScence);
            uploadScenceBtn.onPinchDown.AddListener(UploadScence);
            configScenceBtn.onPinchDown.AddListener(ConfigScence);
            deleteScenceBtn.onPinchDown.AddListener(DeleteScence);

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                int num = i;
                scenceBtns[i].onPinchDown.AddListener(() => { ShowScenceIcon(num); });
            }

            turnPageLastBtn.onPinchDown.AddListener(TurnLastPage);
            turnPageNextBtn.onPinchDown.AddListener(TurnNextPage);

            mainloadBtn.onPinchDown.AddListener(LoadMainScence);
        }

        void LoadMainScence()
        {
            //------------ Modify by zh ------------
            PlayerManage.InitPlayerPosEvt();
            //------------------End------------------
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        void RemoveScenceEvent()
        {
            createScenceBtn.onPinchDown.RemoveListener(CreateScence);
            downloadScenceBtn.onPinchDown.RemoveListener(DownLoadScence);
            uploadScenceBtn.onPinchDown.RemoveListener(UploadScence);
            configScenceBtn.onPinchDown.RemoveListener(ConfigScence);
            deleteScenceBtn.onPinchDown.RemoveListener(DeleteScence);

            for (int i = 0; i < scenceBtns.Length; i++)
            {
                scenceBtns[i].onPinchDown.RemoveAllListeners();
            }

            turnPageLastBtn.onPinchDown.RemoveAllListeners();
            turnPageNextBtn.onPinchDown.RemoveAllListeners();
            mainloadBtn.onPinchDown.RemoveAllListeners();
        }

        public void LoadUserScenceList(UserData ud)
        {
            if (!isInitPage)
                InitPage();

            this.userData = ud;

            //公共地址
            publicScenceUrl = ud.scn_url;
            //赋值总页数
            SetTotalPageNum(ud.scn_data.Length);

            for (int i = 0; i < ud.scn_data.Length; i++)
            {
                string label = ud.scn_data[i];
                //开启新协程
                IEnumerator _ieGetfile = YoopInterfaceSupport.GetScenceFileFromURL(publicScenceUrl + label + "/scence.scn",
                    //回调
                    (sd) =>
                    {
                    //数据添加
                    scenceDataList.Add(sd);
                    //修改场景按钮状态
                    SetScenceBtnState();
                    }
                    ,
                    (str) =>
                    {
                        Debug.Log("ScenceFile:" + str);
                    });

                ActionQueue.InitOneActionQueue().AddAction(_ieGetfile).StartQueue();
            }
        }

        /// <summary>
        /// 当前行，选中其中一列
        /// </summary>
        /// <param name="index"></param>
        void ShowScenceIcon(int index)
        {
            //表示选中状态
            seletTran.gameObject.SetActive(true);
            for (int i = 0; i < scenceBtns.Length; i++)
            {
                if (i == index)
                {
                    curScenceBtnIndex = i;
                    seletTran.position = scenceBtns[i].transform.position;
                    materials[i].SetColor("_BaseColor", EditorControl.Instance.chooseColor);
                }
                else
                {
                    materials[i].SetColor("_BaseColor", EditorControl.Instance.normalColor);
                }
            }
        }
        /// <summary>
        /// 去到起名界面，或者从起名界面回来
        /// </summary>
        void ToOrBackName(bool isToName)
        {
            if (isToName)
            {
                //打开起名界面
                EditorControl.Instance.setName.gameObject.SetActive(true);
                EditorControl.Instance.keyBoardManager.gameObject.SetActive(true);
                transform.parent.gameObject.SetActive(false);
                //EditorControl.Instance.editBtn.transform.parent.parent.gameObject.SetActive(false);
            }
            else
            {
                //恢复界面
                transform.parent.gameObject.SetActive(true);
                EditorControl.Instance.setName.gameObject.SetActive(false);
                EditorControl.Instance.keyBoardManager.gameObject.SetActive(false);
                //EditorControl.Instance.editBtn.transform.parent.parent.gameObject.SetActive(true);
            }
        }
        void CreateScence()
        {
            EditorControl.Instance.setName.StartSetName(
                () =>
                {
                //恢复到场景管理界面
                ToOrBackName(false);
                },
            (name) => {
            //数据添加
            ScenceData scenceData = new ScenceData();
                scenceData.Label = name;
                scenceDataList.Add(scenceData);
            //赋值总页数
            SetTotalPageNum(scenceDataList.Count);

            //控制整体按钮显示和隐藏
            SetScenceBtnState();

            //恢复到场景管理界面
            ToOrBackName(false);
            });
            //打开起名界面
            ToOrBackName(true);
        }

        void DownLoadScence()
        {
            int curScenceId = (curPageScenceId * pageCount + curScenceBtnIndex);
            if (curScenceId <= -1)
                return;
            EditorControl.Instance.ToEditRoom();

            EditorControl.Instance.roomManager.LoadScenceData(scenceDataList[curScenceId]);
        }

        void UploadScence()
        {
            int curScenceId = (curPageScenceId * pageCount + curScenceBtnIndex);
            if (curScenceId <= -1)
                return;

            WWWForm wwwFrom = new WWWForm();
            wwwFrom.AddField("username", userData.username);
            wwwFrom.AddField("label", scenceDataList[curScenceId].Label);

            byte[] scenceByte = MyFormSerial(scenceDataList[curScenceId]);
            //根据自己长传的文件修改格式
            wwwFrom.AddBinaryData("game_file", scenceByte, "scence.scn");

            IEnumerator enumerator = YoopInterfaceSupport.Instance.UploadFile<YanzhengData>(wwwFrom, InterfaceName.uploadscencefile,
                (ud) =>
                {
                    if (ud.state)
                    {
                        EditorControl.Instance.ShowTipTime("上传成功", 2f);
                    }
                    else
                    {
                        EditorControl.Instance.ShowTipTime("上传失败:" + ud.error, 2f);
                    }
                });
            ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }
        /// <summary>
        /// 根据数据生成覆盖本地配置文件
        /// </summary>
        void ConfigScence()
        {
            int curScenceId = (curPageScenceId * pageCount + curScenceBtnIndex);
            if (curScenceId <= -1)
                return;

            //BitConverter方式
            MySerial(EditorControl.Instance.path, scenceDataList[curScenceId]);
            EditorControl.Instance.ShowTipTime("已配置场景数据", 2f);
        }
        /// <summary>
        /// 删除场景，删除内存和服务器的
        /// </summary>
        void DeleteScence()
        {
            int curScenceId = (curPageScenceId * pageCount + curScenceBtnIndex);
            if (curScenceId <= -1)
                return;

            if (curScenceBtnIndex > -1 && curScenceBtnIndex < materials.Length)
                materials[curScenceBtnIndex].SetColor("_BaseColor", EditorControl.Instance.normalColor);
            //如果删的是第一个位置的，更新当前页
            if (curScenceBtnIndex == 0)
            {
                curScenceBtnIndex = pageCount - 1;
                curPageScenceId--;
                if (curPageScenceId < 0)
                    curPageScenceId = 0;
            }
            //删除服务器数据
            WWWForm wwwFrom = new WWWForm();
            wwwFrom.AddField("username", userData.username);

            wwwFrom.AddField("label", scenceDataList[curScenceId].Label);

            //删除链表中的数据
            scenceDataList[curScenceId].Clear();
            scenceDataList.RemoveAt(curScenceId);

            //赋值总页数
            SetTotalPageNum(scenceDataList.Count);
            //如果总页数到头了
            if (scenceDataList.Count == 0)
            {
                curScenceBtnIndex = -1;
            }
            SetScenceBtnState();
            seletTran.gameObject.SetActive(false);

            IEnumerator enumerator = YoopInterfaceSupport.Instance.GetHttpData<YanzhengData>(wwwFrom, InterfaceName.deletescencefile,
                (ud) =>
                {
                    if (ud.state)
                    {
                        EditorControl.Instance.ShowTipTime("删除场景成功", 2f);
                    }
                    else
                    {
                        Debug.Log("MyLog::删除失败" + ud.error);
                    }
                });
            ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
        }

        /// <summary>
        /// 序列化（存储path路径下的文件），将数据存储到文件
        /// </summary>
        void MySerial(string path, ScenceData gameObjectDatas)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, gameObjectDatas);
            }
            //fs.Flush();
            //fs.Close();
            //fs.Dispose();
        }

        /// <summary>
        /// 序列化
        /// </summary>
        byte[] MyFormSerial(ScenceData gameObjectDatas)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, gameObjectDatas);

                ms.Position = 0;
                bytes = ms.GetBuffer();

                ms.Read(bytes, 0, bytes.Length);
            }

            return bytes;
        }
    }
}