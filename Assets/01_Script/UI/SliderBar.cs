using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    [SerializeField] private Image currentHp;
    [SerializeField] private Image backgroundHp;

    [Header("Settings")]
    [SerializeField] private float delayBeforeFollow = 0.5f;
    [SerializeField] private float lerpSpeed = 3f;

    private float lastHpFill;
    private Coroutine bgFollowCoroutine;
    private bool _isPreviewing;

    /// <summary>True kalau bar ini lagi nampilin damage preview (bukan HP asli).</summary>
    public bool IsPreviewing => _isPreviewing;

    // ─── Init ─────────────────────────────────────────────────────────────

    private void Start()
    {
        lastHpFill = currentHp.fillAmount;
        backgroundHp.fillAmount = currentHp.fillAmount;
    }

    // ─── Update ───────────────────────────────────────────────────────────

    private void Update()
    {
        if (Mathf.Approximately(currentHp.fillAmount, lastHpFill)) return;

        lastHpFill = currentHp.fillAmount;

        if (bgFollowCoroutine != null)
            StopCoroutine(bgFollowCoroutine);

        bgFollowCoroutine = StartCoroutine(DelayedBackgroundFollow(currentHp.fillAmount));
    }

    // ─── Public API (dipanggil dari PlayerStatusUI atau sumber HP lain) ────

    /// <summary>
    /// Set fill (0..1) kartu HP depan secara langsung/instan — untuk HP ASLI (bukan preview).
    /// Background (backgroundHp) otomatis mengikuti dengan delay lewat Update() di atas —
    /// nggak perlu manggil apa-apa lagi selain ini.
    /// </summary>
    public void SetFill(float normalizedValue)
    {
        if (currentHp == null) return;
        float frac = Mathf.Clamp01(normalizedValue);

        if (_isPreviewing)
        {
            // Real HP update datang SAAT preview masih aktif (mis. damage-nya beneran
            // dieksekusi setelah tadi di-preview). Background lagi dibekukan di HP
            // sebelum-diserang — paksa dia mulai turun ke nilai real yang baru ini,
            // walau kebetulan currentHp.fillAmount gak berubah dari nilai prediksinya
            // (kalau cuma mengandalkan Update() yang bandingin lastHpFill, kasus itu
            // gak akan ke-detect sebagai perubahan, dan backgroundHp bisa nyangkut).
            _isPreviewing = false;
            if (bgFollowCoroutine != null) StopCoroutine(bgFollowCoroutine);
            currentHp.fillAmount = frac;
            lastHpFill = frac;
            bgFollowCoroutine = StartCoroutine(DelayedBackgroundFollow(frac));
            return;
        }

        currentHp.fillAmount = frac; // jalur normal — Update() yang urus background follow
    }

    /// <summary>Shortcut: hitung fraction dari currentHealth/maxHealth lalu SetFill.</summary>
    public void SetHP(int currentHealth, int maxHealth)
    {
        float frac = maxHealth > 0 ? (float)Mathf.Max(0, currentHealth) / maxHealth : 0f;
        SetFill(frac);
    }

    /// <summary>Ganti warna fill depan (mis. hijau → merah sesuai persentase HP).</summary>
    public void SetFillColor(Color color)
    {
        if (currentHp != null) currentHp.color = color;
    }

    // ─── Damage Preview ─────────────────────────────────────────────────
    // Kebalikan dari efek chip-damage: currentHp langsung "dijatuhkan" ke nilai
    // prediksi (HP setelah kena damage ini), sementara backgroundHp DIBEKUKAN di
    // HP asli sekarang (bukan ikut turun). Selisih antara keduanya jadi kelihatan
    // sebagai segmen "ini yang akan hilang" — tanpa HP asli beneran berubah.

    /// <summary>Tampilkan preview damage dari fraction (0..1) HP asli & HP prediksi setelah kena damage.</summary>
    public void ShowDamagePreview(float actualFrac, float predictedFrac)
    {
        if (currentHp == null || backgroundHp == null) return;

        if (bgFollowCoroutine != null)
        {
            StopCoroutine(bgFollowCoroutine);
            bgFollowCoroutine = null;
        }

        _isPreviewing = true;
        backgroundHp.fillAmount = Mathf.Clamp01(actualFrac);   // dibekukan di HP asli
        currentHp.fillAmount = Mathf.Clamp01(predictedFrac);   // langsung turun ke prediksi
        lastHpFill = currentHp.fillAmount;                     // cegah Update() salah nganggap ini perubahan HP asli
    }

    /// <summary>Shortcut: tampilkan preview dari currentHealth/maxHealth/incomingDamage (int).</summary>
    public void ShowDamagePreview(int currentHealth, int maxHealth, int incomingDamage)
    {
        if (maxHealth <= 0) return;
        float actualFrac = (float)Mathf.Max(0, currentHealth) / maxHealth;
        float predictedFrac = (float)Mathf.Max(0, currentHealth - Mathf.Max(0, incomingDamage)) / maxHealth;
        ShowDamagePreview(actualFrac, predictedFrac);
    }

    /// <summary>Batalkan preview, kembalikan bar ke HP asli (dipanggil saat target/card di-deselect, ganti target, atau attack dibatalkan).</summary>
    public void ClearDamagePreview(float actualFrac)
    {
        if (currentHp == null || backgroundHp == null) return;
        if (!_isPreviewing) return; // sudah bersih, gak perlu ngapa-ngapain

        _isPreviewing = false;
        if (bgFollowCoroutine != null)
        {
            StopCoroutine(bgFollowCoroutine);
            bgFollowCoroutine = null;
        }

        currentHp.fillAmount = Mathf.Clamp01(actualFrac);
        backgroundHp.fillAmount = Mathf.Clamp01(actualFrac);
        lastHpFill = currentHp.fillAmount;
    }

    /// <summary>Shortcut: batalkan preview dari currentHealth/maxHealth (int).</summary>
    public void ClearDamagePreview(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return;
        float actualFrac = (float)Mathf.Max(0, currentHealth) / maxHealth;
        ClearDamagePreview(actualFrac);
    }

    // ─── Coroutine ────────────────────────────────────────────────────────

    private IEnumerator DelayedBackgroundFollow(float targetFill)
    {
        yield return new WaitForSeconds(delayBeforeFollow);

        while (Mathf.Abs(backgroundHp.fillAmount - targetFill) > 0.001f)
        {
            backgroundHp.fillAmount = Mathf.Lerp(
                backgroundHp.fillAmount,
                targetFill,
                lerpSpeed * Time.deltaTime
            );
            yield return null;
        }

        backgroundHp.fillAmount = targetFill;
        bgFollowCoroutine = null;
    }
}