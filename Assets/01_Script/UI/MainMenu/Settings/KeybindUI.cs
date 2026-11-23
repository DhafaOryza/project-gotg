using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Builds the keybind list from the Player action map.
/// Each non-composite binding gets its own KeybindEntryUI row.
/// Rebind overrides are saved to PlayerPrefs when any row finishes.
/// </summary>
public class KeybindUI : MonoBehaviour
{
    [Header("Asset")]
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private string _actionMapName = "Player";

    [Header("Row Prefab & Container")]
    [SerializeField] private KeybindEntryUI _rowPrefab;
    [SerializeField] private Transform _rowContainer;

    private const string OverridesKey = "KeybindOverrides";
    private readonly List<KeybindEntryUI> _rows = new List<KeybindEntryUI>();

    private void OnEnable()
    {
        LoadOverrides();
        BuildRows();
    }

    private void OnDisable()
    {
        ClearRows();
    }

    // ── Build ─────────────────────────────────────────────────────────────

    private void BuildRows()
    {
        ClearRows();

        if (_inputActions == null || _rowPrefab == null || _rowContainer == null) return;

        var map = _inputActions.FindActionMap(_actionMapName, false);
        if (map == null)
        {
            Debug.LogWarning($"[KeybindUI] Action map '{_actionMapName}' tidak ditemukan.", this);
            return;
        }

        foreach (var action in map.actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];

                // Skip composite headers (e.g. "2D Vector") — only show leaves.
                if (binding.isComposite) continue;

                KeybindEntryUI row = Instantiate(_rowPrefab, _rowContainer);
                row.Initialize(_inputActions, action, i);
                row.OnRebindCompleted += SaveOverrides;
                _rows.Add(row);
            }
        }
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
        Debug.Log("[KeybindUI] Keybind tersimpan.");
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