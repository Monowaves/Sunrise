using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

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
        if (Transition.Singleton) Transition.Singleton.Abort();
    }

    private void Start() 
    {
        if (!WorldRoom.Singleton) return;

        PlayerCamera.Singleton.SetBounds(WorldRoom.Singleton.Bounds);
    }

    private void SceneChanged(Scene scene, LoadSceneMode mode)
    {
        if (!WorldRoom.Singleton || !_lastEntered) return;

        PlayerCamera.Singleton.SetBounds(WorldRoom.Singleton.Bounds);

        Passage passage = Array.Find(WorldRoom.Singleton.Passages, passage => passage.Link == _lastEntered);

        if (!passage) throw new Exception($"Cannot find passage with needed link in {SceneManager.GetActiveScene().name}");
        transform.position = passage.ExitPosition;

        PlayerCamera.Singleton.EnableFollow();

        PlayerCamera.Singleton.TeleportCamera();
        Transition.Singleton.FadeOut();

        PlayerBase.Singleton.BlockSlamInputs = false;
        PlayerBase.Singleton.BlockJumpInputs = false;
        PlayerBase.Singleton.BlockWallChecker = false;

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
                Jump();
                PlayerBase.Singleton.Move(1f, PlayerBase.Singleton.Facing == PlayerFacing.Left ? -1 : 1, true);
            }
            else PlayerBase.Singleton.DontWriteMoveInputs = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent(out Passage passage) && !_alreadyEntering)
        {
            _lastEntered = passage.Link;

            if (!_lastEntered) throw new Exception($"Passage {passage.name} isn't connected to any passage link! Make sure you assinged passage link in inspector");

            PlayerBase.Singleton.DontWriteMoveInputs = true;
            PlayerBase.Singleton.BlockWallChecker = true;

            PlayerBase.Singleton.BlockSlamInputs = true;
            PlayerBase.Singleton.BlockJumpInputs = true;

            PlayerCamera.Singleton.DisableFollow();
            
            string targetSceneName = Array.Find(_lastEntered.Linked, scene => scene.SceneName != SceneManager.GetActiveScene().name);
            Transition.Singleton.FadeIn(() => 
            {
                StopCoroutine(nameof(PassageEnter));
                SceneManager.LoadScene(targetSceneName);
            });

            _alreadyEntering = true;

            StartCoroutine(nameof(PassageEnter), passage);
        }
    }

    private IEnumerator PassageEnter(Passage passage)
    {
        Rigidbody2D rb = PlayerBase.Singleton.Rigidbody;
        Vector2 lastVelocity = rb.velocity;

        if (!passage.IsVertical)
        {
            float direction = transform.position.x > passage.transform.position.x ? -1 : 1;
            PlayerBase.Singleton.Move(1f, direction);
            
            yield break;
        }

        while (true)
        {
            if (passage.ExitPosition.y < passage.transform.position.y)
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(10, lastVelocity.y));
            }

            yield return null;
        }
    }

    private void Jump()
    {
        PlayerBase.Singleton.Rigidbody.velocity = Vector2.zero;
        PlayerBase.Singleton.Rigidbody.AddForce(Vector2.up * 18.5f, ForceMode2D.Impulse);
    }
}
