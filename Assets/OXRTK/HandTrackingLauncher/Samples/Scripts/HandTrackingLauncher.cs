using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XR;
using OXRTK.ARHandTracking;
using TMPro;
using UnityEngine.Events;

public class HandTrackingLauncher : MonoBehaviour
{
    public Canvas uiCanvas;
    public GameObject buttonPrefab;    

    int m_SceneNum;

    LauncherButtonController[] m_Buttons;

    int m_BoarderHeight = 125;

    public string[] sceneNamesChn; 
    
    void Start()
    {        
        m_SceneNum = SceneManager.sceneCountInBuildSettings;

        if (m_SceneNum <= 1)
        {
            Debug.LogError("HandTrackingLauncher scene only works when there are other scenes existing besides itself.");
            return;
        }        

        m_Buttons = new LauncherButtonController[m_SceneNum];

        RectTransform rt = uiCanvas.GetComponent<RectTransform>();        
        int unitHeight = (int)(rt.sizeDelta.y - m_BoarderHeight * 2) / (m_SceneNum + 1);

        for (int i = 1; i < m_SceneNum; i++)
        {
            string pathToScene = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(pathToScene);            

            m_Buttons[i] = Instantiate(buttonPrefab).GetComponent<LauncherButtonController>();
            m_Buttons[i].name = "button_" + sceneName;
            
            string[] nSceneName = Regex.Split(sceneName, @"(?<!^)(?=[A-Z])");
            string naturalName = "";
            foreach (string s in nSceneName)
            {
                naturalName += s + ' ';
            }

            // m_Buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = naturalName;
            foreach (TextMeshPro tmp in m_Buttons[i].chnName)
            {
                tmp.text = sceneNamesChn[i-1];
            }

            foreach (TextMeshPro tmp in m_Buttons[i].engName)
            {
                tmp.text = naturalName;
            }

            m_Buttons[i].gameObject.transform.SetParent(uiCanvas.gameObject.transform, false);
            m_Buttons[i].gameObject.transform.localPosition = new Vector3(0, rt.sizeDelta.y / 2 - m_BoarderHeight - unitHeight * (i + 1), 0);
            int num = i;
          //  m_Buttons[i].onClick.AddListener(delegate { LoadScene(num); });
            
            m_Buttons[i].GetComponent<ButtonRayReceiver>().onPinchDown.AddListener(delegate { LoadScene(num); });
        }
    }   
    
    public void LoadScene(int i)
    {        
        string pathToScene = SceneUtility.GetScenePathByBuildIndex(i);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(pathToScene);
        if (HandTrackingPlugin.debugLevel > 0) { Debug.Log("Selected scene: " + sceneName); }        
        
        SceneManager.LoadScene(i);
    }

    void Update()
    {
        if (uiCanvas.worldCamera == null && XRCameraManager.Instance != null)
        {
            uiCanvas.gameObject.GetComponent<Canvas>().worldCamera = XRCameraManager.Instance.eventCamera;
        }

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {                
                if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
                {
                    int numOfKey = (int)key - 48;
                    if (numOfKey < m_SceneNum)
                    {                                                                   
                        SceneManager.LoadScene(numOfKey);
                    }                    
                }
            }
        }
    }
}
