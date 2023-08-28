using System;
using System.Collections.Generic;
using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.UI;

public class AmmoBar : MonoBehaviour
{
    public static AmmoBar Singleton { get; private set; }

    [SerializeField] private GameObject Seperator;
    [field: SerializeField] public Slider AmmoSlider { get; private set; }
    [field: SerializeField] public Transform AmmoSeperators { get; private set; }

    private int _barsCount;
    private RectTransform _rectTransform;
    private RectTransform _sliderTransform;

    float EndValue => _sliderTransform.rect.width;

    private void Awake() 
    {
        Singleton = this;

        _rectTransform = GetComponent<RectTransform>();
        _sliderTransform = AmmoSlider.GetComponent<RectTransform>();
    }

    public void ClearAmmo()
    {
        foreach (Transform seperator in AmmoSeperators)
        {
            Destroy(seperator.gameObject);
        }
    }

    public void SetMaxAmmo(int count, float? defaultValue = null)
    {
        if (count == 0) throw new Exception("");

        ClearAmmo();
        _barsCount = count;

        for (int i = 0; i < count - 1; i++)
        {
            Instantiate(Seperator, AmmoSeperators);
        }

        AmmoSlider.value = defaultValue ?? 1f;
    }

    public void SetAmmoCount(int count)
    {
        if (count > _barsCount) throw new Exception("");
        List<float> bars = GetBars();
        
        AmmoSlider.value = bars[count].Remap(new Span(0, EndValue), Span.ZeroPositive);
    }

    private List<float> GetBars()
    {
        List<float> output = new()
        {
            0f
        };

        foreach (RectTransform seperator in AmmoSeperators)
        {
            output.Add(seperator.anchoredPosition.x + _sliderTransform.offsetMin.x);
        }

        return output;
    }
}
