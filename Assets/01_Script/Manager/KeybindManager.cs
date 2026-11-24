using System;
using System.Collections.Generic;
using System.Text;
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

    /// <summary>
    /// Fired after any override is saved, cleared, or reset.
    /// PlayerController listens so its runtime InputActionAsset clone stays in sync.
    /// </summary>
    public static event Action OnOverridesChanged;

    [SerializeField] private InputActionAsset _inputActionAsset;

    private const string KeyPrefix = "Keybind_";
    private const string IndexKey = "Keybind__Index";

    private readonly Dictionary<string, string> _overrides = new();

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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetBinding(string key, string bindingPath)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(bindingPath)) return;

        _overrides[key] = bindingPath;
        PlayerPrefs.SetString(KeyPrefix + key, bindingPath);
        WriteIndex();
        PlayerPrefs.Save();
        NotifyOverridesChanged();
    }

    public void ClearBinding(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        _overrides.Remove(key);
        PlayerPrefs.DeleteKey(KeyPrefix + key);
        WriteIndex();
        PlayerPrefs.Save();
        NotifyOverridesChanged();
    }

    public string GetOverride(string key)
    {
        return _overrides.TryGetValue(key, out var path) ? path : null;
    }

    public IReadOnlyDictionary<string, string> GetAllOverrides() => _overrides;

    public void ResetAllBindings()
    {
        foreach (var key in _overrides.Keys)
            PlayerPrefs.DeleteKey(KeyPrefix + key);

        _overrides.Clear();
        PlayerPrefs.DeleteKey(IndexKey);
        PlayerPrefs.Save();
        NotifyOverridesChanged();
    }

    private void LoadOverrides()
    {
        _overrides.Clear();

        var index = PlayerPrefs.GetString(IndexKey, string.Empty);
        if (!string.IsNullOrEmpty(index))
        {
            foreach (var key in index.Split('\n'))
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (!PlayerPrefs.HasKey(KeyPrefix + key)) continue;
                _overrides[key] = PlayerPrefs.GetString(KeyPrefix + key);
            }
            return;
        }

        if (_inputActionAsset == null) return;

        foreach (var actionMap in _inputActionAsset.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                foreach (var scheme in new[] { "Keyboard", "Controller" })
                {
                    var key = $"{action.name}_{scheme}";
                    if (PlayerPrefs.HasKey(KeyPrefix + key))
                        _overrides[key] = PlayerPrefs.GetString(KeyPrefix + key);
                }
            }
        }

        if (_overrides.Count > 0)
            WriteIndex();
    }

    private void WriteIndex()
    {
        var builder = new StringBuilder();
        var first = true;
        foreach (var key in _overrides.Keys)
        {
            if (!first) builder.Append('\n');
            first = false;
            builder.Append(key);
        }

        PlayerPrefs.SetString(IndexKey, builder.ToString());
    }

    private static void NotifyOverridesChanged()
    {
        OnOverridesChanged?.Invoke();
    }
}