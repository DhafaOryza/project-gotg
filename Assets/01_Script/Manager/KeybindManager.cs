using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Persists custom keybind overrides across scenes via PlayerPrefs.
/// Keys are scheme-specific (e.g. "Skill 1_Keyboard" vs "Skill 1_Controller")
/// so keyboard and controller bindings don't overwrite each other.
/// </summary>
public class KeybindManager : MonoBehaviour
{
    public static KeybindManager Instance { get; private set; }

    [SerializeField] private InputActionAsset _inputActionAsset;

    private const string KeyPrefix = "Keybind_";

    private readonly Dictionary<string, string> _overrides = new();

    /// <summary>
    /// Returns the runtime InputActionAsset used by this manager.
    /// </summary>
    public InputActionAsset InputActionAsset => _inputActionAsset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadOverrides();
    }

    /// <summary>
    /// Saves a custom binding for the given key and flushes to disk.
    /// Key should be formatted as "{actionName}_{schemeName}".
    /// </summary>
    public void SetBinding(string key, string bindingPath)
    {
        if (string.IsNullOrEmpty(key)) return;

        _overrides[key] = bindingPath;
        PlayerPrefs.SetString(KeyPrefix + key, bindingPath);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Clears a custom binding, reverting to the asset default.
    /// </summary>
    public void ClearBinding(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        _overrides.Remove(key);
        PlayerPrefs.DeleteKey(KeyPrefix + key);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns the saved override path for a key, or null if none.
    /// </summary>
    public string GetOverride(string key)
    {
        return _overrides.TryGetValue(key, out var path) ? path : null;
    }

    /// <summary>
    /// Returns a snapshot of all saved overrides.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetAllOverrides() => _overrides;

    /// <summary>
    /// Clears every saved override, restoring all bindings to asset defaults.
    /// </summary>
    public void ResetAllBindings()
    {
        foreach (var key in _overrides.Keys)
            PlayerPrefs.DeleteKey(KeyPrefix + key);

        _overrides.Clear();
        PlayerPrefs.Save();
    }

    private void LoadOverrides()
    {
        _overrides.Clear();

        if (_inputActionAsset == null) return;

        foreach (var actionMap in _inputActionAsset.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                // Load both keyboard and controller overrides
                foreach (var scheme in new[] { "Keyboard", "Controller" })
                {
                    var key = $"{action.name}_{scheme}";
                    if (PlayerPrefs.HasKey(KeyPrefix + key))
                    {
                        _overrides[key] = PlayerPrefs.GetString(KeyPrefix + key);
                    }
                }
            }
        }
    }
}