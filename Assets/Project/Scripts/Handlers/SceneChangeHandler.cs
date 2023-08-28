using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeHandler : DontDestroyOnLoadBehaviour
{
    [SerializeField] private GameObject _importantPrefab;

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        if (FindObjectOfType<WorldRoom>())
        {
            if (Important.Singleton == null) _importantPrefab.Spawn(Vector3.zero);
        }
        else
        {
            if (Important.Singleton != null) Important.Singleton.Finish();
        }
    }

    protected override void OnSceneDeload(Scene scene)
    {
        WorldRoom.Singleton = null;
    }
}
