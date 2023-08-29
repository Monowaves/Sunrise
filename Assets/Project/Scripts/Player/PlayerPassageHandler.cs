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
        if (!WorldRoom.Singleton || !_lastEntered) return;

        Passage passage = Array.Find(WorldRoom.Singleton.Passages, passage => passage.Link == _lastEntered);
        transform.position = passage.ExitPosition;

        PlayerCamera.Singleton.TeleportCamera();
        Transition.Singleton.FadeOut();

        float direction = transform.position.x > passage.transform.position.x ? 1 : -1;
        PlayerBase.Singleton.Move(0.5f, direction);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent(out Passage passage))
        {
            _lastEntered = passage.Link;

            //PlayerBase.Singleton.Rigidbody.velocity = Vector2.zero;

            PlayerBase.Singleton.DontWriteMoveInputs = true;
            PlayerBase.Singleton.BlockJumpInputs = true;
            
            string targetSceneName = Array.Find(_lastEntered.Linked, scene => scene.SceneName != SceneManager.GetActiveScene().name);
            Transition.Singleton.FadeIn(() => SceneManager.LoadScene(targetSceneName));
        }
    }
}
