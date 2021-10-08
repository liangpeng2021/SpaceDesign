using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using XR;

[RequireComponent(typeof(CopyFiles))]
public class ImageTargetTest : MonoBehaviour
{
    [HideInInspector] public Vector3 m_TargetSize = Vector3.one;

    private bool m_isStart, m_isAoCTracker, m_isGetInfo = false;

    private Image2DTracking.Image2DTrackerInfo m_TrackerInfo;

    private Coroutine CurrentCoroutine;

    public delegate void TargetStatusDelegate();

    public TargetStatusDelegate OnLossTarget, OnFindTarget;

    public delegate void TrackingStatusDelegate();

    public TrackingStatusDelegate OnAddTacker, OnGetTrackerInfo, OnChangeTarcker, OnStart, OnStop;

    private bool m_isFind = false;

    /// <summary>
    /// 启动Track算法
    /// </summary>
    public void TrackStart(string m_TrackerPath, string m_feamName)
    {
        AddTracker(m_TrackerPath, m_feamName);
        var xrError = Image2DTracking.Start();
        if (xrError != XRError.XR_ERROR_SUCCESS)
        {
            LogPrint("Start Fail");
            m_isStart = false;
            return;
        }
        else
        {
            LogPrint("Start Success", false);
            m_isStart = true;
        }

        GetTrackerInfo();

        OnStart?.Invoke();


        StartCoroutine(TrackingIEnumerator());
        /*if (CurrentCoroutine == null)
            CurrentCoroutine = StartCoroutine(TrackingIEnumerator());*/
    }

    /// <summary>
    /// 停止Track算法
    /// </summary>
    public void TrackStop()
    {
        var xrError = Image2DTracking.Stop();
        if (xrError != XRError.XR_ERROR_SUCCESS)
        {
            LogPrint("Stop Fail");
        }
        else
        {
            LogPrint("Stop Success", false);
            m_isStart = false;

            OnStop?.Invoke();
        }
    }

    private string _Path;

    /// <summary>
    /// 添加TrackerModel
    /// </summary>
    /// <param name="TarckerPath">TrackerModel路径</param>
    private void AddTracker(string TarckerPath, string m_feamName)
    {
        if (string.IsNullOrEmpty(TarckerPath))
        {
            LogPrint("Please add tracker");
            return;
        }

        if (string.IsNullOrEmpty(m_feamName))
        {
            LogPrint("Please add feamFile");
            return;
        }

        string feamFile = Application.streamingAssetsPath + "/TrackModel/" + TarckerPath + "/" + m_feamName +
                          ".feam";
        string mlistFile = Application.streamingAssetsPath + "/TrackModel/" + TarckerPath + "/" + "targets" +
                           ".mlist";
        _Path = Application.persistentDataPath + "/TrackModel/" + TarckerPath;
        if (Directory.Exists(_Path))
        {
            Directory.Delete(_Path, true);
        }

        CopyFiles.Copy(feamFile, _Path + "/" + m_feamName + ".feam");
        CopyFiles.Copy(mlistFile, _Path + "/" + "targets" + ".mlist");

        char[] modelpath_char = _Path.ToCharArray();
        var AddError = Image2DTracking.AddImage2DTracker(modelpath_char.Length, modelpath_char);
        if (AddError != XRError.XR_ERROR_SUCCESS)
        {
            LogPrint("Add Tracker Fail");
            LogPrint("path=" + _Path);
            m_isAoCTracker = false;
        }
        else
        {
            LogPrint("Add Tracker Success", false);
            m_isAoCTracker = true;

            OnAddTacker?.Invoke();
        }
    }

    /// <summary>
    /// 更换TrackerModel
    /// </summary>
    /// <param name="TarckerPath">TrackerModel路径</param>
    public void ChangeTracker(string TarckerPath)
    {
    }

    private void GetTrackerInfo()
    {
        var infoError = Image2DTracking.GetImage2DTrackerInfo(ref m_TrackerInfo);
        if (infoError != XRError.XR_ERROR_SUCCESS)
        {
            LogPrint("Get TrackerInfo Fail");
            m_isGetInfo = false;
        }
        else
        {
            LogPrint("Get TrackerInfo Success", false);
            m_isGetInfo = true;

            OnGetTrackerInfo?.Invoke();
        }
    }


    private PoseState m_PoseState;
    Quaternion m_targetQuaternion;
    Vector3 m_targetPosition;

    private Quaternion correctQuaternion = Quaternion.Euler(-90, 180, 0);


    private Vector3 lastVec3 = Vector3.zero;
    private Vector3 PoseVec3 = Vector3.zero;
    private float limitNum = 0.5f;

    private IEnumerator TrackingIEnumerator()
    {
        while (true)
        {
            if (m_isStart & m_isAoCTracker & m_isGetInfo)
            {
                yield return new WaitForSeconds(0.01f);
                XRError getPoseError =
                    Image2DTracking.GetImage2DTrackerPos(m_TrackerInfo.trakerId, ref m_PoseState);
                if (getPoseError == XRError.XR_ERROR_SUCCESS)
                {
                    LogPrint("视野中", false);
                    m_targetQuaternion.w = m_PoseState.rotation.w;
                    m_targetQuaternion.x = -m_PoseState.rotation.x;
                    m_targetQuaternion.y = -m_PoseState.rotation.y;
                    m_targetQuaternion.z = m_PoseState.rotation.z;

                    m_targetPosition.x = m_PoseState.position.x;
                    m_targetPosition.y = m_PoseState.position.y;
                    m_targetPosition.z = -m_PoseState.position.z;

                    PoseVec3 = m_targetPosition;
                    lastVec3 = filter(lastVec3, PoseVec3, limitNum);

                    transform.localRotation = m_targetQuaternion * correctQuaternion;
                    gameObject.transform.localPosition = lastVec3;

                    float minScale = m_TrackerInfo.dimensionWidth > m_TrackerInfo.dimensionHeight
                        ? m_TrackerInfo.dimensionHeight
                        : m_TrackerInfo.dimensionWidth;

                    gameObject.transform.localScale = Vector3.one * minScale * 0.1f;
                    LogPrint("dimensionWidth:" + m_TrackerInfo.dimensionWidth.ToString("0.0000"));
                    LogPrint("dimensionHeight:" + m_TrackerInfo.dimensionHeight.ToString("0.0000"));
                    LogPrint("Quaternion:" + m_targetQuaternion.ToString("0.0000"), false);
                    LogPrint("Position:" + lastVec3.ToString("0.0000"), false);

                    if (!m_isFind)
                    {
                        OnFindTarget?.Invoke();
                        m_isFind = true;
                    }
                }
                else
                {
                    if (m_isFind)
                    {
                        LogPrint("视野外");
                        OnLossTarget?.Invoke();
                        m_isFind = false;
                    }
                }
            }
            else
            {
                LogPrint("Can`t Start Tracking");
                CurrentCoroutine = null;
                yield break;
            }
        }
    }

    private Vector3 filter(Vector3 lastVec3, Vector3 PoseVec3, float limitNum)
    {
        return limitNum * lastVec3 + (1 - limitNum) * PoseVec3;
    }
    
    private void LogPrint(string information, bool isError = true)
    {
        if (isError)
        {
            Debug.LogError("XR_Image2DTracking: " + information);
        }
        else
        {
            Debug.Log("XR_Image2DTracking: " + information);
        }
    }

    public void StarTrack1Destroy2()
    {
        //销毁
        //if (Image2DTrackingManager.Instance)
        //{
        //    Image2DTrackingManager.Instance.TrackStop();
        //    Destroy(Image2DTrackingManager.Instance.gameObject);
        //}
        //StartTrack1();

        //Invoke("StartTrack1", 1f);
        TrackStop();
        TrackStart("Model1", "3c11f9a3e98c523a45f23f07b76586ab_28062021145112");
    }
    public void StartTrack2Destroy1()
    {
        //if (Image2DTrackingManager.Instance)
        //{
        //    Image2DTrackingManager.Instance.TrackStop();
        //    Destroy(Image2DTrackingManager.Instance.gameObject);
        //}
        //StartTrack2();

        //Invoke("StartTrack2", 1f);
        TrackStop();
        TrackStart("Model2", "813132e91e1145de132859962d35046e_15072021175537");
    }
}
