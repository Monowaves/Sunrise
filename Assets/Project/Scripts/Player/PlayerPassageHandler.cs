using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerPassageHandler : MonoBehaviour
{
    private PassageLink _lastEntered;

    private void Awake() 
    {
        SceneManager.sceneLoaded += SceneChanged;
    }

    private void OnDestroy() 
    {
        SceneManager.sceneLoaded -= SceneChanged;
    }

    private void SceneChanged(Scene scene, LoadSceneMode mode)
    {
        if (WorldRoom.Singleton == null) return;

        Passage passage = Array.Find(WorldRoom.Singleton.Passages, passage => passage.Link == _lastEntered);
        transform.position = passage.ExitPosition;

        PlayerCamera.Singleton.TeleportCamera();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent(out Passage passage))
        {
            _lastEntered = passage.Link;

            string targetSceneName = Array.Find(_lastEntered.Linked, scene => scene.SceneName != SceneManager.GetActiveScene().name);
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
