using System;
using System.Collections;
using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.UI;

public class Transition : DontDestroyOnLoadBehaviour
{
    public static Transition Singleton { get; private set; }

    [SerializeField] private Image _transitionGraphic;
    [SerializeField] private float _duration;

    protected override void Initialize()
    {
        Singleton = this;
    }

    public void FadeIn(Action onEnd = null)
    {
        StopCoroutine(nameof(CO_Fade));
        StartCoroutine(nameof(CO_Fade), new Settings(1, onEnd));
    }

    public void FadeOut(Action onEnd = null)
    {
        StopCoroutine(nameof(CO_Fade));
        StartCoroutine(nameof(CO_Fade), new Settings(0, onEnd));
    }

    private struct Settings
    {
        public float EndValue;
        public Action Callback;

        public Settings(float endValue, Action callback)
        {
            EndValue = endValue;
            Callback = callback;
        }
    }

    private IEnumerator CO_Fade(Settings settings)
    {
        Color startColor = new(0, 0, 0, 1 - settings.EndValue);
        Color endColor = new(0, 0, 0, settings.EndValue);

        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            _transitionGraphic.color = Color.Lerp(startColor, endColor, elapsed.Remap(new Span(0, _duration), Span.ZeroPositive));

            yield return null;
        }

        _transitionGraphic.color = endColor;
        settings.Callback?.Invoke();
    }
}
