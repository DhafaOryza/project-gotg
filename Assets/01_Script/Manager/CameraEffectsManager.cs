using UnityEngine;

public class CameraEffectsManager : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private bool _enableShake = true;
    [SerializeField] private bool _enableFreeze = true;

    [Header("On Entity Damaged")]
    [SerializeField, Min(0f)] private float _damageShakeAmplitude = 1.5f;
    [SerializeField, Min(0f)] private float _damageShakeFrequency = 2f;
    [SerializeField, Min(0f)] private float _damageShakeDuration = 0.2f;
    [SerializeField, Min(0f)] private float _damageFreezeDuration = 0.05f;

    private CameraShake _shake;
    private CameraFreeze _freeze;

    public static CameraEffectsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CameraEffectsManager] Lebih dari satu instance di scene.");
            return;
        }

        Instance = this;
        _shake = GetComponent<CameraShake>();
        _freeze = GetComponent<CameraFreeze>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Triggers shake and freeze with the standard "entity took damage" settings.
    /// </summary>
    public void PlayDamageEffect()
    {
        if (_enableShake && _shake != null)
            _shake.Shake(_damageShakeAmplitude, _damageShakeFrequency, _damageShakeDuration);

        if (_enableFreeze && _freeze != null)
            _freeze.Freeze(_damageFreezeDuration);
    }

    /// <summary>
    /// Triggers a stronger shake and freeze, e.g. for a heavy hit or death.
    /// </summary>
    public void PlayHeavyHitEffect()
    {
        if (_enableShake && _shake != null)
            _shake.Shake(_damageShakeAmplitude * 2f, _damageShakeFrequency * 1.5f, _damageShakeDuration * 1.5f);

        if (_enableFreeze && _freeze != null)
            _freeze.Freeze(_damageFreezeDuration * 2f);
    }
}