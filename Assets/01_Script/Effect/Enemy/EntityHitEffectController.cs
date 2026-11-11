using System.Collections;
using UnityEngine;

public class EntityHitEffectController : MonoBehaviour
{
    [SerializeField] private float _freezeDuration = 0.08f;
    [SerializeField] private float _shakeDuration = 0.15f;
    [SerializeField] private float _shakeMagnitude = 0.15f;

    private BaseEntity _baseEntity;
    private static Coroutine _hitEffectRoutine;

    private void Awake()
    {
        _baseEntity = GetComponent<BaseEntity>();
    }

    private void OnEnable()
    {
        if (_baseEntity != null)
        {
            _baseEntity.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (_baseEntity != null)
        {
            _baseEntity.OnDamaged -= HandleDamaged;
        }
    }

    /// <summary>
    /// Handles the damaged event and triggers the camera hit effect.
    /// </summary>
    private void HandleDamaged(int damageAmount)
    {
        if (!gameObject.activeInHierarchy) return;

        if (_hitEffectRoutine != null)
        {
            StopAllCoroutines();
        }
        _hitEffectRoutine = StartCoroutine(CameraHitEffectRoutine());
    }

    /// <summary>
    /// Executes the hit stop (freeze) and camera shake effect on the main camera.
    /// </summary>
    private IEnumerator CameraHitEffectRoutine()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;

        Transform cameraTransform = mainCamera.transform;
        Vector3 originalPosition = cameraTransform.localPosition;

        // 1. FREEZE (HIT STOP)
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        float freezeElapsed = 0f;
        while (freezeElapsed < _freezeDuration)
        {
            freezeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = previousTimeScale;

        // 2. CAMERA SHAKE
        float shakeElapsed = 0f;
        while (shakeElapsed < _shakeDuration)
        {
            shakeElapsed += Time.unscaledDeltaTime;

            float offsetX = Random.Range(-_shakeMagnitude, _shakeMagnitude);
            float offsetY = Random.Range(-_shakeMagnitude, _shakeMagnitude);

            cameraTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }
}