using UnityEngine;
using UnityEngine.UI;

public class GunPreview : MonoBehaviour
{
    public static GunPreview Singleton { get; private set; } 

    [SerializeField] private Image _preview;

    private void Awake() 
    {
        Singleton = this;
    }

    public void SetPreview(Sprite gun)
    {
        _preview.sprite = gun;
        _preview.SetNativeSize();
    }
}
