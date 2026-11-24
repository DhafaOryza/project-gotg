using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages all game settings with logarithmic volume conversion
/// and PlayerPrefs persistence.
/// </summary>
public class SettingsData : MonoBehaviour
{
    public static SettingsData Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _musicVolumeParam = "MusicVolume";
    [SerializeField] private string _sfxVolumeParam = "SFXVolume";

    // PlayerPrefs keys
    private const string KeyMusic = "musicVolume";
    private const string KeySFX = "sfxVolume";
    private const string KeyFullscreen = "isFullscreen";
    private const string KeyShake = "cameraShake";

    // Default values
    private const float DefaultVolume = 1f;
    private const bool DefaultFullscreen = true;
    private const bool DefaultShake = true;

    // Current values
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public bool IsFullscreen { get; private set; }
    public bool ShakeEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Check if settings exist, otherwise initialize with defaults
        if (PlayerPrefs.HasKey(KeyMusic))
        {
            LoadSettings();
        }
        else
        {
            SetDefaultSettings();
        }
    }

    /// <summary>
    /// Loads all settings from PlayerPrefs and applies them.
    /// </summary>
    private void LoadSettings()
    {
        MusicVolume = PlayerPrefs.GetFloat(KeyMusic, DefaultVolume);
        SFXVolume = PlayerPrefs.GetFloat(KeySFX, DefaultVolume);
        IsFullscreen = PlayerPrefs.GetInt(KeyFullscreen, DefaultFullscreen ? 1 : 0) == 1;
        ShakeEnabled = PlayerPrefs.GetInt(KeyShake, DefaultShake ? 1 : 0) == 1;

        ApplyAllSettings();
    }

    /// <summary>
    /// Sets default values and saves them to PlayerPrefs.
    /// </summary>
    private void SetDefaultSettings()
    {
        MusicVolume = DefaultVolume;
        SFXVolume = DefaultVolume;
        IsFullscreen = DefaultFullscreen;
        ShakeEnabled = DefaultShake;

        SaveAllSettings();
        ApplyAllSettings();
    }

    /// <summary>
    /// Applies all current settings to their respective systems.
    /// </summary>
    private void ApplyAllSettings()
    {
        ApplyMusicVolume();
        ApplySFXVolume();
        ApplyFullscreen();
        ApplyShake();
    }

    /// <summary>
    /// Saves all current settings to PlayerPrefs.
    /// </summary>
    public void SaveAllSettings()
    {
        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
        PlayerPrefs.SetInt(KeyFullscreen, IsFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KeyShake, ShakeEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ── Music Volume ──────────────────────────────────────────────────

    /// <summary>
    /// Sets music volume from slider (linear 0-1) and applies to AudioMixer (logarithmic).
    /// </summary>
    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
    }

    /// <summary>
    /// Applies current MusicVolume to AudioMixer using logarithmic conversion.
    /// </summary>
    private void ApplyMusicVolume()
    {
        if (_audioMixer == null) return;
        _audioMixer.SetFloat(_musicVolumeParam, LinearToLogarithmic(MusicVolume));
    }

    // ── SFX Volume ────────────────────────────────────────────────────

    /// <summary>
    /// Sets SFX volume from slider (linear 0-1) and applies to AudioMixer (logarithmic).
    /// </summary>
    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        ApplySFXVolume();
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
    }

    /// <summary>
    /// Applies current SFXVolume to AudioMixer using logarithmic conversion.
    /// </summary>
    private void ApplySFXVolume()
    {
        if (_audioMixer == null) return;
        _audioMixer.SetFloat(_sfxVolumeParam, LinearToLogarithmic(SFXVolume));
    }

    // ── Fullscreen ────────────────────────────────────────────────────

    /// <summary>
    /// Sets fullscreen mode and saves to PlayerPrefs.
    /// </summary>
    public void SetFullscreen(bool fullscreen)
    {
        IsFullscreen = fullscreen;
        ApplyFullscreen();
        PlayerPrefs.SetInt(KeyFullscreen, IsFullscreen ? 1 : 0);
    }

    /// <summary>
    /// Applies current fullscreen setting to Screen.
    /// </summary>
    private void ApplyFullscreen()
    {
        Screen.fullScreenMode = IsFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }

    // ── Camera Shake ──────────────────────────────────────────────────

    /// <summary>
    /// Sets camera shake enabled state and saves to PlayerPrefs.
    /// </summary>
    public void SetShake(bool enabled)
    {
        ShakeEnabled = enabled;
        ApplyShake();
        PlayerPrefs.SetInt(KeyShake, ShakeEnabled ? 1 : 0);
    }

    /// <summary>
    /// Applies current shake setting to CameraEffectsManager.
    /// </summary>
    private void ApplyShake()
    {
        if (CameraEffectsManager.Instance != null)
        {
            CameraEffectsManager.Instance.SetShakeEnabled(ShakeEnabled);
        }
    }

    // ── Conversion Helpers ────────────────────────────────────────────

    /// <summary>
    /// Converts linear volume (0-1) to logarithmic decibel value for AudioMixer.
    /// Returns -80 dB for values near zero (effectively silent).
    /// </summary>
    private static float LinearToLogarithmic(float linear)
    {
        // Prevent log(0) which would cause -Infinity
        if (linear <= 0.0001f)
        {
            return -80f;
        }

        return Mathf.Log10(linear) * 20f;
    }
}