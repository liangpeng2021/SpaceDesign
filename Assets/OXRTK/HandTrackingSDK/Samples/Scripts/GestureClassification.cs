using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OXRTK.ARHandTracking;

public class GestureClassification : MonoBehaviour
{
    HandTrackingPlugin.HandInfo m_RightHand;
    HandTrackingPlugin.HandInfo m_LeftHand;

    public TextMesh textMesh;

    HandTrackingPlugin.DynamicGesture m_LastDetectedDynamicGesture_R = HandTrackingPlugin.DynamicGesture.None;
    HandTrackingPlugin.DynamicGesture m_LastDetectedDynamicGesture_L = HandTrackingPlugin.DynamicGesture.None;
    HandTrackingPlugin.DynamicGesture m_LastDynamicGesture_R = HandTrackingPlugin.DynamicGesture.None;
    HandTrackingPlugin.DynamicGesture m_LastDynamicGesture_L = HandTrackingPlugin.DynamicGesture.None;
    int m_Counter_R, m_Counter_L = 1;
    string m_DynamicGestureString_R, m_DynamicGestureString_L = "";

    void Update()
    {
        m_RightHand = HandTrackingPlugin.instance.rightHandInfo;
        m_LeftHand = HandTrackingPlugin.instance.leftHandInfo;
        textMesh.text = "";
        if (m_RightHand.handDetected)
        {
            textMesh.text += "StaticGesture_R: ";
            if (m_RightHand.staticGesture != HandTrackingPlugin.StaticGesture.None)
            {                
                textMesh.text += m_RightHand.staticGesture + "\n";
            }
            else
            {
                textMesh.text += " None \n";
            }

            textMesh.text += "DynamicGesture_R: ";
            if (m_RightHand.dynamicGesture != HandTrackingPlugin.DynamicGesture.None && 
                m_RightHand.dynamicGesture != m_LastDynamicGesture_R)
            {
                if (m_LastDetectedDynamicGesture_R == m_RightHand.dynamicGesture)
                {
                    m_Counter_R++;
                }
                else
                {
                    m_Counter_R = 1;
                }
                m_DynamicGestureString_R = m_RightHand.dynamicGesture + ", " + m_Counter_R;
                m_LastDetectedDynamicGesture_R = m_RightHand.dynamicGesture;
            }
            m_LastDynamicGesture_R = m_RightHand.dynamicGesture;
            textMesh.text += m_DynamicGestureString_R;
            textMesh.text += "\n";
        }
        else
        {
            textMesh.text += "Right hand not detected \n";
        }

        if (m_LeftHand.handDetected)
        {
            textMesh.text += "StaticGesture_L: ";
            if (m_LeftHand.staticGesture != HandTrackingPlugin.StaticGesture.None)
            {
                textMesh.text += m_LeftHand.staticGesture + "\n";
            }
            else
            {
                textMesh.text += " None \n";
            }

            textMesh.text += "DynamicGesture_L: ";
            if (m_LeftHand.dynamicGesture != HandTrackingPlugin.DynamicGesture.None &&
                m_LeftHand.dynamicGesture != m_LastDynamicGesture_L)
            {
                if (m_LastDetectedDynamicGesture_L == m_LeftHand.dynamicGesture)
                {
                    m_Counter_L++;
                }
                else
                {
                    m_Counter_L = 1;
                }
                m_DynamicGestureString_L = m_LeftHand.dynamicGesture + ", " + m_Counter_L;
                m_LastDetectedDynamicGesture_L = m_LeftHand.dynamicGesture;
            }
            m_LastDynamicGesture_L = m_LeftHand.dynamicGesture;
            textMesh.text += m_DynamicGestureString_L;
            textMesh.text += "\n";
        }
        else
        {
            textMesh.text += "Left hand not detected \n";
        }
    }
}
