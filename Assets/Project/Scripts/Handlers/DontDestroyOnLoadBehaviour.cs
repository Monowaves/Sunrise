using UnityEngine;

public class DontDestroyOnLoadBehaviour : MonoBehaviour
{
    protected virtual void Initialize() { }

    private void Awake() 
    {
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        Initialize();
    }
}
