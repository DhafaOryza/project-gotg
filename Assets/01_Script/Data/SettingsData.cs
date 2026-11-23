using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Loads, saves and applies all settings.
/// Values persist via PlayerPrefs across sessions.
/// </summary>
public class SettingsData : MonoBehaviour
{
    public static SettingsData Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _musicVolumeParam = "MusicVolume";
    [SerializeField] private string _sfxVolumeParam   = "SFXVolume";

    // PlayerPrefs keys
    private const string KeyMusic      = "Settings_MusicVolume";
    private const string KeySFX        = "Settings_SFXVolume";
    private const string KeyFullscreen = "Settings_Fullscreen";
    private const string KeyShake      = "Settings_CameraShake";

    // Current values (0–1 for volume, bool for the rest)
    public float MusicVolume { get; private set; }
    public float SFXVolume   { get; private set; }
    public bool  IsFullscreen { get; private set; }
    public bool  ShakeEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    /// <summary>Reads saved values and applies them immediately.</summary>
    public void Load()
    {
        MusicVolume  = PlayerPrefs.GetFloat(KeyMusic, 1f);
        SFXVolume    = PlayerPrefs.GetFloat(KeySFX, 1f);
        IsFullscreen = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
        ShakeEnabled = PlayerPrefs.GetInt(KeyShake, 1) == 1;

        Apply();
    }

    /// <summary>Applies current values to Unity systems without saving.</summary>
    public void Apply()
    {
        SetMusicVolume(MusicVolume);
        SetSFXVolume(SFXVolume);
        SetFullscreen(IsFullscreen);
        SetShake(ShakeEnabled);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        if (_audioMixer != null)
            _audioMixer.SetFloat(_musicVolumeParam, LinearToDecibel(MusicVolume));
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        if (_audioMixer != null)
            _audioMixer.SetFloat(_sfxVolumeParam, LinearToDecibel(SFXVolume));
    }

    public void SetFullscreen(bool fullscreen)
    {
        IsFullscreen = fullscreen;
        Screen.fullScreenMode = fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }

    public void SetShake(bool enabled)
    {
        ShakeEnabled = enabled;
        if (CameraEffectsManager.Instance != null)
            CameraEffectsManager.Instance.SetShakeEnabled(enabled);
    }

    /// <summary>Writes current values to PlayerPrefs and flushes to disk.</summary>
    public void Save()
    {
        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
        PlayerPrefs.SetInt(KeyFullscreen, IsFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KeyShake, ShakeEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static float LinearToDecibel(float linear)
    {
        // Clamp above zero to avoid log(0); -80 dB is effectively silent.
        return linear > 0.0001f ? 20f * Mathf.Log10(linear) : -80f;
    }
}