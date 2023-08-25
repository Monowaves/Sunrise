using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeHadnler : MonoBehaviour
{
    public static SceneChangeHadnler Singleton { get; private set; }

    private void Awake() 
    {
        if (FindObjectOfType<SceneChangeHadnler>() != null)
        {
            Destroy(gameObject);
        }

        Singleton = this;
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }
}
