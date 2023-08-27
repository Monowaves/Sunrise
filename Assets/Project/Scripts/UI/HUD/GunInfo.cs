using System;
using System.Collections;
using MonoWaves.QoL;
using TMPro;
using UnityEngine;

public class GunInfo : MonoBehaviour
{
    public static GunInfo Singleton { get; private set;}

    [SerializeField] private CanvasGroup _canvasGroup;

    [Space(9)]

    [SerializeField] private TMP_Text _name;
    [SerializeField] private GunStat _rarity;
    [SerializeField] private GunStat _damage;
    [SerializeField] private GunStat _capacity;
    [SerializeField] private GunStat _spread;

    private void Awake() 
    {
        Singleton = this;
    }

    public void Hide()
    {
        StopCoroutine(nameof(FadeInfo));
        StartCoroutine(nameof(FadeInfo), 0f);
    }

    public void Show()
    {
        StopCoroutine(nameof(FadeInfo));
        StartCoroutine(nameof(FadeInfo), 1f);
    }

    private IEnumerator FadeInfo(float endValue)
    {
        if (_canvasGroup.alpha == endValue) yield break;

        float elapsed = 0;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime * 6f;

            _canvasGroup.alpha = Mathf.Lerp(1 - endValue, endValue, elapsed);

            yield return null;
        }

        _canvasGroup.alpha = endValue;
    }

    public void SetInfo(GunBaseSettings info)
    {
        _name.text = info.Name;

        (string rarityName, string rarityColor) = GetRarityParams(info.Rarity);

        _rarity.Value.text = rarityName;
        _rarity.Value.color = rarityColor.HexToColor();

        _damage.Value.text = info.Damage.ToString();
        _capacity.Value.text = info.Capacity.ToString();
        _spread.Value.text = info.SpreadAngle.ToString();
    }

    private (string name, string color) GetRarityParams(GunRarity rarity)
    {
        return rarity switch
        {
            GunRarity.Common => ("common", "#394a50"),
            GunRarity.Fancy => ("fancy", "#d7b594"),
            GunRarity.Rare => ("rare", "#a8ca58"),
            GunRarity.Epic => ("epic", "#c65197"),
            GunRarity.Unique => ("unique", "#73bed3"),
            GunRarity.Mythical => ("mythical", "#cf573c"),
            GunRarity.Legendary => ("legendary", "#de9e41"),
            _ => throw new Exception("пизда")
        };
    }
}
