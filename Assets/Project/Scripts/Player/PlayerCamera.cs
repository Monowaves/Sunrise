using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Singleton { get; private set;}
    public static Camera Camera { get; private set; }

    [SerializeField] private CinemachineVirtualCamera _vcam;
    [SerializeField] private Camera _camera;
    private CinemachineBasicMultiChannelPerlin _noiser;

    private void Awake() 
    {
        Singleton = this;
        Camera = _camera;
        
        _noiser = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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
}
