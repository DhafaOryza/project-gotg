using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Transisi siang -> malam: layar menggelap, di tengah (layar sedang tertutup penuh) onMidpoint dijalankan,
/// lalu teks "Night N" muncul sebentar, dan layar terang kembali.
/// </summary>
public class DayTransitionUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup nightGroupCanvas;
    [SerializeField] TextMeshProUGUI nightText;

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

    /// <summary>
    /// Mulai transisi. onMidpoint dipanggil saat layar tertutup penuh (tempat paling aman untuk
    /// mengganti dunia/lingkungan). onComplete dipanggil setelah layar terang lagi.
    /// Return false kalau transisi sedang berjalan (panggilan diabaikan).
    /// </summary>
    public bool Play(string label, Action onMidpoint, Action onComplete = null)
    {
        if (IsPlaying) return false;

        // Coroutine hanya bisa berjalan di objek aktif.
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        playRoutine = StartCoroutine(PlayRoutine(label, onMidpoint, onComplete));
        return true;
    }

    private IEnumerator PlayRoutine(string label, Action onMidpoint, Action onComplete)
    {
        IsPlaying = true;
        ResetVisuals();

        if (nightText != null)
            nightText.text = label;

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

        // 3) Teks malam muncul, tertahan, lalu hilang
        yield return Fade(nightGroupCanvas, 0f, 1f, textFadeDuration);
        if (textHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(textHoldDuration);
        yield return Fade(nightGroupCanvas, 1f, 0f, textFadeDuration);

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
    }
}