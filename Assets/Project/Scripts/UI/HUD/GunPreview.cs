using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GunPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static GunPreview Singleton { get; private set; } 

    [SerializeField] private Image _preview;
    [SerializeField] private Image _background;

    [SerializeField] private Sprite[] _rarities;

    private void Awake() 
    {
        Singleton = this;
        _background.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }

    public void SetPreview(Sprite gun, GunRarity rarity)
    {
        _preview.sprite = gun;
        _preview.SetNativeSize();

        _background.sprite = _rarities[(int)rarity];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _background.color = Color.white;

        Invoke(nameof(ShowInfo), 0.5f);
    }

    private void ShowInfo()
    {
        GunInfo.Singleton.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelInvoke(nameof(ShowInfo));
        HideInfo();
    }

    private void HideInfo()
    {
        GunInfo.Singleton.Hide();
        _background.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }
}
