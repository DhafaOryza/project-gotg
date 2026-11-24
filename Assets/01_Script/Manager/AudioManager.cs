using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("BGM Playlists")]
    [SerializeField] private AudioClip[] _dayBgmClips;
    [SerializeField] private AudioClip[] _nightBgmClips;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 1.5f;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayDayBGM();
    }

    // ═══════════════════════════════════════════════════════════════
    // BGM Methods
    // ═══════════════════════════════════════════════════════════════
    public void PlayDayBGM()
    {
        AudioClip newClip = GetRandomClip(_dayBgmClips);
        if (newClip != null) TransitionToClip(newClip);
    }

    public void PlayNightBGM()
    {
        AudioClip newClip = GetRandomClip(_nightBgmClips);
        if (newClip != null) TransitionToClip(newClip);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    private void TransitionToClip(AudioClip newClip)
    {
        if (_bgmSource == null) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeTrackRoutine(newClip));
    }

    private IEnumerator FadeTrackRoutine(AudioClip newClip)
    {
        float targetVolume = 1f;

        if (_bgmSource.isPlaying)
        {
            float startVolume = _bgmSource.volume;
            float timer = 0f;
            while (timer < _fadeDuration)
            {
                timer += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / _fadeDuration);
                yield return null;
            }
            _bgmSource.Stop();
        }

        _bgmSource.clip = newClip;
        _bgmSource.loop = true;
        _bgmSource.Play();

        float fadeTimer = 0f;
        while (fadeTimer < _fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, targetVolume, fadeTimer / _fadeDuration);
            yield return null;
        }
        _bgmSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    // ═══════════════════════════════════════════════════════════════
    // SFX Methods
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Memutar SFX sekali (one-shot). Cocok untuk efek suara seperti klik, jump, hit, dll.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Memutar SFX dengan volume spesifik (0-1). 
    /// Volume ini akan dikalikan dengan volume master SFX dari AudioMixer.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    /// <summary>
    /// Memutar SFX dengan loop (misal: suara mesin, angin, api).
    /// Panggil StopSFXLoop() untuk menghentikannya.
    /// </summary>
    public void PlaySFXLoop(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        if (_sfxSource.isPlaying) _sfxSource.Stop();
        _sfxSource.clip = clip;
        _sfxSource.loop = true;
        _sfxSource.Play();
    }

    /// <summary>
    /// Menghentikan SFX yang sedang loop.
    /// </summary>
    public void StopSFXLoop()
    {
        if (_sfxSource == null) return;
        _sfxSource.Stop();
    }
}