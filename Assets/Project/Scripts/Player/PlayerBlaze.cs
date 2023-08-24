using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBlaze : MonoBehaviour
{
    public static PlayerBlaze Singleton { get; private set; }

    [Header("Properties")]
    [SerializeField] private Slider _blazeBar;
    [SerializeField] private float _startBlaze = 100f;
    [SerializeField] private float _blazeGain = 5f;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public float Blaze { get; private set; }

    private void Awake() 
    {
        Singleton = this;
        _blazeBar.minValue = 0;
        _blazeBar.maxValue = _startBlaze;

        Blaze = _startBlaze;
    }

    private void Update() 
    {
        if (Blaze < _startBlaze) Blaze += Time.deltaTime * _blazeGain;

        _blazeBar.value = Mathf.Lerp(_blazeBar.value, Blaze, Time.deltaTime * 10);
    }

    public void Consume(float amount)
    {
        if (Blaze < amount) return;

        Blaze -= amount.ClampMinimum(0);
    }

    public bool CanConsume(float amount)
    {
        if (Blaze < amount) return false;

        return true;
    }
}
