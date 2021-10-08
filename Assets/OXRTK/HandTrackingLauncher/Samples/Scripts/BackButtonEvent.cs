using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButtonEvent : MonoBehaviour
{
    
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Disabled the button if the scene is launcher
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
