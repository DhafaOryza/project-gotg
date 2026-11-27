using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// One row in the keybind panel.
/// Shows the action name on the left and a rebind button on the right.
/// Clicking the button starts an interactive rebind; pressing Escape cancels.
/// </summary>
public class KeybindEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _actionNameText;
    [SerializeField] private Button _rebindButton;
    [SerializeField] private TextMeshProUGUI _bindingText;

    [Header("Listening State")]
    [SerializeField] private string _listeningLabel = "Press any key...";
    [SerializeField] private string _listeningLabelController = "Press any button...";
    [SerializeField] private Color _listeningColor = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color _normalColor = Color.white;

    [Header("Text Truncation")]
    [SerializeField] private int _maxDisplayLength = 16;

    private InputActionAsset _asset;
    private InputAction _action;
    private int _bindingIndex;
    private string _schemeName;
    private string _previousOverridePath;
    private bool _isListening;

    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    /// <summary>
    /// Raised when a rebind finishes successfully.
    /// Parameters: the rebound action and the binding index that changed.
    /// </summary>
    public event Action<InputAction, int> OnRebindCompleted;

    private void Awake()
    {
        if (_rebindButton != null)
            _rebindButton.onClick.AddListener(StartRebind);
    }

    private void OnDestroy()
    {
        if (_rebindButton != null)
            _rebindButton.onClick.RemoveListener(StartRebind);
        _rebindOp?.Dispose();
    }

    /// <summary>
    /// Called by KeybindUI after instantiation.
    /// </summary>
    public void Initialize(InputActionAsset asset, InputAction action, int bindingIndex, string schemeName, string displayName = null)
    {
        _asset        = asset;
        _action       = action;
        _bindingIndex = bindingIndex;
        _schemeName   = schemeName;
    
        if (_actionNameText != null)
            _actionNameText.text = string.IsNullOrEmpty(displayName) ? action.name : displayName;
    
        RefreshBindingDisplay();
    }

    // ── Rebind ────────────────────────────────────────────────────────────

    private void StartRebind()
    {
        _previousOverridePath = _action.bindings[_bindingIndex].overridePath;

        if (_isListening) return;
        if (_action == null) return;

        _asset?.Disable();
        _isListening = true;
        SetListeningVisual(true);

        var operation = _action
            .PerformInteractiveRebinding(_bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op => FinishRebind(false))
            .OnCancel(op  => FinishRebind(true));

        // Restrict rebind to controls that belong to the active scheme
        if (_schemeName == "Controller")
            operation.WithControlsHavingToMatchPath("<Gamepad>");
        else
            operation.WithControlsHavingToMatchPath("<Keyboard>|<Mouse>");

        _rebindOp = operation.Start();
    }

    private void FinishRebind(bool cancelled)
    {
        _rebindOp?.Dispose();
        _rebindOp = null;
        _isListening = false;
        _asset?.Enable();
    
        var reverted = false;
        if (!cancelled)
        {
            string newPath = _action.bindings[_bindingIndex].effectivePath;
            if (IsUsedByAnotherBinding(newPath))
            {
                RestorePreviousBinding();
                reverted = true;
                Debug.Log($"[Keybind] '{newPath}' sudah dipakai binding lain. Dikembalikan ke tombol sebelumnya.");
            }
        }
    
        SetListeningVisual(false);
        RefreshBindingDisplay();
    
        if (!cancelled && !reverted)
            OnRebindCompleted?.Invoke(_action, _bindingIndex);
    }
    
    private bool IsUsedByAnotherBinding(string path)
    {
        if (string.IsNullOrEmpty(path) || _action?.actionMap == null) return false;
    
        foreach (var action in _action.actionMap.actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action == _action && i == _bindingIndex) continue;
    
                var binding = action.bindings[i];
                if (binding.isComposite) continue;
                if (!BelongsToActiveScheme(action, i)) continue;
                if (PathsMatch(binding.effectivePath, path))
                    return true;
            }
        }
    
        return false;
    }
    
    private void RestorePreviousBinding()
    {
        if (string.IsNullOrEmpty(_previousOverridePath))
            _action.RemoveBindingOverride(_bindingIndex);
        else
            _action.ApplyBindingOverride(_bindingIndex, _previousOverridePath);
    }
    
    private bool BelongsToActiveScheme(InputAction action, int bindingIndex)
    {
        var groups = action.bindings[bindingIndex].groups;
        if (string.IsNullOrEmpty(groups))
        {
            for (int i = bindingIndex; i >= 0; i--)
            {
                if (!action.bindings[i].isComposite) continue;
                groups = action.bindings[i].groups;
                break;
            }
        }
    
        if (!string.IsNullOrEmpty(groups))
        {
            if (groups.Contains(_schemeName)) return true;
            if (_schemeName == "Controller" && groups.Contains("Gamepad")) return true;
            if (_schemeName == "Keyboard" && groups.Contains("Keyboard")) return true;
            return false;
        }
    
        var path = action.bindings[bindingIndex].effectivePath ?? string.Empty;
        if (_schemeName == "Controller")
            return path.Contains("Gamepad") || path.Contains("Joystick");
    
        return path.Contains("Keyboard") || path.Contains("Mouse");
    }
    
    private static bool PathsMatch(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    // ── Display ───────────────────────────────────────────────────────────

    public void RefreshBindingDisplay()
    {
        if (_action == null || _bindingText == null) return;

        string display = _action.GetBindingDisplayString(_bindingIndex,
            InputBinding.DisplayStringOptions.DontIncludeInteractions);

        if (display.Length > _maxDisplayLength)
            display = display.Substring(0, _maxDisplayLength) + "...";

        _bindingText.text = string.IsNullOrEmpty(display) ? "—" : display;
    }

    private void SetListeningVisual(bool listening)
    {
        if (_bindingText == null) return;

        if (listening)
        {
            // Use scheme-appropriate prompt text
            _bindingText.text = (_schemeName == "Controller")
                ? _listeningLabelController
                : _listeningLabel;
            _bindingText.color = _listeningColor;
        }
        else
        {
            _bindingText.color = _normalColor;
        }
    }
}