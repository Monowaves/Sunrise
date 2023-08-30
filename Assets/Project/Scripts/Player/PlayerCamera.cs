using System.Collections;
using Cinemachine;
using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Singleton { get; private set; }
    public static Camera Camera { get; private set; }

    [SerializeField] private CinemachineVirtualCamera _vcam;
    [SerializeField] private Camera _camera;
    [SerializeField] private AudioLowPassFilter _lowPassFilter;
    [SerializeField] private Volume _hitVolume;

    [Header("Properties")]
    [SerializeField] private float _lowPassActivated;
    [SerializeField] private float _lowPassDeactivated;
    [SerializeField] private float _lowPassGain = 15f;
    [SerializeField] private float _lowPassReduce = 7f;

    [Space(9)]

    [SerializeField] private float _hitVolumeGain = 15f;
    [SerializeField] private float _hitVolumeReduce = 7f;

    private CinemachineBasicMultiChannelPerlin _noiser;
    private CinemachineFramingTransposer _transposer;

    private float _originalDamping;

    private void Awake() 
    {
        Singleton = this;
        Camera = _camera;
        
        _noiser = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _transposer = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        _lowPassFilter.cutoffFrequency = _lowPassDeactivated;

        _originalDamping = _transposer.m_XDamping;
    }

    public void HitVolume(float duration = 0.4f)
    {
        StopCoroutine(nameof(CO_HitVolume));
        StartCoroutine(nameof(CO_HitVolume), duration);
    }

    private IEnumerator CO_HitVolume(float duration)
    {
        float elapsed = 0f;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime * _hitVolumeGain;
            _hitVolume.weight = elapsed.ClampMaximum(1f);
            
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        float remaining = 1f;
        while (remaining > 0)
        {
            remaining -= Time.deltaTime * _hitVolumeReduce;
            _hitVolume.weight = remaining.ClampMinimum(0f);
            
            yield return null;
        }
    }

    public void LowPass(float duration = 0.4f)
    {
        StopCoroutine(nameof(CO_LowPass));
        StartCoroutine(nameof(CO_LowPass), duration);
    }

    private IEnumerator CO_LowPass(float duration)
    {
        float elapsed = 0f;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime * _lowPassGain;
            _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassDeactivated, _lowPassActivated, elapsed.ClampMaximum(1f));
            
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        float remaining = 1f;
        while (remaining > 0)
        {
            remaining -= Time.deltaTime * _lowPassReduce;
            _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassDeactivated, _lowPassActivated, remaining.ClampMinimum(0f));
            
            yield return null;
        }
    }

    public void Shake(float amplitude, float duration = 0.2f)
    {
        StopCoroutine(nameof(CO_Shake));
        StartCoroutine(nameof(CO_Shake), new ShakeSettings() { Amplitude = amplitude, Duration = duration });
    }

    private class ShakeSettings
    {
        public float Amplitude;
        public float Duration;
    }

    private IEnumerator CO_Shake(ShakeSettings settings)
    {
        _noiser.m_AmplitudeGain = settings.Amplitude;

        yield return new WaitForSeconds(settings.Duration);

        _noiser.m_AmplitudeGain = 0;
    }

    public void DisableFollow() => _vcam.enabled = false;
    public void EnableFollow() => _vcam.enabled = true;

    public void TeleportCamera()
    {
        StartCoroutine(nameof(CO_Teleport));
    }

    private IEnumerator CO_Teleport()
    {
        _transposer.m_XDamping = 0f;

        yield return null;

        _transposer.m_XDamping = _originalDamping;
    }
}
