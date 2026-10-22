using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Jenis transisi: menentukan panel teks mana yang ditampilkan.</summary>
public enum DayTransitionType
{
    Night,  // siang -> malam (sebelum combat), memakai panel Night
    Day     // malam -> siang (setelah combat, menang atau kalah), memakai panel Day
}

/// <summary>
/// Transisi layar: layar menggelap, di tengah (layar sedang tertutup penuh) onMidpoint dijalankan,
/// lalu teks panel Night atau Day muncul sebentar, dan layar terang kembali.
/// </summary>
public class DayTransitionUI : MonoBehaviour
{
    [Header("Curtain")]
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Night Panel (siang -> malam)")]
    [SerializeField] CanvasGroup nightGroupCanvas;
    [SerializeField] TextMeshProUGUI nightText;

    [Header("Day Panel (malam -> siang)")]
    [Tooltip("Kalau kosong, transisi Day memakai panel Night sebagai cadangan.")]
    [SerializeField] CanvasGroup dayGroupCanvas;
    [SerializeField] TextMeshProUGUI dayText;

    [Header("Timing (detik)")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [Tooltip("Jeda singkat saat layar tertutup penuh setelah onMidpoint, supaya perubahan dunia sempat ter-render.")]
    [SerializeField] private float midpointDelay = 0.2f;
    [SerializeField] private float textFadeDuration = 0.5f;
    [SerializeField] private float textHoldDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    private Coroutine playRoutine;

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        ResetVisuals();
    }

    void OnDisable()
    {
        // Kalau objek dinonaktifkan di tengah transisi, coroutine ikut berhenti: rapikan supaya layar tidak tertahan hitam.
        IsPlaying = false;
        playRoutine = null;
        ResetVisuals();
    }

    /// <summary>Transisi siang -> malam (panel Night). Dipertahankan agar pemanggil lama tetap jalan.</summary>
    public bool Play(string label, Action onMidpoint, Action onComplete = null)
    {
        return Play(DayTransitionType.Night, label, onMidpoint, onComplete);
    }

    /// <summary>
    /// Mulai transisi. type menentukan panel teks (Night / Day). onMidpoint dipanggil saat layar
    /// tertutup penuh (tempat paling aman untuk mengganti dunia/lingkungan). onComplete dipanggil
    /// setelah layar terang lagi. Return false kalau transisi sedang berjalan (panggilan diabaikan).
    /// </summary>
    public bool Play(DayTransitionType type, string label, Action onMidpoint, Action onComplete = null)
    {
        if (IsPlaying) return false;

        // Coroutine hanya bisa berjalan di objek aktif.
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        playRoutine = StartCoroutine(PlayRoutine(type, label, onMidpoint, onComplete));
        return true;
    }

    private IEnumerator PlayRoutine(DayTransitionType type, string label, Action onMidpoint, Action onComplete)
    {
        IsPlaying = true;
        ResetVisuals();

        // Pilih panel teks sesuai jenis transisi (Day jatuh ke panel Night kalau panel Day belum diisi).
        CanvasGroup textGroup;
        TextMeshProUGUI textLabel;

        if (type == DayTransitionType.Day && dayGroupCanvas != null)
        {
            textGroup = dayGroupCanvas;
            textLabel = dayText;
        }
        else
        {
            textGroup = nightGroupCanvas;
            textLabel = nightText;
        }

        if (textLabel != null)
            textLabel.text = label;

        // Layar menerima klik selama transisi supaya UI di bawahnya tidak bisa ditekan.
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // 1) Layar menggelap
        yield return Fade(canvasGroup, 0f, 1f, fadeInDuration);

        // 2) Pertengahan transisi: layar tertutup penuh
        SafeInvoke(onMidpoint);
        if (midpointDelay > 0f)
            yield return new WaitForSecondsRealtime(midpointDelay);

        // 3) Teks panel muncul, tertahan, lalu hilang
        yield return Fade(textGroup, 0f, 1f, textFadeDuration);
        if (textHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(textHoldDuration);
        yield return Fade(textGroup, 1f, 0f, textFadeDuration);

        // 4) Layar terang kembali
        yield return Fade(canvasGroup, 1f, 0f, fadeOutDuration);

        ResetVisuals();
        IsPlaying = false;
        playRoutine = null;

        SafeInvoke(onComplete);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        // Waktu unscaled: transisi tetap jalan walau Time.timeScale = 0.
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
    }

    // Exception di dalam callback tidak boleh menghentikan coroutine, kalau tidak layar tertahan hitam.
    private void SafeInvoke(Action action)
    {
        if (action == null) return;

        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    private void ResetVisuals()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (nightGroupCanvas != null)
            nightGroupCanvas.alpha = 0f;

        if (dayGroupCanvas != null)
            dayGroupCanvas.alpha = 0f;
    }
}