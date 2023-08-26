using UnityEngine;

public class DontDestroyOnLoadBehaviour : MonoBehaviour
{
    protected virtual void Initialize() { }

    private void Awake() 
    {
        if (FindObjectOfType(GetType()) != null)
        {
            Destroy(gameObject);
        }

        Initialize();
    }
}
