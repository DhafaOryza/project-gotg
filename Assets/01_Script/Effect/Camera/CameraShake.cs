using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField, Min(0f)] private float _defaultAmplitude = 1.5f;
    [SerializeField, Min(0f)] private float _defaultFrequency = 2f;
    [SerializeField, Min(0f)] private float _defaultDuration = 0.2f;

    private CinemachineBasicMultiChannelPerlin _noise;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        var cam = GetComponent<CinemachineCamera>();
        if (cam != null)
            _noise = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (_noise == null)
            Debug.LogWarning("[CameraShake] CinemachineBasicMultiChannelPerlin tidak ditemukan. " +
                             "Tambahkan komponen itu ke GameObject yang sama.", this);

        _noise.AmplitudeGain = 0f;
        _noise.FrequencyGain = 0f;
    }

    /// <summary>
    /// Triggers a shake with the default settings.
    /// </summary>
    public void Shake()
    {
        Shake(_defaultAmplitude, _defaultFrequency, _defaultDuration);
    }

    /// <summary>
    /// Triggers a shake with custom amplitude, frequency and duration.
    /// </summary>
    public void Shake(float amplitude, float frequency, float duration)
    {
        if (_noise == null) return;

        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);

        _shakeCoroutine = StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    private IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        _noise.AmplitudeGain = amplitude;
        _noise.FrequencyGain = frequency;

        // Wait the full duration in real time, so freeze (timeScale 0) does not cut the shake short.
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Smooth decay over the last 30% of the duration so the shake does not snap to zero.
        float decayDuration = duration * 0.3f;
        float startAmplitude = amplitude;
        elapsed = 0f;
        while (elapsed < decayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / decayDuration;
            _noise.AmplitudeGain = Mathf.Lerp(startAmplitude, 0f, t);
            yield return null;
        }

        _noise.AmplitudeGain = 0f;
        _noise.FrequencyGain = 0f;
        _shakeCoroutine = null;
    }
}