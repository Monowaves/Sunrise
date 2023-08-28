using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private SceneField _startingScene;

    public void StartGame()
    {
        SceneManager.LoadScene(_startingScene.SceneName);
    }
}
