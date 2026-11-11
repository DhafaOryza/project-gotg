using System.Collections;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private EnemyBaseEntity enemyEntity;

    [Header("Fade Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeSpeed = 3f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (enemyEntity == null)
            enemyEntity = GetComponentInParent<EnemyBaseEntity>();

        if (healthBar == null)
            healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Start()
    {
        if (enemyEntity != null && healthBar != null)
        {
            // Inisialisasi health bar dengan sumber nyawa dari enemy
            healthBar.Initialize(enemyEntity);
            
            // Daftarkan event perubahan health untuk mendeteksi saat enemy kena damage
            enemyEntity.OnHealthChanged += HandleHealthChanged;
        }

        // Sembunyikan health bar di awal (tidak tampil sebelum kena damage)
        SetAlpha(0f);
    }

    private void OnDestroy()
    {
        if (enemyEntity != null)
        {
            enemyEntity.OnHealthChanged -= HandleHealthChanged;
        }
    }

    /// <summary>
    /// Dipanggil setiap kali health enemy berubah (misal: terkena damage).
    /// </summary>
    private void HandleHealthChanged(int current, int max)
    {
        // Munculkan kembali bar secara penuh dan reset durasi fade
        ShowBarFully();
    }

    private void ShowBarFully()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // 1. Munculkan instan / set alpha ke 1 (penuh)
        SetAlpha(1f);

        // 2. Tahan beberapa detik sesuai displayDuration
        yield return new WaitForSeconds(displayDuration);

        // 3. Lakukan fade out secara halus
        while (canvasGroup != null && canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            // Matikan interactable/blocksRaycasts agar tidak mengganggu klik/raycast gameplay
            canvasGroup.interactable = alpha > 0f;
            canvasGroup.blocksRaycasts = alpha > 0f;
        }
    }
}