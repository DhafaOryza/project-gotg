using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Generates one KeybindEntryUI row per action in the target action map,
/// filtered by the currently active control scheme (Keyboard or Controller).
/// Wires rebind events to KeybindManager for persistence.
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
            if (action.type == InputActionType.PassThrough) continue;
            if (action.bindings.Count == 0) continue;

            int bindingIndex = FindBindingIndexForScheme(action, ActiveSchemeName);
            if (bindingIndex < 0) continue;

            ApplySavedOverride(action, bindingIndex);

            var entry = Instantiate(_rowPrefab, _rowContainer);
            entry.Initialize(_asset, action, bindingIndex, ActiveSchemeName);
            entry.OnRebindCompleted += HandleRebindCompleted;
            _entries.Add(entry);
        }
    }

    public void ResetAllToDefault()
    {
        if (_actionMap == null) return;

        foreach (var action in _actionMap.actions)
        {
            action.RemoveAllBindingOverrides();
            KeybindManager.Instance?.ClearBinding($"{action.name}_{_keyboardSchemeName}");
            KeybindManager.Instance?.ClearBinding($"{action.name}_{_controllerSchemeName}");
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

    private static int FindBindingIndexForScheme(InputAction action, string schemeName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isPartOfComposite) continue;
            if (binding.isComposite) continue;

            if (!string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(schemeName))
                return i;
        }
        return -1;
    }

    private void HandleRebindCompleted(InputAction action, int bindingIndex)
    {
        if (action == null) return;

        string effectivePath = action.bindings[bindingIndex].effectivePath;
        string key = $"{action.name}_{ActiveSchemeName}";
        KeybindManager.Instance?.SetBinding(key, effectivePath);
    }

    private void ApplySavedOverride(InputAction action, int bindingIndex)
    {
        string key = $"{action.name}_{ActiveSchemeName}";
        var saved = KeybindManager.Instance?.GetOverride(key);
        if (!string.IsNullOrEmpty(saved))
            action.ApplyBindingOverride(bindingIndex, saved);
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