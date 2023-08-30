using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerPassageHandler : MonoBehaviour
{
    private PassageLink _lastEntered;
    private bool _alreadyEntering;

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

        PlayerCamera.Singleton.EnableFollow();

        PlayerCamera.Singleton.TeleportCamera();
        Transition.Singleton.FadeOut();

        PlayerBase.Singleton.BlockSlamInputs = false;
        PlayerBase.Singleton.BlockJumpInputs = false;
        PlayerBase.Singleton.BlockWallChecker = false;

        PlayerBase.Singleton.Rigidbody.velocity = Vector2.zero;

        _alreadyEntering = false;

        if (!passage.IsVertical)
        {
            float direction = transform.position.x > passage.transform.position.x ? 1 : -1;
            PlayerBase.Singleton.Move(0.5f, direction);
        }
        else
        {
            if (transform.position.y > passage.transform.position.y)
            {
                PlayerBase.Singleton.Jump();
                PlayerBase.Singleton.Move(1f, PlayerBase.Singleton.Facing == PlayerFacing.Left ? -1 : 1);
            }
            else PlayerBase.Singleton.DontWriteMoveInputs = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent(out Passage passage) && !_alreadyEntering)
        {
            _lastEntered = passage.Link;

            PlayerBase.Singleton.DontWriteMoveInputs = true;
            PlayerBase.Singleton.BlockWallChecker = true;

            PlayerBase.Singleton.BlockSlamInputs = true;
            PlayerBase.Singleton.BlockJumpInputs = true;

            PlayerCamera.Singleton.DisableFollow();
            
            string targetSceneName = Array.Find(_lastEntered.Linked, scene => scene.SceneName != SceneManager.GetActiveScene().name);
            Transition.Singleton.FadeIn(() => SceneManager.LoadScene(targetSceneName));

            _alreadyEntering = true;
        }
    }
}
