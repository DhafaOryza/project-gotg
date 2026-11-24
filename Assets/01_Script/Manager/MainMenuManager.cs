using UnityEngine;

/// <summary>
/// Root manager for the Main Menu scene.
/// Persists across scene loads to keep AudioManager and SettingsData alive.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private SettingsData _settingsData;

    /// <summary>
    /// Provides access to the AudioManager instance.
    /// </summary>
    public AudioManager AudioManager => _audioManager;

    /// <summary>
    /// Provides access to the SettingsData instance.
    /// </summary>
    public SettingsData SettingsData => _settingsData;

    private void Awake()
    {
        // Singleton pattern to prevent duplicates when reloading Main Menu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Makes this GameObject and all its children survive scene changes
        DontDestroyOnLoad(gameObject);
    }
}