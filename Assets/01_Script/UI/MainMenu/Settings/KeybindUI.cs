using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Builds the keybind list from the Player action map.
/// Only bindings matching the currently active control scheme are shown.
/// When the active device changes, the list is rebuilt automatically.
/// </summary>
public class KeybindUI : MonoBehaviour
{
    [Header("Asset")]
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private string _actionMapName = "Player";

    [Header("Scheme Names (must match Input Actions asset)")]
    [SerializeField] private string _keyboardSchemeName = "Keyboard";
    [SerializeField] private string _controllerSchemeName = "Controller";

    [Header("Row Prefab & Container")]
    [SerializeField] private KeybindEntryUI _rowPrefab;
    [SerializeField] private Transform _rowContainer;

    private const string OverridesKey = "KeybindOverrides";
    private readonly List<KeybindEntryUI> _rows = new List<KeybindEntryUI>();
    private string _activeScheme = "";

    private void OnEnable()
    {
        InputSystem.onDeviceChange += HandleDeviceChange;
        LoadOverrides();
        DetectAndBuild();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
        ClearRows();
    }

    // ── Device Detection ─────────────────────────────────────────────────

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // Rebuild whenever a gamepad is added, removed, or reconnected.
        if (device is Gamepad)
        {
            DetectAndBuild();
            return;
        }

        // Rebuild whenever a keyboard is added or reconnected.
        if (device is Keyboard &&
            (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected))
        {
            DetectAndBuild();
        }
    }

    private void DetectAndBuild()
    {
        string scheme = IsGamepadConnected()
            ? _controllerSchemeName
            : _keyboardSchemeName;

        if (scheme == _activeScheme && _rows.Count > 0) return;

        _activeScheme = scheme;
        BuildRows(_activeScheme);
    }

    private static bool IsGamepadConnected()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad && device.enabled)
                return true;
        }

        return false;
    }

    // ── Build ─────────────────────────────────────────────────────────────

    private void BuildRows(string schemeName)
    {
        ClearRows();

        if (_inputActions == null || _rowPrefab == null || _rowContainer == null) return;

        var map = _inputActions.FindActionMap(_actionMapName, false);
        if (map == null)
        {
            Debug.LogWarning($"[KeybindUI] Action map '{_actionMapName}' tidak ditemukan.", this);
            return;
        }

        // Find the index of the target control scheme so we can match bindings by group.
        int schemeIndex = _inputActions.FindControlSchemeIndex(schemeName);
        string groupFilter = schemeIndex >= 0
            ? _inputActions.controlSchemes[schemeIndex].bindingGroup
            : schemeName;

        foreach (var action in map.actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];

                // Skip composite headers (e.g. "2D Vector") — only show leaves.
                if (binding.isComposite) continue;

                // Skip bindings that do not belong to the active scheme.
                if (!BindingMatchesGroup(binding, groupFilter)) continue;

                KeybindEntryUI row = Instantiate(_rowPrefab, _rowContainer);
                row.Initialize(_inputActions, action, i);
                row.OnRebindCompleted += SaveOverrides;
                _rows.Add(row);
            }
        }
    }

    /// <summary>
    /// Returns true if the binding's groups string contains the target group.
    /// A binding with an empty groups string is considered universal and is always shown.
    /// </summary>
    private static bool BindingMatchesGroup(InputBinding binding, string groupFilter)
    {
        if (string.IsNullOrEmpty(binding.groups)) return true;

        foreach (string group in binding.groups.Split(';'))
        {
            if (string.Equals(group.Trim(), groupFilter,
                    System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
        {
            if (row == null) continue;

            row.OnRebindCompleted -= SaveOverrides;
            Destroy(row.gameObject);
        }

        _rows.Clear();
    }

    // ── Persistence ───────────────────────────────────────────────────────

    private void SaveOverrides()
    {
        if (_inputActions == null) return;

        string json = _inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(OverridesKey, json);
        PlayerPrefs.Save();
    }

    private void LoadOverrides()
    {
        if (_inputActions == null) return;

        if (!PlayerPrefs.HasKey(OverridesKey)) return;

        string json = PlayerPrefs.GetString(OverridesKey);
        if (!string.IsNullOrEmpty(json))
            _inputActions.LoadBindingOverridesFromJson(json);
    }
}