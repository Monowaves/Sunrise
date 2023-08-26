using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeHandler : DontDestroyOnLoadBehaviour
{
    public static SceneChangeHandler Singleton { get; private set; }

    protected override void Initialize() 
    {
        Singleton = this;
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }
}
