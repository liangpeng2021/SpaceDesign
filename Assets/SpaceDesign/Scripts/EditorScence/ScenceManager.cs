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
public class ScenceManager : MonoBehaviour
{
    /// <summary>
    /// 场景列表界面
    /// </summary>
    public Transform scenceParentTran;
    /// <summary>
    /// 2D场景icon预设
    /// </summary>
    public GameObject scence2DPrefab;
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

    /// <summary>
    /// 当前在编辑的场景下标
    /// </summary>
    int curScenceId = -1;

    int lastloadScenceID = -1;

    [HideInInspector]
    public List<ScenceData> scenceDataList = new List<ScenceData>();
    List<Image> scenceIconList = new List<Image>();
    ///// <summary>
    ///// 房间管理
    ///// </summary>
    //public RoomManager roomManager;
    
    /// <summary>
    /// 公共的场景文件URL地址
    /// </summary>
    string publicScenceUrl;

    UserData userData;
    private void OnDestroy()
    {
        scenceParentTran = null;
        scence2DPrefab = null;
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

        for (int i = 0; i < scenceIconList.Count; i++)
        {
            scenceIconList[i].GetComponent<ButtonRayReceiver>().onPinchDown.RemoveAllListeners();
            Destroy(scenceIconList[i].gameObject);
            scenceIconList[i] = null;
        }
        scenceIconList.Clear();
        scenceIconList = null;
        
        userData.Clear();
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
    }

    void RemoveScenceEvent()
    {
        createScenceBtn.onPinchDown.RemoveListener(CreateScence);
        downloadScenceBtn.onPinchDown.RemoveListener(DownLoadScence);
        uploadScenceBtn.onPinchDown.RemoveListener(UploadScence);
        configScenceBtn.onPinchDown.RemoveListener(ConfigScence);
        deleteScenceBtn.onPinchDown.RemoveListener(DeleteScence);
    }

    public void LoadUserScenceList(UserData ud)
    {
        this.userData = ud;

        //公共地址
        publicScenceUrl = ud.scn_url;
        //将上一个icon切换为非选中状态
        if (scenceIconList.Count > 0)
            scenceIconList[scenceIconList.Count - 1].color = Color.white;
        
        for (int i = 0; i < ud.scn_data.Length; i++)
        {
            string label = ud.scn_data[i];
            //开启新协程
            IEnumerator _ieGetfile = YoopInterfaceSupport.GetScenceFileFromURL(publicScenceUrl + label + "/scence.scn",
                //回调
                (sd) =>
                {
                    //表示非选中状态
                    if (scenceIconList.Count > 0)
                    {
                        curScenceId = scenceIconList.Count - 1;
                        scenceIconList[scenceIconList.Count - 1].color = Color.white;
                    }

                    //实例化2Dicon
                    GameObject obj = Instantiate(scence2DPrefab);
                    obj.transform.parent = scenceParentTran;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localEulerAngles = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    obj.transform.GetChild(0).GetComponent<Text>().text = label;
                    obj.name = label;

                    scenceIconList.Add(obj.GetComponent<Image>());
                    obj.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { ShowScenceIcon(label); });

                    scenceDataList.Add(sd);

                    //表示选中状态
                    if (scenceIconList.Count > 0)
                    {
                        curScenceId = scenceIconList.Count - 1;
                        scenceIconList[scenceIconList.Count - 1].color = Color.green;
                    }
                }
                ,
                (str) =>
                {
                    Debug.Log("ScenceFile:" + str);
                });

            ActionQueue.InitOneActionQueue().AddAction(_ieGetfile).StartQueue();
        }
    }

    void ShowScenceIcon(string name)
    {
        //表示选中状态
        for (int i = 0; i < scenceIconList.Count; i++)
        {
            if (scenceIconList[i].gameObject.name.Equals(name))
            {
                curScenceId = i;
                scenceIconList[i].color = Color.green;
            }
            else
            {
                scenceIconList[i].color = Color.white;
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
        }
        else
        {
            //恢复界面
            transform.parent.gameObject.SetActive(true);
            EditorControl.Instance.setName.gameObject.SetActive(false);
            EditorControl.Instance.keyBoardManager.gameObject.SetActive(false);
        }
    }
    void CreateScence()
    {
        EditorControl.Instance.setName.StartSetName(
            ()=>
            {
                //恢复到场景管理界面
                ToOrBackName(false);
            },
        (name)=>{
            if (scenceIconList.Count > 0)
                scenceIconList[scenceIconList.Count - 1].color = Color.white;
            //实例化icon
            GameObject obj = Instantiate(scence2DPrefab);
            obj.transform.parent = scenceParentTran;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            //表示选中状态
            scenceIconList.Add(obj.GetComponent<Image>());
            scenceIconList[scenceIconList.Count - 1].color = Color.green;

            obj.transform.GetChild(0).GetComponent<Text>().text = name;
            obj.name = name;

            ScenceData scenceData = new ScenceData();
            scenceData.Label = name;

            scenceDataList.Add(scenceData);
            //当前选中对象
            curScenceId = scenceIconList.Count - 1;
            //添加事件
            obj.GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(() => { ShowScenceIcon(name); });

            //恢复到场景管理界面
            ToOrBackName(false);
        });
        //打开起名界面
        ToOrBackName(true);
    }

    void DownLoadScence()
    {
        if (curScenceId == -1)
            return;
        EditorControl.Instance.ToEditRoom();
        if (lastloadScenceID == curScenceId)
            return;
        lastloadScenceID = curScenceId;
        
        EditorControl.Instance.roomManager.LoadScenceData(scenceDataList[curScenceId]);
    }

    void UploadScence()
    {
        if (curScenceId == -1)
            return;
        
        WWWForm wwwFrom = new WWWForm();
        wwwFrom.AddField("username",userData.username);
        wwwFrom.AddField("label", scenceDataList[curScenceId].Label);
        
        byte[] scenceByte = MyFormSerial(scenceDataList[curScenceId]);
        //根据自己长传的文件修改格式
        wwwFrom.AddBinaryData("game_file", scenceByte,"scence.scn");

        IEnumerator enumerator = YoopInterfaceSupport.UploadFile<YanzhengData>(wwwFrom, InterfaceName.uploadscencefile,
            (ud) =>
            {
                if (ud.state)
                {
                    EditorControl.Instance.ShowTipTime("上传成功", 2f);
                }
                else
                {
                    EditorControl.Instance.ShowTipTime("上传失败:"+ ud.error, 2f);
                }
            });
        ActionQueue.InitOneActionQueue().AddAction(enumerator).StartQueue();
    }
    /// <summary>
    /// 根据数据生成覆盖本地配置文件
    /// </summary>
    void ConfigScence()
    {
        if (curScenceId == -1)
            return;

        //BitConverter方式
        MySerial(EditorControl.Instance.path, scenceDataList[curScenceId]);
        EditorControl.Instance.ShowTipTime("已配置场景数据",2f);
    }
    /// <summary>
    /// 删除场景，删除内存和服务器的
    /// </summary>
    void DeleteScence()
    {
        if (curScenceId == -1)
            return;
        
        //删除服务器数据
        WWWForm wwwFrom = new WWWForm();
        wwwFrom.AddField("username", userData.username);

        wwwFrom.AddField("label", scenceDataList[curScenceId].Label);
        
        IEnumerator enumerator = YoopInterfaceSupport.GetHttpData<YanzhengData>(wwwFrom, InterfaceName.deletescencefile,
            (ud) =>
            {
                if (ud.state)
                {
                    //删除链表中的数据
                    scenceDataList[curScenceId].Clear();
                    scenceDataList.RemoveAt(curScenceId);

                    //删除icon
                    //清除点击事件
                    scenceIconList[curScenceId].GetComponent<ButtonRayReceiver>().onPinchDown.RemoveAllListeners();
                    Destroy(scenceIconList[curScenceId].gameObject);
                    scenceIconList.RemoveAt(curScenceId);

                    //选中其他对象
                    if (scenceIconList.Count > 0)
                    {
                        curScenceId = scenceIconList.Count - 1;
                        scenceIconList[scenceIconList.Count - 1].color = Color.green;
                    }
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
