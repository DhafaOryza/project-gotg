using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Generates one KeybindEntryUI row per action in the target action map,
/// wires rebind events to KeybindManager for persistence, and applies
/// saved overrides on rebuild.
/// </summary>
public class KeybindUI : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset _asset;
    [SerializeField] private string _actionMapName = "Player";

    [Header("Row Prefab & Container")]
    [SerializeField] private KeybindEntryUI _rowPrefab;
    [SerializeField] private RectTransform _rowContainer;

    private readonly List<KeybindEntryUI> _entries = new();
    private InputActionMap _actionMap;

    private void OnEnable()  => RebuildRows();
    private void OnDisable() => ClearRows();

    /// <summary>
    /// Destroys existing rows and regenerates one per action, applying saved overrides.
    /// Skips composite bindings (e.g. 2DVector) since they cannot be rebound directly.
    /// </summary>
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

        foreach (var action in _actionMap.actions)
        {
            // Skip PassThrough actions (used for composites like 2DVector)
            if (action.type == InputActionType.PassThrough) continue;

            // Skip actions with composite bindings (cannot be rebound directly)
            if (HasCompositeBinding(action)) continue;

            if (action.bindings.Count == 0) continue;

            // Apply saved override BEFORE creating the row so the label shows it.
            ApplySavedOverride(action);

            var entry = Instantiate(_rowPrefab, _rowContainer);
            entry.Initialize(_asset, action, 0);
            entry.OnRebindCompleted += HandleRebindCompleted;
            _entries.Add(entry);
        }
    }

    /// <summary>
    /// Checks if the action uses any composite bindings (e.g. 2DVector, 1DComposite).
    /// </summary>
    private static bool HasCompositeBinding(InputAction action)
    {
        foreach (var binding in action.bindings)
        {
            if (binding.isComposite) return true;
        }
        return false;
    }

    /// <summary>
    /// Resets every action back to its asset default binding.
    /// </summary>
    public void ResetAllToDefault()
    {
        if (_actionMap == null) return;

        foreach (var action in _actionMap.actions)
        {
            action.RemoveAllBindingOverrides();
            KeybindManager.Instance?.ClearBinding(action.name);
        }

        foreach (var entry in _entries)
            entry.RefreshBindingDisplay();
    }

    private void HandleRebindCompleted(InputAction action, int bindingIndex)
    {
        if (action == null) return;

        // Read the effective path of the binding that was just changed.
        string effectivePath = action.bindings[bindingIndex].effectivePath;
        KeybindManager.Instance?.SetBinding(action.name, effectivePath);
    }

    private void ApplySavedOverride(InputAction action)
    {
        var saved = KeybindManager.Instance?.GetOverride(action.name);
        if (!string.IsNullOrEmpty(saved))
            action.ApplyBindingOverride(0, new InputBinding { overridePath = saved });
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