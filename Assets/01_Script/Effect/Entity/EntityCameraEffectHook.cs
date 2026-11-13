using UnityEngine;

/// <summary>
/// Attach this ONLY to enemy prefabs.
/// Triggers camera shake and freeze when this enemy takes damage or dies.
/// The player taking damage does NOT trigger the effect.
/// </summary>
public class EntityCameraEffectHook : MonoBehaviour
{
    [SerializeField] private bool _useHeavyHitOnDeath = true;

    private BaseEntity _entity;

    private void Awake()
    {
        _entity = GetComponent<BaseEntity>();

        if (_entity == null)
            Debug.LogWarning("[EntityCameraEffectHook] BaseEntity tidak ditemukan.", this);
    }

    private void OnEnable()
    {
        if (_entity == null) return;

        _entity.OnDamaged += HandleDamaged;
        _entity.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (_entity == null) return;

        _entity.OnDamaged -= HandleDamaged;
        _entity.OnDied -= HandleDied;
    }

    private void HandleDamaged(int amount)
    {
        if (amount <= 0) return;

        CameraEffectsManager.Instance?.PlayDamageEffect();
    }

    private void HandleDied()
    {
        if (_useHeavyHitOnDeath)
            CameraEffectsManager.Instance?.PlayHeavyHitEffect();
        else
            CameraEffectsManager.Instance?.PlayDamageEffect();
    }
}