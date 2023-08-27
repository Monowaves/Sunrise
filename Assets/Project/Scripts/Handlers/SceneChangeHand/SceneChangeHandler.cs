using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeHandler : DontDestroyOnLoadBehaviour
{
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }
}
