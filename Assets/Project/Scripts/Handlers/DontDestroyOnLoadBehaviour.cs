using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoadBehaviour : MonoBehaviour
{
    protected virtual void Initialize() { }
    protected virtual void OnSceneLoad(Scene scene, LoadSceneMode mode) { }
    protected virtual void OnSceneDeload(Scene scene) { }
    protected virtual void OnFinish() { }

    private void Awake() 
    {
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneDeload;

        Initialize();
    }

    public void Finish()
    {
        OnFinish();
        Destroy(gameObject);
    }
}
