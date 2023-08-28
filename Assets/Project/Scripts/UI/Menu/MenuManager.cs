using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private SceneField _startingScene;

    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private GameObject _camera;

    private bool _listenerAdded;

    private void Start()
    {
        _eventSystem.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(_startingScene.SceneName);
    }

    private void Update() 
    {
        if (!_listenerAdded)
        {
            _camera.AddComponent<AudioListener>();
            _listenerAdded = true;
        }
    }
}
