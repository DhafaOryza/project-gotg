using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityFlashEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField, Range(0.01f, 0.5f)] private float flashDuration = 0.1f;

    private SpriteRenderer[] spriteRenderers;
    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    private Coroutine flashRoutine;
    private BaseEntity baseEntity;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        baseEntity = GetComponent<BaseEntity>();

        foreach (var sr in spriteRenderers)
        {
            if (sr != null && !originalColors.ContainsKey(sr))
            {
                originalColors[sr] = sr.color;
            }
        }
    }

    private void OnEnable()
    {
        if (baseEntity != null)
        {
            baseEntity.OnDamaged += TriggerFlash;
        }

        RestoreOriginalColors();
    }

    private void OnDisable()
    {
        if (baseEntity != null)
        {
            baseEntity.OnDamaged -= TriggerFlash;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        RestoreOriginalColors();
    }

    private void TriggerFlash(int damageAmount)
    {
        if (gameObject.activeInHierarchy)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.color = flashColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreOriginalColors();
        flashRoutine = null;
    }

    private void RestoreOriginalColors()
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.color = kvp.Value;
            }
        }
    }
}