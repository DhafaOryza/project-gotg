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
    private InputActionRebindingExtensions.RebindingOperation _rebindOp;
    private bool _isListening;

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
        SetListeningVisual(false);
        RefreshBindingDisplay();

        if (!cancelled)
            OnRebindCompleted?.Invoke(_action, _bindingIndex);
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