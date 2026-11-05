using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;

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

    /// <summary>
    /// Plays a random background music track from the day playlist with a smooth fade transition.
    /// </summary>
    public void PlayDayBGM()
    {
        AudioClip newClip = GetRandomClip(_dayBgmClips);
        if (newClip != null)
        {
            TransitionToClip(newClip);
        }
    }

    /// <summary>
    /// Plays a random background music track from the night playlist with a smooth fade transition.
    /// </summary>
    public void PlayNightBGM()
    {
        AudioClip newClip = GetRandomClip(_nightBgmClips);
        if (newClip != null)
        {
            TransitionToClip(newClip);
        }
    }

    /// <summary>
    /// Selects a random audio clip from the given array.
    /// </summary>
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }

    /// <summary>
    /// Manages the smooth fading transition between the current audio clip and a new audio clip.
    /// </summary>
    private void TransitionToClip(AudioClip newClip)
    {
        if (_bgmSource == null) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeTrackRoutine(newClip));
    }

    /// <summary>
    /// Coroutine that fades out the current volume, swaps the clip, and fades in the new volume.
    /// </summary>
    private IEnumerator FadeTrackRoutine(AudioClip newClip)
    {
        float targetVolume = 1f;

        // 1. Fade Out (jika audio sedang berputar)
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

        // 2. Ganti AudioClip baru dan atur volumenya ke 0
        _bgmSource.clip = newClip;
        _bgmSource.loop = true;
        _bgmSource.Play();

        // 3. Fade In
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