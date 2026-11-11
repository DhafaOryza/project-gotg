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
        // Ambil semua SpriteRenderer pada entity dan child-nya
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        baseEntity = GetComponent<BaseEntity>();

        // Simpan warna asli setiap sprite agar tidak merusak kustomisasi warna awal
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
    }

    private void OnDisable()
    {
        if (baseEntity != null)
        {
            baseEntity.OnDamaged -= TriggerFlash;
        }
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
        // Ubah warna semua sprite menjadi warna flash (misal: Putih terang)
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.color = flashColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Kembalikan ke warna asli masing-masing sprite
        foreach (var sr in spriteRenderers)
        {
            if (sr != null && originalColors.TryGetValue(sr, out Color originalColor))
            {
                sr.color = originalColor;
            }
        }
    }
}
