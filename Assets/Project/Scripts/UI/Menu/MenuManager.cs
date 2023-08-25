using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartParkour()
    {
        SceneManager.LoadScene("Parkour");
    }

    public void StartFighting()
    {
        SceneManager.LoadScene("Fighting");
    }
}
