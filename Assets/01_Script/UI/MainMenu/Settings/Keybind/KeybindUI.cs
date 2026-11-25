using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Generates one KeybindEntryUI row per action in the target action map,
/// filtered by the currently active control scheme (Keyboard or Controller).
/// Composite actions such as Move get one row per direction (Up/Down/Left/Right).
/// </summary>
public class KeybindUI : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset _asset;
    [SerializeField] private string _actionMapName = "Player";

    [Header("Scheme Names (must match Input Actions asset)")]
    [SerializeField] private string _keyboardSchemeName = "Keyboard";
    [SerializeField] private string _controllerSchemeName = "Controller";

    [Header("Row Prefab & Container")]
    [SerializeField] private KeybindEntryUI _rowPrefab;
    [SerializeField] private RectTransform _rowContainer;

    private readonly List<KeybindEntryUI> _entries = new();
    private InputActionMap _actionMap;

    public string ActiveSchemeName { get; private set; }

    private void Awake()
    {
        InputSystem.onDeviceChange += HandleDeviceChange;
        ActiveSchemeName = DetectActiveScheme();
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
        ClearRows();
    }

    private void OnEnable() => RebuildRows();
    private void OnDisable() => ClearRows();

    public void RebuildRows()
    {
        ClearRows();

        if (_asset == null || _rowPrefab == null || _rowContainer == null)
        {
            Debug.LogError("[KeybindUI] Asset, Row Prefab, or Row Container is missing.");
            return;
        }

        _actionMap = _asset.FindActionMap(_actionMapName, false);
        if (_actionMap == null)
        {
            Debug.LogError($"[KeybindUI] Action Map '{_actionMapName}' not found.");
            return;
        }

        ActiveSchemeName = DetectActiveScheme();

        foreach (var action in _actionMap.actions)
        {
            if (action.bindings.Count == 0) continue;

            var addedPart = false;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isPartOfComposite) continue;
                if (!MatchesScheme(action, i, ActiveSchemeName)) continue;

                addedPart = true;
                var partLabel = PrettyPartName(binding.name);
                AddRow(action, i, $"{action.name} {partLabel}");
            }

            if (addedPart) continue;

            int bindingIndex = FindSimpleBindingIndex(action, ActiveSchemeName);
            if (bindingIndex < 0) continue;
            AddRow(action, bindingIndex, action.name);
        }
    }

    public void ResetAllToDefault()
    {
        if (_actionMap == null) return;

        foreach (var action in _actionMap.actions)
        {
            action.RemoveAllBindingOverrides();
            KeybindManager.Instance?.ClearBinding(action.name);
            KeybindManager.Instance?.ClearBinding(OverrideKey(action, -1, _keyboardSchemeName));
            KeybindManager.Instance?.ClearBinding(OverrideKey(action, -1, _controllerSchemeName));

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isPartOfComposite) continue;
                KeybindManager.Instance?.ClearBinding(OverrideKey(action, i, _keyboardSchemeName));
                KeybindManager.Instance?.ClearBinding(OverrideKey(action, i, _controllerSchemeName));
            }
        }

        foreach (var entry in _entries)
            entry.RefreshBindingDisplay();
    }

    private string DetectActiveScheme()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.added && gamepad.enabled)
                return _controllerSchemeName;
        }
        return _keyboardSchemeName;
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad) return;

        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
        {
            if (ActiveSchemeName != _controllerSchemeName)
                RebuildRows();
        }
        else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
        {
            if (ActiveSchemeName == _controllerSchemeName)
                RebuildRows();
        }
    }

    private void AddRow(InputAction action, int bindingIndex, string displayName)
    {
        ApplySavedOverride(action, bindingIndex);

        var entry = Instantiate(_rowPrefab, _rowContainer);
        entry.Initialize(_asset, action, bindingIndex, ActiveSchemeName, displayName);
        entry.OnRebindCompleted += HandleRebindCompleted;
        _entries.Add(entry);
    }

    private int FindSimpleBindingIndex(InputAction action, string schemeName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isPartOfComposite || binding.isComposite) continue;
            if (MatchesScheme(action, i, schemeName))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// "Skill 1_Keyboard" for a normal binding.
    /// "Move|up_Keyboard" for one direction of a 2D Vector composite.
    /// </summary>
    public static string OverrideKey(InputAction action, int bindingIndex, string schemeName)
    {
        if (bindingIndex >= 0 && bindingIndex < action.bindings.Count)
        {
            var binding = action.bindings[bindingIndex];
            if (binding.isPartOfComposite)
            {
                var part = string.IsNullOrEmpty(binding.name) ? bindingIndex.ToString() : binding.name;
                return $"{action.name}|{part}_{schemeName}";
            }
        }

        return $"{action.name}_{schemeName}";
    }

    private void HandleRebindCompleted(InputAction action, int bindingIndex)
    {
        if (action == null) return;

        string effectivePath = action.bindings[bindingIndex].effectivePath;
        string key = OverrideKey(action, bindingIndex, ActiveSchemeName);
        KeybindManager.Instance?.SetBinding(key, effectivePath);
    }

    private void ApplySavedOverride(InputAction action, int bindingIndex)
    {
        string key = OverrideKey(action, bindingIndex, ActiveSchemeName);
        var saved = KeybindManager.Instance?.GetOverride(key);
        if (!string.IsNullOrEmpty(saved))
            action.ApplyBindingOverride(bindingIndex, saved);
    }

    private bool MatchesScheme(InputAction action, int bindingIndex, string schemeName)
    {
        var groups = GroupsOf(action, bindingIndex);
        if (string.IsNullOrEmpty(groups)) return false;
        if (groups.Contains(schemeName)) return true;

        // Stick bindings in the asset are grouped as "Gamepad", the UI scheme is "Controller".
        if (schemeName == _controllerSchemeName && groups.Contains("Gamepad")) return true;
        if (schemeName == _keyboardSchemeName && groups.Contains("Keyboard")) return true;
        return false;
    }

    private static string GroupsOf(InputAction action, int bindingIndex)
    {
        var groups = action.bindings[bindingIndex].groups;
        if (!string.IsNullOrEmpty(groups)) return groups;

        for (int i = bindingIndex; i >= 0; i--)
        {
            if (!action.bindings[i].isComposite) continue;
            return action.bindings[i].groups ?? string.Empty;
        }

        return string.Empty;
    }

    private static string PrettyPartName(string part)
    {
        if (string.IsNullOrEmpty(part)) return "Binding";
        return char.ToUpper(part[0]) + part.Substring(1);
    }

    private void ClearRows()
    {
        foreach (var entry in _entries)
        {
            entry.OnRebindCompleted -= HandleRebindCompleted;
            Destroy(entry.gameObject);
        }
        _entries.Clear();
    }
}