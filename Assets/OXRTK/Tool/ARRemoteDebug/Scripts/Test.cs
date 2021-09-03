using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OXRTK.ARRemoteDebug;

namespace OXRTK.Tester
{
    public class Test : MonoBehaviour
    {
        [System.Serializable]
        class MPos
        {
            public float x;
            public float y;
            public float z;

            public MPos()
            {

            }

            public MPos(Vector3 v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
            }
        }
        
        // Start is called before the first frame update
        bool m_AndroidSide;
        public GameObject cursor;
        
        void Start()
        {
            ARRemoteDebugWrapper.Init();

            m_AndroidSide = ARRemoteDebugWrapper.CheckAndroidSimulation();
            if (m_AndroidSide || Application.platform == RuntimePlatform.Android)
            {
                cursor.SetActive(false);
                ARRemoteDebugWrapper.AddEditorDataListener(OnEditorDataReceived);
            }
            else
            {
                ARRemoteDebugWrapper.AddAndroidDataListener(OnAndroidDataReceived);
                cursor.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(m_AndroidSide || Application.platform == RuntimePlatform.Android)
            {
                MPos mousePos;
                if (Application.platform == RuntimePlatform.Android && Input.touchCount>0)
                    mousePos = new MPos(Input.GetTouch(0).position);
                else
                    mousePos = new MPos(Input.mousePosition);

                ARRemoteDebugWrapper.SendDataToEditor(mousePos);
            }
        }

        void OnAndroidDataReceived(object data)
        {
            if(data is MPos)
            {
                MPos mousePos = (MPos)data;
                cursor.transform.localPosition = new Vector3(mousePos.x - Screen.width / 2,
                     mousePos.y - Screen.height / 2, mousePos.z);

            }
        }
        
        void OnEditorDataReceived(object data)
        {

        }

    }
}

