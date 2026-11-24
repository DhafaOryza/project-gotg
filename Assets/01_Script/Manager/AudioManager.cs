using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton that owns the persistent BGM and SFX audio sources.
/// Lives on the root "Managers" GameObject in the first loaded scene
/// and survives scene changes via DontDestroyOnLoad.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM Source")]
    [SerializeField] private AudioSource _bgmSource;

    [Header("SFX Source")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("BGM Playlists")]
    [SerializeField] private AudioClip[] _dayBgmClips;
    [SerializeField] private AudioClip[] _nightBgmClips;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 1.5f;

    private Coroutine _fadeCoroutine;

    /// <summary>
    /// Returns true while the background music is actively playing.
    /// </summary>
    public bool IsBgmPlaying => _bgmSource != null && _bgmSource.isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        PlayDayBgm();
    }

    /// <summary>
    /// Plays a random track from the day playlist with a fade transition.
    /// </summary>
    public void PlayDayBgm()
    {
        PlayBgmFrom(_dayBgmClips);
    }

    /// <summary>
    /// Plays a random track from the night playlist with a fade transition.
    /// </summary>
    public void PlayNightBgm()
    {
        PlayBgmFrom(_nightBgmClips);
    }

    /// <summary>
    /// Plays a one-shot sound effect through the SFX source.
    /// </summary>
    public void PlaySfx(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays a one-shot sound effect with a custom volume multiplier (0–1).
    /// The multiplier is applied on top of the SFX group volume set by the mixer.
    /// </summary>
    public void PlaySfx(AudioClip clip, float volume)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private void PlayBgmFrom(AudioClip[] clips)
    {
        AudioClip newClip = GetRandomClip(clips);
        if (newClip != null)
        {
            TransitionToClip(newClip);
        }
    }

    private static AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    private void TransitionToClip(AudioClip newClip)
    {
        if (_bgmSource == null) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeTrackRoutine(newClip));
    }

    private IEnumerator FadeTrackRoutine(AudioClip newClip)
    {
        const float targetVolume = 1f;

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
}