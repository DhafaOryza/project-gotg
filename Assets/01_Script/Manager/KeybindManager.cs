using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Persists custom keybind overrides across scenes via PlayerPrefs.
/// Attach to the root Managers GameObject alongside AudioManager and SettingsData.
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
    /// Saves a custom binding for the given action and flushes to disk.
    /// </summary>
    public void SetBinding(string actionName, string bindingPath)
    {
        if (string.IsNullOrEmpty(actionName)) return;

        _overrides[actionName] = bindingPath;
        PlayerPrefs.SetString(KeyPrefix + actionName, bindingPath);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Clears a custom binding, reverting to the asset default.
    /// </summary>
    public void ClearBinding(string actionName)
    {
        if (string.IsNullOrEmpty(actionName)) return;

        _overrides.Remove(actionName);
        PlayerPrefs.DeleteKey(KeyPrefix + actionName);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns the saved override path for an action, or null if none.
    /// </summary>
    public string GetOverride(string actionName)
    {
        return _overrides.TryGetValue(actionName, out var path) ? path : null;
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
        foreach (var actionName in _overrides.Keys)
            PlayerPrefs.DeleteKey(KeyPrefix + actionName);

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
                var key = KeyPrefix + action.name;
                if (PlayerPrefs.HasKey(key))
                {
                    _overrides[action.name] = PlayerPrefs.GetString(key);
                }
            }
        }
    }
}